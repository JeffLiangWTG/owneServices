using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.ServiceProcess;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using ServiceManager.Integration.CW;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceHostUtilities;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ProductRegistration.GUI
{
	public static class ServiceTaskHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "False alarm, running Service Manager Host process, not opening a file or url")]
		public static void StopAll()
		{
			var serviceHostsCache = ObjectFactory.Get<IServiceHostsCache>();
			var hosts = serviceHostsCache.AvailableServiceHosts.Select(e => e.HostName.Hostname).ToList();
			var hostsToStop = new List<string>();

			foreach (var hostName in hosts)
			{
				bool serviceExists = TryGetServiceStatus(hostName, out ServiceControllerStatus status);
				if (serviceExists && status == ServiceControllerStatus.Running)
				{
					hostsToStop.Add(hostName);
				}
			}

			if (hostsToStop.Count > 0)
			{
				string binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				string path = Path.Combine(binPath, ServiceManagerConstants.ServiceManagerHostExe);
				string hostExePath = (File.Exists(path)) ? path : null;

				foreach (var host in hostsToStop)
				{
					try
					{
						ProcessStartInfo startInfo = new ProcessStartInfo(hostExePath);
						startInfo.CreateNoWindow = true;
						string[] args = new string[] { "-stop", String.Format("\"-host:{0}\"", host), Db.ServerName, Db.DatabaseName };
						startInfo.Arguments = String.Join(" ", args);

						if (new VersionHelper().IsWindowsVistaOrGreater() && !new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator)) // Vista needs elevation
						{
							startInfo.Verb = "runas";
						}
						else
						{
							startInfo.UseShellExecute = false;
						}

						Process process = new Process();
						process.StartInfo = startInfo;
						process.Start();
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
					}
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1031", Justification = "This is a very narrow case in which we want to catch and report any unhandled issues to us")]
		[SuppressMessage("Microsoft.Design", "CA1021", Justification = "Out parameter is required by design")]
		static bool TryGetServiceStatus(string host, out ServiceControllerStatus status, bool throwUnhandled = false)
		{
			status = 0;
			using (var sc = new ServiceController(ServiceHostProcess.GetServiceName(ServiceType.ProcessController, Db.ServerName, Db.DatabaseName), host))
			{
				try
				{
					status = sc.Status;
					return true;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (IgnoreTryGetServiceStatusException(ex))
					{
						return false;
					}

					if (throwUnhandled)
					{
						throw;
					}

					ErrorReporter.ReportOnce("Exception when getting service status", ex);
					return false;
				}
			}
		}

		internal static bool IgnoreTryGetServiceStatusException(Exception exception)
		{
			if (exception.InnerException is Win32Exception win32Ex)
			{
				return win32Ex.NativeErrorCode == 5 // ERROR_ACCESS_DENIED
						|| win32Ex.NativeErrorCode == 1060 // ERROR_SERVICE_DOES_NOT_EXIST
						|| win32Ex.NativeErrorCode == 1722 // RPC_S_SERVER_UNAVAILABLE
						|| win32Ex.NativeErrorCode == 1727 // RPC_S_CALL_FAILED_DNE
					;
			}
			return false;
		}
	}
}
