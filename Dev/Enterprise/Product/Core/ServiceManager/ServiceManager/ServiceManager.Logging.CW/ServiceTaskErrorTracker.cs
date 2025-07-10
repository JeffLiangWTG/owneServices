using System;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace ServiceManager.Logging.CW
{
	class ServiceTaskErrorTracker : IServiceTaskErrorTracker
	{
		public ServiceTaskErrorTracker()
			: this(new ServiceHostMessageDispatcher())
		{
		}

		internal ServiceTaskErrorTracker(IServiceHostMessageDispatcher serviceHostMessageDispatcher)
		{
			this.serviceHostMessageDispatcher = serviceHostMessageDispatcher ?? throw new ArgumentNullException(nameof(serviceHostMessageDispatcher));
		}

		public void TrackServiceTaskError(string taskCode)
		{
			if (!string.IsNullOrEmpty(taskCode))
			{
				serviceHostMessageDispatcher.SendErrorReport(taskCode);
			}
		}

		readonly IServiceHostMessageDispatcher serviceHostMessageDispatcher;
	}
}
