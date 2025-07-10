using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS328Processor : AISUCC5MessageProcessor<AISUCC5InboundEDIMessage, Messaging.UCC5.TS328Provider>
	{
		public TS328Processor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected override string MessageFriendlyNameCore => CommonResStrings.TS328MessageFriendlyName;

		public override string GetLogicalStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS328Provider provider) => LogicalStatusList.Codes.Accepted;

		public override string GetEntryStatus(AISUCC5InboundEDIMessage message, IMessageAttachee messageAttachee, Messaging.UCC5.TS328Provider provider) => AISEntryStatusList.Codes.Accepted;

		protected override void UpdateMessageAttacheeCore(IMessageAttachee messageAttachee, Messaging.UCC5.TS328Provider provider)
		{
			if (messageAttachee is TemporaryStorageHeader temporaryStorageHeader)
			{
				if (!provider.MovementReferenceNumber.IsEmpty)
				{
					temporaryStorageHeader.MRN = provider.MovementReferenceNumber;
				}
				if (!provider.AcceptanceDate.IsEmpty)
				{
					temporaryStorageHeader.CustomsStatusDate = provider.AcceptanceDate;
				}
			}
		}

		protected override Type MessageInterpreterType => typeof(TS328MessageInterpreter);
	}
}
