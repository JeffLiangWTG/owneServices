using System.Diagnostics;

namespace ServiceManager.Common.Abstractions
{
	public interface IProcessFactory
	{
		IProcess Create(ProcessStartInfo startInfo, bool enableRaisingEvents = false, ProcessPriorityClass priority = ProcessPriorityClass.Normal);
	}
}
