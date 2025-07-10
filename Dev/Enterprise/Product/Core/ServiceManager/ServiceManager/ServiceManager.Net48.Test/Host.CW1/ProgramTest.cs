using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ServiceManager.Shared.Testing;
using Enterprise.ServiceManager.Shared.Testing.Logging;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Win32;
using Moq;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Host.CW;
using ServiceManager.Integration.ServiceHostUtilities;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host.Testing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "RS0030:SYSLIB0024", Justification = "Pending Migration")]   // WI00669071 - Do not use System.AppDomain.
	public class ProgramTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending Migration")]   // WI00669071 - Do not use System.AppDomain.
		public void TestCorrectEnvironmentIsLoaded()
		{
			// Arrange
			Globals.IsUserInteractive = false;
			var appDomain = AppDomain.CreateDomain(nameof(TestCorrectEnvironmentIsLoaded));
			try
			{
				appDomain.DoCallBack(() =>
				{
					// Arrange
					var dbConnectionSetup = new Mock<IDbConnectionSetup>();
					dbConnectionSetup.Setup(o => o.TryConnectAndHandleErrors()).Throws<Exception>();

					var serviceManagerHost = new ServiceCollection()
						.AddRegistrations(new[] { "server", "database" })
						.RemoveAll<IDbConnectionSetup>()
						.AddSingleton(o => dbConnectionSetup.Object)
						.BuildServiceProvider()
						.GetService<ServiceManagerHost>();

					// Act
					serviceManagerHost.Run();

					AppDomain.CurrentDomain.SetData("env", DbEnv.Instance.GetType().FullName);
					AppDomain.CurrentDomain.SetData("isPooling", DbEnv.Instance.ConnectionPooling.IsPooling);
				});

				// Assert
				AssertEquals(typeof(ServiceManagerDbEnvironmentPooled).FullName, (string)appDomain.GetData("env"));
				AssertEquals(true, (bool)appDomain.GetData("isPooling"));
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
		}

		public void TestServerAndDatabaseAreRequired()
		{
			Test(null, null, 2);
			Test(Db.ServerName, null);
			Test(null, Db.DatabaseName);

			void Test(string server, string db, int errorCount = 1)
			{
				var argv = new List<string>();
				if (server != null)
				{
					argv.Add(server);
				}
				if (db != null)
				{
					argv.Add(db);
				}

				var e = AssertExceptionThrown<CommandLineException>(() => Program.Main(argv.ToArray()));

				for (int i = 0; i < errorCount; i++)
				{
					AssertContains("Required argument missing for command:", e.Errors.ElementAt(i).Message);
				}
			}
		}

		[TestRequiresAdministrativePrivileges("Requires read and write Windows event log")]
		public void TestInstallExceptionsAreHandled()
		{
			ExceptionSource
				.NotCriticalExceptions
				.Concat(ExceptionSource.CriticalExceptions)
				.ForEach(x => TestInstallUninstall("install", x));
		}

		[TestRequiresAdministrativePrivileges("Requires read and write Windows event log")]
		public void TestUninstallExceptionsAreHandled()
		{
			ExceptionSource
				.NotCriticalExceptions
				.Concat(ExceptionSource.CriticalExceptions)
				.ForEach(x => TestInstallUninstall("uninstall", x));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending Migration")]   // WI00669071 - Do not use System.AppDomain.
		static void TestInstallUninstall<T>(string installUninstall, T exception) where T : Exception, new()
		{
			// Arrange
			var expectedMessagePrefix = $"CargoWiseOneServiceHost has encountered an {installUninstall} error";

			var appDomain = AppDomain.CreateDomain(nameof(TestInstallUninstall));
			appDomain.SetData("argv", new[] { $"-{installUninstall}", Db.ServerName, Db.DatabaseName });
			appDomain.SetData("exception", exception);
			var timeBeforeProgramStart = DateTime.Now;

			using (new DisposableAction(() => AppDomain.Unload(appDomain)))
			{
				appDomain.DoCallBack(() =>
				{
					using var memoryTarget = new MemoryTarget(nameof(MemoryTarget));
					using (LoggerTestHelper.TemporaryLoggingConfigurationWithTarget(memoryTarget))
					{
						var copiedException = (T)AppDomain.CurrentDomain.GetData("exception");
						var productKeyMock = new Mock<IProductRegistrationKey>();
						productKeyMock
							.Setup(a => a.EnterpriseCode)
							.Returns(() =>
							{
								var stackTrace = new StackTrace();
								if (stackTrace.ToString().Contains(nameof(ServiceInstallerStartupCommand)))
								{
									throw copiedException;
								}
								return string.Empty;
							});

						var productRegistrationMock = new Mock<IProductRegistration>();
						productRegistrationMock
							.Setup(x => x.Key)
							.Returns(productKeyMock.Object);

						var argv = (string[])AppDomain.CurrentDomain.GetData("argv");

						using (CargoWise.Application.ObjectFactory.Substitute(productRegistrationMock.Object))
						{
							// Act
							var result = Program.Main(argv);
							AppDomain.CurrentDomain.SetData("result", result);
						}

						// Assert
						// Note: NUnit4 assertions do not play nice with AppDomain
						var logs = memoryTarget.Logs.Where(o => o.Contains("CargoWiseOneServiceHost has encountered an"));
						AssertGreaterThan($"Error log not found, logs contain {string.Join("\r\n", memoryTarget.Logs)}", logs.Count(), 0);
					}
				});

				CombineAssertions(() =>
				{
					AssertEquals("Process exited with error", -1, (int)appDomain.GetData("result"));
				});
			}
		}

		[TestRequiresAdministrativePrivileges("Requires read and write Windows event log")]
		public void TestHandlingReportExceptionForInstall()
		{
			ExceptionSource
				.NotCriticalExceptions
				.Concat(ExceptionSource.CriticalExceptions)
				.ForEach(x =>
				{
					TestHandlingReportExceptionForInstallUninstall("install", x);
				});
		}

		[TestRequiresAdministrativePrivileges("Requires read and write Windows event log")]
		public void TestHandlingReportExceptionForUninstall()
		{
			ExceptionSource
				.NotCriticalExceptions
				.Concat(ExceptionSource.CriticalExceptions)
				.ForEach(x =>
				{
					TestHandlingReportExceptionForInstallUninstall("uninstall", x);
				});
		}

		[UseSnapshotProtection(true)]
		[TestRequiresAdministrativePrivileges("Installation and uninstallation of the service")]
		public void TestUninstallDeletesHostRecord()
		{
			// Arrange

			var dummyHosts = new[] { "host1", "host2" };
			var hostExePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ServiceManager.Host.CW.exe");
			var serviceName = ServiceHostProcess.GetServiceName(ServiceType.ProcessController, Db.ServerName, Db.DatabaseName);

			CreateHostRecordsInDatabase();

			using (new DisposableAction(CreateProcessControllerServiceIfAbsents, DeleteProcessControllerServiceIfExists))
			{
				var startInfo = new ProcessStartInfo
				{
					FileName = hostExePath,
					Arguments = string.Join(" ",
						new[]
							{
								Db.ServerName,
								Db.DatabaseName,
								HostCommandLineOptions.OptionUninstall,
								HostCommandLineOptions.RemoveDbRecord,
							}
							.Select(s => $"\"{s}\"")),
					CreateNoWindow = true,
				};

				// Act
				using (var process = Process.Start(startInfo))
				{
					// Assert
					AssertEquals(true, process.WaitForExit((int)TimeSpan.FromMinutes(1).TotalMilliseconds));
					AssertGreaterThanOrEqualTo(process.ExitCode, 0);

					var result = ReadAllHostRecordsFromDatabase();
					AssertContainsExactElementsInAnyOrder(StringComparer.OrdinalIgnoreCase, dummyHosts, result);
				}
			}

			void CreateHostRecordsInDatabase()
			{
				var factory = new BusinessObjectFactory();
				CreateHost(ServiceManagerHelper.GetHostName());
				foreach (var dummyHost in dummyHosts)
				{
					CreateHost(dummyHost);
				}

				factory.Save();

				void CreateHost(string hostName)
				{
					var host = factory.New<StmServiceHost>();
					host.SH_HostName = hostName;
					host.SH_IsActive = true;
				}
			}

			IEnumerable<string> ReadAllHostRecordsFromDatabase()
			{
				var factory = new BusinessObjectFactory();
				return factory
					.Load<StmServiceHost>(new ZQuery())
					.Select(host => host.SH_HostName.ToString());
			}

			bool ServiceExists()
			{
				return ServiceController
					.GetServices()
					.Any(controller => string.Equals(controller.ServiceName, serviceName));
			}

			void CreateProcessControllerServiceIfAbsents()
			{
				if (!ServiceExists())
				{
					using (var process = Process.Start("sc", $"CREATE \"{serviceName}\" start=disabled binpath= \"{hostExePath}\""))
					{
						process.WaitForExit();
						AssertEquals("Installation of the service", 0, process.ExitCode);
					}
				}
			}

			void DeleteProcessControllerServiceIfExists()
			{
				if (ServiceExists())
				{
					using (var process = Process.Start("sc", $"DELETE \"{serviceName}\""))
					{
						process.WaitForExit();
						AssertEquals("Removal of the service", 0, process.ExitCode);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending Migration")]   // WI00669071 - Do not use System.AppDomain.
		static void TestHandlingReportExceptionForInstallUninstall<T>(string installUninstall, T exception) where T : Exception, new()
		{
			// Arrange
			var expectedMessagePrefix = $"CargoWiseOneServiceHost has encountered an {installUninstall} error";

			var appDomain = AppDomain.CreateDomain(nameof(TestInstallUninstall));
			appDomain.SetData("argv", new[] { $"-{installUninstall}", Db.ServerName, Db.DatabaseName });
			appDomain.SetData("exception", exception);
			var timeBeforeProgramStart = DateTime.Now;

			using (new DisposableAction(() => AppDomain.Unload(appDomain)))
			{
				appDomain.DoCallBack(() =>
				{
					using var memoryTarget = new MemoryTarget(nameof(MemoryTarget));
					using (LoggerTestHelper.TemporaryLoggingConfigurationWithTarget(memoryTarget))
					{
						var copiedException = (T)AppDomain.CurrentDomain.GetData("exception");
						var productRegistrationMock = new Mock<IProductRegistration>();
						productRegistrationMock
							.SetupSequence(x => x.Key)
							.Returns(Mock.Of<IProductRegistrationKey>())
							.Throws(copiedException);

						var argv = (string[])AppDomain.CurrentDomain.GetData("argv");

						using (CargoWise.Application.ObjectFactory.Substitute(productRegistrationMock.Object))
						{
							// Act
							var result = Program.Main(argv);
							AppDomain.CurrentDomain.SetData("result", result);
						}

						// Assert
						AssertEquals(true, memoryTarget.Logs.Any(o => o.Contains(copiedException.Message)));
					}
				});

				CombineAssertions(() =>
				{
					AssertEquals("Process exited with error", -1, (int)appDomain.GetData("result"));
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			envProvider = Env.GetCurrentProvider();
			serviceManagerEnvProvider = new ServiceManagerEnvProvider(true);
			serviceManagerEnvProvider.Enable();
		}

		protected override void TearDown()
		{
			envProvider.Enable();
			serviceManagerEnvProvider.Dispose();
			base.TearDown();
		}

		class TcpIpSettingsTest : TestCase
		{
			[TestRequiresAdministrativePrivileges("Installs service and writes to registry")]
			[RequiresSoftware(RequiredSoftware.IsVM)]
			public void TestInstallSetsTcpIpSettings()
			{
				//Arrange
				Uninstall(silent: true);

				DeleteTcpIpRegistrySettings();

				using (new DisposableAction(() => Uninstall(silent: true)))
				{
					//Act
					Install();

					//Assert
					var (maxUserPort, timedWaitDelay) = ReadTcpIpRegistrySettings();
					AssertEquals("Expected maxUserPort be set during install.", 65534, maxUserPort);
					AssertEquals("Expected timedWaitDelay be set during install.", 30, timedWaitDelay);
				}
			}

			[TestRequiresAdministrativePrivileges("Installs service and writes to registry")]
			[RequiresSoftware(RequiredSoftware.IsVM)]
			public void TestUninstallLeavesTcpIpSettings()
			{
				using (new DisposableAction(() => Uninstall(silent: true)))
				{
					//Arrange
					Install();

					//Act
					Uninstall();

					//Assert
					var (maxUserPort, timedWaitDelay) = ReadTcpIpRegistrySettings();
					AssertEquals("Expected not to touch maxUserPort value during uninstall.", maxUserPort, 65534);
					AssertEquals("Expected not to touch timedWaitDelay value during uninstall.", timedWaitDelay, 30);
				}
			}

			void Install(bool silent = false)
			{
				InstallUninstall("install", silent);
			}

			void Uninstall(bool silent = false)
			{
				InstallUninstall("uninstall", silent);
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending Migration")]   // WI00669071 - Do not use System.AppDomain.
			void InstallUninstall(string installUninstall, bool silent)
			{
				var appDomain = AppDomain.CreateDomain(nameof(TestInstallUninstall));
				appDomain.SetData("argv", new[] { $"-{installUninstall}", Db.ServerName, Db.DatabaseName });

				using (new DisposableAction(() => AppDomain.Unload(appDomain)))
				{
					appDomain.DoCallBack(() =>
					{
						var argv = (string[])AppDomain.CurrentDomain.GetData("argv");
						var textWritter = new StringWriter();
						Console.SetError(textWritter);

						AppDomain.CurrentDomain.SetData("success", Program.Main(argv) == 0);
						AppDomain.CurrentDomain.SetData("error", textWritter.ToString());
					});

					if (!silent)
					{
						var error = (string)appDomain.GetData("error");
						var success = (bool)appDomain.GetData("success");

						CombineAssertions(() =>
						{
							AssertEquals(true, success);
							AssertEquals(string.Empty, error);
						});
					}
				}
			}

			(int? maxUserPort, int? timedWaitDelay) ReadTcpIpRegistrySettings()
			{
				using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				using (var key = hklm32.OpenSubKey(TcpipRegistrySubKey, false))
				{
					if (key == null)
					{
						return (null, null);
					}

					var maxUserPort = key.GetValue(TcpipRegistryParameter_MaxUserPort) as int?;
					var timedWaitDelay = key.GetValue(TcpipRegistryParameter_TcpTimedWaitDelay) as int?;

					return (maxUserPort, timedWaitDelay);
				}
			}

			void DeleteTcpIpRegistrySettings()
			{
				using (var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
				using (var key = hklm32.OpenSubKey(TcpipRegistrySubKey, true))
				{
					key.DeleteValue(TcpipRegistryParameter_MaxUserPort, throwOnMissingValue: false);
					key.DeleteValue(TcpipRegistryParameter_TcpTimedWaitDelay, throwOnMissingValue: false);
				}
			}

			const string TcpipRegistrySubKey = @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters";
			const string TcpipRegistryParameter_MaxUserPort = "MaxUserPort";
			const string TcpipRegistryParameter_TcpTimedWaitDelay = "TcpTimedWaitDelay";
		}

		ServiceManagerEnvProvider serviceManagerEnvProvider;
		EnvProvider envProvider;
	}
}
