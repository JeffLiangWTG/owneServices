using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocumentSigningServiceCredentialsRegistryItem : StronglyTypedRegistryItem<DocumentSigningServiceCredentialsConfiguration>
	{
		public DocumentSigningServiceCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocumentSigningServiceCredentialsConfigurationRegistryDataType(), storage))
		{
		}

		public DocumentSigningServiceCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DocumentSigningServiceCredentialsConfiguration defaultConfiguration,
			string accessKeyCaption, string keyIDCaption)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocumentSigningServiceCredentialsConfigurationRegistryDataType(accessKeyCaption, keyIDCaption), storage, options, defaultConfiguration))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.Registry.GUI.DocumentSigningServiceCredentialsConfigurationRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	public class DocumentSigningServiceCredentialsConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocumentSigningServiceCredentialsConfiguration>
	{
		public DocumentSigningServiceCredentialsConfigurationRegistryDataType()
		{
		}

		public DocumentSigningServiceCredentialsConfigurationRegistryDataType(string accessKeyCaption, string keyIDCaption)
		{
			AccessKeyCaption = accessKeyCaption;
			KeyIDCaption = keyIDCaption;
		}

		public readonly string AccessKeyCaption;
		public readonly string KeyIDCaption;
	}
}
