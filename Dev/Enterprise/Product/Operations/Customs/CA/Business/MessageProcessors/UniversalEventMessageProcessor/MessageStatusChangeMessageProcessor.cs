using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class MessageStatusChangeMessageProcessor : CAUniversalEventMessageProcessor
	{
		public MessageStatusChangeMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entryHeader)
			: base(logger, universalEvent, message, entryHeader)
		{
		}

		protected override ZGuid NotifyEmailGroup => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup);

		protected override ZString NotifyEmailMode => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements);

		protected override ZString GetMessageTypeDescription() => AutoEvents.MessageStatusChange.Description;

		protected override ZString GetResponseTypeDescription() => Res.GetString("8b39c89b-d039-4066-9de2-c0b6e2ae42d8", "A Status Information message");

		protected override ZBool ShouldUpdateMessageStatus(CusEntryHeader entryHeader) => ZBool.False;
	}
}
