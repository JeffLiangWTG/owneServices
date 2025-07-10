using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared;
using Enterprise.ServiceManager.Shared.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using Moq.Protected;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.DummySleepingService;
using ServiceManager.Host.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManager.Shared.CW1.Test;
using WTG.AppDomainWrappers.Net;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;

[assembly: UsesConstants(typeof(DummySleepingTask))]
[assembly: UsesConstants(typeof(ServiceManagerConstants))]

namespace ServiceManager.Runner.CW1.Test
{
	[UseSnapshotProtection]
	class ProgramTest : TestCase
	{
		[CrossPlatformTestDoNotFailIfStdoutIsNotEmpty]
		[ExpectNoExceptions]
		public void TestConnectionPoolingEnabled_RunnerPoolsConnection()
		{
			ConnectionPoolingTest(true);
		}

		[CrossPlatformTestDoNotFailIfStdoutIsNotEmpty]
		[ExpectNoExceptions]
		public void TestConnectionPoolingDisabled_RunnerDoesNotPoolConnection()
		{
			ConnectionPoolingTest(false);
		}

		static void ConnectionPoolingTest(bool isPoolingEnabled)
		{
			// Arrange
			using (ErrorReporter.SetTemporaryInstanceForTest(Mock.Of<IErrorReporter>()))
			using (SystemDataRegistry.Instance.ServiceTaskRunnerConnectionPoolingEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isPoolingEnabled))
			using (var appDomainWrapper = new AppDomainWrapper($"{nameof(ConnectionPoolingTest)}_{isPoolingEnabled}", isTestAppDomain: true))
			{
				var domainData = new Dictionary<string, object>
				{
					{ "ServerName", Db.ServerName },
					{ "DatabaseName", Db.DatabaseName },
					{ "Pooling", isPoolingEnabled ? "-EnableConnectionPooling" : null }
				};
				var returnData = new Dictionary<string, object>
				{
					{ "Exception", null },
					{ "isPooling", null }
				};

				var result = appDomainWrapper.RunActionInAppDomain(() =>
				{
					var grpcEventHandles = new GrpcEventHandleNames();
					using (var consoleOutput = new ConsoleOutput())
					using (var grpcClientSynchronizer = new GrpcClientSynchronizer(grpcEventHandles))
					{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
						var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
						var databaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
						var pooling = (string)AppDomain.CurrentDomain.GetData("Pooling");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
						var args = new[] { serverName, databaseName, $"-grpc:{grpcEventHandles.BaseEventWaitHandleName}", pooling, "-ProductKey:ProductKey" };

						// Act
						var main = Task.Run(() => ApplicationStarter.Main(args.WhereNotNull().ToArray()));
						var controller = Task.Run(() =>
						{
							using (var runner = GetCommandQueueProvider(grpcClientSynchronizer, consoleOutput))
							{
								runner.Stop();
								Task.Delay(TimeSpan.FromSeconds(5)).Wait();
							}
						});
						Task.WaitAll(new[] { main, controller }, TimeSpan.FromSeconds(30));
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
						AppDomain.CurrentDomain.SetData("isPooling", DbEnv.Instance.ConnectionPooling.IsPooling);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
					}
				}, domainData, returnData);

				AssertNotNull(result["isPooling"]);
				AssertEquals("Pooling configured", isPoolingEnabled, (bool)result["isPooling"]);
			}
		}

		public void TestInitializationDoesNotLogDatabaseUpgradeException()
		{
			var logFileName = GetHostLoggerFilePath();
			File.Delete(logFileName);

			// Arrange
			using (var appDomainWrapper = new AppDomainWrapper(nameof(TestCriticalExceptionIsNotReported), true))
			{
				var domainData = new Dictionary<string, object>
						{
							{ "ServerName", Db.ServerName },
							{ "DatabaseName", Db.DatabaseName }
						};

				appDomainWrapper.RunActionInAppDomain(() =>
				{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
					Db.InitializeDatabaseDetails(
						(string)AppDomain.CurrentDomain.GetData("ServerName"),
						(string)AppDomain.CurrentDomain.GetData("DatabaseName"));
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

					EnterpriseApplicationConfiguration.ConfigureObjectFactory();

					var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
					var versionMock = new Mock<IDatabaseAspectVersions>();
					versionMock
						.Setup(x => x.SchemaVersion)
						.Returns(new VersionLabel(bumpedSchemaVersion, 0));
					ObjectFactory.Substitute(versionMock.Object);

					var textWriter = new Mock<TextWriter>();
					Console.SetError(textWriter.Object);

					var errorReporterMock = new Mock<IErrorReporter>();
					ObjectFactory.Substitute(new Func<IClientHostedServiceAttributeProvider>(() =>
					{
						ErrorReporter.Instance = errorReporterMock.Object;
						return Mock.Of<IClientHostedServiceAttributeProvider>();
					}));

					var ipcChannel = Invariant($"{Guid.Empty}");
					ErrorReporter.Instance = errorReporterMock.Object;
					var args = new[]
					{
								Db.ServerName,
								Db.DatabaseName,
								"-grpc:" + ipcChannel,
								"-ProductKey:ProductKey",
					};

					// Act
					var result = (RunnerExitCode)ApplicationStarter.Main(args);

					// Assert
					AssertEquals(RunnerExitCode.RunnerFailure, result);
					errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
					textWriter.Verify(x => x.WriteLine(It.IsAny<string>()), Times.Never);
				}, domainData);
			}

			AssertEquals(expected: false, File.Exists(logFileName));
		}

