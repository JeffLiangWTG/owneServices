using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CRM.Common
{
	[ModuleID(ModuleId.CrmOpportunity)]
	public class CrmOpportunityCollection : ActiveBusinessObjectCollection<CrmOpportunity>
	{
		public CrmOpportunityCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}
}
