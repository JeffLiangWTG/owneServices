using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class MessageAcceptedMessageProcessor : CAUniversalEventMessageProcessor
	{
		public MessageAcceptedMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entryHeader)
			: base(logger, universalEvent, message, entryHeader)
		{
		}

		protected override ZGuid NotifyEmailGroup => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup);

		protected override ZString NotifyEmailMode => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements);

		protected override ZString GetMessageTypeDescription() => AutoEvents.MessageAccepted.Description;

		protected override ZString GetResponseTypeDescription() => Res.GetString("d52d8960-c0bf-4843-aec3-82433935e072", "A 'Message Accepted and Passed Application Edits' response");

		protected override ZString GetCalculatedMessageStatus(ZString status) => MessageStatusList.GetAppropriateClearCode(status);
	}
}
