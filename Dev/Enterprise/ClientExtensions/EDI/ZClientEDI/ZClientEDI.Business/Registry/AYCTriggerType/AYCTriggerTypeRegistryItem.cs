using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class AYCTriggerTypeRegistryItem : StronglyTypedRegistryItem<AYCTriggerTypeSettings>
	{
		public AYCTriggerTypeRegistryItem(string itemName, MultilingualString category, MultilingualString caption, MultilingualString hint)
	: base(new RegistryItemImpl(
		itemName,
		category,
		caption,
		hint,
		new AYCTriggerTypeDataType(),
		RegistryStorageFlags.System,
		RegistryOptions.Default))
		{
		}
	}
}
