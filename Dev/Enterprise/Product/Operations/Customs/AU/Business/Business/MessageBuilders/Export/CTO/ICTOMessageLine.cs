using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ICTOMessageLine
	{
		ZString CTOEstablishmentID { get; }
		EDIMessageCollection Messages { get; }

		ZString ExportDeclarationExemptionCode { get; }
		ZString CustomsAuthorityNumber { get; }
		ZString CustomsContingencyAuthorityNumber { get; }
		ZString CarrierPartyID { get; }
		ZDate ProposedDateOfDeparture { get; }
		ZBool OffloadIndicator { get; }
		ZString VesselID { get; }
		ZString VoyageNumber { get; }
		ZString GoodsOwnerPartyID { get; }
		ZString OwnerName { get; }
		ZString GoodsDescription { get; }
		ZString CountryOfDestination { get; }
		ZString AirWaybill { get; }
		ZString ContainerNumber { get; }
		ZString NonContainerisedIdentifier { get; }

		ZBool IsAir { get; }
		ZBool IsSea { get; }
	}
}
