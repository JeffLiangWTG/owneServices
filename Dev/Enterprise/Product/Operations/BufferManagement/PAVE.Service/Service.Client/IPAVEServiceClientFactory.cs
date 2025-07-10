using Enterprise.BufferManagement.Service.Shared;

namespace Enterprise.BufferManagement.Service.Client
{
	public interface IPAVEServiceClientFactory
	{
		ISchematicService GetSchematicServiceClient();
		ICapabilityTaskAutoAssignmentService GetCapabilityTaskAutoAssignmentServiceClient();
	}
}
