using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class NBStandaloneMessageSendingObject : SADMessageSendingObject, ISadOutgoingCustomsMessageGeneratorValuesProvider
{
	public NBStandaloneMessageSendingObject(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent)
		: base(header, jobDeclarationMessageSendingObjectParent, false)
	{
	}

	public NBStandaloneMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent, bool isForDeterminingMessageChangedStatus)
		: base(entryHeader, jobDeclarationMessageSendingObjectParent, isForDeterminingMessageChangedStatus)
	{
	}

	public IEnumerable<INBMessageSendingObject> NbMessageSendingObjects => nbMessages ?? (nbMessages = GetNbMessages());
	IEnumerable<INBMessageSendingObject> nbMessages;

	#region Implementation

	protected override ZString GetMessageSubType() => SADConstants.MessageSubTypes.NB;

	protected override ZString GetCombinedCustomsMessageSubType() => GetMessageSubType();

	IEnumerable<INBMessageSendingObject> GetNbMessages()
	{
		var mergedLinesWithSendableGroupedPreviousDocuments = Header.MergedLinesWithSendableGroupedPreviousDocuments;
		for (int i = 0; i < mergedLinesWithSendableGroupedPreviousDocuments.Count(); i++)
		{
			var entryLine = mergedLinesWithSendableGroupedPreviousDocuments.ElementAt(i);
			if (entryLine.GroupedPreviousDocuments.Any())
			{
				yield return new NBStandaloneMessageWrapper(entryLine, i + 1);
			}
		}
	}

	#endregion

	#region ISadOutgoingCustomsMessageGeneratorValuesProvider

	IEnumerable<ISadCustomsMessage> ISadOutgoingCustomsMessageGeneratorValuesProvider.GetCustomsMessageObjects()
	{
		foreach (var nbMessage in NbMessageSendingObjects)
		{
			yield return new NBMessage(nbMessage, this);
		}
	}

	ICustomsMessageFountainProvider ISadOutgoingCustomsMessageGeneratorValuesProvider.FountainProvider => fountainProvider ?? (fountainProvider = new JobDeclarationFountainProvider(Declaration));
	ICustomsMessageFountainProvider fountainProvider;

	#endregion
}
