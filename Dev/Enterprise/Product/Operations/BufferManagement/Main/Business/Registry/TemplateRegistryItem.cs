
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class TemplateRegistryItem : StronglyTypedRegistryItem<TemplateCriteria>
	{
		public TemplateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new TemplateCriteria())
		{
		}

		public TemplateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, TemplateCriteria defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TemplateRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.BufferManagement.GUI.TemplateRegistryItemEditor, Enterprise.BufferManagement.GUI")]
	public class TemplateRegistryDataType : NonPersistentBusinessObjectRegistryDataType<TemplateCriteria>
	{
	}
}
