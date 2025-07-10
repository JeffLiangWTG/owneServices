using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing
{
	class AzureApplicationManagementCreator : IAzureApplicationManagementCreator
	{
		public AzureApplicationManagement CreateAzureApplicationManagement(string tenantId, string graphClientId)
		{
			return new AzureApplicationManagement(tenantId, graphClientId);
		}
	}
}
