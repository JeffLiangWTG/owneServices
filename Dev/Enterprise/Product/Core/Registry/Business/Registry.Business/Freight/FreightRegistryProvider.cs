using Enterprise.Integration.Freight;

namespace Enterprise.Registry.Business
{
	public class FreightRegistryProvider : IFreightRegistryProvider
	{
		public bool EnableBoleroEBLIntegration => FreightDataRegistry.Instance.EnableBoleroEBLIntegration.Value.EnableEBLIntegration;
	}
}
