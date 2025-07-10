using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business
{
	public interface ILogViewerDataProviderFactory
	{
		IEnumerable<ILogViewerDataProvider> GetProviders(string taskCode);
	}
}
