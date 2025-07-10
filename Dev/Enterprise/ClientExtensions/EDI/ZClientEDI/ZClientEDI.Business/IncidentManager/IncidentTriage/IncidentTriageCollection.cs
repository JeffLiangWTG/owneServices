using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID("IncidentTriage")]
	public class IncidentTriageCollection : BusinessObjectCollection<IncidentTriage>
	{
		public IncidentTriageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public IncidentTriageCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
