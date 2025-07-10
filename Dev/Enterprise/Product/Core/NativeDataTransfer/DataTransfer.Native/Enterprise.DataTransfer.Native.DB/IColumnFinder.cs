namespace Enterprise.DataTransfer.Native.DB
{
	public interface IColumnFinder
	{
		ColumnDef Find(string name);
	}
}