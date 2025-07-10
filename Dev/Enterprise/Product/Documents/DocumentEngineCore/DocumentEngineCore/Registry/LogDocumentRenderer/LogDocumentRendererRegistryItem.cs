namespace Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer
{
	using Enterprise.Integration;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;

	public sealed class LogDocumentRendererRegistryItem : StronglyTypedRegistryItem<LogDocumentRendererRegistry>
	{
		public LogDocumentRendererRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, LogDocumentRendererRegistry defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new LogDocumentRendererRegistryDataType(), RegistryStorageFlags.All, RegistryOptions.IsOnlyForDevelopers, defaultValue))
		{
		}
	}
}
