using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM864;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM864Processor : AISMessageProcessor<AISInboundEDIMessage, IM864Provider>
	{
		public IM864Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("2AB87E78-2AD9-4DF8-9ABA-7C5D1FE59369", "IM864: Invalidation Request Cancellation");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM864Provider provider) => xmlObjectType.IsAssignableFrom(typeof(AIS_H7_Version1_0.Im864)) ? LogicalStatusList.Codes.Accepted : null;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM864Provider provider) => AISEntryStatusList.Codes.Control;

		protected override Type MessageInterpreterType => typeof(IM864MessageInterpreter);
	}
}
