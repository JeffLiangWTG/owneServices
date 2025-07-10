using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(EInvoicingCertificateExpiryNotificationGroupRegistryItem))]
	class EInvoicingCertificateExpiryNotificationGroupRegistryItemTest : StronglyTypedRegistryItemTestCase<EInvoicingCertificateExpiryNotificationGroup>
	{
		protected override StronglyTypedRegistryItem<EInvoicingCertificateExpiryNotificationGroup, EInvoicingCertificateExpiryNotificationGroup> GetNewRegistryItem()
		{
			return new EInvoicingCertificateExpiryNotificationGroupRegistryItem("", null, null, null, RegistryStorageFlags.Company | RegistryStorageFlags.Branch, new EInvoicingCertificateExpiryNotificationGroup());
		}
	}
}