		public void TestCriticalExceptionIsNotReported()
		{
			AssertNoExceptionThrown(() =>
			{
				Test<OutOfMemoryException>();
				Test<CargoWise.Data.Utils.SqlLockLostException>();
			});

			void Test<T>() where T : Exception
			{
				var logFileName = GetHostLoggerFilePath();
				File.Delete(logFileName);

				// Arrange
				using (var appDomainWrapper = new AppDomainWrapper($"{nameof(TestCriticalExceptionIsNotReported)}_{typeof(T).Name}", true))
				{
					var domainData = new Dictionary<string, object>
					{
						{ "ServerName", Db.ServerName },
						{ "DatabaseName", Db.DatabaseName }
					};

					appDomainWrapper.RunActionInAppDomain(() =>
					{
						var errorReporterMock = new Mock<IErrorReporter>();
						var grpcEventHandles = new GrpcEventHandleNames();
						using (var consoleOutput = new ConsoleOutput())
						using (var grpcClientSynchronizer = new GrpcClientSynchronizer(grpcEventHandles))
						{
							using (ObjectFactory.Substitute(new Func<IClientHostedServiceAttributeProvider>(() =>
							{
								ErrorReporter.Instance = errorReporterMock.Object;
								throw Activator.CreateInstance<T>();
							})))
							{
								var args = new[]
								{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
									(string)AppDomain.CurrentDomain.GetData("ServerName"),
									(string)AppDomain.CurrentDomain.GetData("DatabaseName"),
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
									"-grpc:" + grpcEventHandles.BaseEventWaitHandleName,
									"-ProductKey:ProductKey",
								};

								// Act
								var main = Task.Run(() => ApplicationStarter.Main(args));
								grpcClientSynchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == !string.IsNullOrEmpty(consoleOutput.GetOuput())), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(60));
								main.Wait(new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token);

								var result = (RunnerExitCode)main.Result;

								// Assert
								AssertEquals(RunnerExitCode.RunnerFailure, result);
								errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
							}
						}
					}, domainData);
				}

