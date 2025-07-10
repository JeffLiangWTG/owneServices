using System;

namespace Enterprise.DataTransfer.Native.Common
{
	/// <summary>
	/// A set of column values.
	/// Can be either a DataRow or a list of Criteria.
	/// Used for efficent matching of row/criteria to row/criteria
	/// </summary>
	interface IColumnValueSet : IEquatable<IColumnValueSet>
	{
		object GetValue(int columnIndex);
		int ColumnCount { get; }
	}
}
