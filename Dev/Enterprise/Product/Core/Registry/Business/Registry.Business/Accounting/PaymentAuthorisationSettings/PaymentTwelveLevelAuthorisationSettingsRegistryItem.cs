using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class PaymentTwelveLevelAuthorisationSettingsRegistryItem : StronglyTypedRegistryItem<PaymentTwelveLevelAuthorisationSettingsCollection>
	{
		public PaymentTwelveLevelAuthorisationSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PaymentTwelveLevelAuthorisationSettingsRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.PaymentAuthorisationSettingsRegistryItemEditor, Enterprise.Registry.GUI")]
	class PaymentTwelveLevelAuthorisationSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PaymentTwelveLevelAuthorisationSettingsCollection>
	{
		public PaymentTwelveLevelAuthorisationSettingsRegistryDataType()
		{
		}
	}
}
