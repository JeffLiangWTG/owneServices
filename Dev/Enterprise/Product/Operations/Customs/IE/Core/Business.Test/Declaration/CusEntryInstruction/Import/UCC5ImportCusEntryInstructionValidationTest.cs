using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(UCC5ImportCusEntryInstructionValidation))]
	sealed class UCC5ImportCusEntryInstructionValidationTest : CommonImportCusEntryInstructionValidationTest<UCC5ImportCusEntryInstructionValidation>
	{
		public void TestValidateSupportingDocuments_C512()
		{
			var expectedMessage = "[BR1026] If Declaration Type is 'I1' and Sub Style is either 'C' or 'F' supporting documents must be submitted 'C512' at either Entry Instruction, Invoice Header or Invoice Line level.";
			instruction.CEI_Style = "I1";
			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			validation.ValidateAll();
			AssertNoRowMessageError("CEI_Style I1, no C512", instruction, expectedMessage);

			instruction.CEI_SubStyle = "C";
			validation.ValidateAll();
			AssertHasRowMessageError("CEI_Style I1, CEI_SubStyle C, no C512", instruction, expectedMessage);

			instruction.CEI_SubStyle = "F";
			validation.ValidateAll();
			AssertHasRowMessageError("CEI_Style I1, CEI_SubStyle F, no C512", instruction, expectedMessage);

			instruction.SupportingDocuments.AddNew().CSI_Code = Constants.SupportingDocumentCodes._C512;
			validation.ValidateAll();
			AssertNoRowMessageError("CEI_Style I1, CEI_SubStyle F, instruction has C512", instruction, expectedMessage);

			var supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._C512;
			validation.ValidateAll();
			AssertNoRowMessageError("CEI_Style I1, CEI_SubStyle F, invoiceHeader has C512", instruction, expectedMessage);

			supportingDocument.Delete();
			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = Constants.SupportingDocumentCodes._C512;
			validation.ValidateAll();
			AssertNoRowMessageError("CEI_Style I1, CEI_SubStyle F, invoiceLine has C512", instruction, expectedMessage);
		}

		public override void TestValidateBR2037()
		{
			var message = "[BR2037] At least one of the following supporting documents is required under the Entry Instructions > Supporting Documents tab: 'D005', 'D008', 'N325', 'N380', 'N864', 'N935', '1N09', '1N21', '1N22', '1N99' provided that at least one invoice does not contain the additional procedure 'C08'.";
			var supportingDocumentCodes = new[]
			{
				Constants.SupportingDocumentCodes._D005,
				Constants.SupportingDocumentCodes._D008,
				Constants.SupportingDocumentCodes._N325,
				Constants.SupportingDocumentCodes._N380,
				Constants.SupportingDocumentCodes._N864,
				Constants.SupportingDocumentCodes._N935,
				Constants.SupportingDocumentCodes._1N09,
				Constants.SupportingDocumentCodes._1N21,
				Constants.SupportingDocumentCodes._1N22,
				Constants.SupportingDocumentCodes._1N99,
			};
			TestValidateBR2037(supportingDocumentCodes, message);
		}

		public void TestValidateBR20319()
		{
			var message = "[BR20319] You have not entered the mandatory 1D24 Scheduled Time of Arrival under Supporting Document.";

			validation.ValidateAll();
			AssertHasRowMessageError("BR20319, requires a 1D24 REF.", instruction, message);

			var additionalProcedureCode = instruction.SupportingDocuments.AddNew();
			additionalProcedureCode.CSI_Code = Constants.SupportingDocumentCodes._1D24;
			validation.ValidateAll();
			AssertNoRowMessageError("BR20319, requires a 1D24 REF.", instruction, message);
		}

		public override void TestCheckCEI_Style_BR3005() => CombineAssertions(() =>
		{
			var messageError = "[BR3005] Please enter an Additional Information code where Kind is 'INF' and Full Type is '00200' to the Entry Instructions > Additional Documents grid.";

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			var additionalDocument = instruction.AdditionalInfos.AddNew();
			var exporter = Factory.NewWithValidTestData<OrgHeader>();
			var addressPK = exporter.MainAddress.PK;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError("H1, single inv line, no valid add doc", instruction.CEI_StyleInfo, messageError);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			AssertNoMessageError("H2, single inv line,", instruction.CEI_StyleInfo, messageError);

			additionalDocument.CSI_SubType = "INF";
			additionalDocument.CSI_Code = "00200";
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError("H1, single inv line, no invoice exporter address, has valid add doc", instruction.CEI_StyleInfo, messageError);

			additionalDocument.CSI_Code = "";
			invoiceLine.JI_OA_ExporterAddress = addressPK;
			var exporter2 = Factory.NewWithValidTestData<OrgHeader>();
			var addressPK2 = exporter2.MainAddress.PK;
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_CEI = instruction.PK;
			invoiceLine2.JI_OA_ExporterAddress = addressPK2;

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			instruction.Validation.ValidateCEI_Style();
			AssertHasMessageError("H1, multiple lines with different Exporters, no valid add doc", instruction.CEI_StyleInfo, messageError);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError("H2, multiple lines with different Exporters, no valid add doc", instruction.CEI_StyleInfo, messageError);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			additionalDocument.CSI_Code = "00200";
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError("H1, multiple lines with different Exporters, has valid add doc", instruction.CEI_StyleInfo, messageError);

			invoiceLine.JI_OA_ExporterAddress = addressPK2;
			additionalDocument.CSI_Code = "";
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			AssertNoMessageError("H1, multiple lines with same Exporter, no valid add doc", instruction.CEI_StyleInfo, messageError);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			AssertNoMessageError("H2, multiple lines with same Exporter, no valid add doc", instruction.CEI_StyleInfo, messageError);
		});

		protected override UCC5ImportCusEntryInstructionValidation GetValidation() => new UCC5ImportCusEntryInstructionValidation(instruction);

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			jobDeclaration.JE_ApplicationCode = "V1";
			instruction = jobDeclaration.CustomsEntryInstructions[0];
			validation = GetValidation();
		}
	}
}
