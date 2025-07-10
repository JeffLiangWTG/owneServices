using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using static Enterprise.Billing.Business.UsageCollector;

namespace Enterprise.Billing.Business.Testing
{
	sealed class ProductionUsageCollectorTest : TestCaseWithFactory
	{
		public void TestNoFeaturesClashWithSTLCollectors()
		{
			var prodKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.Key).Returns(prodKeyMock.Object);
			prodKeyMock.Setup(m => m.DatabaseType).Returns(DatabaseTypes.Codes.Production);
			prodKeyMock.Setup(m => m.HostedLocation).Returns("SYD");

			using (ObjectFactory.Substitute(productRegistrationMock.Object))
			using (RawDataRegistry.Instance.ObtainDynamicSTLCollectorDefinitionsFromTheCode.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var items = ObjectFactory.Get<IScriptFactory>().CreateScripts(Factory);
				foreach (var item in items)
				{
					Assert($"An STL collector already exists with code {item.Code}, usage feature must be changed to a different code", !Features.ContainsKey(item.Code));
				}
			}
		}
	}
}
