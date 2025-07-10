using CargoWise.Types;

namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbBookingInstructionPkgDivot
	{
		ZGuid PK { get; }
		ZGuid KD_KP_Package { get; set; }
		ZInt KD_Quantity { get; set; }
	}
}
