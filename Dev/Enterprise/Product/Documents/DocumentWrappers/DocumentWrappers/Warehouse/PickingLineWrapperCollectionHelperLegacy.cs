using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class PickingLineWrapperCollectionHelperLegacy : PickingLineWrapperCollectionHelperBase
	{
		public PickingLineWrapperCollectionHelperLegacy(WhsPick pick, BusinessObjectFactory factory, bool isPickByBiggestEnabled)
			: base(pick, factory, isPickByBiggestEnabled)
		{ }

		protected override IPickingSlipLineWrapperCollection GetRolledUpLines(WhsPickLine[] pickLines)
		{
			return new DocWhsPickLineCollection(Factory);
		}

		protected override IPickingSlipLineWrapper GetDocPickLine(WhsPickLine pickLine)
		{
			return DocWhsPickLine.New(pickLine, Factory);
		}

		protected override IPickingSlipLineWrapperCollection SplitItems(IPickingSlipLineWrapperCollection rolledUpLines)
		{
			return rolledUpLines;
		}
	}
}