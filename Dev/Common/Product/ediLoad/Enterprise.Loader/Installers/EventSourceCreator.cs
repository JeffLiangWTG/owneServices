using System.Diagnostics;
using CargoWise.ApplicationManager.Common;
using CargoWise.BrandManager;
using CargoWise.Loader.Common;

namespace Enterprise.Loader
{
	class EventSourceCreator : AppManagerInvoker, IAppManagerInvocable
	{
		EventSourceCreator() : base(null)
		{
		}

		public EventSourceCreator(Installation installation) : base(installation)
		{
		}

		public AppManagerResult Invoke(bool waitedForMutex, object state)
		{
			var sourceName = (string)state;
			if (!EventLog.SourceExists(sourceName))
			{
				EventLog.CreateEventSource(sourceName, "");
			}
			return new AppManagerResult(AppManagerResultStatus.Success);
		}

		protected virtual string GetEventSourceName()
		{
			return string.IsNullOrWhiteSpace(BrandingFactory.Instance.ProductName)
				? "The current application"
				: BrandingFactory.Instance.ProductName;
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			return InvokeAppManager<EventSourceCreator>(GetEventSourceName(), null);
		}
	}
}
