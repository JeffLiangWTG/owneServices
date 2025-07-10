using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class BankAccountBasedOnCurrencyRegistryItem : StronglyTypedRegistryItem<BankAccountBasedOnCurrencyCollection>
	{
		public BankAccountBasedOnCurrencyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new BankAccountBasedOnCurrencyDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.BankAccountBasedOnCurrencyRegistryItemEditor, Enterprise.Registry.GUI")]
	class BankAccountBasedOnCurrencyDataType : NonPersistentBusinessObjectRegistryDataType<BankAccountBasedOnCurrencyCollection>
	{
		public BankAccountBasedOnCurrencyDataType()
		{
		}
	}
}
