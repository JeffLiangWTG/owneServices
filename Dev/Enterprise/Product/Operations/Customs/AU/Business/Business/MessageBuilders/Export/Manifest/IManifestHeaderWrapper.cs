
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IManifestHeaderWrapper
	{
		bool IsAir
		{
			get;
		}

		bool IsSea
		{
			get;
		}

		ZString VoyageNumber
		{
			get;
		}

		ZString FlightNumber
		{
			get;
		}

		ZString VesselID
		{
			get;
		}

		ZString PortOfLoading
		{
			get;
		}

		ZString CountryOfDischarge
		{
			get;
		}

		ZDateTime DateOfDeparture
		{
			get;
		}

		ZString AirlineCode
		{
			get;
		}

		ZString CAN
		{
			get;
		}

		ZString CCAN
		{
			get;
		}

		int TotalPackageCount
		{
			get;
		}

		int TotalContainerCount
		{
			get;
		}

		int TotalEmptyContainerCount
		{
			get;
		}

		IManifestLineWrapper[] Lines
		{
			get;
		}

		ZString DepotPremiseID
		{
			get;
		}
	}
}
