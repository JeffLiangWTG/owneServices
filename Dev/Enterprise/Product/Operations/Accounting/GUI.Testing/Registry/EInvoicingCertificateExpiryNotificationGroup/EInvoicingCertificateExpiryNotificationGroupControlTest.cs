using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.GUI.Testing
{
	[TestedType(typeof(EInvoicingCertificateExpiryNotificationGroupControl))]
	class EInvoicingCertificateExpiryNotificationGroupControlTest : RegistryBusinessObjectTemplateZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new EInvoicingCertificateExpiryNotificationGroup();
		}
	}
}
