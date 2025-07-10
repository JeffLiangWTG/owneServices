using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.Exceptions
{
	class UndisposedSqlLockExceptionTest : EnvironmentCorruptedException<UndisposedSqlLockException>
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

			var expectedMessage = $@"The service task code: 'xxx - typeName1, assemblyName1' has one or more undisposed SQL locks at the end of the run.{System.Environment.NewLine}Undisposed lock keys:{System.Environment.NewLine}Key1";

			// Act
			var exception = new UndisposedSqlLockException(serviceTaskConfigMock.Object, new List<string>() { "Key1" });

			// Assert
			Assert.That(exception.Message, Is.EqualTo(expectedMessage));
			Assert.That(exception.Message, Contains.Substring(taskCode));
		}
	}
}
