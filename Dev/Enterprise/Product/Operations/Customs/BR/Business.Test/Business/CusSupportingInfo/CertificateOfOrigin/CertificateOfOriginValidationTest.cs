using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CertificateOfOriginValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var certificateOfOriginCusSupportingCollection = Factory.New<JobDeclaration>().Invoices
				.AddNew().JobComInvoiceLines.AddNew().CertificateOfOriginCollection.AddNew();
			var targetInfo = certificateOfOriginCusSupportingCollection.CSI_ReferenceNumberInfo;
			certificateOfOriginCusSupportingCollection.CSI_ReferenceNumber = "EPJ110100053F";
			AssertNoNotifications(targetInfo);
			certificateOfOriginCusSupportingCollection.CSI_ReferenceNumber = ZString.Empty;
			AssertHasNotifications(targetInfo);
		}
	}
}
