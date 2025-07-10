using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class SuspensionDrawbackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			oSuspensionDrawback.CSI_ReferenceNumber2 = ZString.Empty;
			oSuspensionDrawback.CSI_ReferenceNumber = "97442770000126";
			AssertNoErrors(oSuspensionDrawback.CSI_ReferenceNumberInfo);
			oSuspensionDrawback.CSI_ReferenceNumber2 = ZString.Empty;
			oSuspensionDrawback.CSI_ReferenceNumber = "97442770000127";
			AssertHasMessageErrorContaining(oSuspensionDrawback.CSI_ReferenceNumberInfo, "The entered CNPJ is not valid");
			oSuspensionDrawback.CSI_ReferenceNumber2 = "CA_Number";
			oSuspensionDrawback.CSI_ReferenceNumber = "97442770000127";
			AssertHasMessageErrorContaining(oSuspensionDrawback.CSI_ReferenceNumberInfo, "The entered CNPJ is not valid");
			oSuspensionDrawback.CSI_ReferenceNumber2 = "CA_Number";
			oSuspensionDrawback.CSI_ReferenceNumber = "97442770000126";
			AssertNoErrors(oSuspensionDrawback.CSI_ReferenceNumberInfo);
			oSuspensionDrawback.CSI_ReferenceNumber2 = "CA_Number";
			oSuspensionDrawback.CSI_ReferenceNumber = ZString.Empty;
			AssertHasMessageErrorContaining(oSuspensionDrawback.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_Beneficiary()
		{
			var oTestHeaderSupplier = Factory.New<OrgHeader>();
			oTestHeaderSupplier.FillWithValidTestData();
			oTestHeaderSupplier.PrimaryRegistrationNumber.Number = "97442770000126";
			oInvoiceHeader.JZ_OH_Supplier = oTestHeaderSupplier.PK;
			oInvoiceLine = oInvoiceHeader.InvoiceLines.AddNew();
			oSuspensionDrawback = oInvoiceLine.SuspensionDrawbackCollection.AddNew();
			oSuspensionDrawback.CSI_ReferenceNumber = "97442770000126";
			AssertEquals(true, oSuspensionDrawback.CSI_IsSupplierBeneficiary);
			oTestHeaderSupplier.PrimaryRegistrationNumber.Number = "00405884000164";
			AssertEquals(false, oSuspensionDrawback.CSI_IsSupplierBeneficiary);
			oTestHeaderSupplier.PrimaryRegistrationNumber.Number = ZString.Empty;
			AssertEquals(false, oSuspensionDrawback.CSI_IsSupplierBeneficiary);
			oSuspensionDrawback.CSI_ReferenceNumber = ZString.Empty;
			oTestHeaderSupplier.PrimaryRegistrationNumber.Number = "00405884000164";
			AssertEquals(false, oSuspensionDrawback.CSI_IsSupplierBeneficiary);
		}

		JobDeclaration oJobDeclaration;
		JobComInvoiceLine oInvoiceLine;
		JobComInvoiceHeader oInvoiceHeader;
		SuspensionDrawback oSuspensionDrawback;

		protected override void SetUp()
		{
			base.SetUp();
			oJobDeclaration = Factory.New<JobDeclaration>();
			oInvoiceHeader = oJobDeclaration.Invoices.AddNew();
			oInvoiceLine = oJobDeclaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			oSuspensionDrawback = oInvoiceLine.SuspensionDrawbackCollection.AddNew();
		}
	}
}
