namespace Enterprise.ZArchitecture.Web.GUI
{
	public interface IUniqueKeyColumn
	{
		int UniqueKey { get; }
		int ColumnIndex { get; set; }
		object ColumnKey { get; set; }
	}
}
