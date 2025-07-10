using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ElectronicProcessingChargeConfigurationRegistryItem : StronglyTypedRegistryItem<ElectronicProcessingChargeConfigurationCollection>
	{
		public ElectronicProcessingChargeConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, ElectronicProcessingChargeConfigurationCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new ElectronicProcessingChargeConfigurationRegistryDataType(), storage, option, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ElectronicProcessingChargeConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class ElectronicProcessingChargeConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ElectronicProcessingChargeConfigurationCollection>
	{
	}
}
