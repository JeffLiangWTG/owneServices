using System;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class GeneralLedgerBalanceLineCollectionView : BusinessObjectCollectionView<GeneralLedgerBalanceLine>
	{
		public GeneralLedgerBalanceLineCollectionView(BusinessObjectCollection collectionToFilter) : base(collectionToFilter)
		{
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("Creating of new elements is not allowed.");
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var line = (GeneralLedgerBalanceLine)element;
			return !(line.GeneralLedgerAmountDR.IsEmpty && line.GeneralLedgerAmountCR.IsEmpty);
		}
	}
}