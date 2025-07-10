using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine.Business;

public class StmTemplateHistoryCollectionView : BusinessObjectCollectionView<BusinessObject>
{
	public StmTemplateHistoryCollectionView(BusinessObjectCollection eDocsView) : base(eDocsView)
	{
	}

	protected override bool IsThisPartOfTheCollection(BusinessObject element)
	{
		return element is IStorageFile;
	}

	public new IeDocBase this[int index] => (IeDocBase)Elements[index];
}