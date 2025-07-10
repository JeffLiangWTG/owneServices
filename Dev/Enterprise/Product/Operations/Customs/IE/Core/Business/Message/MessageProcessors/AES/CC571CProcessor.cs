using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC571CProcessor : AESMessageProcessor<AESInboundEDIMessage, CC571CProvider>
	{
		public CC571CProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => Res.GetString("9F83DF80-0CB3-465E-9464-0838F494D3D8", "CC571C: Re-Export Notification Registration Notification");

		public override string GetLogicalStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC571CProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AESInboundEDIMessage message, IMessageAttachee messageAttachee, CC571CProvider provider) => AESEntryStatusList.Codes.ReleasedForExport;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, CC571CProvider provider)
		{
			(messageAttachee as CusEntryHeader)?.MovementReferenceNumberSetter(provider.MovementReferenceNumber, provider.ReExportNotificationRegistrationDate);
		}

		protected override Type MessageInterpreterType => typeof(CC571MessageInterpreter);
	}
}
