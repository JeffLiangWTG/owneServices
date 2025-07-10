using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC551CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC551CProvider>
	{
		public CC551CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("B0322C9C-E75E-4BED-B7C8-38EAE82654C7", "CC551C: EXPORT NO RELEASE");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC551CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC551CProvider provider) => AESEntryStatusList.Codes.Rejected;

		protected override Type MessageInterpreterType => typeof(CC551MessageInterpreter);
	}
}
