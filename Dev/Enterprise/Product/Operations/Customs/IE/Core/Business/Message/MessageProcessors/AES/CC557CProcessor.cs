using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC557CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC557CProvider>
	{
		public CC557CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("87965EE8-9037-4685-A2E7-B6FC463320DC", "CC557C: REJECTION FROM OFFICE OF EXIT");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC557CProvider provider) => LogicalStatusList.Codes.Invalid;

		protected override Type MessageInterpreterType => typeof(CC557MessageInterpreter);
	}
}
