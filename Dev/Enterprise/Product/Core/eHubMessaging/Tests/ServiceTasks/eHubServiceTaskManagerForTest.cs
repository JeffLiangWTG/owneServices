using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks
{
	#region Test Service Tasks and Managers
	class InboundServiceTaskForTest : InboundServiceTask
	{
		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new InboundServiceTaskJobForTest(this, Notifier, AdaptorFactory);
		}
	}

	class eHubOutboundServiceTaskForTest : eHubOutboundServiceTask
	{
		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new eHubOutboundServiceTaskJobForTest(this, Notifier, AdaptorFactory) { ProcessSystemInterchanges = true };
			yield return new eHubOutboundServiceTaskJobForTest(this, Notifier, AdaptorFactory) { ProcessSystemInterchanges = false };
		}
	}

	class eAdaptorOutboundServiceTaskForTest : eAdaptorOutboundServiceTask
	{
		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			yield return new eAdaptorOutboundServiceTaskJobForTest(this, Notifier, AdaptorFactory);
		}
	}

	class eHubOutboundServiceTaskJobForTest : eHubOutboundServiceTaskJob
	{
		public eHubOutboundServiceTaskJobForTest(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		protected override bool IsDeveloperSystem()
		{
			return false;
		}
	}

	class InboundServiceTaskJobForTest : InboundServiceTaskJob
	{
		public InboundServiceTaskJobForTest(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		protected override bool IsDeveloperSystem()
		{
			return false;
		}
	}

	class eAdaptorOutboundServiceTaskJobForTest : eAdaptorOutboundServiceTaskJob
	{
		public eAdaptorOutboundServiceTaskJobForTest(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		protected override bool IsDeveloperSystem()
		{
			return false;
		}
	}
	#endregion
}
