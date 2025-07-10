using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class IntercompanyEventConfigurationRegistryItem : StronglyTypedRegistryItem<IntercompanyEventConfiguration>
	{
		public IntercompanyEventConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, object defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IntercompanyEventConfigurationRegistryItemDataType(), storage, option, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Accounting.Registry.GUI.IntercompanyEventConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
		public class IntercompanyEventConfigurationRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<IntercompanyEventConfiguration>
		{
			public IntercompanyEventConfigurationRegistryItemDataType()
			{
			}
		}
	}
}
