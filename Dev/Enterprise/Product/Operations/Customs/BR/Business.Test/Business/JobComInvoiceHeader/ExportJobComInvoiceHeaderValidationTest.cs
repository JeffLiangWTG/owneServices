using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ExportJobComInvoiceHeaderValidationTest : JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest() => typeof(ExportJobComInvoiceHeaderValidation);

		public void TestCheckJZ_OH_Buyer()
		{
			var buyerOrg = Factory.New<OrgHeader>();
			buyerOrg.OH_FullName = "Buyer Org";
			buyerOrg.MainAddress.OA_Address1 = "356 Main Address Buyer Org";
			buyerOrg.OH_Code = "XXX";

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew() as CusEntryInstruction;

			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew() as CusEntryInstruction;

			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = entryInstruction2.PK;

			invoiceHeader.Validation.ValidateJZ_OH_Buyer();
			entryInstruction.CEI_LegalDocument = ZString.Empty;
			entryInstruction2.CEI_LegalDocument = ZString.Empty;
			AssertNoMessageError("Empty Buyer", invoiceHeader.JZ_OH_BuyerInfo, "You have not entered an Importer.");

			entryInstruction.CEI_LegalDocument = LegalDocumentList.Codes.NoInvoice;
			invoiceHeader.JZ_OH_Buyer = buyerOrg.PK;
			AssertNoMessageError("Empty Buyer", invoiceHeader.JZ_OH_BuyerInfo, "You have not entered an Importer.");

			invoiceHeader.JZ_OH_Buyer = ZGuid.Empty;
			AssertHasMessageError("Buyer not entered", invoiceHeader.JZ_OH_BuyerInfo, "You have not entered an Importer.");
		}
	}
}
