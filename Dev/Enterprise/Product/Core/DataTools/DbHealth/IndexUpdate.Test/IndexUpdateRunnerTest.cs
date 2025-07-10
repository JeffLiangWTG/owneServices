using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbHealth.IndexUpdate.Testing
{
	[UseSnapshotProtection]
	sealed class IndexUpdateRunnerTest : TestCase
	{
		public void TestIgnoresBacklogWaiterTimeoutException()
		{
			var indexUpdateRunner = new IndexUpdateRunner();
			var loggerMock = new Mock<ILogger>();
			loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>())).Throws<BacklogWaiterTimeoutException>();
			AssertNoExceptionThrown(() =>
			{
				indexUpdateRunner.Run(Db.ServerName, Db.DatabaseName, loggerMock.Object);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.AtLeastOnce);
			});
		}
	}
}
