﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Data.Schema
{
    class Table
    {
        private readonly string _actualName;
        private readonly string _homogenizedName;
        private readonly string _schema;
        private readonly DatabaseSchema _databaseSchema;
        private readonly Lazy<ColumnCollection> _lazyColumns;

        public Table(string name, string schema, DatabaseSchema databaseSchema)
        {
            _actualName = name;
            _homogenizedName = name.Homogenize();
            _databaseSchema = databaseSchema;
            _schema = schema;
            _lazyColumns = new Lazy<ColumnCollection>(GetColumns);
        }

        public string HomogenizedName
        {
            get { return _homogenizedName; }
        }

        public DatabaseSchema DatabaseSchema
        {
            get { return _databaseSchema; }
        }

        public string Schema
        {
            get { return _schema; }
        }

        public string ActualName
        {
            get { return _actualName; }
        }

        public IEnumerable<Column> Columns
        {
            get { return _lazyColumns.Value.AsEnumerable(); }
        }

        public Column FindColumn(string columnName)
        {
            return _lazyColumns.Value.Find(columnName);
        }

        private ColumnCollection GetColumns()
        {
            return new ColumnCollection(Column.GetColumnsForTable(this));
        }

        public TableJoin GetMaster(string name)
        {
            var master = DatabaseSchema.FindTable(name);
            if (master != null)
            {
                string commonColumnName = GetCommonColumnName(master);

                if (commonColumnName != null)
                {
                    return new TableJoin(master, master.FindColumn(commonColumnName), this, FindColumn(commonColumnName));
                }
            }
            return null;
        }

        private string GetCommonColumnName(Table other)
        {
            return other.Columns
                .Select(c => c.HomogenizedName)
                .Intersect(this.Columns.Select(c => c.HomogenizedName))
                .SingleOrDefault();
        }

        public TableJoin GetDetail(string name)
        {
            var detail = DatabaseSchema.FindTable(name);
            string commonColumnName = GetCommonColumnName(detail);
            if (detail.Columns.Select(c => c.HomogenizedName).Intersect(this.Columns.Select(c => c.HomogenizedName)).Count() == 1)
            {
                return new TableJoin(this, FindColumn(commonColumnName), detail, detail.FindColumn(commonColumnName));
            }
            return null;
        }
    }
}