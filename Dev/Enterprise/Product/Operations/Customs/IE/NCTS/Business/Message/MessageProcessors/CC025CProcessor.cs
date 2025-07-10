using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC025CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC025CProvider>
	{
		public CC025CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("DC6D305D-67DF-40A8-A218-C123C127858A", "CC025C: GOODS RELEASE NOTIFICATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC025CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC025CProvider provider)
		{
			switch (provider.ReleaseIndicator)
			{
				case "1":
					return NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease;
				case "2":
					return NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionPartialRelease;
				case "3":
					return NCTS5ArrivalCustomsStatusList.Codes.ClosedPartialRelease;
				case "4":
					return NCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease;
				default:
					return base.GetEntryStatus(message, messageAttachee, provider);
			}
		}

		protected override Type MessageInterpreterType => typeof(CC025CMessageInterpreter);
	}
}
