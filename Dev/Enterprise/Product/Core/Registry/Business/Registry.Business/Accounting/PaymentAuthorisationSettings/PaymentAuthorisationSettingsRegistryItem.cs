using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class PaymentAuthorisationSettingsRegistryItem : StronglyTypedRegistryItem<PaymentAuthorisationSettingsCollection>
	{
		public PaymentAuthorisationSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PaymentAuthorisationSettingsRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.PaymentAuthorisationSettingsRegistryItemEditor, Enterprise.Registry.GUI")]
	class PaymentAuthorisationSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PaymentAuthorisationSettingsCollection>
	{
		public PaymentAuthorisationSettingsRegistryDataType()
		{
		}
	}
}
