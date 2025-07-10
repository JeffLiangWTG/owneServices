using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbBookingConsolidationExtender
	{
		IEnumerable<IDtbBooking> GetExistingDtbBookings(IDtbBookingConsolidation consolidation, string sourceKey, BusinessObjectFactory factory);
		bool IsCreatingBookingByPassedPackages { get; }
	}
}
