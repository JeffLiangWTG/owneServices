using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC140CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC140CProvider>
	{
		public CC140CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("6570FEEA-DE14-4118-AC33-86398F8E419B", "CC140C: REQUEST ON NON-ARRIVED MOVEMENT");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC140CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC140CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.UnderEnquiry;

		protected override Type MessageInterpreterType => typeof(CC140CMessageInterpreter);
	}
}
