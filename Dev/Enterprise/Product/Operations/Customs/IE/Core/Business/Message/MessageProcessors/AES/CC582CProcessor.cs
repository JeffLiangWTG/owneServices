using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC582CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC582CProvider>
	{
		public CC582CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("1438E300-3F39-49AF-8368-61891EBABBF5", "CC582C: REQUEST ON NON-EXITED EXPORT");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC582CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC582CProvider provider) => AESEntryStatusList.Codes.Requested;

		protected override Type MessageInterpreterType => typeof(CC582MessageInterpreter);
	}
}
