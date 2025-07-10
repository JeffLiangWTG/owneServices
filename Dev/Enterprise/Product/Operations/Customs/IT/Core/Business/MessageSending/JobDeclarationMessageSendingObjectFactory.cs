using System.Linq;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business;

public sealed class JobDeclarationMessageSendingObjectFactory : JobDeclarationMessageSendingObjectAbstractFactory
{
	public JobDeclarationMessageSendingObjectFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent) : base(entryHeader, sendingObjectParent)
	{
	}

	protected override JobDeclarationMessageSendingObject TryGetNewMessageSendingObjectCore()
	{
		if (EntryHeader.IsNbNeRejected)
		{
			return GetNbNeStandaloneMessageSendingObject();
		}

		if (EntryHeader.IsNbRejected)
		{
			return GetNbStandaloneMessageSendingObject();
		}

		return GetSadMessageSendingObject();
	}

	#region Implementation

	JobDeclarationMessageSendingObject GetSadMessageSendingObject()
	{
		var declaration = EntryHeader.Declaration;
		switch (declaration.JE_MessageType)
		{
			case Common.EU.EUJobMessageTypeList.Codes.Import:
				return new IMMessageSendingObject(EntryHeader, SendingObjectParent);

			case Common.EU.EUJobMessageTypeList.Codes.Export:
				return new ETMessageSendingObject(EntryHeader, SendingObjectParent);
		}
		return null;
	}

	JobDeclarationMessageSendingObject GetNbStandaloneMessageSendingObject()
	{
		if (HasAtLeastOneMergedLinesWithSendableGroupedPreviousDocuments())
		{
			return new NBStandaloneMessageSendingObject(EntryHeader, SendingObjectParent);
		}
		return null;
	}

	JobDeclarationMessageSendingObject GetNbNeStandaloneMessageSendingObject()
	{
		if (HasAtLeastOneMergedLinesWithSendableGroupedPreviousDocuments())
		{
			return new NBNEStandaloneMessageSendingObject(EntryHeader, SendingObjectParent);
		}
		return null;
	}

	bool HasAtLeastOneMergedLinesWithSendableGroupedPreviousDocuments() => EntryHeader.MergedLinesWithSendableGroupedPreviousDocuments.Any();

	#endregion
}
