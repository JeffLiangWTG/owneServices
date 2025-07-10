using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BE.Business;

public abstract class EventMessageProcessor : BaseMessageProcessor<UniversalEventWrapper>
{
	protected EventMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => BEIncomingMessageSubTypes.Descriptions.CustomsServiceErrorUniversalEvent;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { BEIncomingMessageSubTypes.Codes.CustomsServiceErrorUniversalEvent };

	protected internal sealed override UniversalEventWrapper GetMessageDataProvider(BEMessage message)  => new UniversalEventWrapper(message.EM_MessageText);

	protected sealed override void ProcessMessageCore(BEMessage message, UniversalEventWrapper messageDataProvider)
	{
		message.EM_MessageInterpretation = new CustomsServiceErrorUniversalEventMessageInterpreter(messageDataProvider).CreateMessageDetailsRejected();
		message.EM_Status = EDIMessage.Status.ProcessedOK;
		SetErrorStatusToLinkedObject(message);
	}

	protected override BusinessObject FindParentOfMessage(BEMessage message, UniversalEventWrapper messageDataProvider) => MessageHelper.LocateHeaderByEdiInterchange(message.Interchange);

	protected abstract void SetErrorStatusToLinkedObject(BEMessage message);
}
