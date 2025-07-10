using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.BMFilterRule)]
	public class FilterRuleWorkflowCollection : ProcessHeaderCollection
	{
		public FilterRuleWorkflowCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}
	}
}
