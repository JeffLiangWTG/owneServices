using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaCargoEstablishmentQueryInformation
	{
		ZString ContainerNumber { get; }
		ZString HouseBill { get; }
		ZString OceanBill { get; }
		ZString ResponsiblePartyID { get; }
		ZString VesselID { get; }
		ZString VoyageNumber { get; }
		ZString EstablishmentID { get; }
	}
}
