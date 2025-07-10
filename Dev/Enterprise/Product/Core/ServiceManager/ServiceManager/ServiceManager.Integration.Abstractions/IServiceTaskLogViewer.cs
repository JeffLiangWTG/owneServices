using System.Collections.Generic;

namespace ServiceManager.Integration.Abstractions
{
	public interface IServiceTaskLogViewer
	{
		string TaskType { get; set; }
		IEnumerable<ILogViewerDataProvider> HostLogProviderCollection { get; }
	}
}
