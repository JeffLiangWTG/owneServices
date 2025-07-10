using CargoWise.Application;
using Enterprise.BufferManagement.Service.Shared;

namespace Enterprise.BufferManagement.Service.Client
{
	public class PAVEServiceClientFactory : IPAVEServiceClientFactory
	{
		public ISchematicService GetSchematicServiceClient() => new SchematicServiceClient();
		public ICapabilityTaskAutoAssignmentService GetCapabilityTaskAutoAssignmentServiceClient() => new CapabilityTaskAutoAssignmentServiceClient();

		internal static IPAVEHttpClient GetHttpClientFactory()
		{
			return ObjectFactory.Get<IPAVEHttpClientFactory>().Create();
		}
	}
}
