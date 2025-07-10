using Enterprise.ServiceManager.Shared;

namespace Enterprise.ServiceManager.Business
{
	public interface ISearchBasedLogViewerDataProvider
	{
		byte[] GetBytes(ServiceTaskLogFilters filters);
	}
}
