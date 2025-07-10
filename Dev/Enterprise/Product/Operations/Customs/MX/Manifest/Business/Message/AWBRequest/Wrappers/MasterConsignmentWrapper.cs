using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class MasterConsignmentWrapper : IMasterConsignment
	{
		internal MasterConsignmentWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			header = this.bill.Header;
		}
		readonly AsycudaBill bill;
		readonly AsycudaManifestHeader header;

		decimal IMasterConsignment.TotalPieceQuantity => (ZDecimal)bill.ABL_ManifestQty;

		string IMasterConsignment.TransportContractDocument => header.AMA_MasterBill;

		ILocation IMasterConsignment.OriginLocation => originLocation ?? (originLocation = new LocationWrapper(bill.Factory, header.AMA_RL_NKPortOfLoading));
		ILocation originLocation;

		ILocation IMasterConsignment.FinalDestinationLocation => finalDestinationLocation ?? (finalDestinationLocation = new LocationWrapper(bill.Factory, header.AMA_RL_NKPortOfDischarge));
		ILocation finalDestinationLocation;

		IHouseConsignment IMasterConsignment.IncludedHouseConsignment => includedHouseConsignment ?? (includedHouseConsignment = new HouseConsignmentWrapper(bill));
		IHouseConsignment includedHouseConsignment;

		IMeasure IMasterConsignment.IncludedTareGrossWeightMeasure => includedTareGrossWeightMeasure ?? (includedTareGrossWeightMeasure = new MeasureWrapper(bill));
		IMeasure includedTareGrossWeightMeasure;
	}

	internal class MeasureWrapper : IMeasure
	{
		internal MeasureWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IMeasure.UnitCode => AWBRequestHelper.WeightUnitCodeCalculator(bill.ABL_GrossWeightUQ);

		decimal IMeasure.Value => AWBRequestHelper.WeightConvertion(bill.ABL_GrossWeightUQ, bill.ABL_GrossWeight);
	}
}
