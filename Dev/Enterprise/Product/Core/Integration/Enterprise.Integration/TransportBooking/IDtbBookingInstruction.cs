using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;

namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbBookingInstruction
	{
		ZGuid PK { get; }
		ZString KN_DropMode { get; set; }
		ZString KN_InstructionType { get; set; }
		ZBool KN_IsContainerRateable { get; set; }
		ZBool KN_IsLooseRateable { get; set; }
		ZGuid KN_KM_BookingMovement { get; set; }
		ZGuid KN_RQ_Equipment { get; set; }
		ZInt KN_Sequence { get; set; }
		ZString KN_ServiceInstruction { get; set; }
		ZString KN_Status { get; set; }
		ZGuid KN_TZ_DomesticZone { get; set; }
		ZGuid KN_KN_MasterBookingInstruction { get; set; }

		IDtbBooking Booking { get; }
		IJobDocAddress Address { get; }
		IEnumerable<IPkgPackage> Packages { get; }
		IActiveBusinessObjectCollection<IDtbBookingInstructionPkgDivot> PackageDivots { get; }
		IEnumerable<IDtbBookingConfirmation> Confirmations { get; }
		ZString OrganisationType { get; set; }

		ZDateTime EstimatedDate(bool isPickUp);
		void Delete();
	}
}
