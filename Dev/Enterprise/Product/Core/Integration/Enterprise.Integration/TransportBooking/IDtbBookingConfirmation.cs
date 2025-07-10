using CargoWise.Types;

namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbBookingConfirmation
	{
		ZGuid PK {  get; }
		ZString KK_ConfirmationType { get; set; }
		ZGuid KK_KK_MasterBookingConfirmation { get; set; }

		IDtbBookingInstruction Instruction { get; }
		IDtbBooking Booking { get; }
	}
}
