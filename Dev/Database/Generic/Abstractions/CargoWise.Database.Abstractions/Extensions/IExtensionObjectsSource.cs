namespace CargoWise.Database.Abstractions.Extensions
{
	public interface IExtensionObjectsSource
	{
		string DisplayName { get; }
		string ExtensionCode { get; }
		IExtensionObjects ExtensionObjects { get; }
	}
}


