using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.DataProviders
{
	internal interface IDataProvider
	{
		IDataRowSource GetDataRowSource(string tableIdentifier);
		IDataRowSource GetDataRowSource(string tableIdentifier, bool isForDataSection);
		IDataRowSource GetDataRowSource(string tableIdentifier, bool isForDataSection, int maximumNumberOfRows);
		object GetColumnValue(IDataRowSource dataRowSource, int rowIndex, string columnName, bool onlyForCurrentSource = false, Area area = null);
	}
}