				Assert("Log file exists", File.Exists(logFileName));
				AssertContains("Log file contains error", "*FROM RUNNER*: Unhandled exception", File.ReadAllText(logFileName));
			}
		}

		static string GetHostLoggerFilePath()
		{
			var logDir = ServiceManagerHelper.GetLogFilesDirectory(Db.ServerName, Db.DatabaseName);
			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}

			return Path.Combine(logDir, $"HOST_{DateTime.UtcNow:yyyyMMdd}.txt");
		}

		[ExpectNoExceptions]
		public void TestNotCriticalExceptionIsReported_Exception()
		{
			AssertNotCriticalExceptionIsReported<Exception>();
		}

		[ExpectNoExceptions]
		public void TestNotCriticalExceptionIsReported_InvalidOperationException()
		{
			AssertNotCriticalExceptionIsReported<InvalidOperationException>();
		}

		[ExpectNoExceptions]
		public void TestNotCriticalExceptionIsReported_ArgumentException()
		{
			AssertNotCriticalExceptionIsReported<ArgumentException>();
		}

		void AssertNotCriticalExceptionIsReported<T>() where T : Exception
		{
			// Arrange
			using (var appDomainWrapper = new AppDomainWrapper(nameof(TestCriticalExceptionIsNotReported)))
			{
				var domainData = new Dictionary<string, object>
				{
					{ "ServerName", Db.ServerName },
					{ "DatabaseName", Db.DatabaseName }
				};

				appDomainWrapper.RunActionInAppDomain(() =>
				{
					var errorReporterMock = new Mock<IErrorReporter>();
					var grpcEventHandles = new GrpcEventHandleNames();
					using (var consoleOutput = new ConsoleOutput())
					using (var grpcClientSynchronizer = new GrpcClientSynchronizer(grpcEventHandles))
					{
						using (ObjectFactory.Substitute(new Func<IClientHostedServiceAttributeProvider>(() =>
						{
							ErrorReporter.Instance = errorReporterMock.Object;
							throw Activator.CreateInstance<T>();
						})))
						{
							var args = new[]
							{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
								(string)AppDomain.CurrentDomain.GetData("ServerName"),
								(string)AppDomain.CurrentDomain.GetData("DatabaseName"),
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
								"-grpc:" + grpcEventHandles.BaseEventWaitHandleName,
								"-ProductKey:ProductKey",
							};

							// Act
							var main = Task.Run(() => ApplicationStarter.Main(args));
							grpcClientSynchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == !string.IsNullOrEmpty(consoleOutput.GetOuput())), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(60));
							main.Wait(new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token);

							var result = (RunnerExitCode)main.Result;

							// Assert
							AssertEquals(RunnerExitCode.RunnerFailure, result);
							errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
							errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.Is<string>(s => s.StartsWith("Service Manager Runner exception", StringComparison.OrdinalIgnoreCase)), It.Is<Exception>(exception => exception.InnerException.GetType() == typeof(T))), Times.Once);
						}
					}
				}, domainData);
			}
		}

		public void TestRunnerFallDownDuringDatabaseUpgrade()
		{
			// Arrange
			var eventHandleNames = new GrpcEventHandleNames();
			using (var grpcClientSynchronizer = new GrpcClientSynchronizer(eventHandleNames))
			{
				int? grpcPort = null;
				var outputData = new StringBuilder();
				var errorData = new StringBuilder();
				using var process = StartProcess();
				using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

				// Act
				var controller = Task.Run(() =>
				{
					using (var admConn = Db.NewAdminConnection())
					using (var runner = GetCommandQueueProviderForProcess(grpcClientSynchronizer, () => grpcPort))
					{
						runner.Run("Enterprise.ServiceManager.Tasks.DummySleepingTask", "~01", Guid.Empty, string.Empty);
						var stopWatch = Stopwatch.StartNew();
						while (!Mutex.TryOpenExisting(DummySleepingTask.MutexLock, out var mutex))
						{
							if (stopWatch.ElapsedMilliseconds > 5000)
							{
								return;
							}

							DbConnectionKiller.KillOtherConnections(admConn, Db.DatabaseName);
						}
					}
				});

				// Assert
				controller.Wait(cancellationTokenSource.Token);
				process.WaitForExit(5000);
				AssertEquals((int)RunnerExitCode.RunnerFailure, process.ExitCode);

				Process StartProcess()
				{
					var startInfo = GetProcessStartInfo();
					startInfo.RedirectStandardOutput = true;
					startInfo.RedirectStandardError = true;
					startInfo.UseShellExecute = false;
					var p = new Process { StartInfo = startInfo, EnableRaisingEvents = true, };
					p.ErrorDataReceived += ErrorReceivedHandler;
					p.OutputDataReceived += DataReceivedHandler;
					var started = p.Start();
					p.BeginErrorReadLine();
					p.BeginOutputReadLine();
					return p;
				}

				ProcessStartInfo GetProcessStartInfo()
				{
					Db.Connection.EnsureIsOpen();

					var fileName = Path.Combine(AppContext.BaseDirectory, ServiceManagerConstants.ServiceManagerRunnerExe);
					var arguments = Invariant($" -NoUI \"-grpc:{eventHandleNames.BaseEventWaitHandleName}\" {Db.ServerName} {Db.DatabaseName} -ProductKey:ProductKey");

					var sdir = Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith(HostCommandLineOptions.ServerDirectoryForEnterpriseArgumentPrefix, StringComparison.OrdinalIgnoreCase));
					if (sdir != null)
					{
						arguments += " " + CommandLineArgEncoder.EnquoteArgumentIfNeeded(sdir);
					}

					var result = new ProcessStartInfo(fileName, arguments) { WorkingDirectory = AppContext.BaseDirectory, UseShellExecute = false, };

					return result;
				}

				void DataReceivedHandler(object sender, DataReceivedEventArgs e)
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						var grpcPortStrings = e.Data.Split(':');
						AssertEquals("Only STDOUT should be GRPC port", 2, grpcPortStrings.Length);
						AssertEquals("Only STDOUT should be GRPC port", "GRPC", grpcPortStrings[0]);
						grpcPort = int.Parse(grpcPortStrings[1]);
						_ = outputData.AppendLine(e.Data);
					}
				}

				void ErrorReceivedHandler(object sender, DataReceivedEventArgs e)
				{
					if (!string.IsNullOrEmpty(e.Data))
					{
						_ = errorData.AppendLine(e.Data);
					}
				}

				RunnerCommandQueueProvider GetCommandQueueProviderForProcess(GrpcClientSynchronizer sync, Func<int?> portFunc)
				{
					var grpcPortResolverMock = new Mock<IGrpcPortResolver>();
					grpcPortResolverMock
						.SetupGet(r => r.PortOpened)
						.Returns(() => portFunc() != null);
					sync.WaitForServerReadySignal(
						Mock.Of<IRunnableServiceTask>(),
						grpcPortResolverMock.Object,
						Mock.Of<IProcess>(p => !p.HasExited),
						TimeSpan.FromSeconds(60));
					var port = portFunc().Value;
					return new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), port, Mock.Of<IErrorReporterProxy>());
				}
			}
		}

		public void TestUpgradeExceptionAtStartupIsHandled()
		{
			using (var appDomainWrapper = new AppDomainWrapper(nameof(TestUpgradeExceptionAtStartupIsHandled)))
			{
				var domainData = new Dictionary<string, object>
				{
					{ "ServerName", Db.ServerName },
					{ "DatabaseName", Db.DatabaseName }
				};
				var returnData = new Dictionary<string, object>
				{
					{ "ExitCode", null }
				};

				var result = appDomainWrapper.RunActionInAppDomain(() =>
				{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
					Db.InitializeDatabaseDetails(
						(string)AppDomain.CurrentDomain.GetData("ServerName"),
						(string)AppDomain.CurrentDomain.GetData("DatabaseName"));
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

					EnterpriseApplicationConfiguration.ConfigureObjectFactory();
					int result;
					var grpcEventHandles = new GrpcEventHandleNames();
					using (var consoleOutput = new ConsoleOutput())
					using (var grpcClientSynchronizer = new GrpcClientSynchronizer(grpcEventHandles))
					using (var admConn = Db.NewAdminConnection())
					{
						var preTestOpenRetries = Db.Connection.OpenRetries;
						try
						{
							AssertEquals(DbLockoutState.AquiredLockout, admConn.AcquireLockout());
							DbConnectionKiller.KillOtherConnections(admConn, Db.DatabaseName);
							Db.Connection.OpenRetries = 0;
							var main = Task.Run(() => ApplicationStarter.Main(new[] { Db.ServerName, Db.DatabaseName, "-grpc:" + grpcEventHandles.BaseEventWaitHandleName, "-ProductKey:ProductKey", }));
							grpcClientSynchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == !string.IsNullOrEmpty(consoleOutput.GetOuput())), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(60));
							main.Wait(new CancellationTokenSource(TimeSpan.FromSeconds(15)).Token);
							result = main.Result;
						}
						finally
						{
							Db.Connection.OpenRetries = preTestOpenRetries;
							AssertEquals(DbLockoutState.ResetLockout, admConn.ResetLockout());
						}
					}

#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
					AppDomain.CurrentDomain.SetData("ExitCode", result);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
				}, domainData, returnData);

				AssertNotNull(result["ExitCode"]);
				AssertEquals(RunnerExitCode.RunnerFailure, (RunnerExitCode)result["ExitCode"]);
			}
		}

		public void TestUpgradeExceptionAtLoggerFinalizationIsHandled()
		{
			// Arrange
			var applicationExceptionHandlerMock = new Mock<IApplicationExceptionHandler>();

			using var adminConnection = Db.NewAdminConnection();
			IDisposable locker = null;
			applicationExceptionHandlerMock
				.Setup(handler => handler.HandleFromInitialization(It.IsAny<Exception>(), It.IsAny<Func<IRunnerLogger>>()))
				.Callback(() =>
				{
					adminConnection.AcquireLockout(LockoutReason.Upgrade);
					locker = new DisposableAction(() => adminConnection.ResetLockout());
				});

			using var a = new ApplicationStarter(ApplicationLoggingTestHelper.MockLoggerFactory(), applicationExceptionHandlerMock.Object);
			using var memoryStream = new MemoryStream();
			using var stream = new StreamWriter(memoryStream);
			using (new DisposableAction(() => Console.SetError(stream), () => Console.Error.Close()))
			{
				using (locker)
				{
					// Act
					Db.Connection.CloseConnection();
					a.Run(new[] { Db.ServerName, Db.DatabaseName, "-ProductKey:ProductKey" });
				}

				// Assert
				var result = Encoding.UTF8.GetString(memoryStream.ToArray());
				AssertEquals(string.Empty, result);
			}
		}

		class NLogCloseErrorTest : TestCase
		{
			public void TestExceptionThrowByAppDomainInDisposeAndFlush()
			{
				using (ErrorReporter.SetTemporaryInstanceForTest(Mock.Of<IErrorReporter>()))
				using (var appDomainWrapper = new AppDomainWrapper(nameof(TestExceptionThrowByAppDomainInDisposeAndFlush)))
				{
					var domainData = new Dictionary<string, object>
					{
						{ "ServerName", Db.ServerName },
						{ "DatabaseName", Db.DatabaseName }
					};
					var returnData = new Dictionary<string, object>
					{
						{ "ExitCode", null }
					};

					var result = appDomainWrapper.RunActionInAppDomain(() =>
					{
						//Arrange
						var taskWaitResult = false;

						var targetMock = new Mock<Target>();

						targetMock
							.Protected()
							.SetupSequence("CloseTarget")
							.Throws(new OutOfMemoryException())
							.Pass();
						targetMock.Object.Name = "TargetException";

						LogManager.Configuration = new LoggingConfiguration();
						LogManager.Configuration.AddTarget(targetMock.Object);
						LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", targetMock.Object) { RuleName = $"{ServiceManagerHelper.HostLoggerCode}_{nameof(targetMock)}" });
						LogManager.ReconfigExistingLoggers();

#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
						var serverName = (string)AppDomain.CurrentDomain.GetData("ServerName");
						var databaseName = (string)AppDomain.CurrentDomain.GetData("DatabaseName");
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.

						//Act
						var grpcEventHandles = new GrpcEventHandleNames();
						using (var consoleOutput = new ConsoleOutput())
						using (var grpcClientSynchronizer = new GrpcClientSynchronizer(grpcEventHandles))
						{
							var exitCode = (int)RunnerExitCode.RunnerFailure;
							var main = Task.Run(() => exitCode = Enterprise.ServiceManager.Runner.Program.Main(new[] { serverName, databaseName, "-grpc:" + grpcEventHandles.BaseEventWaitHandleName, "-SingleRun", "-ProductKey:MyProduct" }));
							grpcClientSynchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == !string.IsNullOrEmpty(consoleOutput.GetOuput())), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(60));
							taskWaitResult = main.Wait(TimeSpan.FromMinutes(5)); // Long wait as we have no host to correctly close the grpc stream, so it must time out to close
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
							AppDomain.CurrentDomain.SetData("ExitCode", exitCode);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
						}

						//Assert
						Assert("Program should close in 5 minutes", taskWaitResult);

						targetMock
							.Protected()
							.Verify("FlushAsync", Times.Once(), ItExpr.IsAny<AsyncContinuation>());

						targetMock
							.Protected()
							.Verify("CloseTarget", Times.Once());
					}, domainData, returnData);

					AssertNotNull(result["ExitCode"]);
					AssertEquals(RunnerExitCode.NoIssues, (RunnerExitCode)result["ExitCode"]);
				}
			}
		}

		static RunnerCommandQueueProvider GetCommandQueueProvider(GrpcClientSynchronizer grpcClientSynchronizer, ConsoleOutput consoleOutput)
		{
			var stdOut = string.Empty;
			var grpcPortResolverMock = new Mock<IGrpcPortResolver>();
			grpcPortResolverMock
				.SetupGet(r => r.PortOpened)
				.Returns(() =>
				{
					stdOut = consoleOutput.GetOuput();
					return !string.IsNullOrEmpty(stdOut);
				});
			grpcClientSynchronizer.WaitForServerReadySignal(
				Mock.Of<IRunnableServiceTask>(),
				grpcPortResolverMock.Object,
				Mock.Of<IProcess>(p => !p.HasExited),
				TimeSpan.FromSeconds(60));
			var grpcPortStrings = stdOut.Split(':');
			AssertEquals("Only STDOUT should be GRPC port", 2, grpcPortStrings.Length);
			AssertEquals("Only STDOUT should be GRPC port", "GRPC", grpcPortStrings[0]);
			var grpcPort = int.Parse(grpcPortStrings[1]);
			return new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), grpcPort, Mock.Of<IErrorReporterProxy>());
		}
	}

	class ConsoleOutput : IDisposable
	{
		public ConsoleOutput()
		{
			stringWriter = new StringWriter();
			originalOutput = Console.Out;
			Console.SetOut(stringWriter);
		}

		public string GetOuput()
		{
			return stringWriter.ToString().Trim();
		}

		public void Dispose()
		{
			Console.SetOut(originalOutput);
			stringWriter.Dispose();
		}

		readonly StringWriter stringWriter;
		readonly TextWriter originalOutput;
	}
}
