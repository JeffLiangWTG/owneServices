using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICYDDelivery
	{
		ZGuid PK { get; }

		ZString YDL_DeliveryID { get; set; }

		ZString YDL_TransportReference { get; }

		ICYDYardUnitState GetLinkedYardUnit { get; }

		ICYDUnitLineItem GetUnitLineItem { get; }
	}
}
