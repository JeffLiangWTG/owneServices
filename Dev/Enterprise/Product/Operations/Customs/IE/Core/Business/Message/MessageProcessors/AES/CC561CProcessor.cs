using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC561CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC561CProvider>
	{
		public CC561CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("2346C5C2-125B-4F92-90DB-A645831B95BF", "CC561C: EXIT CONTROL DECISION NOTIFICATION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC561CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC561CProvider provider) => AESEntryStatusList.Codes.ControlledForExit;

		protected override Type MessageInterpreterType => typeof(CC561MessageInterpreter);
	}
}
