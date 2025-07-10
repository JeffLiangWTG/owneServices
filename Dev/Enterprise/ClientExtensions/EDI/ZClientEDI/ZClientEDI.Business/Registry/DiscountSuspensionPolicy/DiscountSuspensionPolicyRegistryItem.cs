using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class DiscountSuspensionPolicyRegistryItem : StronglyTypedRegistryItem<DiscountSuspensionPolicyCollection>
	{
		public DiscountSuspensionPolicyRegistryItem(string itemName, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlags, DiscountSuspensionPolicyCollection defaultValue)
			: base(new RegistryItemImpl(itemName, category, caption, hint, new DiscountSuspensionPolicyDataType(), storageFlags, RegistryOptions.Default, defaultValue))
		{
		}
	}
}

