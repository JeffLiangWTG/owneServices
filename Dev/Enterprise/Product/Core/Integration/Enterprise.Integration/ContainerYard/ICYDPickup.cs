using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICYDPickup
	{
		ZGuid PK { get; }

		ZString YPL_PickupID { get; set; }

		ZString YPL_TransportReference { get; }

		ICYDYardUnitState GetLinkedYardUnit { get; }

		ICYDUnitLineItem GetUnitLineItem { get; }
	}
}
