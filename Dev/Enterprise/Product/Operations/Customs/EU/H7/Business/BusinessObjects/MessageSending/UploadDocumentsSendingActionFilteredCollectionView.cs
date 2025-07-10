using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.H7.Business;

public class UploadDocumentsSendingActionFilteredCollectionView<T> : NonPersistentBusinessObjectCollectionView<T> where T : UploadDocumentsSendingAction
{
	public UploadDocumentsSendingActionFilteredCollectionView(BusinessObjectCollection collectionToFilter) : base(collectionToFilter)
	{
	}

	public override void Load(ZQuery filter)
	{
		filterQuery = filter;
		Rebuild();
	}

	ZQuery filterQuery;

	protected override bool IsThisPartOfTheCollection(BusinessObject element)
	{
		var result = false;
		if (element is UploadDocumentsSendingAction uploadDocumentsSendingAction)
		{
			result = uploadDocumentsSendingAction.Bill.MatchesFilter(filterQuery);
		}

		return result;
	}

	protected override bool AllowNewCore => false;

	protected override T CreateNonPersistentBusinessObject()
	{
		throw new System.NotImplementedException();
	}
}
