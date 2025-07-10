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
	public class CC045CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC045CProvider>
	{
		public CC045CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC045CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC045CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed;

		protected override void UpdateGuaranteeTransactionsIfNeeded(NCTSInboundEDIMessage message, CC045CProvider provider)
		{
			if (message.GetOutboundMessage() is NCTSOutboundEDIMessage outgoingMessage)
			{
				PermitHelper.UpdatePendingTransactions(message, outgoingMessage, NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.Ireland, false, PermitTransactionStatusList.Codes.Confirmed);
				PermitHelper.RollbackPermitTransactions(message, outgoingMessage, null, NctsPermitHelper.GetPermitAppIdForMessage, ZString.Empty, Core.Constants.CountryCodes.Ireland, false, PermitTransactionStatusList.Codes.Confirmed);
			}
		}
		protected override string MessageFriendlyNameCore => Res.GetString("F7C42D81-F446-47BF-919D-82973C063F66", "CC045C: WRITE-OFF NOTIFICATION");
		protected override Type MessageInterpreterType => typeof(CC045CMessageInterpreter);
	}
}
