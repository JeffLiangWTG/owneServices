using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class DocumentsAllowedForSigningRegistryItem : StronglyTypedRegistryItem<DocumentsAllowedForSigning>
	{
		public DocumentsAllowedForSigningRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DocumentsAllowedForSigningRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.DocumentsAllowedForSigningRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	public class DocumentsAllowedForSigningRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocumentsAllowedForSigning>
	{
		public DocumentsAllowedForSigningRegistryDataType()
		{
		}
	}
}
