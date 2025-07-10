using CargoWise.Types;

namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbAgentBooking
	{
		ZGuid PK { get; }
		ZString LTB_JobID { get; set; }
		ZGuid LTB_KM_TransportBooking { get; set; }
	}
}
