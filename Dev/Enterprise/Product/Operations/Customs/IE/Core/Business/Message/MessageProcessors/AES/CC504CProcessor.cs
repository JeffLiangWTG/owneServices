using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC504CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC504CProvider>
	{
		public CC504CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("67DA7D24-3F64-414C-910A-1D019900365A", "CC504C: EXPORT DECLARATION AMENDMENT ACCEPTANCE");

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC504CProvider provider) =>
			 messageAttachee.EntryStatus == AESEntryStatusList.Codes.AmendmentRequested ? AESEntryStatusList.Codes.ControlledForExport : (string)messageAttachee.EntryStatus;

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC504CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(CC504MessageInterpreter);
	}
}
