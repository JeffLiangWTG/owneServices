namespace CargoWise.Data.Providers.Common
{
	public interface IWindowsRegistry
	{
		object GetValue(string keyName, string valueName, object defaultValue);
	}
}
