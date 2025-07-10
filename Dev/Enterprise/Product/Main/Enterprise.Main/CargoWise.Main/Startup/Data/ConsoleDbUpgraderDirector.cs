#if DEBUG

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.Startup
{
	sealed class ConsoleDbUpgraderDirector : DbUpgraderDirector
	{
		readonly bool keepConsoleOpenOnError;

		public ConsoleDbUpgraderDirector(bool keepConsoleOpenOnError)
		{
			this.keepConsoleOpenOnError = keepConsoleOpenOnError;
			NativeMethods.EnsureConsole();
		}

		protected override bool LoginForUpgrade()
		{
			return true;
		}

		protected internal override void ShowDenyUpgradeMessage(string messageSufix, string denyReason, string denyReasonFullExplanation)
		{
			Console.WriteLine("Unable to upgrade: {0}", messageSufix);
			Console.WriteLine(denyReasonFullExplanation ?? denyReason);
		}

		protected override void ShowSoftwareUpgradeSuccessMessage(Version version, string information)
		{
			if (!string.IsNullOrEmpty(information))
			{
				Console.WriteLine("Upgrade to version {0} completed successfully. However, an exception occurred during upgrade conclusion steps, and some post upgrade cleanup may not have finished: {1}", version, information);
			}
			else
			{
				Console.WriteLine("Successfully upgraded to version {0}", version);
			}
		}

		protected override ValidationResponse DoUpgrade()
		{
			var databaseName = Db.DatabaseName;
			Console.Title = "Upgrading " + databaseName;
			var fullSilentUpgrade = ObjectFactory.Get<IDbUpgraderRunner>().FullSilentUpgrade(UpgradeVersionInfo, softwareUpgrade, OnUpgraderEvent);

			if (keepConsoleOpenOnError && !fullSilentUpgrade.Successful)
			{
				Console.Title = "Failed to Upgrade " + databaseName;
				Console.WriteLine("Upgrade failed for database {0}. Please see the log above for details.", databaseName);
				Console.ReadKey();
			}

			return fullSilentUpgrade;
		}

		protected override string UpgradeLogDirectory
		{
			get { throw new NotSupportedException(); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Console I/O is required for a console-based UI.")]

		void OnUpgraderEvent(UpgradeEventType eventType, string message)
		{
			if (eventType == UpgradeEventType.TaskFailed)
			{
				Console.WriteLine();
			}
			Console.Write(LogTimestamp);
			Console.Write('\t');
			if (eventType == UpgradeEventType.SubtaskStarted)
			{
				Console.Write('\t');
			}
			Console.Write(message);
			Console.WriteLine();
			if (eventType == UpgradeEventType.TaskFailed)
			{
				Console.WriteLine();
			}
		}
	}

	static class NativeMethods
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AttachConsole(uint dwProcessId);

		const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;
		const int ERROR_ACCESS_DENIED = 5;
		const int ERROR_INVALID_HANDLE = 6;

		public static void EnsureConsole()
		{
			if (!AttachConsole(ATTACH_PARENT_PROCESS))
			{
				var hresult = Marshal.GetLastWin32Error();
				if (hresult != ERROR_ACCESS_DENIED && hresult != ERROR_INVALID_HANDLE)
				{
					throw new Win32Exception(hresult);
				}

				if (!AllocConsole())
				{
					throw new Win32Exception();
				}
			}
		}
	}
}

#endif
