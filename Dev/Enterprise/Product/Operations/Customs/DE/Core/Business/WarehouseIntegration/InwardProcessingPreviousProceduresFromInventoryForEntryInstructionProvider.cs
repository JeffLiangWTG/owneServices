using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.WarehouseIntegration
{
	sealed class InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider :
		PreviousProceduresFromInventoryForEntryInstructionProvider
	{
		public InwardProcessingPreviousProceduresFromInventoryForEntryInstructionProvider(CusEntryInstruction instruction) : base(instruction)
		{
		}

		protected override IReadOnlyCollection<JobComInvoiceLine> GetApplicableLines()
		{
			return Instruction.InvoiceLines.Cast<JobComInvoiceLine>().Where(x => x.IsOutOfInwardProcessing).ToList();
		}

		protected override void CreatePreviousProceduresCore()
		{
			foreach (var invoiceLine in ApplicableLines)
			{
				CreatePreviousProcedure(invoiceLine);
			}
		}

		void CreatePreviousProcedure(JobComInvoiceLine invoiceLine)
		{
			var previousDocument = GetPreviousDocumentToPopulate(PreviousProcedureList.Codes._ATAV);

			var previousEntryNumber = invoiceLine.JI_PreviousEntryNumber;
			previousDocument.CSI_ReferenceNumber = previousEntryNumber;
			previousDocument.CSI_LineNo = invoiceLine.JI_PreviousEntryLineNumber;
			previousDocument.CSI_Description = invoiceLine.JI_Tariff;
			previousDocument.Status = previousDocument.CSI_ReferenceNumber.IsValidAtlasReferenceForInwardProcessing();
		}
	}
}
