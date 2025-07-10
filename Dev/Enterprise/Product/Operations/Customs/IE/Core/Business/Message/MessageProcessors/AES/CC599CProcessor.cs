using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC599CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC599CProvider>
	{
		public CC599CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("EB1C17AA-0EE0-4150-9455-58F468932F9F", "CC599C: EXPORT NOTIFICATION");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC599CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC599CProvider provider) =>
			provider.ExitResultCode == ExitResultsCodeList.Codes.B1 ? AESEntryStatusList.Codes.Refused : AESEntryStatusList.Codes.Exported;

		protected override Type MessageInterpreterType => typeof(CC599MessageInterpreter);
	}
}
