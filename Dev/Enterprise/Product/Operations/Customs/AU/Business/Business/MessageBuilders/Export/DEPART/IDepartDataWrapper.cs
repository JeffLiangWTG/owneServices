using CargoWise.Types;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDepartDataWrapper
	{
		bool IsAir
		{
			get;
		}

		bool IsSea
		{
			get;
		}

		ZString FlightNumber
		{
			get;
		}

		ZString AirlineCode
		{
			get;
		}

		ZString VoyageNumber
		{
			get;
		}

		ZString VesselID
		{
			get;
		}

		ZString CTOEstablishmentID
		{
			get;
		}

		ZDateTime DepartureDateTime
		{
			get;
		}

		ZString PortOfDestination
		{
			get;
		}

		ZString CarrierPartyID
		{
			get;
		}
	}
}
