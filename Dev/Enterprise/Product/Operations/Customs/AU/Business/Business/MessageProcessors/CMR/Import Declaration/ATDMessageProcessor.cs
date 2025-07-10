using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ATDMessageProcessor : BaseImportDeclarationMessageProcessor
	{
		public ATDMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.ATD, "Authority to Deal Message (ATD)")
		{
		}

		protected override void SetEntryHeaderStatus()
		{
			if (cUSRES != null && entryHeader != null)
			{
				entryHeader.CH_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
				entryHeader.Declaration.JE_EntryStatus = entryHeader.Declaration.SummaryEntryStatusCalculator.SummaryEntryStatus;

				SetIsSubjectToRedLineProcessing();
			}
		}
	}
}
