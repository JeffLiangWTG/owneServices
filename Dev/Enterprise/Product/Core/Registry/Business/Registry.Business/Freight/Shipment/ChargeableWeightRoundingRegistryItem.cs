using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ChargeableWeightRoundingRegistryItem : StronglyTypedRegistryItem<ChargeableWeightRoundingCollection>
	{
		public ChargeableWeightRoundingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ChargeableWeightRoundingCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ChargeableWeightRoundingRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ChargeableWeightRoundingRegistryItemEditor, Enterprise.Registry.GUI")]
	class ChargeableWeightRoundingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChargeableWeightRoundingCollection>
	{
		public ChargeableWeightRoundingRegistryDataType()
		{
		}
	}
}
