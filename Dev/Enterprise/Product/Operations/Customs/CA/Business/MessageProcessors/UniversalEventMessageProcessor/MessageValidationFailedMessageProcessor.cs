using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class MessageValidationFailedMessageProcessor : CAUniversalEventMessageProcessor
	{
		public MessageValidationFailedMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entryHeader)
			: base(logger, universalEvent, message, entryHeader)
		{ }

		protected override ZGuid NotifyEmailGroup => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageErrorsToGroup);

		protected override ZString NotifyEmailMode => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageErrors);

		protected override ZString GetMessageTypeDescription() => AutoEvents.MessageValidationFailed.Description;

		protected override ZString GetResponseTypeDescription() => Res.GetString("0917597a-6a5e-437d-b91e-01fc4d173e8a", "A 'Message Syntax Rejection' response");

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
