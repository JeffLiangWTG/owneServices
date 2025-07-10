using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC628CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC628CProvider>
	{
		public CC628CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("5DBF33B2-0C4D-4EA5-8051-FD355E3061B3", "CC628C: Exit Summary Declaration Acknowledgement");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC628CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC628CProvider provider) => AESEntryStatusList.Codes.ReleasedForExport;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC628CProvider provider)
		{
			(messageAttachee as CusEntryHeader)?.MovementReferenceNumberSetter(provider.MovementReferenceNumber, provider.DeclarationAcceptanceDate);
		}

		protected override Type MessageInterpreterType => typeof(CC628MessageInterpreter);
	}
}
