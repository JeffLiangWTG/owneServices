using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM460Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM460Provider>
	{
		public IM460Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A6910234-DB52-43B6-AD56-A073E90FACE5", "IM460: Control Notice");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM460Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, Messaging.IMessageAttachee messageAttachee, IM460Provider provider) => AISEntryStatusList.Codes.Control;

		protected override Type MessageInterpreterType => typeof(IM460MessageInterpreter);
	}
}
