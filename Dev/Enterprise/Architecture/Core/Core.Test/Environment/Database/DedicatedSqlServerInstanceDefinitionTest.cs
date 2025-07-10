using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DedicatedSqlServerInstanceDefinitionTest : TestCase
	{
		public void TestGetIsDedicatedServerInstanceWhenTrue()
		{
			// Arrange
			var productRetistrationMock = new Mock<IProductRegistration>();
			var productRetistrationKeyMock = new Mock<IProductRegistrationKey>();

			productRetistrationMock.SetupGet(registration => registration.Key).Returns(productRetistrationKeyMock.Object);
			productRetistrationKeyMock.SetupGet(key => key.EnterpriseCode).Returns("TTT");
			using (ObjectFactory.Substitute(productRetistrationMock.Object))
			{
				var dedicatedServerInstanceDefinition = new DedicatedSqlServerInstanceDefinition();

				// Act
				// Assert
				Assert("Server instance matching 'Instance<enterprise code>' pattern should be considered dedicated.", dedicatedServerInstanceDefinition.IsDedicated("InstanceTTT"));
			}
		}

		public void TestGetIsDedicatedServerInstanceWhenFalse()
		{
			// Arrange
			var productRetistrationMock = new Mock<IProductRegistration>();
			var productRetistrationKeyMock = new Mock<IProductRegistrationKey>();

			productRetistrationMock.SetupGet(registration => registration.Key).Returns(productRetistrationKeyMock.Object);
			productRetistrationKeyMock.SetupGet(key => key.EnterpriseCode).Returns("TTT");
			using (ObjectFactory.Substitute(productRetistrationMock.Object))
			{
				var dedicatedServerInstanceDefinition = new DedicatedSqlServerInstanceDefinition();

				// Act
				// Assert
				Assert("Server instance not matching 'Instance<enterprise code>' pattern should be considered dedicated.", !dedicatedServerInstanceDefinition.IsDedicated("InstanceRTT"));
			}
		}
	}
}
