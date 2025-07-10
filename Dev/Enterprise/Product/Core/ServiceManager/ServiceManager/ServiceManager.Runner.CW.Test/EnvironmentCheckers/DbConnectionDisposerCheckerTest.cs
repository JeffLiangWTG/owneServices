using System;
using System.Threading.Tasks;
using CargoWise.Data;
using Enterprise.ServiceManager.Runner;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.EnvironmentCheckers;

class DbConnectionDisposerCheckerTest : TestCase
{
	public void TestDbConnectionDisposerCorruptedException()
	{
		// Arrange
		const string taskCode = "xxx";
		Exception exception = null;
		var serviceTaskConfigMock = Mock.Of<IHostedServiceAttribute>(x =>
			x.Code == taskCode &&
			x.TypeName == "typeName1" &&
			x.TypeAssemblyName == "assemblyName1"
		);
		var serviceTaskMock = Mock.Of<IServiceTaskHandler>(x => x.HostedServiceAttribute == serviceTaskConfigMock);

		Task
			.Run(() =>
			{
				using var disposer = Db.DisposableActionForDbConnection();
				disposer.Dispose();

				// Act
				exception = AssertExceptionThrown<DbConnectionDisposerCorruptedException>(() =>
					new DbConnectionDisposerChecker().CheckOnServiceTaskCompletion(serviceTaskMock));
			})
			.Wait();

		// Assert
		AssertNotNull(exception);
		AssertEquals(true, exception.Message.Contains(taskCode));
		AssertEquals(true,
			exception.Message.EndsWith("has corrupted the Db Connection Disposer at the end of the run."));
	}
}
