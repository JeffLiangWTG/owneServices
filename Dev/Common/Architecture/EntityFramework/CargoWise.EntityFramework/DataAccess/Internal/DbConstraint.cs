using System;

namespace CargoWise.EntityFramework
{
	[Flags]
	public enum DbConstraintType
	{
		Unknown = 1 << 0,
		Default = 1 << 1,
		Check = 1 << 2,
		Unique = 1 << 3,
		PrimaryKey = 1 << 4,
		ForeignKey = 1 << 5
	}

	public class DbConstraint
	{
		public DbConstraint(string constraintName, DbConstraintType type, string[] columnNames, string foreignTable, string data)
		{
			this.Type = type;
			this.ColumnNames = columnNames;
			this.ConstraintName = constraintName;
			this.ForeignTable = foreignTable;
			this.Data = data;
		}

		public readonly DbConstraintType Type;
		public readonly string[] ColumnNames;
		public readonly string ConstraintName;
		public readonly string ForeignTable;
		public readonly string Data;
	}
}
