using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC560CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC560CProvider>
	{
		public CC560CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("1DE53CFC-BF03-4841-89CD-1654319FB0A4", "CC560C: EXPORT CONTROL DECISION NOTIFICATION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC560CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC560CProvider provider) => provider.ControlNotificationType == "2" ? AESEntryStatusList.Codes.PendingControl : AESEntryStatusList.Codes.ControlledForExport;

		protected override Type MessageInterpreterType => typeof(CC560MessageInterpreter);
	}
}
