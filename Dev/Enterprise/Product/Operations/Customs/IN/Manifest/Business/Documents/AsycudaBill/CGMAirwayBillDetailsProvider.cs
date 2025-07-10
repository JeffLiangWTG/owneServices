using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business;

class CGMAirwayBillDetailsProvider : ICGMAirwayBillDetails
{
	public CGMAirwayBillDetailsProvider(CGMAsycudaBill bill)
	{
		Bill = Argument.NotNull(bill, nameof(bill));
	}

	CGMAsycudaBill Bill { get; }

	ZString ICGMAirwayBillDetails.BillNumber => Bill.ABL_BillNumber;

	ZDate ICGMAirwayBillDetails.BillDate => Bill.ABL_BillIssueDate;

	ZString ICGMAirwayBillDetails.PortOfOrigin => Bill.PortOfOriginCode;

	ZString ICGMAirwayBillDetails.PortOfDestination => Bill.PortOfFinalDestinationCode;

	ZInt ICGMAirwayBillDetails.NumberOfPackages => Bill.ABL_ManifestQty;

	ZDecimal ICGMAirwayBillDetails.GrossWeightInKilos => Bill.MassInKilos;

	ZString ICGMAirwayBillDetails.CargoDescription => Bill.ABL_GoodsDescription;
}
