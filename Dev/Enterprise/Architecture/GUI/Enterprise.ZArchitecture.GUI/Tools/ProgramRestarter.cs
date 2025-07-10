using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Windows.UI.Controls.Internal;
using Enterprise.Core;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public class ProgramRestarter : IProgramRestarter
	{
		public static ProgramRestarter Instance
		{
			get => instance ?? (instance = new ProgramRestarter());
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static ProgramRestarter instance;

		ProgramRestarter()
		{
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Command line argument, shouldn't be localized")]
		public const string OptionPersist = "-Persist:";

		/// <summary>
		/// DLL Initialization Failed
		/// </summary>
		/// <remarks>
		/// 0xC000026B {DLL Initialization Failed} The application failed to initialize because the window station is shutting down.
		/// </remarks>
		/// <see cref="https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-erref/596a1078-e883-4972-9bbc-49e60bebca55"/>
		const int STATUS_DLL_INIT_FAILED_LOGOFF = -1073741205;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "False alarm, we are starting Enterprise not opening a file or url, May not have access to DB")]
		public void Restart(string exeFilePath = null, CommandLineArguments arguments = null)
		{
			try
			{
				exeFilePath ??= DefaultExeFilePath;
				arguments ??= CommandLineArguments.UsedToLaunchApplication.Clone();
				RestartCore(arguments);
				var startInfo = new ProcessStartInfo(exeFilePath, arguments.ToString());
				startInfo.UseShellExecute = false;
				var newCw1Process = Process.Start(startInfo);
				Thread.Sleep(TimeSpan.FromSeconds(1)); // Wait one seconds to give the new process a chance to start
				HandleProcessExit(newCw1Process);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				MessageBox.Show(Constants.ProductName + " was unable to restart itself automatically.", Constants.ProductName + " Closing", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
				throw;
			}
			finally
			{
				if (!Globals.IsTest)
				{
					ExceptionReporter.SuppressGui();
					GCTracker.StopTracking();
					KillCurrentProcess();
				}
			}

			void KillCurrentProcess()
			{
				// CW supposes to exit immediately, however, according to MS, process might be killed asynchronously.
				// Just make thread wait for 2 secs to let CW end gracefully.
				// Reference: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.kill, https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-terminateprocess
				Task.Run(() => Process.GetCurrentProcess().Kill());
				Thread.Sleep(TimeSpan.FromSeconds(2));
				using (Db.DisableSchemaVersionCheckOnCurrentThread())
				{
					ErrorReporter.ReportOnce("CWIsNotKilledImmediately", "Please check PC I/O to see why it is not shutdown immediately.");
				}
			}
		}

		static string DefaultExeFilePath => Path.Combine(AssemblyLoader.GetBinPath(), ExeFileNames.CargoWiseWindowsDesktopExe);

		static bool ShouldReportProcessExitError(int exitCode)
		{
			int[] allowedExitCodes =
			{
				CargoWise.Definitions.ExitCodes.Success,
				CargoWise.Definitions.ExitCodes.InstallCurrentVersionTaskExit,
				STATUS_DLL_INIT_FAILED_LOGOFF
			};

			return !allowedExitCodes.Contains(exitCode);
		}

		public static void HandleProcessExit(Process process)
		{
			if (process.HasExited && ShouldReportProcessExitError(process.ExitCode))
			{
				using (Db.DisableSchemaVersionCheck())
				{
					try
					{
						ErrorReporter.ReportOnce("CWExitsImmediately", $"CargoWise failed to restart. Exit code: {process.ExitCode}");
					}
					catch
					{
					}
				}
			}
		}

		internal void RestartCore(CommandLineArguments arguments)
		{
			try
			{
				arguments.OptionalArgs[OptionPersist] = ObjectFactory.Get<IWindowPersister>().GetOpenFormUrls();
			}
			catch (DatabaseUpgradedException)
			{
				//When DatabaseUpgradeException occurred, don't re-try and raise DeveloperNotificationException.
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May not have access to DB")]
		public void ShutdownEnterpriseWithMessage(string exitMessage)
		{
			if (exitMessage == null)
			{
				exitMessage = "";
			}

			if (Globals.IsWeb)
			{
				throw new OdysseyException(exitMessage);
			}
			else
			{
				if (!IsAlreadyClosing && Globals.IsUserInteractive && !string.IsNullOrEmpty(exitMessage)) // Are we sure that we want the exit message to have something...?
				{
					IsAlreadyClosing = true;
					var message = exitMessage + System.Environment.NewLine + "Would you like " + Constants.ProductName + " to attempt to restart?";
					var result = MessageBox.Show(message, Constants.ProductName + " Closing", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
					if (result == DialogResult.Yes)
					{
						Restart();
					}
					else
					{
						ExceptionReporter.SuppressGui();
						GCTracker.StopTracking();
						Process.GetCurrentProcess().Kill();
					}
				}
			}
		}

		public bool IsAlreadyClosing { get; private set; }
	}
}
