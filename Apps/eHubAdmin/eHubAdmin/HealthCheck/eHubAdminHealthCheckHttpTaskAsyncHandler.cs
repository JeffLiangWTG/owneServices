using CargoWise.eServices.Monitoring.HealthCheck.API;

namespace eServices.eHubAdmin.HealthCheck
{
    public class eHubAdminHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
    {

        public eHubAdminHealthCheckHttpTaskAsyncHandler()
           : base(new IHealthCheckItemProvider[] { new eHubAdminHealthCheckItemProvider() })
        { }
    }
}