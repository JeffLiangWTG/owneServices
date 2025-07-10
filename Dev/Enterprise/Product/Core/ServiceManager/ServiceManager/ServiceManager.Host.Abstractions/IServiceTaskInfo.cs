using System.Collections.Generic;
using ServiceManager.Integration.Abstractions;

namespace ServiceManager.Host.Abstractions
{
	public interface IServiceTaskInfo
	{
		public string Code { get; }
		public string Description { get; }
		public string Category { get; }
		public IEnumerable<string> BestResultsFromRequirementsCheck { get; }
		public IHostedServiceAttribute HostedServiceAttribute { get; }
		public bool ErrorOnLastRun { get; set; }
	}
}
