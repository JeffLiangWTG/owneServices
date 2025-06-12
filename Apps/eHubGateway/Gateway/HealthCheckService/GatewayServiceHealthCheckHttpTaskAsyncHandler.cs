using CargoWise.eHub.Gateway.HealthCheckService;
using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace CargoWise.eHub.Gateway.HealthCheck
{
	public class GatewayServiceHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
	{
		public GatewayServiceHealthCheckHttpTaskAsyncHandler() 
            : base(new IHealthCheckItemProvider[] {new GatewayServiceHealthCheckItemProvider()})
		{
		}
	}
}