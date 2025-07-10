using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DecimalEffectiveDateRegistryItem : StronglyTypedRegistryItem<DecimalEffectiveDate>
	{
		public DecimalEffectiveDateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DecimalEffectiveDateRegistryDataType(), storage))
		{
		}

		public DecimalEffectiveDateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DecimalEffectiveDate defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DecimalEffectiveDateRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.DecimalEffectiveDateRegistryItemEditor, Enterprise.Registry.GUI")]
	public class DecimalEffectiveDateRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DecimalEffectiveDate>
	{
	}
}
