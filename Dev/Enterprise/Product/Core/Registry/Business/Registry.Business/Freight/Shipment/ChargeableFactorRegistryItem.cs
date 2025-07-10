using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ChargeableFactorRegistryItem : StronglyTypedRegistryItem<ChargeableFactor>
	{
		public ChargeableFactorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ChargeableFactor defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ChargeableFactorRegistryDataType(), storage, defaultValue))
		{
		}

		public ChargeableFactorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ChargeableFactor defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ChargeableFactorRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ChargeableFactorRegistryItemEditor, Enterprise.Registry.GUI")]
	class ChargeableFactorRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ChargeableFactor>
	{
		public ChargeableFactorRegistryDataType()
		{
		}
	}
}
