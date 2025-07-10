using WTG.AzureApplicationIntegration;

namespace Enterprise.Client.EDI.ServiceTasks.GraphAPI
{
	public interface IGraphServiceFactory
	{
		GraphService CreateGraphService(string tenantId, string graphClientId);
	}
}
