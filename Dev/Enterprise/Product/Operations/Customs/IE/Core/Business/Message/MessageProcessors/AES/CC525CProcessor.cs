using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC525CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC525CProvider>
	{
		public CC525CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("8511CC78-4846-492C-85EC-F856E63EB666", "CC525C: EXIT RELEASE NOTIFICATION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC525CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC525CProvider provider) => AESEntryStatusList.Codes.ReleasedForExit;

		protected override Type MessageInterpreterType => typeof(CC525MessageInterpreter);
	}
}
