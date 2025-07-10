using System;
using Enterprise.DataTransfer.Native.DB.Keys;

namespace Enterprise.DataTransfer.Native.DB
{
	class ColumnBuilder
	{
		public ColumnBuilder(Table table)
		{
			this.table = table;
		}
		readonly Table table;

		protected ColumnDef columnDef;

		public ColumnDef Construct(ColumnSchema columnSchema)
		{
			BuildColumn();
			BuildName(columnSchema);
			BuildDataType(columnSchema);
			BuildDefaultValue(columnSchema);
			BuildNullable(columnSchema);
			BuildLength(columnSchema);
			BuildPrecision(columnSchema);
			BuildKey();
			return columnDef;
		}

		internal void BuildPrecision(ColumnSchema columnSchema)
		{
			columnDef.Scale = (int)(columnSchema.Scale ?? 0); // a value of zero is equivalent to no precision. If we actually wanted no decimals, we'd use an integer data type.
			columnDef.Precision = (byte)(columnSchema.Precision ?? (byte)0);
		}

		#region Construct

		internal void BuildColumn()
		{
			columnDef = new ColumnDef(table);
		}

		internal void BuildName(ColumnSchema columnSchema)
		{
			columnDef.Name = columnSchema.Name;
		}

		internal void BuildDataType(ColumnSchema columnSchema)
		{
			columnDef.DataType = columnSchema.DataType;
		}

		internal void BuildDefaultValue(ColumnSchema columnSchema)
		{
			columnDef.DefaultValue = columnSchema.DefaultValue;
		}

		internal void BuildNullable(ColumnSchema columnSchema)
		{
			var hasDefaultValue = columnDef.DefaultValue != DBNull.Value;
			columnDef.Nullable = columnSchema.IsNullable == "YES";
			columnDef.DoesNotRequireAValue = columnDef.Nullable || hasDefaultValue;
		}

		internal void BuildLength(ColumnSchema columnSchema)
		{
			if (columnDef.DataType.ToLower().Contains(DbDataType.Char) || columnDef.DataType.ToLower().Contains(DbDataType.Text))
			{
				columnDef.Length = (int)columnSchema.Length;
			}
		}

		internal void BuildKey()
		{
			switch (columnDef.Type)
			{
				case ColumnType.PolymorphicKey:
					columnDef = new PolymorphicKey(columnDef);
					break;
				case ColumnType.ForeignKey:
					columnDef = new ForeignKey(columnDef);
					break;

				case ColumnType.NaturalKey:
					columnDef = new NaturalKey(columnDef);
					break;

				case ColumnType.PrimaryKey:
					columnDef = new PrimaryKey(columnDef);
					break;
			}
		}
		#endregion
	}
}