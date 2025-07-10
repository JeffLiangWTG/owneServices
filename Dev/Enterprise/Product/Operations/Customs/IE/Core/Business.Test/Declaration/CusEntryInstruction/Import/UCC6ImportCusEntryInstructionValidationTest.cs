using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(UCC6ImportCusEntryInstructionValidation))]
	sealed class UCC6ImportCusEntryInstructionValidationTest : CommonImportCusEntryInstructionValidationTest<UCC6ImportCusEntryInstructionValidation>
	{
		public override void TestValidateBR2037()
		{
			var message = "[BR2037] At least one of the following supporting documents is required under the Entry Instructions > Supporting Documents tab: 'D005', 'D008', 'N325', 'N380', 'N864', 'N935', provided that at least one invoice does not contain the additional procedure 'C08'.";
			var supportingDocumentCodes = new[]
{
				Constants.SupportingDocumentCodes._D005,
				Constants.SupportingDocumentCodes._D008,
				Constants.SupportingDocumentCodes._N325,
				Constants.SupportingDocumentCodes._N380,
				Constants.SupportingDocumentCodes._N864,
				Constants.SupportingDocumentCodes._N935,
			};
			TestValidateBR2037(supportingDocumentCodes, message);
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
			AssertHasMessageError("H1, inv header without Exporter, no valid add doc", instruction.CEI_StyleInfo, messageError);

			invoiceHeader.JZ_OA_ExporterAddress = addressPK;
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError("H1, inv header with Exporter, no valid add doc", instruction.CEI_StyleInfo, messageError);

			invoiceHeader.JZ_OA_ExporterAddress = ZGuid.Empty;
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H2;
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError("H2, inv header without Exporter, no valid add doc", instruction.CEI_StyleInfo, messageError);

			instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
			additionalDocument.CSI_SubType = "INF";
			additionalDocument.CSI_Code = "00200";
			instruction.Validation.ValidateCEI_Style();
			AssertNoMessageError("H1, inv header without Exporter, has valid add doc", instruction.CEI_StyleInfo, messageError);
		});

		protected override UCC6ImportCusEntryInstructionValidation GetValidation() => new UCC6ImportCusEntryInstructionValidation(instruction);

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = MessageType;
			jobDeclaration.JE_ApplicationCode = "V2";
			instruction = jobDeclaration.CustomsEntryInstructions[0];
			validation = GetValidation();
		}
	}
}
