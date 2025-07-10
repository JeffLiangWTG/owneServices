using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business;

interface ICGMAirwayBillDetails
{
	ZString BillNumber { get; }

	ZDate BillDate { get; }

	ZString PortOfOrigin { get; }

	ZString PortOfDestination { get; }

	ZInt NumberOfPackages { get; }

	ZDecimal GrossWeightInKilos { get; }

	ZString CargoDescription { get; }
}
