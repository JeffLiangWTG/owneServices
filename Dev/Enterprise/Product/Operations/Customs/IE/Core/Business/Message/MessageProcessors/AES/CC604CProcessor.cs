using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC604CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC604CProvider>
	{
		public CC604CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("A08D5714-EC01-40B7-AC7F-929A1DFDA114", "CC604C: EXIT SUMMARY DECLARATION AMENDMENT ACCEPTANCE");

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC604CProvider provider) => AESEntryStatusList.Codes.ReleasedForExport;

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC604CProvider provider) => LogicalStatusList.Codes.Accepted;

		protected override Type MessageInterpreterType => typeof(CC604MessageInterpreter);
	}
}
