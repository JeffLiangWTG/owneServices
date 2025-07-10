using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC004CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC004CProvider>
	{
		public CC004CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("2109ADAF-D6BD-4CC8-A0C8-9EB060C32572", "CC004C: AMENDMENT ACCEPTANCE");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC004CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC004CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;

		protected override void UpdateGuaranteeTransactionsIfNeeded(NCTSInboundEDIMessage message, CC004CProvider provider)
		{
			if (message.GetOutboundMessage() is NCTSOutboundEDIMessage outgoingMessage
				&& message.EM_LinkedObject is NctsDepartureMovementHeader movementHeader)
			{
				if (movementHeader.BM_CustomsStatus.EqualsIgnoringCase(NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested))
				{
					Customs.Business.PermitHelper.UpdatePendingTransactions(message, outgoingMessage, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.Ireland, false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(CC004CMessageInterpreter);
	}
}
