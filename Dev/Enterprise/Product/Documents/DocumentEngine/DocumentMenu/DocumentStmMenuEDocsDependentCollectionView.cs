using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngine.Business
{
	public class DocumentStmMenuEDocsDependentCollectionView : BusinessObjectCollectionView<DocumentStmMenuEDocs>
	{
		public DocumentStmMenuEDocsDependentCollectionView(DocumentStmMenuEDocsDependentCollection collection) : base(collection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return !((DocumentStmMenuEDocs)element).SX_IsClientSupressed;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
