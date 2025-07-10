using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExitSummaryCusEntryInstructionValidationTest : CommonExportCusEntryInstructionValidationTest<ExitSummaryCusEntryInstructionValidation>
	{
		public void TestCheckTransportDocuments()
		{
			string rowMessageError = CommonResStrings.ProvideAtLeastOneTransportDocument;
			var additionalInfo = instruction.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("When has instruction level TransportDocuments", instruction, rowMessageError);

			additionalInfo.Delete();
			instruction.Validation.ValidateAll();
			AssertHasRowMessageError("When no TransportDocuments", instruction, rowMessageError);

			var invoice = jobDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			additionalInfo = invoice.AdditionalInfos.AddNew();
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			instruction.Validation.ValidateAll();
			AssertNoRowMessageError("When has invoice level TransportDocuments", instruction, rowMessageError);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.ExitSummary;

		protected override ExitSummaryCusEntryInstructionValidation GetValidation() => new ExitSummaryCusEntryInstructionValidation(instruction);
	}
}
