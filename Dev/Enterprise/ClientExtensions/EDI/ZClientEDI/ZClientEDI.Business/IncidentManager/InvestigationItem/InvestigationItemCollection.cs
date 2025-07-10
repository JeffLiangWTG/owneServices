using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID("InvestigationItem")]
	public class InvestigationItemCollection : BusinessObjectCollection<InvestigationItem>
	{
		public InvestigationItemCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public InvestigationItemCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}
	}
}
