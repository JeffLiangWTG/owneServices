using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ElectronicLogisticInvoiceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var electronicLogisticInvoice = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ElectronicLogisticInvoiceCollection.AddNew();
			var targetInfo = electronicLogisticInvoice.CSI_ReferenceNumberInfo;
			var warningMessage1 = "NF-E Key allows numeric characters.";
			var warningMessage2 = "NF-E Key must consist 44 numeric characters.";
			electronicLogisticInvoice.CSI_ReferenceNumber = "EPJ110100053F";
			AssertHasWarning(targetInfo, warningMessage1);
			electronicLogisticInvoice.CSI_ReferenceNumber = "11111111111111111111111111111111111111111111";
			AssertNoWarning(targetInfo, warningMessage2);
			electronicLogisticInvoice.CSI_ReferenceNumber = "12345678901";
			AssertHasWarning(targetInfo, warningMessage2);
		}

		public void TestCheckCSI_LineNo()
		{
			var electronicLogisticInvoice = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ElectronicLogisticInvoiceCollection.AddNew();
			var targetInfo = electronicLogisticInvoice.CSI_LineNoInfo;
			electronicLogisticInvoice.CSI_ReferenceNumber = "11111111111111111111111111111111111111111111";
			electronicLogisticInvoice.CSI_LineNo = 0;
			AssertHasWarning(targetInfo, "NF-E Item Number cannot enter value ZERO.");
		}
	}
}
