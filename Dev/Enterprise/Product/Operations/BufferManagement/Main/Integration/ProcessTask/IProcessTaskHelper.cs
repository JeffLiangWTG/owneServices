using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessTaskHelper
	{
		bool IsTaskStartable(IProcessTask task);

		bool CanDelete(IProcessTask task);
	}
}
