using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC182CProcessor : NCTSDepartureMessageProcessor<NCTSInboundEDIMessage, CC182CProvider>
	{
		public CC182CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("C5C9406E-526C-41E9-ACC0-976E272CEE7C", "CC182C: FORWARDED INCIDENT NOTIFICATION");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC182CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC182CProvider provider) => NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered;

		protected override Type MessageInterpreterType => typeof(CC182CMessageInterpreter);
	}
}
