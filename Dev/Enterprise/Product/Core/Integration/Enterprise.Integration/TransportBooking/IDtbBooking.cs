using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportCommon;

namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbBooking : IDtbTransport
	{
		ZGuid KM_KM_MasterBooking { get; set; }
		ZBool KM_IsMaster {  get; set; }
		IDtbBookingInstruction[] Instructions { get; }
		IPkgPackageJob PackageJob { get; }
		IEnumerable<IBusiness> RelatedJobs { get; }
		ZString KM_Direction { get; set; }
		IJobDocAddress Address { get; }
		IDtbBookingConsolidation ConsolidationSingleJob { get; }
		bool IsSendingXUSToCTO { get; }
		ZString KM_Status { get; set; }
		ZBool KM_IsActive { get; set; }
	}
}
