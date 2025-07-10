using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class UncommittedTransactionExceptionTest : EnvironmentCorruptedException<UncommittedTransactionException>
	{
		[Test]
		public void TestTaskCodeAndMessage()
		{
			// Arrange
			const string taskCode = "xxx";
			var serviceTaskConfigMock = new Mock<IHostedServiceAttribute>();
			serviceTaskConfigMock.Setup(x => x.Code).Returns(taskCode);
			serviceTaskConfigMock.Setup(x => x.TypeName).Returns("typeName1");
			serviceTaskConfigMock.Setup(x => x.TypeAssemblyName).Returns("assemblyName1");

			const string expectedMessage = @"The service task code: 'xxx - typeName1, assemblyName1' has left [1] opened transaction(s) at the end of the run.";

			// Act
			var exception = new UncommittedTransactionException(serviceTaskConfigMock.Object, 1);

			// Assert
			Assert.That(exception.Message, Is.EqualTo(expectedMessage));
			Assert.That(exception.Message, Contains.Substring(taskCode));
		}
	}
}
