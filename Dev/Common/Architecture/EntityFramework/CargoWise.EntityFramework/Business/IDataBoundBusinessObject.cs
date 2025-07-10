namespace CargoWise.EntityFramework
{
	public interface IDataBoundBusinessObject
	{
		bool HasProperty(string propertyName);

		bool TryGetValue<TValueType>(string propertyName, out TValueType value);
	}
}
