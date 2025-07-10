using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX562Processor : AESMessageProcessor<AESInboundEDIMessage, EX562Provider>
	{
		public EX562Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("15C5D476-ABBC-48EB-B5BB-8B39F9496785", "EX562: REQUEST DECLARATION AMENDMENT");
		protected override bool GetIsFailure(EX562Provider provider) => true;
		protected override Type MessageInterpreterType => typeof(EX562MessageInterpreter);

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX562Provider provider) => LogicalStatusList.Codes.Accepted;
		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, EX562Provider provider) => AESEntryStatusList.Codes.AmendmentRequested;
	}
}
