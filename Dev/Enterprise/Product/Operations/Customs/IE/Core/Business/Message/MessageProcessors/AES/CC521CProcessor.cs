using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC521CProcessor : ExitControlMessageProcessor<AESInboundEDIMessage, CC521CProvider>
	{
		public CC521CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("3A9CEA8A-945A-4C8A-99DF-6BF1072C104F", "CC521C: DIVERSION REJECTION NOTIFCATION");
		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC521CProvider provider) => LogicalStatusList.Codes.Accepted;
		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC521CProvider provider) => AESEntryStatusList.Codes.DiversionRequestRejected;
		protected override Type MessageInterpreterType => typeof(CC521MessageInterpreter);
	}
}
