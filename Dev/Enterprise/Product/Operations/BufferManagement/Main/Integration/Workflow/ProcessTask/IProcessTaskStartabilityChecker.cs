using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTaskStartabilityChecker
	{
		bool IsTaskStartable(IProcessTask task);
		bool IsProcessHeaderBlocked(IProcessTask task);
		bool IsProcessHeaderOnlyBlockedByOtherJob(IProcessTask task);
	}
}
