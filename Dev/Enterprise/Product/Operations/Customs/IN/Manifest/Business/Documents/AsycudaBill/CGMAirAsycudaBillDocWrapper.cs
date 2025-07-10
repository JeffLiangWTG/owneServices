using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMAirAsycudaBillDocWrapper : DocumentWrapper
{
	public CGMAirAsycudaBillDocWrapper(CGMAsycudaBill bill)
	{
		Bill = bill;
	}

	CGMAsycudaBill Bill { get; }

	public ZString HAWBNumber => HouseBillDetails.BillNumber;

	public ZDateTime HAWBDate => HouseBillDetails.BillDate;

	public ZString PortOfOrigin => HouseBillDetails.PortOfOrigin;

	public ZString PortOfDestination => HouseBillDetails.PortOfDestination;

	public ZInt NumberOfPackages => HouseBillDetails.NumberOfPackages;

	public ZDecimal GrossWeightInKilos => HouseBillDetails.GrossWeightInKilos;

	public ZString CargoDescription => HouseBillDetails.CargoDescription;

	ICGMAirwayBillDetails HouseBillDetails => houseBillDetails ??= new CGMAirwayBillDetailsProvider(Bill);
	ICGMAirwayBillDetails houseBillDetails;
}
