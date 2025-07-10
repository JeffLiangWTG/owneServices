using CargoWise.Customs.AR.MessageContracts;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.AR.Manifest.Business
{
	internal class MeasureWrapper : IMeasure
	{
		internal MeasureWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IMeasure.UnitCode => ARHelperClass.WeightUnitCodeCalculator(bill.ABL_GrossWeightUQ);

		decimal IMeasure.Value => ARHelperClass.WeightConvertion(bill.ABL_GrossWeightUQ, bill.ABL_GrossWeight);
	}
}
