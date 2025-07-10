using System;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Moq;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	public static class RetrieverTestHelper
	{
		public static IDisposable MockProductRegistration(string databaseType, bool isInternalSystem = false, string hostedLocation = "SYD", string enterpriseCode = "EDI", string serverCode = "DAT")
		{
			var prodKeyMock = new Mock<IProductRegistrationKey>();
			var productRegistrationMock = new Mock<IProductRegistration>();
			productRegistrationMock.Setup(m => m.IsWiseTechGlobalInternalSystem()).Returns(isInternalSystem);
			productRegistrationMock.Setup(m => m.Key).Returns(prodKeyMock.Object);
			prodKeyMock.Setup(m => m.DatabaseType).Returns(databaseType);
			prodKeyMock.Setup(m => m.HostedLocation).Returns(hostedLocation);
			prodKeyMock.Setup(m => m.EnterpriseCode).Returns(enterpriseCode);
			prodKeyMock.Setup(m => m.ServerCode).Returns(serverCode);

			return ObjectFactory.Substitute(productRegistrationMock.Object);
		}
	}
}
