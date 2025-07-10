using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CustomDefaultBranchConfigurationRegistryItem : StronglyTypedRegistryItem<CustomDefaultBranchConfiguration>
	{
		public CustomDefaultBranchConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new CustomDefaultBranchRegistryDataType(), storage, option))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CustomDefaultBranchRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class CustomDefaultBranchRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CustomDefaultBranchConfiguration>
	{
		public CustomDefaultBranchRegistryDataType()
		{
		}
	}
}