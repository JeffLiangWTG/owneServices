using CargoWise.ComponentModel;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks
{
	internal class eHubOutboundServiceTaskJobMock : eHubOutboundServiceTaskJob
	{
		public eHubOutboundServiceTaskJobMock(IeHubServiceTaskSupport serviceTaskSupport, INotifications notifier, IAdaptorFactory adaptorFactory, long maxReceivedMessageSize = -1, int outboxSizeLimitInBytes = -1)
			: base(serviceTaskSupport, notifier, adaptorFactory)
		{
			this.maxReceivedMessageSize = maxReceivedMessageSize == -1 ? base.GatewayMaxReceivedMessageLimitInBytes : maxReceivedMessageSize;
			this.outboxSizeLimitInBytes = outboxSizeLimitInBytes == -1 ? base.AdapterOutboxSizeLimitInBytes : outboxSizeLimitInBytes;
		}

		protected override bool CompanyShouldBeServiced(GlbCompany company)
		{
			return true;
		}

		readonly long maxReceivedMessageSize;
		readonly int outboxSizeLimitInBytes;

		internal override long GatewayMaxReceivedMessageLimitInBytes => maxReceivedMessageSize;
		internal override int AdapterOutboxSizeLimitInBytes => outboxSizeLimitInBytes;
	}
}
