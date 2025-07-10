using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM917Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM917Provider>
	{
		public IM917Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("6CDC6FE8-5E09-431A-A428-8D554CAD7E45", "IM917: Syntax Error Notification");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM917Provider provider) => LogicalStatusList.Codes.Error;

		protected override Type MessageInterpreterType => typeof(IM917MessageInterpreter);
	}
}
