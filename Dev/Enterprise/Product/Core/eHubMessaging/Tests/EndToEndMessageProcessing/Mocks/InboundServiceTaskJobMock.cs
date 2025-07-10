using CargoWise.ComponentModel;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks
{
	internal class InboundServiceTaskJobMock : InboundServiceTaskJob
	{
		internal InboundServiceTaskJobMock(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
		}

		protected override bool CompanyShouldBeServiced(GlbCompany company)
		{
			return true;
		}
	}
}
