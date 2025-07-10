using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class BillingDbUsageCodesRegistryItem : StronglyTypedRegistryItem<BillingDbUsageCodesCollection>
	{
		public BillingDbUsageCodesRegistryItem(string itemName, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(
				itemName,
				category,
				caption,
				hint,
				new BillingDbUsageCodesDataType(),
				RegistryStorageFlags.System,
				RegistryOptions.Default))
		{
		}
	}
}

