using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocumentSigningServicePartnerCredentialsRegistryItem : StronglyTypedRegistryItem<DocumentSigningServicePartnerCredentials>
	{
		public DocumentSigningServicePartnerCredentialsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DocumentSigningServicePartnerCredentials defaultConfiguration)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocumentSigningServicePartnerCredentialsRegistryDataType(), storage, options, defaultConfiguration))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.Registry.GUI.DocumentSigningServicePartnerCredentialsRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	public class DocumentSigningServicePartnerCredentialsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocumentSigningServicePartnerCredentials>
	{
		public DocumentSigningServicePartnerCredentialsRegistryDataType()
		{
		}
	}
}
