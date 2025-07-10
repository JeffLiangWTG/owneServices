using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocumentSigningServiceCredentialsWithProviderRegistryItem : StronglyTypedRegistryItem<DocumentSigningServiceCredentialsWithProviderConfiguration>
	{
		public DocumentSigningServiceCredentialsWithProviderRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DocumentSigningServiceCredentialsWithProviderConfiguration defaultConfiguration)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocumentSigningServiceCredentialsWithProviderRegistryDataType(), storage, defaultConfiguration))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.Registry.GUI.DocumentSigningServiceCredentialsConfigurationRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	public class DocumentSigningServiceCredentialsWithProviderRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocumentSigningServiceCredentialsWithProviderConfiguration>
	{
	}
}
