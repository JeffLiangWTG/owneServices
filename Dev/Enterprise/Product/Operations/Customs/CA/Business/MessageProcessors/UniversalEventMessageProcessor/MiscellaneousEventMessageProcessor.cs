using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class MiscellaneousEventMessageProcessor : CAUniversalEventMessageProcessor
	{
		public MiscellaneousEventMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entryHeader)
			: base(logger, universalEvent, message, entryHeader)
		{
		}

		protected override ZGuid NotifyEmailGroup => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup);

		protected override ZString NotifyEmailMode => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements);

		protected override ZString GetMessageTypeDescription() => AutoEvents.MiscellaneousEvent.Description;

		protected override ZString GetResponseTypeDescription() => Res.GetString("c0d8f8c9-3742-408b-a996-445ae2f8688d", "An information message");

		protected override ZBool ShouldUpdateMessageStatus(CusEntryHeader entryHeader) => ZBool.False;
	}
}
