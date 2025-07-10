using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM464Processor : AISMessageProcessor<AISInboundEDIMessage, IM464Provider>
	{
		public IM464Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("DD59A4BA-3500-4176-96B1-7BD80619D0D5", "IM464: Request Declaration Invalidation");

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM464Provider provider) => AISEntryStatusList.Codes.CancellationRequested;

		protected override Type MessageInterpreterType => typeof(IM464MessageInterpreter);
	}
}
