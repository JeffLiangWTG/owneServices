using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS376Processor : AISMessageProcessor<AISInboundEDIMessage, TS376Provider>
	{
		public TS376Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("3C9E7A97-3A9B-4E7D-9157-50514803E802", "TS376: Goods Status Report Declaration (ND4) Rejection");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS376Provider provider) => LogicalStatusList.Codes.Invalid;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, TS376Provider provider) => AISEntryStatusList.Codes.Rejected;

		protected override Type MessageInterpreterType => typeof(TS376MessageInterpreter);
	}
}
