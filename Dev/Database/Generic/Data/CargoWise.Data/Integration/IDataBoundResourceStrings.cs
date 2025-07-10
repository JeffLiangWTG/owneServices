namespace CargoWise.Integration
{
	public interface IDataBoundResourceStrings
	{
		string GetStringForTable(string tableName);
		string GetStringForProperty(string tableName, string propertyName);
	}
}