using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public interface IPropagationHandler
	{
		void Propagate(ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded);
		void UnPropagate(ProcessHandlingInfo processHandlingInfo, IStmALog logBeingRemoved);
	}
}
