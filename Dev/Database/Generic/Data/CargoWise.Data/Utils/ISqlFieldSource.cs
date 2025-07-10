using System;

namespace CargoWise.Data
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface ISqlFieldSource : IRowFieldInfo
	{
		// Will have a Connection in the future.
	}

	public interface IRowFieldInfo
	{
		string TableName { get; }
		string ColumnName { get; }
		string PKColumnName { get; }
		Guid RowPK { get; }
	}
}
