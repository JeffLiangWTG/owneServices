using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ActiveDirectory;
using CargoWise.Common;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace ServiceManager.Host.CW
{
	abstract class ServiceControllerStartupCommand : IHostStartupCommand
	{
		protected ServiceControllerStartupCommand(IHostLogger hostLogger, IServiceManagerHostOptions hostOptions, IServiceControllerManager serviceControllerManager)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			hostStartupOptions = hostOptions ?? throw new ArgumentNullException(nameof(hostOptions));
			this.serviceControllerManager = serviceControllerManager ?? throw new ArgumentNullException(nameof(serviceControllerManager));
		}

		public int Execute()
		{
			WindowsIdentityImpersonator impersonator = null;
			try
			{
				if (!Enterprise.ZArchitecture.AssemblyMetaDataReader.FilesExist)
				{
					hostLogger.Log(LogLevel.Error, string.Format("{0} are missing.", nameof(Enterprise.ZArchitecture.AssemblyMetaDataReader.AssemblyMetaDataFiles)));
					return 0x3EB;
				}

				if (!string.IsNullOrEmpty(hostStartupOptions.Username) && !string.IsNullOrEmpty(hostStartupOptions.Password))
				{
					impersonator = new WindowsIdentityImpersonator(hostStartupOptions.Username, hostStartupOptions.Password, () => { });
				}

				var serviceControllers = serviceControllerManager.GetServiceControllers(hostStartupOptions);
				try
				{
					if (!serviceControllers.TryGetValue(ServiceType.ProcessController, out var sc))
					{
						hostLogger.Log(LogLevel.Error, "The specified service does not exist as an installed service");
						return 0x424; // from http://msdn.microsoft.com/en-us/library/windows/desktop/ms681383(v=vs.85).aspx
					}

					ExecuteCore(serviceControllers);

					return 0;
				}
				finally
				{
					serviceControllers.Values.ForEach(sc => sc.Dispose());
				}
			}
			catch (System.ServiceProcess.TimeoutException ex)
			{
				var waitRequest = ex.Data.Contains("waitRequest") ? ex.Data["waitRequest"] : "unknown";
				var serviceName = ex.Data.Contains("serviceName") ? ex.Data["serviceName"] : "unknown";
				hostLogger.Log(LogLevel.Error, string.Format("The service {0} did not respond to the {1} request in a timely fashion.", serviceName, waitRequest));
				return 1053;
			}
			catch (Win32Exception ex)
			{
				hostLogger.Log(LogLevel.Error, ex.Message);
				return ex.ErrorCode;
			}
			catch (InvalidOperationException ex)
			{
				if (ex.InnerException is Win32Exception win32Ex)
				{
					hostLogger.Log(LogLevel.Error, win32Ex.Message);
					return win32Ex.ErrorCode;
				}

				throw;
			}
			finally
			{
				if (impersonator != null)
				{
					impersonator.Dispose();
				}
			}
		}

		protected abstract void ExecuteCore(IReadOnlyDictionary<ServiceType, IServiceController> serviceControllers);

		protected readonly IHostLogger hostLogger;
		protected readonly IServiceManagerHostOptions hostStartupOptions;
		protected readonly IServiceControllerManager serviceControllerManager;
	}
}
