using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC528CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC528CProvider>
	{
		public CC528CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("DE41B09B-77F6-4260-9EB1-6A50A129C841", "CC528C: EXPORT MRN ALLOCATED");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC528CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC528CProvider provider) => AESEntryStatusList.Codes.MrnAllocated;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC528CProvider provider)
		{
			(messageAttachee as CusEntryHeader)?.MovementReferenceNumberSetter(provider.MovementReferenceNumber, provider.DeclarationAcceptanceDate);
		}

		protected override Type MessageInterpreterType => typeof(CC528MessageInterpreter);
	}
}
