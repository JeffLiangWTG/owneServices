using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PAYINVMessageProcessor : CMRMessageResponseProcessor
	{
		public PAYINVMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.PAYINV, "Invalid Payment Record Advice - (PAYINV)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return false; }
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();

			if (result && entryHeader != null)
			{
				var responseMessage = (CMRPAYINVMessage)incomingMessage;
				if (entryHeader.CH_EntryStatus != CMRImportEntryAdvice.Processing.Code)
				{
					ZString entryNumber = responseMessage.EntryNumber;
					if (!entryNumber.IsEmpty)
					{
						entryHeader.EntryNumber = entryNumber;
					}
				}

				if (consolidatedDeclaration != null)
				{
					entryHeader.DeriveConsolidatedStatus();
					consolidatedDeclaration.SyncStatusAfterMessageProcessing(incomingMessage);
				}
			}

			return result;
		}
	}
}
