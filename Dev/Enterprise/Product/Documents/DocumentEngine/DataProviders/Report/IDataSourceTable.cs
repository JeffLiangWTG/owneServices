namespace Enterprise.DocumentEngine.DataProviders
{
	using System.Data;

	interface IDataSourceTable
	{
		DataTable Table { get; }
	}
}