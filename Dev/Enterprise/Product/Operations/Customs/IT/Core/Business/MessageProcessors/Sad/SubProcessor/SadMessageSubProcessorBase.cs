using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;

namespace Enterprise.Customs.IT.Business;

public abstract class SadMessageSubProcessorBase
{
	protected SadMessageSubProcessorBase(ISadCustomsLinkedObjectAdapter entryAdapter)
	{
		EntryAdapter = Argument.NotNull(entryAdapter, nameof(entryAdapter));
	}
	protected ISadCustomsLinkedObjectAdapter EntryAdapter { get; }

	protected bool IsChangeStatusAllowed(string currentStatus, string newStatus) => SadCustomsStatusInformationProvider.CompareCustomsStatusOrder(StatusWithInformationOrderCollection, currentStatus, newStatus) > 0;

	ImmutableArray<CustomsStatusOrder> StatusWithInformationOrderCollection
	{
		get
		{
			if (statusWithInformationOrderCollection == null)
			{
				statusWithInformationOrderCollection = LoadCustomsStatusWithInformationOrderCollection().ToImmutableArray();
			}
			return statusWithInformationOrderCollection;
		}
	}
	ImmutableArray<CustomsStatusOrder> statusWithInformationOrderCollection;

	protected abstract IEnumerable<CustomsStatusOrder> LoadCustomsStatusWithInformationOrderCollection();
	protected string GetInvalidAttemptToChangeStatusExceptionMessage() => Res.GetString("8FEBEA3D-B054-429C-A2E5-E63526665071", "Entry {0} for job {1} not processed: Entry Status is not allowed.", EntryAdapter.EntryReferenceNumber, EntryAdapter.JobReferenceNumber);

	protected const string Empty = "";
}
