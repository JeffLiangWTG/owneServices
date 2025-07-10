using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusOutTurnCollectionForSplit : BusinessObjectCollectionView<CusOutTurn>
	{
		public CusOutTurnCollectionForSplit(SplitConsignment split)
			: base(split.AWB.OutTurnsCollection)
		{
			this.split = split;
			Rebuild();
		}

		readonly SplitConsignment split;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return split != null && ((CusOutTurn)element).SplitReferenceToWhichThisPertains == split.SplitReference;
		}
	}
}
