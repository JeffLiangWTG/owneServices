using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.CW;

namespace Enterprise.ServiceManager.Business.Testing.ServiceProviderImpl
{
	class ServiceTaskRequirementCheckTimeoutExceptionTest : TestCase
	{
		public void TestCheckIfNewInformationIsCorrectForServiceTaskRequirementCheckTimeoutException()
		{
			// Assert
			const string taskCode = "xxx";
			var serviceTaskConfigMock = new Mock<IHostedServiceAttribute>();
			serviceTaskConfigMock.Setup(x => x.Code).Returns(taskCode);
			serviceTaskConfigMock.Setup(x => x.Description).Returns("TestTask");

			const string expectedMessage = @"Service task xxx (TestTask) timed out on the method1 requirement check (15s limit).";

			// Act
			var exception = new ServiceTaskRequirementCheckTimeoutException(serviceTaskConfigMock.Object, "method1", 15);

			// Assert
			AssertEquals(expectedMessage, exception.Message);
			AssertContains(taskCode, exception.Message);
		}
	}
}
