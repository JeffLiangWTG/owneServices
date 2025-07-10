using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM862Processor : AISMessageProcessor<AISInboundEDIMessage, IM862Provider>
	{
		public IM862Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("D797B44B-DB87-4A53-A3EA-B55B2BDD72D2", "IM862: Declaration Amendment Request Cancellation");

		public override string GetLogicalStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM862Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISInboundEDIMessage message, IMessageAttachee messageAttachee, IM862Provider provider) => AISEntryStatusList.Codes.Control;

		protected override Type MessageInterpreterType => typeof(IM862MessageInterpreter);
	}
}

