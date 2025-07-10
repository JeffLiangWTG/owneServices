using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class AllowBackPostingSubLedgerTransactionRegistryItem : BooleanRegistryItem
	{
		public AllowBackPostingSubLedgerTransactionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public AllowBackPostingSubLedgerTransactionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AllowBackPostingSubLedgerTransactionRegistryDataType(), null, storage, options, defaultValue, false))
		{
		}
	}
}