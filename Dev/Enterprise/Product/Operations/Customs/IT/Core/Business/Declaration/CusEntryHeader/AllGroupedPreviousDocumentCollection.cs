using System;
using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AllGroupedPreviousDocumentCollection : NonPersistentBusinessObjectCollection<GroupedPreviousDocument>
{
	AllGroupedPreviousDocumentCollection(CusEntryHeader entryHeader) : base(entryHeader.Factory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
	}
	readonly CusEntryHeader entryHeader;

	public static AllGroupedPreviousDocumentCollection LoadNew(CusEntryHeader entryHeader)
	{
		var result = new AllGroupedPreviousDocumentCollection(entryHeader);
		result.LoadCollection();
		return result;
	}

	protected override bool AllowNewCore => false;

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new InvalidOperationException("It is not possible to create a new grouped previous document object from this collection");
	}

	void LoadCollection()
	{
		foreach (CusEntryLine entryLine in entryHeader.MergedLines)
		{
			foreach (GroupedPreviousDocument groupedPreviousDocument in entryLine?.GroupedPreviousDocuments)
			{
				Add(groupedPreviousDocument);
			}
		}
	}
}
