using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class MessageRejectedMessageProcessor : CAUniversalEventMessageProcessor
	{
		public MessageRejectedMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entryHeader)
			: base(logger, universalEvent, message, entryHeader)
		{
		}

		protected override ZGuid NotifyEmailGroup => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup);

		protected override ZString NotifyEmailMode => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageErrors);

		protected override ZString GetMessageTypeDescription() => AutoEvents.MessageRejected.Description;

		protected override ZString GetResponseTypeDescription() => Res.GetString("ae46c871-6a38-4ea4-ba9e-c082dcc00487", "An 'Application Error Rejection' response");

		protected override ZBool ShouldUpdateMessageStatus(CusEntryHeader entryHeader) => !MessageStatusList.IsError(entryHeader.CH_Status);

		protected override ZString GetCalculatedMessageStatus(ZString status) => MessageStatusList.GetAppropriateErrorCode(status);

		protected override void UpdateEntryStatusIfNeeded(CusEntryHeader entryHeader)
		{
			if (entryHeader != null)
			{
				entryHeader.CH_EntryStatus = B3EntryStatusList.Codes.Error;
			}
		}
	}
}
