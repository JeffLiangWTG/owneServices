using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM493Processor : AISMessageProcessor<AISInboundEDIMessage, IM493Provider>
	{
		public IM493Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("F9FF4DA7-9517-48DB-9C0E-B7FD38282D4F", "IM493: Amendment Notification for Partial or Deferred Quota Allocation");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM493Provider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(IM493MessageInterpreter);
	}
}
