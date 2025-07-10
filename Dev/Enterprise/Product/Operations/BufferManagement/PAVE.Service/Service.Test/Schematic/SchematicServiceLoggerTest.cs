using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.BufferManagement.Service.Test
{
	public class SchematicServiceLoggerTest : TestCase
	{
		public void TestLog_ShouldAddInLogsAndCallLogger()
		{
			string logDBServer, logDBName, logProgramCode, logSuffix;
			logDBServer = logDBName = logProgramCode = logSuffix = null;

			loggerFactoryMock.Setup(lf => lf.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
				.Callback((string dbServer, string dbName, string programCode, string suffix) =>
					{
						logDBServer = dbServer;
						logDBName = dbName;
						logProgramCode = programCode;
						logSuffix = suffix;
					})
				.Returns(loggerMock.Object);

			var schematicServiceLogger = new SchematicServiceLogger("programCode", "suffix");

			AssertEquals(Db.ServerName, logDBServer);
			AssertEquals(Db.DatabaseName, logDBName);
			AssertEquals(Db.DatabaseName, logDBName);
			AssertEquals("programCode", logProgramCode);
			AssertEquals("suffix", logSuffix);

			schematicServiceLogger.Log(LogType.Information, "first log");

			Exception exception = null;
			AssertEquals(logs.First(), (LogType.Information, "first log", exception));
			AssertEquals(schematicServiceLogger.Logs.First(), $"{nameof(SchematicService)}|{LogType.Information}: first log");

			exception = new Exception("Oh no!");
			schematicServiceLogger.Log(LogType.Error, "last log", exception);

			AssertEquals(logs.Last(), (LogType.Error, "last log", exception));
			AssertEquals(schematicServiceLogger.Logs.Last(), $"{nameof(SchematicService)}|{LogType.Error}: last log Exception: Oh no!");
		}

		#region Setup

		Mock<ILogger> loggerMock;
		Mock<ILoggerFactory> loggerFactoryMock;
		readonly List<(LogType type, string message, Exception ex)> logs = new List<(LogType, string, Exception)>();

		protected override void SetUp()
		{
			base.SetUp();

			loggerMock = new Mock<ILogger>();

			loggerMock.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType type, string message) => logs.Add((type, message, null)));

			loggerMock.Setup(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()))
				.Callback((LogType type, string message, Exception ex) => logs.Add((type, message, ex)));

			loggerFactoryMock = new Mock<ILoggerFactory>();

			ObjectFactory.Substitute(loggerFactoryMock.Object);
		}

		#endregion
	}
}
