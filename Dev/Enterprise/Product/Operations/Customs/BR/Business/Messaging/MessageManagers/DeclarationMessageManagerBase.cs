using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business;

public abstract class DeclarationMessageManager : BaseMessageManager
{
	public DeclarationMessageManager(DeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected DeclarationMessageSendingObject MessageSender => messageSender as DeclarationMessageSendingObject;

	public override bool CanSendOriginal => true;

	public override bool CanSendWithdrawal => false;

	public override bool IsWaitingForResponse => MessageSender.Header.IsWaitingForResponse;

	public override bool HasActiveMessages => MessageSender.Header.HasBeenLodgedAtCustoms;

	protected override void AfterGenerateMessage(IEnumerable<EDIMessage> messages)
	{
		base.AfterGenerateMessage(messages);

		if (MessageSender.Header is CusEntryHeader entryHeader)
		{
			entryHeader.Messages.AddRange(messages);

			if ((messages.Any() && entryHeader.Declaration.IsImportOnly) || !entryHeader.Declaration.IsImportOnly)
			{
				entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			}

			if (entryHeader.CH_EntrySubmittedDate.IsEmpty)
			{
				entryHeader.CH_EntrySubmittedDate = ZDateTime.Now;
			}
		}
	}

	public override void RollbackOnSavingFailed()
	{
		var entryHeader = MessageSender.Header;
		entryHeader.CH_EntrySubmittedDate = (ZDateTime)entryHeader.CH_EntrySubmittedDateInfo.OriginalValue;
		entryHeader.Logs.LogsNotInDB.ForEach(m => m.Delete());
	}
}
