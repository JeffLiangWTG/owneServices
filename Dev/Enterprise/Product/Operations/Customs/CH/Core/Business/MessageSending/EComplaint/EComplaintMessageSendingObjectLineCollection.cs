using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageSendingObjectLineCollection : NonPersistentBusinessObjectCollection<EComplaintMessageSendingObjectLine>
{
	public EComplaintMessageSendingObjectLineCollection(CusEntryHeader entryHeader)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	readonly CusEntryHeader entryHeader;

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		return new EComplaintMessageSendingObjectLine(entryHeader);
	}
}
