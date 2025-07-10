using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC055CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC055CProvider>
	{
		public CC055CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F9ECCEFF-231E-4007-BB7E-442751124E55", "CC055C: GUARANTEE NOT VALID");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC055CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC055CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.GuaranteeInvalid;

		protected override void UpdateGuaranteeTransactionsIfNeeded(NCTSInboundEDIMessage message, CC055CProvider provider)
		{
			var invalidGuaranteeReasonCode = provider.GuaranteeReferences.FirstOrDefault()?.InvalidGuaranteeReasons.FirstOrDefault().InvalidGuaranteeReasonCode ?? ZString.Empty;
			switch (invalidGuaranteeReasonCode.ToUpperInvariant())
			{
				case NCTS5InvalidGuaranteeReason.Codes.G01:
				case NCTS5InvalidGuaranteeReason.Codes.G02:
				case NCTS5InvalidGuaranteeReason.Codes.G05:
				case NCTS5InvalidGuaranteeReason.Codes.G09:
				case NCTS5InvalidGuaranteeReason.Codes.G10:
					if (message.GetOutboundMessage() is NCTSOutboundEDIMessage outgoingMessage)
					{
						Customs.Business.PermitHelper.UpdatePendingTransactions(message, outgoingMessage, EU.NCTS.Business.NctsPermitHelper.GetPermitAppIdForMessage, Core.Constants.CountryCodes.Ireland, false, Customs.Business.PermitTransactionStatusList.Codes.Deleted);
					}
					break;
			}
		}

		protected override Type MessageInterpreterType => typeof(CC055CMessageInterpreter);
	}
}
