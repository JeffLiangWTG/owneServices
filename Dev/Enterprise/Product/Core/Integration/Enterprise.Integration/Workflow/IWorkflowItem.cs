using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IWorkflowItem : IBusiness, IWorkflowTypeProvider
	{
		ZGuid ParentID { get; }
		ZString ParentTableCode { get; }
		ZGuid CompanyPK { get; }
		ZString Description { get; set; }
		ZString WorkflowItemType { get; }
		ZInt Sequence { get; }

		bool IsDeleted { get; }
	}
}
