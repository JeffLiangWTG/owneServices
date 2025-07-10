using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.eHub.Adapter;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;

namespace Enterprise.eHubMessaging.Tests.EndToEndMessageProcessing.Mocks
{
	internal class InboundServiceTaskMock : InboundServiceTask
	{
		readonly IeHubMessage[] retrieveMessages;

		public InboundServiceTaskMock(ICompanySettingsManager companySettingsManager, IeHubMessage[] retrieveMessages)
		{
			this.companySettingsManager = companySettingsManager;
			this.retrieveMessages = Argument.NotNull(retrieveMessages, "retrieveMessages");
		}

		public InboundServiceTaskJobMock InboundJob { get; private set; }

		public override ICompanySettingsManager CompanySettingsManager
		{
			get { return companySettingsManager; }
		}
		readonly ICompanySettingsManager companySettingsManager;

		internal override IEnumerable<IeHubServiceTaskJob> GetJobs()
		{
			InboundJob = new InboundServiceTaskJobMock(this, Notifier, new AdaptorFactoryMockWithOneAdaptor(new EHubAdapterMock(retrieveMessages)));
			yield return InboundJob;
		}
	}
}
