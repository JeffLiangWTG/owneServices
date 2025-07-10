using System;
using Enterprise.DataTransfer.Native.DB.Sql;

namespace Enterprise.DataTransfer.Native.DB
{
	public struct DataCell
	{
		public DataCell(ColumnDef columnDef)
			: this()
		{
			ColumnDef = columnDef;
		}

		public ColumnDef ColumnDef { get; private set; }

		public Table Table
		{
			get { return ColumnDef.Table; }
		}

		public object Value
		{
			get { return value ?? DBNull.Value; }
			set { this.value = value; }
		}
		object value;

		public static explicit operator Criteria(DataCell cell)
		{
			return new Criteria
			{
				ColumnName = cell.ColumnDef.Name,
				TableName = cell.Table.Name,
				Value = cell.Value
			};
		}
	}
}
