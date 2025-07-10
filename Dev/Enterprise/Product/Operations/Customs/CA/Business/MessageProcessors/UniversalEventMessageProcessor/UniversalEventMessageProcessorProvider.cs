using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public static class UniversalEventMessageProcessorProvider
	{
		public static CAUniversalEventMessageProcessor GetProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entryHeader)
		{
			switch (universalEvent.EventType.Value)
			{
				case AutoEvents.MessageValidationPassedCode:
					return new MessageValidationPassedMessageProcessor(logger, universalEvent, message, entryHeader);
				case AutoEvents.MessageValidationFailedCode:
					return new MessageValidationFailedMessageProcessor(logger, universalEvent, message, entryHeader);
				case AutoEvents.MessageAcceptedCode:
					return new MessageAcceptedMessageProcessor(logger, universalEvent, message, entryHeader);
				case AutoEvents.MessageRejectedCode:
					return new MessageRejectedMessageProcessor(logger, universalEvent, message, entryHeader);
				case AutoEvents.MessageStatusChangeCode:
					return new MessageStatusChangeMessageProcessor(logger, universalEvent, message, entryHeader);
				case AutoEvents.MiscellaneousEventCode:
					return new MiscellaneousEventMessageProcessor(logger, universalEvent, message, entryHeader);
				case AutoEvents.CustomsManifestStatusCode:
					return new CustomsManifestStatusMessageProcessor(logger, universalEvent, message, entryHeader);

				default:
					return null;
			}
		}
	}
}
