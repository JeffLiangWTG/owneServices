using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.Schema;

public class IncidentMetricsCollection : ActiveBusinessObjectCollection<IncidentMetrics>
{
	public IncidentMetricsCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}

	public IncidentMetricsCollection(BusinessObjectFactory factory, string incidentNumber)
		: base(factory, GetMetricsFilter(incidentNumber))
	{
	}

	static ZQuery GetMetricsFilter(string incidentNumber)
	{
		return new ZQuery(IncidentMetricsSchema.IME_IncidentNumber, incidentNumber);
	}
}
