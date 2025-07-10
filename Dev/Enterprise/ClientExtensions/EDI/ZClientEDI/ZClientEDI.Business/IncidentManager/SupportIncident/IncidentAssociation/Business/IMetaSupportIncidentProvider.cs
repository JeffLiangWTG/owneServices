using System;
using System.Collections.Generic;

namespace Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation
{
	public interface IMetaSupportIncidentProvider
	{
		IEnumerable<IMetaSupportIncident> AllBetween(DateTime fromDate, DateTime toDate);
		IMetaSupportIncident FromGuid(Guid guid);
		IEnumerable<IMetaSupportIncident> FromGuids(IEnumerable<Guid> guids);
	}
}