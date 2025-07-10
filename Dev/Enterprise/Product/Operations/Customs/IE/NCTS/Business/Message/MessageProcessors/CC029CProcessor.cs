using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC029CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC029CProvider>
	{
		public CC029CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B6449A58-78C9-424B-8BFC-064F898B4E7E", "CC029C: RELEASED FOR TRANSIT");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC029CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC029CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.ReleasedForTransit;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC029CProvider provider)
		{
			if (messageAttachee is NctsDepartureMovementHeader movementHeader && movementHeader.Header?.MovementReferenceEntryNumber is CusEntryNumber entryNumber)
			{
				entryNumber.CE_IssueDate = provider.ReleaseDate;
			}
		}

		protected override void UpdateGuaranteeTransactionsIfNeeded(NCTSInboundEDIMessage message, CC029CProvider provider)
		{
			if (message.GetOutboundMessage() is NCTSOutboundEDIMessage outgoingMessage)
			{
				Customs.Business.PermitHelper.UpdatePendingTransactions(message, outgoingMessage, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.Ireland, false, Customs.Business.PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		protected override Type MessageInterpreterType => typeof(CC029CMessageInterpreter);
	}
}
