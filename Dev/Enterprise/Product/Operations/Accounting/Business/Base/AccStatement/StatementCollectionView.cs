using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.AccStatement
{
	public class UnreconciledStatementCollectionView : BusinessObjectCollectionView<Statement>
	{
		public UnreconciledStatementCollectionView(StatementCollection collectionToFilter) : base(collectionToFilter)
		{
		}

		#region Overrides of SubsetBusinessObjectCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var source = element as Statement;
			return source != null && !source.IsCleared;
		}

		#endregion
	}

	public class ReconciledStatementCollectionView : BusinessObjectCollectionView<Statement>
	{
		public ReconciledStatementCollectionView(StatementCollection collectionToFilter) : base(collectionToFilter)
		{
		}

		#region Overrides of SubsetBusinessObjectCollection

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var source = element as Statement;
			return source != null && source.IsCleared;
		}

		#endregion
	}
}
