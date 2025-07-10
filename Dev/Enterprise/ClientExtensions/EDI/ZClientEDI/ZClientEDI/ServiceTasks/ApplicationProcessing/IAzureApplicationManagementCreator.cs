using Enterprise.Client.EDI.ServiceTasks.AzureApplicationProcessing;

namespace Enterprise.Client.EDI.ServiceTasks.ApplicationProcessing
{
	interface IAzureApplicationManagementCreator
	{
		AzureApplicationManagement CreateAzureApplicationManagement(string tenantId, string graphClientId);
	}
}
