using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX862Processor : AESMessageProcessor<AESInboundEDIMessage, EX862Provider>
	{
		public EX862Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("BFA903EE-AA28-411D-84C8-E1BAD03DDD6C", "EX862: Declaration Amendment Request Cancellation");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX862Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX862Provider provider) => AESEntryStatusList.Codes.ControlledForExport;

		protected override Type MessageInterpreterType => typeof(EX862MessageInterpreter);
	}
}
