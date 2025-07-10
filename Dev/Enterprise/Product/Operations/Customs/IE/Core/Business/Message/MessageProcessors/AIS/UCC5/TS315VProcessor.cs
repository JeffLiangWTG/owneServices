using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS315VProcessor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, Messaging.UCC5.TS315VProvider>
	{
		public TS315VProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS315VMessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS315VProvider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS315VProvider provider) => AISEntryStatusList.Codes.Registered;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, Messaging.UCC5.TS315VProvider provider)
		{
			if (messageAttachee is TemporaryStorageHeader header)
			{
				header.MRN = provider.MovementReferenceNumber;
				header.CustomsStatusDate = provider.AcknowledgementDate;
			}
		}

		protected override Type MessageInterpreterType => typeof(TS315VMessageInterpreter);
	}
}
