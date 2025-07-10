using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ComplementaryLogisticInvoiceValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var oTestSupplier = Factory.New<OrgHeader>();
			oTestSupplier.PrimaryRegistrationNumber.Number = "58500398000105";
			var oInvoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew();
			oInvoiceLine.InvoiceHeader.JZ_OA_SupplierAddress = oTestSupplier.MainAddress.PK;
			oInvoiceLine.JI_NFeNumber = "35170658500398000105550010001023631156448633";
			var cLogisticInvoice = oInvoiceLine.ComplementaryLogisticInvoiceCollection.AddNew();
			var targetInfo = cLogisticInvoice.CSI_ReferenceNumberInfo;
			cLogisticInvoice.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			cLogisticInvoice.CSI_ReferenceNumber = "AAAAA111111111111111111111111111111111111111";
			AssertHasMessageError(targetInfo, "NF-E Key allows numeric characters.");
			cLogisticInvoice.CSI_ReferenceNumber = "1";
			AssertHasMessageError(targetInfo, "NF-E Key must consist 44 numeric characters.");
			cLogisticInvoice.CSI_ReferenceNumber = "35170658500398000105550010001023631156448633";
			AssertHasMessageError(targetInfo, "Complementary NF-E Key must not be as the same as NF-E Key");
			cLogisticInvoice.CSI_ReferenceNumber = "11111158500398000102222222222222222222222222";
			AssertHasMessageError(targetInfo, "Complementary NF-E Key was not issued by the supplier");
		}

		public void TestCheckCSI_LineNo()
		{
			var complinvoice1 = Factory.New<JobDeclaration>().Invoices.AddNew().JobComInvoiceLines.AddNew().ComplementaryLogisticInvoiceCollection.AddNew();
			complinvoice1.CSI_ReferenceNumber = "11111111111111111111111111111111111111111111";
			complinvoice1.CSI_LineNo = 0;
			AssertHasMessageError(complinvoice1.CSI_LineNoInfo, "NF-E Item Number cannot enter value ZERO.");
		}
	}
}
