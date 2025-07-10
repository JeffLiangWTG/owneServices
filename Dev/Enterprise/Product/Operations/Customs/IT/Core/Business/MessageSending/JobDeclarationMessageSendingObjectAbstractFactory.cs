using CargoWise.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public abstract class JobDeclarationMessageSendingObjectAbstractFactory
{
	protected JobDeclarationMessageSendingObjectAbstractFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent)
	{
		EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		SendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
		Argument.NotNull(entryHeader.Declaration, nameof(entryHeader.Declaration));
	}

	protected CusEntryHeader EntryHeader { get; }

	protected JobDeclarationMessageSendingObjectParent SendingObjectParent { get; }

	public JobDeclarationMessageSendingObject TryGetNewMessageSendingObject() => TryGetNewMessageSendingObjectCore();

	protected abstract JobDeclarationMessageSendingObject TryGetNewMessageSendingObjectCore();
}
