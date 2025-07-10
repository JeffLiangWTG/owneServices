namespace CargoWise.EntityFramework
{
	public interface IDesignTimeDataSourceType : ITopLevelDataSourceType
	{
		string DataSourceAssemblyName { get; set; }
		string DataSourceTypeName { get; set; }
	}
}
