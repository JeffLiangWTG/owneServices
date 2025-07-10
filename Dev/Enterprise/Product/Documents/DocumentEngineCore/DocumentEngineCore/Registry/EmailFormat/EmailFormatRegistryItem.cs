using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public class EmailFormatRegistryItem : StronglyTypedRegistryItem<EmailFormat>
	{
		public EmailFormatRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new EmailFormatRegistryDataType(), storage))
		{
		}

		public EmailFormatRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new EmailFormatRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.EmailFormatRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	class EmailFormatRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EmailFormat>
	{
		public EmailFormatRegistryDataType()
		{
		}
	}
}
