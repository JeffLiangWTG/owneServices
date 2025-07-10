using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	public class JobComInvoiceHeaderValidationTEST : EU.Business.Declaration.Testing.JobComInvoiceHeaderValidationTest
	{
		protected override Type GetTypeForTest()
		{
			return typeof(CDS.Declaration.CDSJobComInvoiceHeaderValidation);
		}

		protected override Customs.Business.BaseJobComInvoiceHeader GetInvoiceHeader()
		{
			var dec = Factory.New<JobDeclaration>();
			return dec.Invoices.AddNew();
		}

		public void TestCheckJZ_IncoTerm()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var ceiA = declaration.CustomsEntryInstructions.AddNew();
			var ceiB = declaration.CustomsEntryInstructions.AddNew();
			var line11 = invoice1.InvoiceLines.AddNew();
			var line12 = invoice1.InvoiceLines.AddNew();
			var line21 = invoice2.InvoiceLines.AddNew();
			var line22 = invoice2.InvoiceLines.AddNew();

			// Both invoices invovled in both CEIs 
			line11.JI_CEI = ceiA.PK;
			line12.JI_CEI = ceiB.PK;
			line21.JI_CEI = ceiA.PK;
			line22.JI_CEI = ceiB.PK;
			invoice1.JZ_IncoTerm = "AAA";
			invoice2.JZ_IncoTerm = "BBB";
			AssertHasMessageErrorContaining(invoice1.JZ_IncoTermInfo, "Mixed Inco");
			AssertHasMessageErrorContaining(invoice2.JZ_IncoTermInfo, "Mixed Inco");

			// Each invoice on only one CEI - different INCO is OK
			line11.JI_CEI = ceiA.PK;
			line12.JI_CEI = ceiA.PK;
			line21.JI_CEI = ceiB.PK;
			line22.JI_CEI = ceiB.PK;
			invoice1.JZ_IncoTerm = "CCC";
			invoice2.JZ_IncoTerm = "DDD";
			AssertNoMessageErrorContaining(invoice1.JZ_IncoTermInfo, "Mixed Inco");
			AssertNoMessageErrorContaining(invoice2.JZ_IncoTermInfo, "Mixed Inco");

			// All on one CEI, no mixed terms allowed
			line11.JI_CEI = ceiA.PK;
			line12.JI_CEI = ceiA.PK;
			line21.JI_CEI = ceiA.PK;
			line22.JI_CEI = ceiA.PK;
			invoice1.JZ_IncoTerm = "AAA";
			invoice2.JZ_IncoTerm = "BBB";
			AssertHasMessageErrorContaining(invoice1.JZ_IncoTermInfo, "Mixed Inco");
			AssertHasMessageErrorContaining(invoice2.JZ_IncoTermInfo, "Mixed Inco");
		}

		public override void TestCheckJZ_OH_Supplier()
		{
			base.TestCheckJZ_OH_Supplier();
			var dec = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DAN";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AGI";
			var inv = dec.Invoices.AddNew();

			dec.SupplierDocumentaryAddress.E2_CompanyName = "SLURP";
			inv.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrorContaining(inv.JZ_OH_SupplierInfo, needOne);

			dec.SupplierDocumentaryAddress.E2_CompanyName = ZString.Empty;
			inv.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(inv.JZ_OH_SupplierInfo, needOne);

			dec.JE_OH_Supplier = org.PK;
			inv.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrorContaining(inv.JZ_OH_SupplierInfo, needOne);
			inv.JZ_OH_Supplier = org2.PK;
			dec.JE_OH_Supplier = ZGuid.Empty;
			inv.JZ_OH_Supplier = ZGuid.Empty;
			inv.Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageErrorContaining(inv.JZ_OH_SupplierInfo, needOne);
			AssertNoMessageErrorContaining(inv.JZ_OH_SupplierInfo, dontUseTwo);

			inv.JZ_OH_Supplier = org2.PK;
			AssertHasMessageErrorContaining(inv.JZ_OH_SupplierInfo, "EORI");
			org2.CustomsCodes.AddNew("TRN", "123456789000");
			inv.Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageErrorContaining(inv.JZ_OH_SupplierInfo, "EORI");

			dec.JE_OH_Supplier = org.PK;
			inv.JZ_OH_Supplier = org2.PK;
			AssertHasMessageErrorContaining(inv.JZ_OH_SupplierInfo, dontUseTwo);
		}

		public void TestCheckJZ_OH_Buyer()
		{
			var dec = Factory.New<JobDeclaration>();
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "DAN";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "AGI";
			var inv = dec.Invoices.AddNew();

			dec.ImporterDocumentaryAddress.E2_CompanyName = "SLURP";
			inv.Validation.ValidateJZ_OH_Buyer();
			AssertNoMessageErrorContaining(inv.JZ_OH_BuyerInfo, needOne);

			dec.ImporterDocumentaryAddress.E2_CompanyName = ZString.Empty;
			inv.Validation.ValidateJZ_OH_Buyer();
			AssertHasMessageErrorContaining(inv.JZ_OH_BuyerInfo, needOne);

			dec.JE_OH_Importer = org.PK;
			inv.Validation.ValidateJZ_OH_Buyer();
			AssertNoMessageErrorContaining(inv.JZ_OH_BuyerInfo, needOne);
			inv.JZ_OH_Buyer = org2.PK;
			dec.JE_OH_Importer = ZGuid.Empty;
			inv.JZ_OH_Buyer = ZGuid.Empty;
			inv.Validation.ValidateJZ_OH_Buyer();
			AssertHasMessageErrorContaining(inv.JZ_OH_BuyerInfo, needOne);
			AssertNoMessageErrorContaining(inv.JZ_OH_BuyerInfo, dontUseTwo);

			inv.JZ_OH_Buyer = org2.PK;
			dec.JE_MessageType = "IMP";
			inv.Validation.ValidateJZ_OH_Buyer();
			AssertHasMessageErrorContaining(inv.JZ_OH_BuyerInfo, "EORI");
			org2.CustomsCodes.AddNew("TRN", "123456789000");
			inv.Validation.ValidateJZ_OH_Buyer();
			AssertNoMessageErrorContaining(inv.JZ_OH_BuyerInfo, "EORI");

			dec.JE_OH_Importer = org.PK;
			inv.JZ_OH_Buyer = org2.PK;
			AssertHasMessageErrorContaining(inv.JZ_OH_BuyerInfo, dontUseTwo);
		}

		public void TestCheckJZ_InvoiceNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportSADDeclarationTypeList.Codes.ExportClearanceRequest;
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "INV1";
			invHeader.PreviousDocuments.RemoveAndDeleteAll();
			invHeader.JZ_InvoiceNumber = "INV1";
			AssertHasWarningContaining(invHeader.JZ_InvoiceNumberInfo, invoiceHasNoPrevoiusDocuments);
			instruction.CEI_Style = ExportSADDeclarationTypeList.Codes.ExitSummaryDeclaration;
			invHeader.JZ_InvoiceNumber = "INV1";
			AssertNoWarningContaining(invHeader.JZ_InvoiceNumberInfo, invoiceHasNoPrevoiusDocuments);
		}

		protected override EU.Business.Declaration.JobDeclaration GetJobDeclarationForTest()
		{
			var declaration =  base.GetJobDeclarationForTest();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
		}

		readonly string needOne = "You need to select a party since you have not selected one at declaration header level";
		readonly string dontUseTwo = "The header-level and invoice-level parties differ. For a bulk entry, remove the declaration party.";
		readonly string invoiceHasNoPrevoiusDocuments = "This invoice has no previous documents and not all of its invoice lines have one. Without a previous document the entry may be rejected.";
	}
}
