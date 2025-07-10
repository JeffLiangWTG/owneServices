using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[ModuleID(ModuleId.ProcessHeader)]
	public class ProcessJobHeaderCollection : ActiveBusinessObjectCollection<ProcessJobHeader>
	{
		public ProcessJobHeaderCollection(BusinessObjectFactory factory)
			: base(factory, GetQuery())
		{
		}

		public ProcessJobHeaderCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, GetQuery(query))
		{
		}

		public ProcessJobHeaderCollection(BusinessObjectFactory factory, ZString workflowType)
			: base(factory, GetQuery())
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ProcessHeader.ModuleFilterConstants.JobOrWorkflow, "Property0", ZBool.True, isRemovable: false));
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ProcessHeader.ModuleFilterConstants.WorkflowType, "Property", workflowType, isRemovable: true));
		}

		static ZQuery GetQuery(ZQuery otherQuery = null)
		{
			var result = new ZQuery(ProcessHeaderSchema.FH_FH_ParentHeader, null);
			result.AddToFilter(ProcessHeaderSchema.FH_P0_Template, null);

			if (otherQuery != null)
			{
				result.AddToFilter(otherQuery);
			}

			return result;
		}
	}
}
