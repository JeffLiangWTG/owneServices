using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC051CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC051CProvider>
	{
		public CC051CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("CF1206E1-946D-42AE-8E99-8199282DFEB6", "CC051C: NO RELEASE FOR TRANSIT");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC051CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC051CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.NotReleasedForTransit;

		protected override Type MessageInterpreterType => typeof(CC051CMessageInterpreter);
	}
}
