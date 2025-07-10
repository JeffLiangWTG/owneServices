using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DRWBCKRMessageProcessor : CMRMessageResponseProcessor
	{
		public DRWBCKRMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.DRWBCK, "Drawback Response (DRWBCKR)")
		{
		}

		protected override bool ShouldSendAcknowledgementReport
		{
			get { return statusType == CMRDocumentStatus.Lodged.Code || statusType == CMRDocumentStatus.Withdrawn.Code; }
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (result)
			{
				declaration = incomingMessage.EM_LinkedObject as JobDeclaration;
				if (declaration == null)
				{
					Logger.LogWarning("The incoming DRWBCKR message is not responding to a declaration.  Can't continue.");
					result = false;
				}
				else
				{
					result = true;
					SetDrawbackClaimID();
					SetDeclarationStatus();
				}
			}
			return result;
		}

		void SetDrawbackClaimID()
		{
			CMRCUSRESMessage message = incomingMessage;
			if (message != null)
			{
				ZString drawbackClaimID = message.DrawbackClaimID;
				if (!drawbackClaimID.IsEmpty)
				{
					declaration.DeclarationNumber = drawbackClaimID;
				}
			}
		}

		void SetDeclarationStatus()
		{
			if (!statusType.IsEmpty)
			{
				DeclarationStatusProcessor declarationStatusProcessor = new DeclarationStatusProcessor(declaration);
				declarationStatusProcessor.SetDeclarationStatus(statusType);
			}
		}
	}
}
