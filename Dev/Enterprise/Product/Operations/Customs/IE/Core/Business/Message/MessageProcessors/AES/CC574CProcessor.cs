using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC574CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC574CProvider>
	{
		public CC574CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("360FE3FC-14E9-4F5F-889D-C687DBD488B1", "CC574C: RE-EXPORT DECLARATION AMENDMENT ACCEPTANCE");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC574CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC574CProvider provider) => AESEntryStatusList.Codes.ReleasedForExport;

		protected override Type MessageInterpreterType => typeof(CC574MessageInterpreter);
	}
}
