
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	public class DocumentImageTypeRegistryItem : StronglyTypedRegistryItem<DocumentImageTypeCollection>
	{
		public DocumentImageTypeRegistryItem(string name, string category, string caption, string hint)
			: this(name, category, caption, hint, new DocumentImageTypeCollection())
		{
		}

		public DocumentImageTypeRegistryItem(string name, string category, string caption, string hint, DocumentImageTypeCollection defaultValue)
			: base(new RegistryItemImpl(name, (NoResString)category, (NoResString)caption, (NoResString)hint, new DocumentImageTypeRegistryDataType(), RegistryStorageFlags.System, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Client.UPE.Registry.GUI.DocumentImageTypeRegistryItemEditor, ZClientUPE")]
	class DocumentImageTypeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DocumentImageTypeCollection>
	{
		public DocumentImageTypeRegistryDataType()
		{
		}
	}
}
