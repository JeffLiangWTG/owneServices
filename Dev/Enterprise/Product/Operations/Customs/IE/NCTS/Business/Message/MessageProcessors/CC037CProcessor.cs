using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC037CProcessor : NCTSGuaranteeMessageProcessor<NCTSInboundEDIMessage, CC037CProvider>
	{
		public CC037CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F9ECCEFF-231E-4007-BB7E-442751CC037C", "CC037C: RESPONSE QUERY ON GUARANTEES");

		public override string GetLogicalStatus(NCTSInboundEDIMessage message, IMessageAttachee messageAttachee, CC037CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(CC037CMessageInterpreter);
	}
}
