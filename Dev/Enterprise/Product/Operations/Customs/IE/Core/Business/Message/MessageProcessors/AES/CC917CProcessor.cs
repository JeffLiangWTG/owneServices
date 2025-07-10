using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC917CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC917CProvider>
	{
		public CC917CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("6F0DCC9E-97F6-4681-87A4-E99D3EF2A3AB", "CC917C: XML NACK");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC917CProvider provider) => LogicalStatusList.Codes.Error;

		protected override bool GetIsFailure(CC917CProvider provider) => true;

		protected override Type MessageInterpreterType => typeof(CC917MessageInterpreter);
	}
}
