using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.ServiceManager.Host;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NLog;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using WTG.AppDomainWrappers.Net;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ServiceManager.Host.CW1.Test.EndToEndTests
{
	class ControllerServiceTest : TestCase
	{
		[TestRequiresAdministrativePrivileges("Start of the system service")]
		[RequiresSoftware(RequiredSoftware.IsVM)]
		[UseSnapshotProtection]
		public void TestApplicationOnUpgradedDatabaseCanReachUpgraderCall()
		{
			// Arrange
			UpgradeDatabase();
			SetCorrectTcpIpRegistrySettingsToAvoidAdditionalLogging();

			var domainData = new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName },
			};

			using (var appDomainWrapper = new AppDomainWrapper(nameof(TestApplicationOnUpgradedDatabaseCanReachUpgraderCall), true))
			{
				AnonymousMethod test = () =>
					{
						appDomainWrapper.RunActionInAppDomain(() =>
						{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
							Db.InitializeDatabaseDetails(
								(string)AppDomain.CurrentDomain.GetData("ServerName"),
								(string)AppDomain.CurrentDomain.GetData("DatabaseName"));
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

							var stopwatchMock = new Mock<Common.Abstractions.IStopwatch>();
							var errorReporterMock = new Mock<IErrorReporter>();

							var emergencyStopMock = new Mock<IApplicationEmergencyExit>();

							var logs = new List<string>();
							var hostLoggerMock = new Mock<IHostLogger>();
							hostLoggerMock
								.Setup(o => o.Log(It.IsAny<LogLevel>(), It.IsAny<string>()))
								.Callback<LogLevel, string>((logType, message) => logs.Add(message));

							EnterpriseApplicationConfiguration.ConfigureObjectFactory();

							Db.DisableSchemaVersionCheck();

							using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
							using (new DisposableAction(LogManager.Shutdown))
							using (var controllerService = CreateControllerService(emergencyStopMock.Object, hostLoggerMock.Object))
							{
								// Act
								controllerService.ConsoleRun();

								AssertEquals(3, logs.Count);
								AssertContains("OnStart", logs[0]);
								Assert(logs.Any(o => o.Contains("Version")));
								Assert(logs.Any(o => o.Contains("OnShutdown")));
							}

							emergencyStopMock.VerifyNoOtherCalls();
						}, domainData);
					};

				AssertNoExceptionThrown(test);
			}
		}

		static void UpgradeDatabase()
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.ExecuteNonQuery(@"
UPDATE
	dbo.StmData
SET
	SD_BinaryValue = CONVERT(varbinary(max), CONVERT(nvarchar(max), CONVERT(int, CONVERT(nvarchar(max), SD_BinaryValue)) + 10)),
	SD_SystemLastEditTimeUtc = GetUtcDate(),
	SD_SystemLastEditUser = '~BP'
WHERE
	SD_Name = @name;
",
					command => command.AddParameterBasedOnDbColumn("@name", "DATABASE_SCHEMA_VERSION", StmDataSchema.SD_Name));
			}
		}

		static void SetCorrectTcpIpRegistrySettingsToAvoidAdditionalLogging()
		{
			new TcpIpRegistryAdjuster(new WindowsRegistryAdapter()).Adjust();
		}

		static ControllerService CreateControllerService(IApplicationEmergencyExit applicationEmergencyExit, IHostLogger hostLogger)
		{
			return new ServiceCollection()
				.AddRegistrations(new[] { Db.ServerName, Db.DatabaseName, })
				.RemoveAll<IApplicationEmergencyExit>()
				.AddTransient(_ => applicationEmergencyExit)
				.RemoveAll<IHostLogger>()
				.AddTransient(_ => hostLogger)
				.BuildServiceProvider()
				.GetRequiredService<ControllerService>();
		}
	}
}
