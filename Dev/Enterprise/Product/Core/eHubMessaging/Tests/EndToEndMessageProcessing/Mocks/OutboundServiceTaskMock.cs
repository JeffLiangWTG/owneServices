using System.Collections.Generic;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;

namespace Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks
{
	internal class OutboundServiceTaskMock : eHubOutboundServiceTask
	{
		public OutboundServiceTaskMock(ICompanySettingsManager companySettingManager, IAdaptorFactory adaptorFactory, long maxReceivedMessageSize = -1, int outboxSizeLimitInBytes = -1)
		{
			this.companySettingManager = companySettingManager;
			this.adaptorFactory = adaptorFactory;
			this.maxReceivedMessageSize = maxReceivedMessageSize;
			this.outboxSizeLimitInBytes = outboxSizeLimitInBytes;
		}

		public eHubOutboundServiceTaskJobMock OutboundJob { get; private set; }

		public override ICompanySettingsManager CompanySettingsManager
		{
			get { return companySettingManager; }
		}
		readonly ICompanySettingsManager companySettingManager;
		readonly IAdaptorFactory adaptorFactory;
		readonly long maxReceivedMessageSize;
		readonly int outboxSizeLimitInBytes;

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			OutboundJob = new eHubOutboundServiceTaskJobMock(this, Notifier, adaptorFactory, maxReceivedMessageSize, outboxSizeLimitInBytes);
			yield return OutboundJob;
		}
	}
}
