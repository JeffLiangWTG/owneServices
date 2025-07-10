using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IWorkflowTypeProvider : IBusiness
	{
		bool IsTemplate { get; }
		ZString WorkflowProcessType { get; }
	}
}
