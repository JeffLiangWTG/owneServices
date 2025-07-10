using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;

namespace Enterprise.eHubMessaging.ServiceTasks.Outbound.Scavenging
{
	abstract class ScavengingSubmissionServiceTask : eHubServiceTaskWithAdaptor
	{
		public ScavengingSubmissionServiceTask()
			: base()
		{
		}

		protected ScavengingSubmissionServiceTask(IAdaptorFactory adaptorFactory, IEHubCommunicationDiagnosterFactory diagnosterFactory)
			: base(adaptorFactory, diagnosterFactory)
		{
		}

		protected bool SendInterchangesToTestGateway => ObjectFactory.Get<IProductRegistration>().IsWiseTechGlobalInternalSystem();

		public override string DefaultServerAddress
		{
			get
			{
				return SendInterchangesToTestGateway ? eHubMessagingRegistry.Instance.eHubTestGatewayServerAddressList.Value : eHubMessagingRegistry.Instance.eHubGatewayServerAddressList.Value;
			}
		}
	}
}
