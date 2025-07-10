using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC056CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC056CProvider>
	{
		public CC056CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F9ECCEFF-231E-4007-BB7E-442751124E56", "CC056C: REJECTION FROM OFFICE OF DEPARTURE");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC056CProvider provider) => LogicalStatusList.Codes.Invalid;

		protected override void UpdateGuaranteeTransactionsIfNeeded(NCTSInboundEDIMessage message, CC056CProvider provider)
		{
			if (provider.BusinessRejectionType == NCTSOutgoingDepartureMessageTypeList.Codes.DeclarationData
				&& message.GetOutboundMessage() is NCTSOutboundEDIMessage outgoingMessage)
			{
				Customs.Business.PermitHelper.UpdatePendingTransactions(message, outgoingMessage, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.Ireland, false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
			}
		}

		protected override Type MessageInterpreterType => typeof(CC056CMessageInterpreter);
	}
}
