using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public static class ProcessTaskHandlerProvider
	{
		public static IProcessTaskHandler GetHandler(IStmALogParent targetBusinessObject, IStmALog logBeingAdded)
		{
			return new ProcessTaskHandler(targetBusinessObject, logBeingAdded);
		}

		internal static IProcessTaskHandler GetHandler(ProcessHandlingInfo processHandlingInfo, StmALog logBeingAdded)
		{
			return new ProcessTaskHandler(processHandlingInfo, logBeingAdded, logBeingAdded.Master);
		}
	}
}
