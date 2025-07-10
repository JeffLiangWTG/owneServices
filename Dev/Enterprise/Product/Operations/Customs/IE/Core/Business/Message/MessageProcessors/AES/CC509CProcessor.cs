using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC509CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC509CProvider>
	{
		public CC509CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("AD7BC23F-611C-44C2-88A8-A9A1EDD26F17", "CC509C: EXPORT INVALIDATION DECISION");

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC509CProvider provider) => AESEntryStatusList.Codes.Cancelled;

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC509CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(CC509MessageInterpreter);
	}
}
