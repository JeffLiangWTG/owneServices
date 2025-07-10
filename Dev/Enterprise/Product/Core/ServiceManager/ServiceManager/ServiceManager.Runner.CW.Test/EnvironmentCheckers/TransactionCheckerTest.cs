using CargoWise.Data;
using Enterprise.ServiceManager.Runner;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.EnvironmentCheckers;

class TransactionCheckerTest : TestCase
{
	public void TestUncommittedTransactionException()
	{
		// Arrange
		const string taskCode = "xxx";
		var serviceTaskMock = new Mock<IServiceTaskHandler>();
		var serviceTaskConfigMock = new Mock<IHostedServiceAttribute>();
		serviceTaskConfigMock.Setup(x => x.Code).Returns(taskCode);
		serviceTaskConfigMock.Setup(x => x.TypeName).Returns("typeName1");
		serviceTaskConfigMock.Setup(x => x.TypeAssemblyName).Returns("assemblyName1");
		serviceTaskMock.Setup(x => x.HostedServiceAttribute).Returns(serviceTaskConfigMock.Object);

		// Act
		var exception = AssertExceptionThrown<UncommittedTransactionException>(() =>
		{
			using (Db.Connection.BeginTransactionWithManager())
			{
				new TransactionChecker().CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			}
		});

		// Assert
		AssertNotNull(exception);
		AssertEquals(true, exception.Message.Contains(exception.Message));
		AssertEquals(true, exception.Message.EndsWith("opened transaction(s) at the end of the run."));
	}
}
