using System.Collections.Generic;

namespace Enterprise.DataTransfer.Native.DB
{
	public interface IKeyFinder : IColumnFinder
	{
		ColumnDef PrimaryKey { get; }
		IEnumerable<ColumnDef> TableCodes { get; }
	}
}