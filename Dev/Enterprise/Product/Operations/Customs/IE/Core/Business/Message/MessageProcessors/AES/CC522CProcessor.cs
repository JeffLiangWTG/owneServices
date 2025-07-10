using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC522CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC522CProvider>
	{
		public CC522CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("CFA30469-B9FE-4D2F-9716-B759DECFB76D", "CC522C: EXIT RELEASE REJECTION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC522CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC522CProvider provider) => AESEntryStatusList.Codes.ExitReleaseRejected;

		protected override Type MessageInterpreterType => typeof(CC522MessageInterpreter);
	}
}
