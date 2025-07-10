using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC009CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC009CProvider>
	{
		public CC009CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("E0172258-6424-4C86-AFA5-A56E3E45F930", "CC009C: INVALIDATION DECISION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC009CProvider provider) =>
			provider.IsInvalidated ? LogicalStatusList.Codes.Accepted : LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC009CProvider provider) =>
			provider.IsInvalidated ? NCTS5DepartureCustomsStatusList.Codes.Cancelled : null;

		protected override void UpdateGuaranteeTransactionsIfNeeded(NCTSInboundEDIMessage message, CC009CProvider provider)
		{
			if (provider.IsInvalidated && message.GetOutboundMessage() is NCTSOutboundEDIMessage outgoingMessage)
			{
				PermitHelper.UpdatePendingTransactions(message, outgoingMessage, NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.Ireland, false, PermitTransactionStatusList.Codes.Deleted);
				PermitHelper.RollbackPermitTransactions(message, outgoingMessage, null, NctsPermitHelper.GetPermitAppIdForMessage, ZString.Empty, Core.Constants.CountryCodes.Ireland, false, PermitTransactionStatusList.Codes.Confirmed);
			}
		}

		protected override Type MessageInterpreterType => typeof(CC009CMessageInterpreter);
	}
}
