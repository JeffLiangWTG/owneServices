using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public abstract class DeclarationMessageManager : BaseMessageManager<DeclarationMessageSendingObject>
{
	protected DeclarationMessageManager(DeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	public override string MessageFriendlyName => SendingObject?.FriendlyNameForMessageManager ?? ZString.Empty;

	public override bool IsWaitingForResponse => SendingObject?.Header?.IsWaitingForResponse ?? false;

	public override bool HasActiveMessages => SendingObject?.Header?.HasBeenLodgedAtCustoms ?? false;

	protected virtual bool ShouldUpdateStatus => true;

	protected virtual Event DeclarationSentEvent => Events.DeclarationSentToCustoms;

	protected virtual ZString DeclarationSentEventReference => ZString.Empty;

	protected override void AfterGenerateMessage(DeclarationMessageSendingObject sendingObject, EDIMessage message)
	{
		var entryHeader = SendingObject.Header;

		if (ShouldUpdateStatus && SendingObject.MessageType != PassarMessageTypeList.Codes.NI016)
		{
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Sent;
			entryHeader.CH_PhaseStatus = PassarDeclarationPhaseList.GetAppropriateEntryHeaderPhaseStatusCode(SendingObject.MessageType);
		}

		entryHeader.Messages.Add(message);
		entryHeader.Declaration.LogCustomsCommencedIfNeeded();

		if (DeclarationSentEvent != null)
		{
			entryHeader.Logs.AddNew(DeclarationSentEvent, DeclarationSentEventReference);
		}

		entryHeader.PopulateEntrySubmittedDateIfRequired();
	}

	public override void RollbackOnSaveFailed()
	{
		var entryHeader = SendingObject.Header;
		entryHeader.Declaration.Logs.LogsNotInDB.ForEach(l => l.Delete());
		entryHeader.Logs.LogsNotInDB.ForEach(l => l.Delete());
	}
}
