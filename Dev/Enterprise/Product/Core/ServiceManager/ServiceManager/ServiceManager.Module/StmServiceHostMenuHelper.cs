using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using static System.FormattableString;
using static System.String;
using Res = Enterprise.ServiceManager.Module.Res;

namespace Enterprise.ServiceManager
{
	static class StmServiceHostMenuHelper
	{
		#region ServiceMenu

		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, running Service Manager Host process, not opening a file or url")]
		public static bool UpdateHost(BusinessObjectFactory factory, string method, string host, string server, out string errors)
		{
			try
			{
				var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var path = Path.Combine(binPath, ServiceManagerConstants.ServiceManagerHostExe);
				var hostExePath = File.Exists(path) ? path : null;
				var startInfo = new ProcessStartInfo(hostExePath);
				var args = StartProcess(factory, method, host, server, out var canContinue, out errors, startInfo);

				if (!canContinue)
				{
					return false;
				}

				try
				{
					startInfo.Arguments = Join(" ", args);

					if (new VersionHelper().IsWindowsVistaOrGreater() &&
						!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)
					) // Vista needs elevation
					{
						startInfo.Verb = "runas";
					}
					else if (startInfo.CreateNoWindow)
					{
						startInfo.UseShellExecute = false;
						startInfo.RedirectStandardError = true;
						startInfo.RedirectStandardOutput = true;
					}

					using (var process = Process.Start(startInfo))
					{
						process.WaitForExit();
						var success = process.ExitCode == 0;
						if (success)
						{
							if (startInfo.RedirectStandardOutput)
							{
								errors = process.StandardOutput.ReadToEnd();
							}
						}
						else
						{
							if (startInfo.RedirectStandardError)
							{
								errors = process.StandardError.ReadToEnd();
							}
						}

						return success;
					}
				}
				catch (Win32Exception ex)
				{
					if (ex.NativeErrorCode != 1223) // ERROR_CANCELLED
					{
						throw;
					}
				}
				catch (SystemException ex)
				{
					if (ex.InnerException is Win32Exception win32Ex)
					{
						errors = win32Ex.Message;
					}
					else
					{
						throw;
					}
				}
			}
			finally
			{
				ObjectFactory.Get<IServiceHostsCache>().Refresh();
			}
			return false;
		}

		static string[] StartProcess(BusinessObjectFactory factory, string method, string host, string server, out bool canContinue, out string errors, ProcessStartInfo startInfo)
		{
			string[] args = null;
			canContinue = true;
			server = !IsNullOrEmpty(server) ? server : Db.ServerName;
			errors = Empty;
			if (method == InstallServiceText)
			{
				args = Install(factory, host, out canContinue, out errors);
			}
			else if (method == UninstallServiceText)
			{
				args = new[] { "-uninstall", server, Db.DatabaseName };
			}
			else if (method == StartServiceText)
			{
				var companyProvider = new GlbCompanyProvider();
				if (!companyProvider.GetActiveCompanies().Any(c => c.GetActiveBranches().Any()))
				{
					errors = Res.GetString("0f6039d0-93ae-4dfd-9386-4a7a93acf03f", "Cannot start the Process Controller as there are no active (non-demo) companies with active branches in the database.");
					canContinue = false;
				}

				startInfo.CreateNoWindow = true;
				args = new[] { "-start", Invariant($"\"-host:{host}\""), server, Db.DatabaseName };
			}
			else if (method == StopServiceText)
			{
				startInfo.CreateNoWindow = true;
				args = new[] { "-stop", Invariant($"\"-host:{host}\""), server, Db.DatabaseName };
			}
			else if (method == ViewLogsText)
			{
				ShowLogForm();
				canContinue = false;
			}

			return args;
		}

		static string[] Install(BusinessObjectFactory factory, string host, out bool canContinue, out string errors)
		{
			var args = Array.Empty<string>();
			canContinue = false;
			errors = Empty;
			if (!new VersionHelper().IsWindowsXPSP2OrGreater())
			{
				errors = Res.GetString("5a257703-7ca3-47e1-ae7c-6dfee499a986", "You can install a Process Controller only on Windows XP SP2 / Windows Vista / Windows Server 2003 or later.");
				return args;
			}

			var info = new ServiceInstallInfo(factory);
			using var serviceInstallInfoForm = new ServiceInstallInfoForm(info);
			if (ZFormModaliser.ShowDialogAndDispose(serviceInstallInfoForm) != DialogResult.OK)
			{
				return args;
			}

			canContinue = true;

			args = ToInstallationArguments(info);
			SetServiceHostActiveStatus(host, true);
			return args;
		}

		internal static string[] ToInstallationArguments(ServiceInstallInfo info)
		{
			string[] args;
			var argsList = new List<string>
			{
				"-install"
			};
			if (info.Automatic)
			{
				argsList.Add("-automatic");
			}

			if (!info.Username.IsEmpty)
			{
				argsList.Add(CommandLineArgEncoder.EnquoteArgumentIfNeeded(Invariant($"-username:{info.Username}")));
				if (!info.Password.IsEmpty)
				{
					argsList.Add(CommandLineArgEncoder.EnquoteArgumentIfNeeded(Invariant($"-password:{info.Password}")));
				}
			}

			var sdirArg = CommandLineArgEncoder.EnquoteArgumentIfNeeded(System.Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith("-SDir:", StringComparison.Ordinal)));
			argsList.Add(Db.ServerName);
			argsList.Add(Db.DatabaseName);
			if (!IsNullOrWhiteSpace(sdirArg))
			{
				argsList.Add(sdirArg);
			}

			args = argsList.ToArray();
			return args;
		}

		internal static void SetServiceHostActiveStatus(string hostName, bool newStatus)
		{
			ModifyAndSaveWithConcurrencyRetry(StmServiceHost.Schema.TableName, (factory) =>
			{
				var serviceHost = factory.LoadTop1<StmServiceHost>(new ZQuery(StmServiceHostSchema.SH_HostName, hostName)) ?? factory.New<StmServiceHost>();
				serviceHost.SH_HostName = hostName;
				serviceHost.SH_IsActive = newStatus;
			});
		}

		public static void ModifyAndSaveWithConcurrencyRetry(string tableName, Action<BusinessObjectFactory> modificationAction)
		{
			const int maxRetries = 1;
			for (var i = 0; i <= maxRetries; i++)
			{
				var factory = new BusinessObjectFactory();
				modificationAction(factory);

				try
				{
					factory.Save();
					return;
				}
				catch (ZSaveConcurrencyException) when (i < maxRetries)
				{
					RowFactory.ClearSpecificTableFromUberFactory(tableName);
				}
			}
		}

#if DEBUG
		internal
#endif
		static IZForm ShowLogForm()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.ServiceTaskLogViewer);
			var logViewer = new ServiceTaskLogViewer();
			return controller.ShowEditForm(logViewer);
		}

		#endregion

		#region ResStrings

		public static string StartServiceText => Res.GetString("ServiceTaskModule.MenuItem.Start", "Start");

		public static string StopServiceText => Res.GetString("ServiceTaskModule.MenuItem.Stop", "Stop");

		public static string InstallServiceText => Res.GetString("ServiceTaskModule.MenuItem.Install", "Install");

		public static string UninstallServiceText => Res.GetString("ServiceTaskModule.MenuItem.Uninstall", "Uninstall");

		public static string ViewLogsText => Res.GetString("ServiceTaskModule.MenuItem.ViewLogFiles", "View Log Files");

		public static string ConnectingText => Res.GetString("1d86dec1-f2c7-4c62-8053-2cfcf2be8052", "Connecting to service...");

		#endregion
	}
}
