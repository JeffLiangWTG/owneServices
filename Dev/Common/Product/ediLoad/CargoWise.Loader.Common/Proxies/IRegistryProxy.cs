namespace CargoWise.Loader.Common
{
	public interface IRegistryProxy
	{
		object GetValue(string keyName, string valueName, object defaultValue);
	}
}
