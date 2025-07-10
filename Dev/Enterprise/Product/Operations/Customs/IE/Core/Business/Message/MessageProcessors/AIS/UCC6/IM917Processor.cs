using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM917Processor : AISMessageProcessor<AISInboundEDIMessage, IM917Provider>
	{
		public IM917Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("9A975D8B-4D1C-422F-A9E0-E36FCA81AF51", "IM917: Syntax Error Notification");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM917Provider provider) => LogicalStatusList.Codes.Error;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM917Provider provider) => messageAttachee is Integration.Customs.IEH7.IAsycudaBill ? null : CustomsWareEntryStatusList.Codes.Error;

		protected override Type MessageInterpreterType => typeof(IM917MessageInterpreter);
	}
}
