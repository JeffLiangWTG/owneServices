using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[ModuleID("WorkItem")]
	public class NewWorkItemCollection : BusinessObjectCollection<NewWorkItem>
	{
		public NewWorkItemCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NewWorkItemCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}
	}
}

