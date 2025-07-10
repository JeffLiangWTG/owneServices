using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class EInvoicingCertificateExpiryNotificationGroupRegistryItem : StronglyTypedRegistryItem<EInvoicingCertificateExpiryNotificationGroup>
	{
		public EInvoicingCertificateExpiryNotificationGroupRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, EInvoicingCertificateExpiryNotificationGroup defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new EInvoicingCertificateExpiryNotificationGroupRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.EInvoicingCertificateExpiryNotificationGroupRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class EInvoicingCertificateExpiryNotificationGroupRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EInvoicingCertificateExpiryNotificationGroup>
	{
	}
}