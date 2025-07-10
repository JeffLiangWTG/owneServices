using System.Linq;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessTaskStartabilityChecker : IProcessTaskStartabilityChecker
	{
		public bool IsTaskStartable(IProcessTask task)
		{
			return task.IsStartable();
		}

		public bool IsProcessHeaderBlocked(IProcessTask task)
		{
			var processHeader = task.GetProcessHeader();
			return processHeader != null && processHeader.HasOpenPrerequisites;
		}

		public bool IsProcessHeaderOnlyBlockedByOtherJob(IProcessTask task)
		{
			var processHeader = task.GetProcessHeader();
			if (processHeader == null)
			{
				return false;
			}
			else
			{
				var prereqHeaders = processHeader.GetPrerequisitesUpTheTree();
				return prereqHeaders.All(x => x.Parent != processHeader.Parent);
			}
		}
	}
}
