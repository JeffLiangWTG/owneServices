using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using Moq;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NLog.Targets.Wrappers;
using NLog.Time;
using NUnit.Framework;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Logging.CW;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace ServiceManager.Shared.CW.Test.Logging
{
	sealed class LoggerNLogWrapperTest : TestCase
	{
		public class LoggingTest : TransactionedTestCase
		{
			public void TestNLog()
			{
				AssertNLog(TestProgramCode);
			}

			public void TestNLogHost()
			{
				AssertNLog(ServiceManagerHelper.HostLoggerCode);
			}

			static void AssertNLog(string programCode)
			{
				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				{
					//Arrange
					var loggingConfiguration = new LoggingConfiguration();

					TimeSource.Current = new FastUtcTimeSource();

					var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");

					using var fileTarget = new FileTarget(nameof(FileTarget));
					fileTarget.FileName = outputDir + "\\${logger}.txt";
					fileTarget.Layout = "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff:padding=-25}${event-properties:severity:padding=-25}${event-properties:process:objectpath=pid:padding=-25}${message}";
					fileTarget.KeepFileOpen = false;

					loggingConfiguration.AddTarget("logfile", fileTarget);

					var logFileLoggingRule = new LoggingRule("*", NLog.LogLevel.Debug, fileTarget) { RuleName = TestProgramCode };
					loggingConfiguration.LoggingRules.Add(logFileLoggingRule);

					LogManager.Configuration = loggingConfiguration;

					LogManager.ReconfigExistingLoggers();

					Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

					AssertEquals("Output directory not created", false, Directory.Exists(outputDir));

					// Act
					var errorTrackerMock = new Mock<IServiceTaskErrorTracker>();
					using (ObjectFactory.Substitute(errorTrackerMock.Object))
					{
						var testLogger = new LoggerNLogWrapper("", "", programCode);
						testLogger.Log(LogLevel.Error, "An Error: Line 1\r\nLine 2\r\nLine 3");
						AssertEquals("Output directory created", true, Directory.Exists(outputDir));

						var logFile = Path.Combine(outputDir, programCode + ".txt");
						AssertEquals("Log file created", true, File.Exists(logFile));

						// Assert Number of Log Lines
						var fileContents = File.ReadAllText(logFile).TrimEnd();
						var logs = fileContents.Split('\n');
						AssertEquals("Number of log lines (records)", 1, logs.Length);

						// Assert Log Contents
						var logDateTimeStamp = logs[0].Substring(0, 25).Trim();
						var dateTime = new ZDateTime(logDateTimeStamp);
						AssertNotNull(dateTime);
						var logLevel = logs[0].Substring(25, 25).Trim();
						AssertEquals("Log Level", nameof(LogLevel.Error), logLevel);
						var processId = logs[0].Substring(50, 25).Trim();
						int procId;
						Assert(int.TryParse(processId, out procId));
						var logMessage = logs[0].Substring(75).Trim();
						AssertEquals("Log Message", "An Error: Line 1\u21B5Line 2\u21B5Line 3", logMessage);

						testLogger.Log(LogLevel.Information, "An Info: Line 1\r\nLine 2");

						// Assert Number of Log Lines
						fileContents = File.ReadAllText(logFile).TrimEnd();
						logs = fileContents.Split('\n');
						AssertEquals("Number of log lines (records)", 2, logs.Length);

						// Assert 2nd Log Contents
						logDateTimeStamp = logs[1].Substring(0, 25).Trim();
						dateTime = new ZDateTime(logDateTimeStamp);
						AssertNotNull(dateTime);
						logLevel = logs[1].Substring(25, 25).Trim();
						AssertEquals("Log 2 Level", nameof(LogLevel.Information), logLevel);
						processId = logs[1].Substring(50, 25).Trim();
						Assert(int.TryParse(processId, out procId));
						logMessage = logs[1].Substring(75).Trim();
						AssertEquals("Log 2 Message", "An Info: Line 1\u21B5Line 2", logMessage);

						if (programCode.Equals(ServiceManagerHelper.HostLoggerCode, StringComparison.InvariantCultureIgnoreCase))
						{
							errorTrackerMock.Verify(et => et.TrackServiceTaskError(programCode), Times.Never);
						}
						else
						{
							errorTrackerMock.Verify(et => et.TrackServiceTaskError(programCode), Times.Once);
						}
					}
				}
			}

			public void TestLogConstructorChecksDbVersion()
			{
				// Arrange
				var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
				var versionMock = new Mock<IDatabaseAspectVersions>();
				versionMock
					.Setup(x => x.SchemaVersion)
					.Returns(new VersionLabel(bumpedSchemaVersion, 0));

				using (ObjectFactory.Substitute(versionMock.Object))
				{
					// Act
					var actualException = Task.Run(() =>
					{
						try
						{
							new LoggerNLogWrapper("", "", TestProgramCode);
							return null;
						}
						catch (Exception ex)
						{
							return ex;
						}
					}).Result;

					// Assert
					Assert("should throw DatabaseUpgradedException", actualException is DatabaseUpgradedException);
				}
			}

			[ExpectNoExceptions]
			public void TestLoggingDoesNotCheckVersion()
			{
				// Arrange
				var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
				var versionMock = new Mock<IDatabaseAspectVersions>();
				versionMock
					.Setup(x => x.SchemaVersion)
					.Returns(new VersionLabel(bumpedSchemaVersion, 0));

				var errorReporterMock = new Mock<IErrorReporter>();
				var loggingConfiguration = new LoggingConfiguration();
				using var memoryTarget = new MemoryTarget() { Name = $"logfile{TestProgramCode}" };
				loggingConfiguration.AddTarget(memoryTarget.Name, memoryTarget);
				var logFileLoggingRule = new LoggingRule("*", NLog.LogLevel.Debug, memoryTarget) { RuleName = TestProgramCode };
				loggingConfiguration.LoggingRules.Add(logFileLoggingRule);

				LogManager.Configuration = loggingConfiguration;
				LogManager.ReconfigExistingLoggers();

				var logger = new LoggerNLogWrapper("", "", TestProgramCode);

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				using (ObjectFactory.Substitute(versionMock.Object))
				using (ObjectFactory.Substitute(Mock.Of<IServiceTaskErrorTracker>()))
				{
					//Act
					// Assert
					Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							// Force a db hit when getting current time
							TimeFactory.ResetInstance_ForTest();

							using (var conn = Db.NewExtraConnectionToMainDb())
							{
								AssertExceptionThrown<DatabaseUpgradeException>(() => conn.ExecuteScalar("Select Null"));
							}

							AssertNoExceptionThrown(() => logger.Log(LogLevel.Information, "Log first time"));
							AssertNoExceptionThrown(() => logger.Log(LogLevel.Information, "Log second time"));
							AssertNoExceptionThrown(() => logger.Log(LogLevel.Information, "Log third time"));
							errorReporterMock.VerifyNoOtherCalls();
						}
					}).Wait();
				}
			}

			public void TestLog_WhenCriticalExceptionIsThrown_ExceptionIsNotCaught()
			{
				// Arrange
				ObjectFactory.Substitute(Mock.Of<IServiceTaskErrorTracker>());
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());

				var loggingConfiguration = new LoggingConfiguration();
				using var memoryTarget = new MemoryTarget() { Name = $"logfile{TestProgramCode}" };
				loggingConfiguration.AddTarget(memoryTarget.Name, memoryTarget);
				var logFileLoggingRule = new LoggingRule("*", NLog.LogLevel.Debug, memoryTarget) { RuleName = TestProgramCode };
				loggingConfiguration.LoggingRules.Add(logFileLoggingRule);

				LogManager.Configuration = loggingConfiguration;
				LogManager.ReconfigExistingLoggers();

				ThrowExceptionWhenLogging<OutOfMemoryException>();
				ThrowExceptionWhenLogging<AppDomainUnloadedException>();

				void ThrowExceptionWhenLogging<T>() where T : Exception
				{
					var logger = new LoggerNLogWrapperForTest("", "", TestProgramCode, "", configurationFactoryMock.Object, false);
					logger.ExceptionToThrowFromGetCurrentUtcDateTime = Activator.CreateInstance<T>();

					//Act
					//Assert
					AssertExceptionThrown<T>(() => logger.Log(LogLevel.Information, ""));
				}
			}

			[TestDate(2007, 6, 19, 8, 38, 10)]
			public void TestNLogCurrentOutputFileForTest()
			{
				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				{
					try
					{
						//Arrange
						var loggingConfiguration = new LoggingConfiguration();

						TimeSource.Current = new FastUtcTimeSource();

						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");

						using var fileTarget = new FileTarget
						{
							FileName = outputDir + "\\${logger}.txt",
							//Layout = LoggerNLogWrapper.DefaultLayoutFormatter,
							Name = "logfile" + TestProgramCode,
							KeepFileOpen = false,
						};

						loggingConfiguration.AddTarget(fileTarget.Name, fileTarget);

						var logFileLoggingRule = new LoggingRule("*", NLog.LogLevel.Debug, fileTarget) { RuleName = TestProgramCode };
						loggingConfiguration.LoggingRules.Add(logFileLoggingRule);

						LogManager.Configuration = loggingConfiguration;

						LogManager.ReconfigExistingLoggers();

						Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));
						var testNLogWrapper = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, new LoggerNLogConfigurationFactory(), true);

						AssertEquals("Output directory not created", false, Directory.Exists(outputDir));

						// Act
						testNLogWrapper.Log(LogLevel.Error, "An Error: Line 1\r\nLine 2\r\nLine 3");
						AssertEquals("Output directory created", true, Directory.Exists(outputDir));

						var logFile = Path.Combine(outputDir, TestProgramCode + ".txt");
						AssertEquals("NLog attribute file should be equal to Log file created", logFile, testNLogWrapper.CurrentOutputFileForTest(TestProgramCode).ToString());
						AssertEquals("Log file created", true, File.Exists(logFile));
					}
					finally
					{
						LogManager.Shutdown();
					}
				}
			}

			public void TestLoggingCallsConfigurationRefreshOnEachLog()
			{
				CombineAssertions(() =>
				{
					foreach (var value in Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>())
					{
						Test(value);
					}
				});

				void Test(LogLevel logLevel)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
						loggerNLogConfigurationMock
							.SetupGet(configuration => configuration.VerboseLogging)
							.Returns(new VerboseLoggingCollection());
						var logger = new LoggerNLogWrapper(
							Db.ServerName,
							Db.DatabaseName,
							"TST",
							tempDir.DirectoryName,
							Mock.Of<ILoggerNLogConfigurationFactory>(
								factory => factory.GetConfiguration(It.IsAny<string>()) == loggerNLogConfigurationMock.Object),
							false,
							false);
						const int callCount = 10;

						// Act
						for (var i = 0; i < callCount; i++)
						{
							logger.Log(logLevel, $"{logLevel}:{i}");
						}

						// Assert
						AssertNoExceptionThrown(() => loggerNLogConfigurationMock.Verify(configuration => configuration.Refresh(), Times.Exactly(callCount)));
					}
				}
			}

			[ExpectNoExceptions]
			public void TestNLogElasticsearchLogging()
			{
				// Arrange

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
					Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

					var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true, }, };
					newValue.SetDefaultCode(LoggingMethods.ELK, true);
					SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
					SystemDataRegistry.Instance.ElasticsearchServerUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "username");
					SystemDataRegistry.Instance.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "index-test");
					SystemDataRegistry.Instance.ElasticsearchServerPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
					SystemDataRegistry.Instance.ElasticsearchServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://testuri");

					AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());

					// Act
					_ = new LoggerForTesting(outputDir);

					// Assert
					AssertEquals("NLog elasticsearch target should exist", true, LogManager.Configuration.AllTargets.Any(t => t.Name == "elasticsearch"));
					Assert("Output directory should NOT exist", !Directory.Exists(outputDir));
				}

				AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());
			}

			[ExpectNoExceptions]
			public void TestNLogKafkaLogging()
			{
				// Arrange

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
					Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

					var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true, }, };
					newValue.SetDefaultCode(LoggingMethods.KAF, true);
					SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
					SystemDataRegistry.Instance.KafkaTopic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "testTopic");
					SystemDataRegistry.Instance.ProcessControllerKafkaSecurity.SetValue(
						Guid.Empty,
						Guid.Empty,
						Guid.Empty,
						new KafkaSecurity() { SecurityProtocol = KafkaSecurityProtocolOptions.PLAINTEXT });

					AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());

					// Act
					_ = new LoggerForTesting(outputDir);

					// Assert
					AssertEquals("NLog kafka target should exist", true, LogManager.Configuration.AllTargets.Any(t => t.Name == "kafka"));
					Assert("Output directory should NOT exist", !Directory.Exists(outputDir));
				}

				AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());
			}

			[ExpectNoExceptions]
			public void TestNLogSyslogLogging()
			{
				// Arrange

				try
				{
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true, }, new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, SystemDefined = true, }, };
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());

						// Act
						_ = new LoggerForTesting(outputDir);

						// Assert
						AssertEquals("NLog Syslog target should exist", true, LogManager.Configuration.AllTargets.Any(t => t.Name == "syslog"));
						Assert("Output directory should NOT exist", !Directory.Exists(outputDir));
					}

					AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());
				}
				finally
				{
					// Teardown
					InternalLogger.Reset();
					LogManager.LogFactory.Dispose();
					LogManager.ReconfigExistingLoggers(purgeObsoleteLoggers: true);
					LogManager.Flush();
					LogManager.Shutdown();
					AsyncHelper.WaitAllActiveTasksForTest();
				}
			}

			[ExpectNoExceptions]
			public void TestNLogCombinedFileLogging()
			{
				// Arrange

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
					Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

					var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true, }, new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, SystemDefined = true, }, };
					newValue.SetDefaultCode(LoggingMethods.FSL, true);
					SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

					AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());

					// Act
					_ = new LoggerForTesting(outputDir);

					// Assert
					AssertEquals("NLog Combined File target should exist", true, LogManager.Configuration.AllTargets.Any(t => t.Name == "combinedFile"));
					Assert("Output directory should NOT exist", !Directory.Exists(outputDir));
				}

				AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());
			}

			public void TestNLogAllLogTargets()
			{
				// Arrange

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
					Assert("[PRE-CONDITION] Output directory should NOT exist", !Directory.Exists(outputDir));

					var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true, },
						new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true, },
						new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true, },
						new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true, },
						new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true, },
					};
					newValue.SetDefaultCode(LoggingMethods.FSL, true);
					SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
					SystemDataRegistry.Instance.ElasticsearchServerUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "username");
					SystemDataRegistry.Instance.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "index-test");
					SystemDataRegistry.Instance.ElasticsearchServerPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "password");
					SystemDataRegistry.Instance.ElasticsearchServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://testuri");
					SystemDataRegistry.Instance.KafkaTopic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "testTopic");
					SystemDataRegistry.Instance.ProcessControllerKafkaSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new KafkaSecurity { SecurityProtocol = KafkaSecurityProtocolOptions.PLAINTEXT });
					AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());

					// Act
					_ = new LoggerForTesting(outputDir);

					// Assert
					AssertEquals("NLog elasticsearch target should exist", 2, LogManager.Configuration.AllTargets.Count(t => t.Name == "elasticsearch"));
					AssertEquals("NLog kafka target should exist", 2, LogManager.Configuration.AllTargets.Count(t => t.Name == "kafka"));
					AssertEquals("NLog Syslog target should exist", 2, LogManager.Configuration.AllTargets.Count(t => t.Name == "syslog"));
					AssertEquals("NLog file target should exist", 1, LogManager.Configuration.AllTargets.Count(t => t.Name?.StartsWith("logfile") ?? false));
				}

				AssertEquals("NLog should NOT have any configuration target", false, LogManager.Configuration.AllTargets.Any());
				AsyncHelper.WaitAllActiveTasksForTest();
			}

			[UseSnapshotProtection]
			public void TestLoggingIsUsedOnUpgradedDatabase_WhenThreadSchemaVersionCheckDisabled()
			{
				// Arrange
				var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
				var versionMock = new Mock<IDatabaseAspectVersions>();
				versionMock
					.Setup(x => x.SchemaVersion)
					.Returns(new VersionLabel(bumpedSchemaVersion, 0));
				const string logMessage = "Something to log about";
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.SetupGet(configuration => configuration.InternalNLogLoggingEnabled)
					.Returns(true);

				Exception resultException = null;
				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, "TestLoggingOnUpgradedDatabase");

					var someOtherThreadUsingLogger = new Thread(() =>
					{
						Db.DisableThreadSchemaVersionCheckPermanently();

						using (new NLogConfigurationForTest())
						using (Db.DisposableActionForDbConnection())
						using (ObjectFactory.Substitute(versionMock.Object))
						{
							try
							{
								// Act
								var logger = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false);
								logger.Log(LogLevel.Information, logMessage);
							}
							catch (Exception exception)
							{
								resultException = exception;
							}
						}
					});
					someOtherThreadUsingLogger.Start();
					someOtherThreadUsingLogger.Join();

					string resultMessage = null;
					if (Directory.Exists(outputDir))
					{
						var file = Directory.EnumerateFiles(outputDir, $"{TestProgramCode}_*.txt").FirstOrDefault();
						if (file != null)
						{
							resultMessage = File.ReadAllLines(file).SingleOrDefault();
						}
					}

					// Assert
					CombineAssertions(() =>
					{
						AssertNull("Should be no Exception", resultException);
						AssertContains(logMessage, resultMessage);
					});
				}
			}

			public void TestParameterArchiveLogFilesCanDisableFileTargetArchiving()
			{
				// Arrange
				var path = CommonProgramData.GetCargoWiseDirectory("Process Controller", System.Environment.MachineName, Db.DatabaseName);
				using (new NLogConfigurationForTest())
				{
					var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true, }, };
					newValue.SetDefaultCode(LoggingMethods.FSL, true);
					SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

					// Act
					_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "ArchiveParameters", path, new LoggerNLogConfigurationFactory(), false);

					// Assert
					var logWrapperForRunner = LogManager.Configuration.AllTargets.OfType<FileTarget>().Single();
					AssertEquals(-1, logWrapperForRunner.ArchiveAboveSize);
				}
			}

			public void TestParameterArchiveLogFilesCanEnableFileTargetArchiving()
			{
				// Arrange
				var path = CommonProgramData.GetCargoWiseDirectory("Process Controller", System.Environment.MachineName, Db.DatabaseName);
				using (new NLogConfigurationForTest())
				{
					var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true, }, };
					newValue.SetDefaultCode(LoggingMethods.FSL, true);
					SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

					// Act
					_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "ArchiveParametersShouldBeSet", path, new LoggerNLogConfigurationFactory(), true);

					// Assert
					var logWrapperNotForRunner = LogManager.Configuration.AllTargets.OfType<FileTarget>().Single();
					CombineAssertions(() =>
					{
						AssertGreaterThan(logWrapperNotForRunner.ArchiveAboveSize, -1);
						AssertNotNull(logWrapperNotForRunner.ArchiveFileName);
						AssertNotNullOrEmpty(logWrapperNotForRunner.ArchiveDateFormat);
					});
				}
			}

			public void TestTryCreateTarget_ReturnsNullOnException()
			{
				// Arrange
				var mockTargetFactory = new Mock<INLogTargetFactory>();
				mockTargetFactory.SetupSequence(x => x.GetOrCreateTarget())
					.Throws(new InvalidOperationException("Timed out"))
					.Throws(new DatabaseUpgradedException());

				// Act
				var result1 = LoggerHelper.TryGetOrCreateTarget(mockTargetFactory.Object);
				var result2 = LoggerHelper.TryGetOrCreateTarget(mockTargetFactory.Object);

				AssertNull("InvalidOperationException is handled", result1);
				AssertNull("DatabaseUpgradedException is handled", result2);
			}

			public void TestTryGetOrCreateTargetDoesNotCatchDbUpgradeException()
			{
				// Arrange
				using (ObjectFactory.Substitute(Mock.Of<IDatabaseAspectVersions>(v => v.SchemaVersion == new VersionLabel(int.MaxValue, 0))))
				using (var connection = Db.NewExtraConnectionToMainDb())
				using (
					new DisposableAction(
						() => Db.ConnectionOverrideForTest = connection,
						() => Db.ConnectionOverrideForTest = null))
				{
					var factory = new NLogCombinedFileTargetFactory("TST");

					// Act
					var value = LoggerHelper.TryGetOrCreateTarget(factory);

					// Assert
					AssertEquals(nameof(Db.Connection.DatabaseUpgradedExceptionHasBeenThrown), false, Db.Connection.DatabaseUpgradedExceptionHasBeenThrown);
					AssertEquals(nameof(value), null, value);
				}
			}

			public void TestLogWithRefreshExceptionLogsException()
			{
				// Arrange
				var expectedMessage = nameof(TestLogWithRefreshExceptionLogsException);

				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.Setup(o => o.Refresh())
					.Throws(new Exception(expectedMessage));

				var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = TestProgramCode, Bool2 = true, SystemDefined = true, }, };
				loggingMethods.SetDefaultCode(TestProgramCode, true);

				using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, loggingMethods))
				using (new NLogConfigurationForTest())
				{
					using var memoryTarget = new MemoryTarget();
					LogManager.Configuration.AddTarget(TestProgramCode, memoryTarget);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule(TestProgramCode, NLog.LogLevel.Debug, memoryTarget) { RuleName = TestProgramCode });
					LogManager.ReconfigExistingLoggers();

					var logger = new LoggerNLogWrapper(string.Empty, string.Empty, TestProgramCode, string.Empty, configurationFactoryMock.Object, true);

					// Act
					logger.Log(LogLevel.Error, string.Empty);

					// Assert
					Assert(memoryTarget.Logs.Any(o => o.Contains(expectedMessage)));
				}
			}

			public void TestLogWithRefreshExceptionLogsOriginalMessage()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.Setup(o => o.Refresh())
					.Throws<Exception>();

				var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = TestProgramCode, Bool2 = true, SystemDefined = true, }, };
				loggingMethods.SetDefaultCode(TestProgramCode, true);

				using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, loggingMethods))
				using (new NLogConfigurationForTest())
				{
					using var memoryTarget = new MemoryTarget();
					LogManager.Configuration.AddTarget(TestProgramCode, memoryTarget);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule(TestProgramCode, NLog.LogLevel.Debug, memoryTarget) { RuleName = TestProgramCode });
					LogManager.ReconfigExistingLoggers();

					var expectedMessage = nameof(TestLogWithRefreshExceptionLogsOriginalMessage);
					var logger = new LoggerNLogWrapper(string.Empty, string.Empty, TestProgramCode, string.Empty, configurationFactoryMock.Object, true);

					// Act
					logger.Log(LogLevel.Error, expectedMessage);

					// Assert
					Assert(memoryTarget.Logs.Any(o => o.Contains(expectedMessage)));
				}
			}

			public void TestLogWithRefreshDatabaseUpgradeExceptionIsIgnoredNoLogIsReattemptedDbExceptionNotThrownPreviously()
			{
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				AssertLogWithRefreshDatabaseUpgradeExceptionIsIgnoredNoLogIsReattempted();
			}

			public void TestLogWithRefreshDatabaseUpgradeExceptionIsIgnoredNoLogIsReattemptedDbExceptionThrownPreviously()
			{
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;
				AssertLogWithRefreshDatabaseUpgradeExceptionIsIgnoredNoLogIsReattempted();
			}

			void AssertLogWithRefreshDatabaseUpgradeExceptionIsIgnoredNoLogIsReattempted()
			{
				// Arrange
				var databaseUpgradeException = new DatabaseUpgradedException();
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.Setup(o => o.Refresh())
					.Throws(databaseUpgradeException);

				var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection { new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = TestProgramCode, Bool2 = true, SystemDefined = true, }, };
				loggingMethods.SetDefaultCode(TestProgramCode, true);

				using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, loggingMethods))
				using (new NLogConfigurationForTest())
				{
					using var memoryTarget = new MemoryTarget();
					LogManager.Configuration.AddTarget(TestProgramCode, memoryTarget);
					LogManager.Configuration.LoggingRules.Add(new LoggingRule(TestProgramCode, NLog.LogLevel.Debug, memoryTarget) { RuleName = TestProgramCode });
					LogManager.ReconfigExistingLoggers();

					var logMessage = nameof(TestLogWithRefreshExceptionLogsOriginalMessage);
					var logger = new LoggerNLogWrapper(string.Empty, string.Empty, TestProgramCode, string.Empty, configurationFactoryMock.Object, true);

					// Act
					// Assert
					AssertNoExceptionThrown(() =>
					{
						logger.Log(LogLevel.Error, logMessage);
					});
					AssertEquals(false, Db.Connection.DatabaseUpgradedExceptionHasBeenThrown);
					AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), memoryTarget.Logs.Where(log => !log.Contains(logMessage)));
				}
			}

			public void TestDatabaseUpgradeSuppressionIsNotPublic()
			{
				// Arrange
				// Act
				var result = typeof(LoggerNLogWrapper)
					.GetMethods()
					.Where(method => method.Name.Equals("SuppressDbUpgradeExceptionHasBeenThrownInConnection"));

				// Assert
				AssertContainsExactElementsInAnyOrder("Do not expose this suppression function, it's only done here to prevent logging breaking functional code", Array.Empty<MethodInfo>(), result);
			}
		}

		public class StructuredLogTest : TransactionedTestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				SystemDataRegistry.Instance.ProcessControllerNLogInternalLoggingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				LogManager.Configuration = new LoggingConfiguration();
				LogManager.ReconfigExistingLoggers();

				tempDir = new TempDirectory();

				var loggingConfiguration = new LoggingConfiguration();
				TimeSource.Current = new FastUtcTimeSource();

				memoryTarget = new MemoryTarget(nameof(MemoryTarget));
				loggingConfiguration.AddTarget("logfile", memoryTarget);

				loggingConfiguration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, memoryTarget) { RuleName = ServiceManagerHelper.HostLoggerCode });

				LogManager.Configuration = loggingConfiguration;
				LogManager.ReconfigExistingLoggers();
			}

			protected override void TearDown()
			{
				LogManager.Shutdown();
				tempDir.Dispose();
				base.TearDown();
			}

			public void TestStructuredLog_SinglePropertyString() => AssertStructuredLog(
				new[] { new KeyValuePair<string, object>("property1", "value1") },
				"Property1:${scopeproperty:property1} - message:${message}",
				"Property1:value1 - message:hello",
				"hello");
			public void TestStructuredLog_DoublePropertyStringAndInt() => AssertStructuredLog(
				new[] { new KeyValuePair<string, object>("property1", "value1"), new KeyValuePair<string, object>("property2", 1) },
				"Property1:${scopeproperty:property1} Property2:${scopeproperty:property2} - message:${message}",
				"Property1:value1 Property2:1 - message:goodbye",
				"goodbye");
			public void TestStructuredLog_TriplePropertyStringIntAndDouble() => AssertStructuredLog(
				new[] { new KeyValuePair<string, object>("property1", "value1"), new KeyValuePair<string, object>("property2", 1), new KeyValuePair<string, object>("property3", 1.11) },
				"Property1:${scopeproperty:property1} Property2:${scopeproperty:property2} Property3:${scopeproperty:property3} - message:${message}",
				"Property1:value1 Property2:1 Property3:1.11 - message:hello",
				"hello");
			public void TestStructuredLog_SinglePropertyIntArrayAllProperties() => AssertStructuredLog(
				new[] { new KeyValuePair<string, object>("property1", new[] { 1, 2, 3 } ) },
				"Property1:[${scopeproperty:property1}] - message:${message}",
				"Property1:[1, 2, 3] - message:hello",
				"hello");

			public void AssertStructuredLog(IReadOnlyCollection<KeyValuePair<string, object>> properties, string loggerFormat, string expectedLog, string logMessage)
			{
				// Arrange
				memoryTarget.Layout = loggerFormat;
				var logger = new LoggerNLogWrapper(string.Empty, string.Empty, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);

				// Act
				using (logger.BeginScope(properties))
				{
					logger.Log(LogLevel.Information, logMessage);
				}

				// Assert
				var result = memoryTarget.Logs.Last();
				AssertEquals(expectedLog, result);
			}

			public void TestNestedStructuredLog_SinglePropertyString() => AssertNestedStructuredLogs(
				new[] { new KeyValuePair<string, object>("property1", "value1") },
				new[] { new KeyValuePair<string, object>("property2", "value2") },
				"Property1:${scopeproperty:property1} Property2:${scopeproperty:property2} - message:${message}",
				"Property1:value1 Property2:value2 - message:hello",
				"hello");
			public void TestNestedStructuredLog_SinglePropertyInt() => AssertNestedStructuredLogs(
				new[] { new KeyValuePair<string, object>("property1", 1) },
				new[] { new KeyValuePair<string, object>("property2", 2) },
				"Property1:${scopeproperty:property1} Property2:${scopeproperty:property2} - message:${message}",
				"Property1:1 Property2:2 - message:goodbye",
				"goodbye");

			public void AssertNestedStructuredLogs(IReadOnlyCollection<KeyValuePair<string, object>> properties1, IReadOnlyCollection<KeyValuePair<string, object>> properties2, string loggerFormat, string expectedLog, string logMessage)
			{
				// Arrange
				memoryTarget.Layout = loggerFormat;
				var logger = new LoggerNLogWrapper(string.Empty, string.Empty, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);

				// Act
				using (logger.BeginScope(properties1))
				using (logger.BeginScope(properties2))
				{
					logger.Log(LogLevel.Information, logMessage);
				}

				// Assert
				var result = memoryTarget.Logs.Last();
				AssertEquals(expectedLog, result);
			}

			TempDirectory tempDir;
			MemoryTarget memoryTarget;
		}

		public class EnvironmentErrorTest : TransactionedTestCase
		{
			public void TestInternalLoggingWithoutEnvironmentAtAll()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.SetupGet(configuration => configuration.InternalNLogLoggingEnabled)
					.Returns(true);

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestInternalLoggingWithoutEnvironmentAtAll));

					// Act
					// Assert
					AssertNoExceptionThrown(() => _ = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, true));
				}
			}

			public void TestInternalLoggingWithoutEnvironmentAtAllLogsException()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.SetupGet(configuration => configuration.InternalNLogLoggingEnabled)
					.Returns(true);

				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestInternalLoggingWithoutEnvironmentAtAllLogsException));

					using (new NLogConfigurationForTest())
					using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
					{
						// Act
						_ = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false);
					}

					var result = File
						.ReadAllLines(Directory.EnumerateFiles(outputDir, "NLog_*.txt").Single())
						.First();

					// Assert
					AssertContains("Error Time retrieving error. Exception: System.ArgumentNullException: Value cannot be null.", result);
				}
			}

			public void TestLoggingWithoutEnvironmentAtAll()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestLoggingWithoutEnvironmentAtAll));
					var logger = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);

					// Act
					// Assert
					AssertNoExceptionThrown(() => logger.Log(LogLevel.Information, "Hello!"));
				}
			}

			public void TestLoggingWithoutEnvironmentAtAllLogsException()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());

				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestLoggingWithoutEnvironmentAtAllLogsException));

					using (new NLogConfigurationForTest())
					using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
					{
						var logger = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);

						// Act
						logger.Log(LogLevel.Information, "Hello!");
					}

					var result = File
						.ReadAllLines(Directory.EnumerateFiles(outputDir, $"{TestProgramCode}_*.txt").Single())
						.First();

					// Assert
					AssertContains("Error", result);
					AssertContains("Time retrieving error.", result);
					AssertContains("System.ArgumentNullException: Value cannot be null.", result);
				}
			}

			[TestDate(2019, 12, 23, 20, 22, 0)]
			public void TestLoggingWithoutEnvironmentAtAllUsesTimeOffsetOfTheLastSuccessfulLog()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());

				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestLoggingWithoutEnvironmentAtAllLogsException));

					using (new NLogConfigurationForTest())
					{
						var logger = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);
						logger.Log(LogLevel.Information, "Calculating time difference.");

						using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
						{
							// Act
							logger.Log(LogLevel.Information, "Hello!");
						}
					}

					var result = File
						.ReadAllLines(Directory.EnumerateFiles(outputDir, $"{TestProgramCode}_*.txt").Single())
						.Skip(1)
						.First()
						.Substring(0, 25);

					// Assert
					AssertContains("2019-12-23 20:22:00", result, true);
				}
			}

			public void TestLoggingExceptionWithoutEnvironmentAtAll()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestLoggingExceptionWithoutEnvironmentAtAll));
					var logger = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);

					// Act
					// Assert
					AssertNoExceptionThrown(() => logger.Log(LogLevel.Information, new Exception(":("), "Hello!"));
				}
			}

			public void TestLoggingExceptionWithoutEnvironmentAtAllLogsException()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());

				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestLoggingExceptionWithoutEnvironmentAtAllLogsException));

					using (new NLogConfigurationForTest())
					using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
					{
						var logger = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);

						// Act
						logger.Log(LogLevel.Information, new Exception(":("), "Hello!");
					}

					var result = File
						.ReadAllLines(Directory.EnumerateFiles(outputDir, $"{TestProgramCode}_*.txt").Single())
						.First();

					// Assert
					AssertContains("Error", result);
					AssertContains("Time retrieving error.", result);
					AssertContains("System.ArgumentNullException: Value cannot be null.", result);
				}
			}

			[TestDate(2019, 12, 23, 20, 22, 0)]
			public void TestLoggingExceptionWithoutEnvironmentAtAllUsesTimeOffsetOfTheLastSuccessfulLog()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());

				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestLoggingWithoutEnvironmentAtAllLogsException));

					using (new NLogConfigurationForTest())
					{
						var logger = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);
						logger.Log(LogLevel.Information, "Calculating time difference.");

						using (Env.TemporarilySetNullEnvironmentInstanceForTesting())
						{
							// Act
							logger.Log(LogLevel.Information, new Exception(":("), "Hello!");
						}
					}

					var result = File
						.ReadAllLines(Directory.EnumerateFiles(outputDir, $"{TestProgramCode}_*.txt").Single())
						.Skip(1)
						.First()
						.Substring(0, 25);

					// Assert
					AssertContains("2019-12-23 20:22:00", result, true);
				}
			}
		}

		public class LogParametersTest : TransactionedTestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				SystemDataRegistry.Instance.ProcessControllerNLogInternalLoggingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				LogManager.Configuration = new LoggingConfiguration();
				LogManager.ReconfigExistingLoggers();

				tempDir = new TempDirectory();

				var loggingConfiguration = new LoggingConfiguration();
				TimeSource.Current = new FastUtcTimeSource();

				memoryTarget = new MemoryTarget(nameof(MemoryTarget));
				loggingConfiguration.AddTarget("logfile", memoryTarget);

				loggingConfiguration.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Trace, memoryTarget) { RuleName = ServiceManagerHelper.HostLoggerCode });

				LogManager.Configuration = loggingConfiguration;
				LogManager.ReconfigExistingLoggers();
			}

			protected override void TearDown()
			{
				LogManager.Shutdown();
				tempDir.Dispose();
				base.TearDown();
			}

			public void TestSeverity()
			{
				// Arrange
				var logLevels = Enum.GetValues(typeof(LogLevel)).Cast<LogLevel>().Except(LogLevel.None);
				memoryTarget.Layout = "${event-properties:severity}";
				using (SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
							new VerboseLoggingCollection
							{
								new VerboseLoggingBusinessObject { Code = ServiceManagerHelper.HostLoggerCode, Value = ZDateTime.UtcNow.AddDays(1), SystemDefined = true, },
							}))
				{
					var logger = new LoggerNLogWrapper(string.Empty, string.Empty, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);

					CombineAssertions(() =>
					{
						foreach (var logLevel in logLevels)
						{
							Test(logLevel);
						}
					});

					void Test(LogLevel logLevel)
					{
						// Act
						logger.Log(logLevel, string.Empty);

						// Assert
						var result = memoryTarget.Logs.Last();
						AssertEquals($"{logLevel}", result);
					}
				}
			}

			public void TestDatabase()
			{
				// Arrange
				memoryTarget.Layout = "${event-properties:database}";

				CombineAssertions(() =>
				{
					Test("db", "db");
					Test("DataBase", "database");
				});

				void Test(string value, string expected)
				{
					var logger = new LoggerNLogWrapper(string.Empty, value, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);

					// Act
					logger.Log(LogLevel.Information, string.Empty);

					// Assert
					var result = memoryTarget.Logs.Last();
					AssertEquals(expected, result);
				}
			}

			public void TestDatabaseHost()
			{
				// Arrange
				memoryTarget.Layout = "${event-properties:database_host}";

				CombineAssertions(() =>
				{
					Test("host", "host");
					Test("DataBaseHost", "databasehost");
				});

				void Test(string value, string expected)
				{
					var logger = new LoggerNLogWrapper(value, string.Empty, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);

					// Act
					logger.Log(LogLevel.Information, string.Empty);

					// Assert
					var result = memoryTarget.Logs.Last();
					AssertEquals(expected, result);
				}
			}

			public void TestHost()
			{
				// Arrange
				memoryTarget.Layout = "${event-properties:host:objectpath=name}";
				var logger = new LoggerNLogWrapper(string.Empty, string.Empty, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);
				var expected = ServiceManagerHelper.GetHostName().ToLowerInvariant();

				// Act
				logger.Log(LogLevel.Information, string.Empty);

				// Assert
				var result = memoryTarget.Logs.Last();
				AssertEquals(expected, result);
			}

			public void TestProcessController()
			{
				// Arrange
				memoryTarget.Layout = "${event-properties:processcontroller:objectpath=hostname}";
				var logger = new LoggerNLogWrapper(string.Empty, string.Empty, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);
				var expected = ServiceManagerHelper.GetHostName().ToLowerInvariant();

				// Act
				logger.Log(LogLevel.Information, string.Empty);

				// Assert
				var result = memoryTarget.Logs.Last();
				AssertEquals(expected, result);
			}

			public void TestServiceTask()
			{
				// Arrange
				memoryTarget.Layout = "${event-properties:servicetask:objectpath=code}";
				CombineAssertions(() =>
				{
					Test("host", "HOST");
					Test("HOST", "HOST");
					Test("DSA", "DSA");
				});

				void Test(string value, string expected)
				{
					var logger = new LoggerNLogWrapper(string.Empty, string.Empty, value, string.Empty, new LoggerNLogConfigurationFactory(), false);

					// Act
					logger.Log(LogLevel.Information, string.Empty);

					// Assert
					var result = memoryTarget.Logs.Last();
					AssertEquals(expected, result);
				}
			}

			public void TestProcess()
			{
				// Arrange
				memoryTarget.Layout = "${event-properties:process:objectpath=pid}";
				var logger = new LoggerNLogWrapper(string.Empty, string.Empty, ServiceManagerHelper.HostLoggerCode, string.Empty, new LoggerNLogConfigurationFactory(), false);
				var expected = Process.GetCurrentProcess().Id.ToString();

				// Act
				logger.Log(LogLevel.Information, string.Empty);

				// Assert
				var result = memoryTarget.Logs.Last();
				AssertEquals(expected, result);
			}

			public void TestInstallation()
			{
				// Arrange
				memoryTarget.Layout = "${event-properties:installation:objectpath=code}";
				CombineAssertions(() =>
				{
					Test("ENT", "PRD", "ENTPRD");
					Test("SRV", "DTB", "SRVDTB");
				});

				void Test(string enterpriseCode, string serverCode, string expected)
				{
					using (ObjectFactory.Substitute(Mock.Of<IProductRegistration>(registration =>
						registration.Key == Mock.Of<IProductRegistrationKey>(key =>
							key.EnterpriseCode == enterpriseCode
							&& key.ServerCode == serverCode))))
					{
						var logger = new LoggerNLogWrapper(string.Empty,
							string.Empty,
							ServiceManagerHelper.HostLoggerCode,
							string.Empty,
							new LoggerNLogConfigurationFactory(),
							false);

						// Act
						logger.Log(LogLevel.Information, string.Empty);
					}

					// Assert
					var result = memoryTarget.Logs.Last();
					AssertEquals(expected, result);
				}
			}

			public void TestSequenceId()
			{
				var currentSeqId = new LogEventInfo().SequenceID;

				memoryTarget.Layout = @"${event-properties:sequenceId:servicetask:objectpath=code}:sequenceId=${sequenceId}";
				CombineAssertions(() =>
				{
					Test(
						new[] { "ASD", },
						new[] { $"ASD:sequenceId={++currentSeqId}" });
					Test(
						new[] { "ASD", "DSA", },
						new[]
						{
							$"ASD:sequenceId={++currentSeqId}",
							$"DSA:sequenceId={++currentSeqId}",
						});
					Test(
						new[] { "ASD", "DSA", "QWE", "EWQ", },
						new[]
						{
							$"ASD:sequenceId={++currentSeqId}",
							$"DSA:sequenceId={++currentSeqId}",
							$"QWE:sequenceId={++currentSeqId}",
							$"EWQ:sequenceId={++currentSeqId}",
						});
				});

				void Test(string[] logsInOrder, string[] expected)
				{
					// Arrange
					memoryTarget.Logs.Clear();

					// Act
					foreach (var value in logsInOrder)
					{
						var logger = new LoggerNLogWrapper(
							string.Empty,
							string.Empty,
							value,
							string.Empty,
							new LoggerNLogConfigurationFactory(),
							false);
						logger.Log(LogLevel.Information, string.Empty);
					}

					// Assert
					var result = memoryTarget.Logs.ToArray();
					AssertContainsExactElementsInAnyOrder(expected, result);
				}
			}

			TempDirectory tempDir;
			MemoryTarget memoryTarget;
		}

		internal const string TestProgramCode = "~@T";

		public class DoubleInitTest : TestCase
		{
			class CreatesOneTargetTest : TransactionedTestCase
			{
				public void TestFileLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.AllTargets.Count);
						AssertEquals(1, LogManager.Configuration.AllTargets.Count(t => t.Name.Contains("logfile")));
					}
				}

				public void TestCombinedLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.AllTargets.Count);
						AssertEquals(1, LogManager.Configuration.AllTargets.Count(t => t.Name.Contains("combinedFile")));
					}
				}

				public void TestElasticLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.ELK, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(2, LogManager.Configuration.AllTargets.Count);
						AssertEquals("Buffered target and target itself", 2, LogManager.Configuration.AllTargets.Count(t => t.Name.Contains("elasticsearch")));
					}
				}

				public void TestKafkaLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.KAF, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(2, LogManager.Configuration.AllTargets.Count);
						AssertEquals("Buffered target and target itself", 2, LogManager.Configuration.AllTargets.Count(t => t.Name.Contains("kafka")));
					}
				}

				public void TestSyslogLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TST", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(2, LogManager.Configuration.AllTargets.Count);
						AssertEquals("Buffered target and target itself", 2, LogManager.Configuration.AllTargets.Count(t => t.Name.Contains("syslog")));
					}
					LogManager.Flush();
					LogManager.Shutdown(); // Syslog can leave a task running in the background Flush and shutdown the LogManager to kill the task 
					AsyncHelper.WaitAllActiveTasksForTest();
				}
			}

			class CreatesTwoLoggingRulesTest : TransactionedTestCase
			{
				public void TestFileLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS2", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertContainsExactElementsInAnyOrder(
							new[] { "TS1", "TS2" },
							LogManager.Configuration.LoggingRules.Select(rule => rule.RuleName));
					}
				}

				public void TestCombinedFileLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS2", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertContainsExactElementsInAnyOrder(
							new[] { "TS1", "TS2" },
							LogManager.Configuration.LoggingRules.Select(rule => rule.RuleName));
					}
				}

				public void TestElasticLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.ELK, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS2", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertContainsExactElementsInAnyOrder(
							new[] { "TS1", "TS2" },
							LogManager.Configuration.LoggingRules.Select(rule => rule.RuleName));
					}
				}

				public void TestKafkaLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.KAF, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS2", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertContainsExactElementsInAnyOrder(
							new[] { "TS1", "TS2" },
							LogManager.Configuration.LoggingRules.Select(rule => rule.RuleName));
					}
				}

				public void TestSyslogLogging()
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS2", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertContainsExactElementsInAnyOrder(
							new[] { "TS1", "TS2" },
							LogManager.Configuration.LoggingRules.Select(rule => rule.RuleName));
					}
					LogManager.Flush();
					LogManager.Shutdown(); // Syslog can leave a task running in the background Flush and shutdown the LogManager to kill the task 
					AsyncHelper.WaitAllActiveTasksForTest();
				}
			}

			class CreatesOneLoggingRuleTest : TransactionedTestCase
			{
				public void TestFileLogging()
				{
					var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
						},
					};
					loggingMethods.SetDefaultCode(LoggingMethods.FSL, true);

					ExecuteTest(loggingMethods);
				}

				public void TestCombinedFileLogging()
				{
					var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.FSL, SystemDefined = true,
						},
					};
					loggingMethods.SetDefaultCode(LoggingMethods.FSL, true);

					ExecuteTest(loggingMethods);
				}

				public void TestElasticLogging()
				{
					var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true,
						},
					};
					loggingMethods.SetDefaultCode(LoggingMethods.ELK, true);

					ExecuteTest(loggingMethods);
				}

				public void TestKafkaLogging()
				{
					var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
						},
					};
					loggingMethods.SetDefaultCode(LoggingMethods.KAF, true);

					ExecuteTest(loggingMethods);
				}

				public void TestSyslogLogging()
				{
					var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.FSL, SystemDefined = true,
						},
					};
					loggingMethods.SetDefaultCode(LoggingMethods.FSL, true);

					ExecuteTest(loggingMethods);
					LogManager.Flush();
					LogManager.Shutdown(); // Syslog can leave a task running in the background Flush and shutdown the LogManager to kill the task 
					AsyncHelper.WaitAllActiveTasksForTest();
				}

				void ExecuteTest(SystemDefinableCodeDescriptionBoolWithExtraBoolCollection loggingMethods)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");

						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, loggingMethods);
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertEquals(1, LogManager.Configuration.LoggingRules[0].Targets.Count);
					}
				}
			}

			class CreatesOneLoggingRuleWithMultipleTargetsTest : TransactionedTestCase
			{
				public void TestAllLoggingMethods()
				{
					var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
						}
					};
					loggingMethods.SetDefaultCode(LoggingMethods.FSL, true);

					ExecuteTest(loggingMethods);
				}

				void ExecuteTest(SystemDefinableCodeDescriptionBoolWithExtraBoolCollection loggingMethods)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");

						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, loggingMethods);

						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertEquals(loggingMethods.Count, LogManager.Configuration.LoggingRules[0].Targets.Count);
					}

					// Teardown
					InternalLogger.Reset();
					LogManager.LogFactory.Dispose();
					LogManager.ReconfigExistingLoggers(purgeObsoleteLoggers: true);
					LogManager.Flush();
					LogManager.Shutdown(); // Syslog can leave a task running in the background, Flush and shutdown the LogManager to kill the task
					AsyncHelper.WaitAllActiveTasksForTest();
				}

				public void TestLoggingMethodsWithMultipleThreads()
				{
					var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
					{
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.SYS, Bool2 = false, SystemDefined = true,
						},
						new SystemDefinableCodeDescriptionBoolWithExtraBool
						{
							Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
						}
					};
					loggingMethods.SetDefaultCode(LoggingMethods.FSL, true);

					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					using (var syncStart = new ManualResetEvent(false))
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");

						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, loggingMethods);

						// Act
						var tasks = Enumerable.Range(0, 10)
							.Select(o => Task.Run(() => {
								syncStart.WaitOne();
								_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, "TS1", outputDir, new LoggerNLogConfigurationFactory(), true, false);
								}))
							.ToArray();
						syncStart.Set();

						Task.WaitAll(tasks);

						// Assert
						Assert(
							"SysLog should be disabled as it leaves behind uncontrolled Task running even after dispose",
							!loggingMethods.FindElementByCode(LoggingMethods.SYS).Bool2);
						AssertEquals(false, tasks.Any(o => o.IsFaulted));
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertEquals(loggingMethods.Count - 1, LogManager.Configuration.LoggingRules[0].Targets.Count);
					}
				}
			}
		}

		public class LoggingRuleNamesStartFromServiceTaskCodeTest : TransactionedTestCase
		{
			public void TestFileLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertStartsWith("Logging rule name starts from task code", taskCode, LogManager.Configuration.LoggingRules[0].RuleName);
					}
				}
			}

			public void TestCombinedFileLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertStartsWith("Logging rule name starts from task code", taskCode, LogManager.Configuration.LoggingRules[0].RuleName);
					}
				}
			}

			public void TestElasticLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.ELK, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertStartsWith("Logging rule name starts from task code", taskCode, LogManager.Configuration.LoggingRules[0].RuleName);
					}
				}
			}

			public void TestKafkaLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.KAF, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertStartsWith("Logging rule name starts from task code", taskCode, LogManager.Configuration.LoggingRules[0].RuleName);
					}
				}
			}

			public void TestSyslogLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(1, LogManager.Configuration.LoggingRules.Count);
						AssertStartsWith("Logging rule name starts from task code", taskCode, LogManager.Configuration.LoggingRules[0].RuleName);
					}
				}
				LogManager.Flush();
				LogManager.Shutdown(); // Syslog can leave a task running in the background Flush and shutdown the LogManager to kill the task 
				AsyncHelper.WaitAllActiveTasksForTest();
			}
		}

		public class LoggingRuleNamesLoggerNamePatternTest : TransactionedTestCase
		{
			public void TestFileLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(taskCode, LogManager.Configuration.LoggingRules[0].LoggerNamePattern);
					}
				}
			}

			public void TestCombinedFileLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.CFL, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(taskCode, LogManager.Configuration.LoggingRules[0].LoggerNamePattern);
					}
				}
			}

			public void TestElasticLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.ELK, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.ELK, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(taskCode, LogManager.Configuration.LoggingRules[0].LoggerNamePattern);
					}
				}
			}

			public void TestKafkaLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.KAF, Bool2 = true, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.KAF, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(taskCode, LogManager.Configuration.LoggingRules[0].LoggerNamePattern);
					}
				}
			}

			public void TestSyslogLogging()
			{
				Test("TS1");
				Test("TS2");

				void Test(string taskCode)
				{
					// Arrange
					using (new NLogConfigurationForTest())
					using (var tempDir = new TempDirectory())
					{
						var outputDir = Path.Combine(tempDir.DirectoryName, "TestLog");
						var newValue = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
						{
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.SYS, Bool2 = true, SystemDefined = true,
							},
							new SystemDefinableCodeDescriptionBoolWithExtraBool
							{
								Code = LoggingMethods.FSL, SystemDefined = true,
							},
						};
						newValue.SetDefaultCode(LoggingMethods.FSL, true);
						SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

						// Act
						_ = new LoggerNLogWrapper(Db.ServerName, Db.DatabaseName, taskCode, outputDir, new LoggerNLogConfigurationFactory(), true, false);

						// Assert
						AssertEquals(taskCode, LogManager.Configuration.LoggingRules[0].LoggerNamePattern);
					}
				}
				LogManager.Flush();
				LogManager.Shutdown(); // Syslog can leave a task running in the background Flush and shutdown the LogManager to kill the task 
				AsyncHelper.WaitAllActiveTasksForTest();
			}
		}

		public class TestInternalLoggerNLog : TransactionedTestCase
		{
			public void TestInternalLoggerInitialisation_WhenInternalLoggerRegistryTurnOff()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.SetupGet(configuration => configuration.InternalNLogLoggingEnabled)
					.Returns(false);

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestInternalLoggerInitialisation_WhenInternalLoggerRegistryTurnOff));

					// Act
					_ = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);

					// Assert
					AssertEquals(true, InternalLogger.LogToConsoleError);
					AssertEquals(true, InternalLogger.LogWriter != null);
					AssertNullOrEmpty(InternalLogger.LogFile);
					AssertEquals(NLog.LogLevel.Error, InternalLogger.LogLevel);
				}
			}

			[TestDate(2024, 4, 22)]
			public void TestInternalLoggerInitialisation_WhenInternalLoggerRegistryTurnOn()
			{
				// Arrange
				var configurationFactoryMock = new Mock<ILoggerNLogConfigurationFactory>();
				var loggerNLogConfigurationMock = new Mock<ILoggerNLogConfiguration>();
				configurationFactoryMock
					.Setup(factory => factory.GetConfiguration(It.IsAny<string>()))
					.Returns(loggerNLogConfigurationMock.Object);
				loggerNLogConfigurationMock
					.Setup(configuration => configuration.VerboseLogging)
					.Returns(new VerboseLoggingCollection());
				loggerNLogConfigurationMock
					.SetupGet(configuration => configuration.InternalNLogLoggingEnabled)
					.Returns(true);

				using (new NLogConfigurationForTest())
				using (var tempDir = new TempDirectory())
				using (var tempStdErrWriter = new TempStandardErrorWriter())
				{
					var outputDir = Path.Combine(tempDir.DirectoryName, nameof(TestInternalLoggerInitialisation_WhenInternalLoggerRegistryTurnOn));

					// Act
					_ = new LoggerNLogWrapper("", "", TestProgramCode, outputDir, configurationFactoryMock.Object, false, false);

					var stdErrOutput = tempStdErrWriter.ErrorWriter.ToString();
					var fileOutput = string.Join("\n", File.ReadAllLines(Directory.EnumerateFiles(outputDir, "NLog_*.txt").Single()));

					// Assert
					AssertNotContains("Debug Adding target NLog.Targets.FileTarget(Name=logfile~@T)", stdErrOutput); // Internal NLog debug logs
					AssertContains("Debug Adding target NLog.Targets.FileTarget(Name=logfile~@T)", fileOutput); // Internal NLog debug logs
					AssertEquals(false, InternalLogger.LogToConsoleError);
					AssertEquals(true, InternalLogger.LogWriter != null);
					AssertEquals(Path.Combine(outputDir, "NLog_20240422.txt"), InternalLogger.LogFile);
					AssertEquals(NLog.LogLevel.Debug, InternalLogger.LogLevel);
				}
			}

			class TempStandardErrorWriter : IDisposable
			{
				public TempStandardErrorWriter()
				{
					// Redirect Standard Error stream to a StringWriter
					ErrorWriter = new StringWriter();
					originalErrorWriter = Console.Error;
					Console.SetError(ErrorWriter);
				}

				public void Dispose()
				{
					Console.SetError(originalErrorWriter);
				}

				public StringWriter ErrorWriter { get; }
				readonly TextWriter originalErrorWriter;
			}
		}

		public class NLogConfigurationForTest : IDisposable
		{
			public NLogConfigurationForTest()
			{
				SystemDataRegistry.Instance.ProcessControllerNLogInternalLoggingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				LogManager.Configuration = new LoggingConfiguration();
				LogManager.ReconfigExistingLoggers();
			}

			public void Dispose()
			{
				if (LogManager.Configuration != null)
				{
					foreach (var logger in LogManager.Configuration.AllTargets.ToArray())
					{
						if (logger is BufferingTargetWrapper wrapper)
						{
							wrapper.WrappedTarget.Dispose();
						}
						logger.Dispose();
					}
				}

				LogManager.Configuration = new LoggingConfiguration();
				LogManager.ReconfigExistingLoggers();
				InternalLogger.Reset();
			}
		}

		class LoggerNLogWrapperForTest : LoggerNLogWrapper
		{
			public LoggerNLogWrapperForTest(string dbServer, string dbName, string programCode, string directoryPath, ILoggerNLogConfigurationFactory configFactory, bool archiveLogFiles, bool trackServiceTaskErrors = true)
				: base(dbServer, dbName, programCode, directoryPath, configFactory, archiveLogFiles, trackServiceTaskErrors)
			{
			}

			protected override DateTime GetCurrentUtcDateTime()
			{
				if (ExceptionToThrowFromGetCurrentUtcDateTime != null)
				{
					throw ExceptionToThrowFromGetCurrentUtcDateTime;
				}
				return base.GetCurrentUtcDateTime();
			}

			internal Exception ExceptionToThrowFromGetCurrentUtcDateTime { get; set; }
		}
	}
}
