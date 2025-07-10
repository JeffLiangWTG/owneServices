using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID("Project")]
	public class ProjectCollection : BusinessObjectCollection<EDIProject>
	{
		public ProjectCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public ProjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}

