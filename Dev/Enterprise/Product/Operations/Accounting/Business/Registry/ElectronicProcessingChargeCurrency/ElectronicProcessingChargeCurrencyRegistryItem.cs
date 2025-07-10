using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ElectronicProcessingChargeCurrencyRegistryItem : StronglyTypedRegistryItem<ElectronicProcessingChargeCurrencyCollection>
	{
		public ElectronicProcessingChargeCurrencyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option, ElectronicProcessingChargeCurrencyCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new ElectronicProcessingChargeCurrencyRegistryDataType(), storage, option, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ElectronicProcessingChargeCurrencyRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class ElectronicProcessingChargeCurrencyRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ElectronicProcessingChargeCurrencyCollection>
	{
	}
}
