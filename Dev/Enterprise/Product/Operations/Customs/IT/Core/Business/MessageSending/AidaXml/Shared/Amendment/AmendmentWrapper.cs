using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared;

public sealed class AmendmentWrapper : IDeclarationAmendment
{
	public AmendmentWrapper(CusEntryHeader entryHeader, JobDeclarationMessageSendingObject sendingObject)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		Argument.NotNull(sendingObject, nameof(sendingObject));

		lazyMrn = new Lazy<string>(() => entryHeader.MovementReferenceNumber);
		lazyReason = new Lazy<string>(() => sendingObject.VOCReason);
		lazyLegislativeReference = new Lazy<string>(() => sendingObject.CancellationAndAmendmentLegislativeReference);
		lazyCancelledLines = new Lazy<IReadOnlyCollection<int>>(() => GetCancelledLines(entryHeader));
	}

	string IAmendment.Mrn => lazyMrn.Value;
	readonly Lazy<string> lazyMrn;

	string IAmendment.Reason => lazyReason.Value;
	readonly Lazy<string> lazyReason;

	string IAmendment.LegislativeReference => lazyLegislativeReference.Value;
	readonly Lazy<string> lazyLegislativeReference;

	IReadOnlyCollection<int> IDeclarationAmendment.CancelledLines => lazyCancelledLines.Value;
	readonly Lazy<IReadOnlyCollection<int>> lazyCancelledLines;

	#region Implementation

	IReadOnlyCollection<int> GetCancelledLines(CusEntryHeader entryHeader)
	{
		var sentEntryLinesCount = entryHeader.ZG_SentEntryLinesCount;
		var mergedLinesCount = entryHeader.MergedLinesCount;
		if (sentEntryLinesCount > mergedLinesCount)
		{
			return Enumerable.Range(mergedLinesCount + 1, sentEntryLinesCount - mergedLinesCount).ToArray();
		}
		return default;
	}

	#endregion
}
