using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.DataProviders
{
	class EmptyDataProvider : IDataProvider
	{
		IDataRowSource IDataProvider.GetDataRowSource(string tableIdentifier)
		{
			return new EmptyDataSource();
		}

		IDataRowSource IDataProvider.GetDataRowSource(string tableIdentifier, bool isForDataSection)
		{
			return new EmptyDataSource();
		}

		IDataRowSource IDataProvider.GetDataRowSource(string tableIdentifier, bool isForDataSection, int maximumNumberOfRows)
		{
			return new EmptyDataSource();
		}

		object IDataProvider.GetColumnValue(IDataRowSource dataRowSource, int rowIndex, string columnName, bool onlyForCurrentSource, Area area)
		{
			return "";
		}
	}
}
