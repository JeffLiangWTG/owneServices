using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentDiagnosticCriteriaCollection : BusinessObjectCollection<IncidentDiagnosticCriteria>
	{
		public IncidentDiagnosticCriteriaCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public IncidentDiagnosticCriteriaCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
