using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM462Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, IM462Provider>
	{
		public IM462Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("DB59218C-E6A8-402D-9183-130C7D65BA68", "IM462: Request Declaration Amendment");

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM462Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, IM462Provider provider) => AISEntryStatusList.Codes.AmendmentRequested;

		protected override Type MessageInterpreterType => typeof(IM462MessageInterpreter);
	}
}
