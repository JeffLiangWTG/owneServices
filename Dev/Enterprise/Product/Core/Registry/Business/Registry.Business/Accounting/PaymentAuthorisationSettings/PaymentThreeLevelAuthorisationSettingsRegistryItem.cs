using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class PaymentThreeLevelAuthorisationSettingsRegistryItem : StronglyTypedRegistryItem<PaymentThreeLevelAuthorisationSettingsCollection>
	{
		public PaymentThreeLevelAuthorisationSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PaymentThreeLevelAuthorisationSettingsRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.PaymentAuthorisationSettingsRegistryItemEditor, Enterprise.Registry.GUI")]
	class PaymentThreeLevelAuthorisationSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PaymentThreeLevelAuthorisationSettingsCollection>
	{
		public PaymentThreeLevelAuthorisationSettingsRegistryDataType()
		{
		}
	}
}
