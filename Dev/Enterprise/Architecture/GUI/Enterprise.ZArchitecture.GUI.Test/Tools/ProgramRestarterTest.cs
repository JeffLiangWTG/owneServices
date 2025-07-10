using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	public class ProgramRestarterTest : TestCase
	{
		[GuiTest]
		[UseSnapshotProtection]
		public void TestRestart()
		{
			var newProcessName = "";

			newProcessName = ExeFileNames.CargoWiseWindowsDesktopExe;

			var runCwo64bit = (newProcessName == ExeFileNames.CargoWiseWindowsDesktopExe);
			newProcessName = Path.GetFileNameWithoutExtension(newProcessName);
			var actualArguments = CommandLineArguments.UsedToLaunchApplication;
			const string testUrl = "edient:Command=ShowEditForm&LicenceCode=EDIEDIDAT&ControllerID=GlbBranch&BusinessEntityPK=27a55065-ac88-4ec3-8bed-e575e79172cb&Hash=%2bHrRB8nOtdPehrZ1%2bwV%2fgLmHrUYzaSCVm";
			try
			{
				CommandLineArguments.UsedToLaunchApplication = new CommandLineArguments(new string[] { Db.ServerName, Db.DatabaseName }, new Hashtable());
				using (ObjectFactory.Substitute<IWindowPersister>(new MockWindowPersister(testUrl)))
				{
					var existingProcesses = ProcessLocator.Instance.GetCurrentUserVisibleProcessesByName(newProcessName).Select(p => p.Id);

					ProgramRestarter.Instance.Restart();

					Process newProcess = null;
					var hasNewProcess = false;
					var stopwatch = Stopwatch.StartNew();
					do
					{
						Thread.Sleep(100);
						foreach (var process in ProcessLocator.Instance.GetCurrentUserVisibleProcessesByName(newProcessName).Where(p => !existingProcesses.Contains(p.Id)))
						{
							hasNewProcess = true;
							if (GetOpenWindowsFromPID(process.Id).Any(w => w.Contains("Edit Branch")))
							{
								newProcess = process;
							}
						}
					}
					while (newProcess == null && stopwatch.Elapsed < TimeSpan.FromMinutes(2));
					Assert("New process found", hasNewProcess);
					AssertNotNull("'Edit Branch' window should be open in new process", newProcess);
					newProcess.Kill();
				}
			}
			finally
			{
				CommandLineArguments.UsedToLaunchApplication = actualArguments;
			}
		}

		delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
		[DllImport("user32.dll", SetLastError = true)]
		static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
		[DllImport("USER32.DLL")]
		static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);
		[DllImport("USER32.DLL")]
		static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
		[DllImport("USER32.DLL")]
		static extern int GetWindowTextLength(IntPtr hWnd);
		[DllImport("USER32.DLL")]
		static extern bool IsWindowVisible(IntPtr hWnd);
		[DllImport("USER32.DLL")]
		static extern IntPtr GetShellWindow();
		public IEnumerable<string> GetOpenWindowsFromPID(int processID)
		{
			IntPtr hShellWindow = GetShellWindow();
			var windows = new List<string>();
			EnumWindows(delegate(IntPtr hWnd, int lParam)
			{
				if (hWnd == hShellWindow)
				{
					return true;
				}

				if (!IsWindowVisible(hWnd))
				{
					return true;
				}

				int length = GetWindowTextLength(hWnd);
				if (length == 0)
				{
					return true;
				}

				uint windowPid;
				GetWindowThreadProcessId(hWnd, out windowPid);
				if (windowPid != processID)
				{
					return true;
				}

				StringBuilder stringBuilder = new StringBuilder(length);
				GetWindowText(hWnd, stringBuilder, length + 1);
				windows.Add(stringBuilder.ToString());
				return true;
			}, 0);
			return windows;
		}

		class MockWindowPersister : IWindowPersister
		{
			public MockWindowPersister(string openFormUrls)
			{
				this.openFormUrls = openFormUrls;
			}

			public string GetOpenFormUrls()
			{
				return openFormUrls;
			}

			readonly string openFormUrls;
		}

		#region TestCaughtUpgradeExceptionReportsErrorOnReconnectBeforeRestart
		[UseSnapshotProtection]
		public void TestCaughtUpgradeExceptionReportsErrorOnReconnectBeforeRestart()
		{
			var actualDbEnv = DbEnv.Instance;
			using (var extraConnection = Db.Connection)
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					DbConnectionKiller.KillOtherConnections(adminConnection, Db.DatabaseName);
					try
					{
						AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => extraConnection.BeginTransaction());
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) + 1, adminConnection);
					}
					finally
					{
						adminConnection.ResetLockout();
					}

					try
					{
						DbEnv.SetDbEnvironment(new MockWinFormsDbEnvironmentForTest());
						Action<DatabaseUpgradeException> handleDataBaseUpgradeException = (dataBaseUpgradeException) =>
						{
							((MockWinFormsAppDbConnectionGuiPluginForTest)DbEnv.Instance.ConnectionGuiPlugin).HandleDatabaseUpgradeException(dataBaseUpgradeException);
						};
						AssertNoExceptionThrown(() => handleDataBaseUpgradeException(new DatabaseUpgradeInProgressException()));
					}
					finally
					{
						DbRegistry.DatabaseMajorSchemaVersion.SaveValue(DbRegistry.DatabaseMajorSchemaVersion.LoadValue(adminConnection) - 1, adminConnection);
						ErrorReporter.Clear();
						DbEnv.SetDbEnvironment(actualDbEnv);
					}
				}
			}
		}

		class MockWinFormsDbEnvironmentForTest : BaseDbEnvironment
		{
			public override IDbConnectionGuiPlugin ConnectionGuiPlugin { get; } = new MockWinFormsAppDbConnectionGuiPluginForTest();
		}

		class MockWinFormsAppDbConnectionGuiPluginForTest : IDbConnectionGuiPlugin
		{
			public void CheckVersionUpgraded()
			{
				throw new NotImplementedException();
			}

			public bool GetUserConfirmation(string caption, string yesNoQuestion)
			{
				return true;
			}

			public void HandleDatabaseUpgradeException(DatabaseUpgradeException ex)
			{
				if (ex is DatabaseUpgradedException)
				{
					ObjectFactory.DisposeSubstitutions();
					using (ObjectFactory.Substitute<IWindowPersister>(new MockWindowPersisterWhenHandleDataBaseUpgradeException()))
					{
						var arguments = CommandLineArguments.UsedToLaunchApplication.Clone();
						ProgramRestarter.Instance.RestartCore(arguments);
					}
				}
				else if (ex is DatabaseUpgradeInProgressException)
				{
					HandleDatabaseUpgradeException(new DatabaseUpgradedException());
				}
			}

			public void HandleDbConcurrencyException(Exception ex)
			{
				throw new NotImplementedException();
			}

			public IDisposable NewConnectingSplashFormManager()
			{
				return null;
			}
		}

		class MockWindowPersisterWhenHandleDataBaseUpgradeException : IWindowPersister
		{
			public string GetOpenFormUrls()
			{
				string openFormUrls = "";
				using (DbCommand command = Db.Connection.Command("SELECT getdate() "))
				{
					openFormUrls = "edient:Command=ShowEditForm&LicenceCode=" + command.ExecuteScalar().ToString() + "&ControllerID=GlbBranch&BusinessEntityPK=27a55065-ac88-4ec3-8bed-e575e79172cb&Hash=%2bHrRB8nOtdPehrZ1%2bwV%2fgLmHrUYzaSCVm";
				}

				return openFormUrls;
			}
		}
		#endregion
	}
}
