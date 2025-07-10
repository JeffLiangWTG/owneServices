using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IUnderbondMovementRequestHeader
	{
		ZString RequestReasonCode { get; }
		ZString UnderbondBySeaVoyageNumber { get; }
		ZString UnderbondBySeaVesselID { get; }
		ZString ResponsiblePartyID { get; }
		ZString ModeOfTransport { get; }
		ZString DestaintionEstablishmentID { get; }
		ZString OriginatingEstablishmentID { get; }
		ZString DischargeEstablishmentID { get; }
		ZString UnderbondBySeaOverseasRoutingPort { get; }
		bool IsBureau { get; }

		ZInt NumberOfPackages { get; }
		ZString PackageType { get; }
		ZString FlightNumber { get; }
		ZDateTime EstimatedDateOfArrival { get; }
		ZString VoyageNumber { get; }
		ZString VesselID { get; }
		ZString TranshipmentOverseasDestinationPort { get; }
		IUnderbondMovementRequestLine Line { get; }
	}
}
