using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.ServiceTasks.IncidentLinkHealthCheck;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Test
{
	public class IncidentLinkHealthCheckRunnerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIncidentLinkHealthCheckRunnerInterfaceSuccessCase()
		{
			// Arrange
			var optionsMock = new Mock<IIncidentLinkHealthCheckRunner>();
			optionsMock.Setup(o => o.Run(It.IsAny<CancellationToken>()));
			// Act
			// Assert
			optionsMock.Object.Run(new CancellationToken());
		}

		[ExpectNoExceptions]
		public void IncidentLinkHealthCheckRunnerRunNullLoggerSuccessCase()
		{
			// Arrange
			var runner = new IncidentLinkHealthCheckRunner(null);
			// Act
			// Assert
			runner.Run(new CancellationToken());
		}

		[ExpectNoExceptions]
		public void IncidentLinkHealthCheckRunnerRunWithLoggerSuccessCase()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var debugLog = new List<string>();
			var warningLog = new List<string>();
			var infoLog = new List<string>();
			var errorLog = new List<string>();
			mockLogger.Setup(m => m.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string s) =>
				{
					switch (logType)
					{
						case LogType.Debug:
							debugLog.Add(s);
							break;
						case LogType.Warning:
							warningLog.Add(s);
							break;
						case LogType.Information:
							infoLog.Add(s);
							break;
						case LogType.Error:
							errorLog.Add(s);
							break;
					}
				});

			var runner = new IncidentLinkHealthCheckRunner(mockLogger.Object);

			// Act
			runner.Run(new CancellationToken());
			// Assert
			Assert("log info message are added", infoLog.Any());
		}
	}
}
