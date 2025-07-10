using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public interface ITaskOrderable
	{
		bool IsCurrent { get; }
		string Status { get; }
		ZDecimal Nudge { get; }
		ZDateTime ReleaseDate { get; }
		int Sequence { get; }
		string TaskID { get; }

		IWorkflowOrderable WorkflowOrderable { get; }
	}
}
