using eServices.eHubAdmin.HealthCheck;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using CargoWise.eServices.Monitoring.HealthCheck.API.Test;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eServices.eHubAdmin.Tests.HealthCheck
{
    class eHubAdminHealthCheckItemProviderTest : HealthCheckItemProviderBaseClass<eHubAdminHealthCheckItemProvider>
    {
        [Test]
        public async Task TesteHubAdminHealthCheckItemProvider()
        {
            var mockProvider = new Mock<eHubAdminHealthCheckItemProvider> { CallBase = true };
            var provider = mockProvider.Object;

            Assert.AreEqual(provider.Name, "eHubAdminServices");

            mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns((string)null);
            var checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);

            Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.Error));
            Assert.That(checkItem.Description, Is.EqualTo("Authorisation Token Expiration Minute not found in AppSettings in Web.config."));

            mockProvider.Setup<string>(_ => _.GetAppSettings(It.IsAny<string>())).Returns("10");
            checkItem = await provider.CheckHealthAsync().ConfigureAwait(false);

            Assert.That(checkItem.Status, Is.EqualTo(HealthCheckStatus.OK));
            Assert.That(checkItem.Description, Is.EqualTo("Service is alive."));

        }

        protected override List<string> GetAllKindsOfDescriptions()
        {
            var descriptions = new List<string>();

            var mockProvider = new Mock<eHubAdminHealthCheckItemProvider> { CallBase = true };
            var provider = mockProvider.Object;

            var checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);
            checkItem = provider.CheckHealthAsync().Result;
            descriptions.Add(checkItem.Description);

            return descriptions;
        }
    }
}
