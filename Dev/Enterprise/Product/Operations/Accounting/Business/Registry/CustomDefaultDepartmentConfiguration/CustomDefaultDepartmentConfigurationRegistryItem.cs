using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CustomDefaultDepartmentConfigurationRegistryItem : StronglyTypedRegistryItem<CustomDefaultDepartmentConfiguration>
	{
		public CustomDefaultDepartmentConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new RegistryItemImpl(name, category, caption, hint, new CustomDefaultDepartmentRegistryDataType(), storage, option))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CustomDefaultDepartmentRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class CustomDefaultDepartmentRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CustomDefaultDepartmentConfiguration>
	{
		public CustomDefaultDepartmentRegistryDataType()
		{
		}
	}
}