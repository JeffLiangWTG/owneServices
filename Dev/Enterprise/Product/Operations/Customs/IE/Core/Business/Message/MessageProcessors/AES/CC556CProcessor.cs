using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC556CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC556CProvider>
	{
		public CC556CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("F56F77C8-2044-4D08-8E08-36DB582B4809", "CC556C: REJECTION FROM OFFICE OF EXPORT");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC556CProvider provider) => LogicalStatusList.Codes.Invalid;

		protected override Type MessageInterpreterType => typeof(CC556MessageInterpreter);
	}
}
