using CargoWise.EntityFramework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[CodeAlive("To be used in next WI")]
	public class IncidentTriageChecklistItemCollection : BusinessObjectCollection<IncidentTriageChecklistItem>
	{
		public IncidentTriageChecklistItemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public IncidentTriageChecklistItemCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
