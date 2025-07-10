using System.Collections.Generic;

namespace Enterprise.ServiceManager.Shared
{
	public interface IProcessControllerLogger
	{
		List<string> GetFileNames(string serviceHost, string serviceTask);
		byte[] GetStream(string fileName, string serviceHost, string serviceTask);
		byte[] GetStream(ServiceTaskLogFilters filters, bool isFetchAll);
	}
}
