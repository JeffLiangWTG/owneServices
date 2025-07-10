using CargoWise.EntityFramework;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionDocumentPivotView : BusinessObjectCollectionView<OperationalActionDocumentPivot>
	{
		public OperationalActionDocumentPivotView(OperationalActionDocumentPivotCollection collection)
			: base(collection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return true;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}
