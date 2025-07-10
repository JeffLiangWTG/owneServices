using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.CW;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace ServiceManager.Host.CW
{
	abstract class ServiceInstallerStartupCommand : IHostStartupCommand
	{
		protected ServiceInstallerStartupCommand(IHostLogger hostLogger, IServiceManagerHostOptions hostOptions, IManagedInstallerAdapter managedInstallerHelper, IProductRegistration productRegistration)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.hostOptions = hostOptions ?? throw new ArgumentNullException(nameof(hostOptions));
			this.managedInstallerHelper = managedInstallerHelper ?? throw new ArgumentNullException(nameof(managedInstallerHelper));
			this.productRegistration = productRegistration ?? throw new ArgumentNullException(nameof(productRegistration));
		}

		protected int RunInstaller(string[] installerArgs)
		{
			try
			{
				managedInstallerHelper.Install(installerArgs);

				return 0;
			}
			catch (SystemException ex)
			{
				var exception = FindHandledException(ex.InnerException);
				if (exception != null)
				{
					hostLogger.Log(LogLevel.Error, exception.ToString(), exception);
				}

				if (exception is Win32Exception win32Ex)
				{
					return win32Ex.ErrorCode;
				}

				if (exception is not IdentityNotMappedException)
				{
					throw;
				}
			}

			return -1;

			static Exception FindHandledException(Exception exception)
			{
				while (exception != null)
				{
					if (exception is Win32Exception || exception is IdentityNotMappedException)
					{
						break;
					}

					exception = exception.InnerException;
				}

				return exception;
			}
		}

		static string CreateUsernameArgument(ServiceType serviceType, IProductRegistration licenseInfo, string prcUsername)
		{
			string GetUserName()
			{
				if (string.IsNullOrEmpty(prcUsername))
				{
					return "System";
				}
				if (serviceType == ServiceType.ProcessController)
				{
					return prcUsername;
				}
				if (licenseInfo.IsWiseTechGlobalInternalUATSystem())
				{
					return "s_gMSA_PRC1SAND$@sand.wtg.zone";
				}
				var at = prcUsername.IndexOf("@", StringComparison.OrdinalIgnoreCase);
				var domain = at > 0 ? prcUsername.Substring(at) : string.Empty;
				return serviceType switch
				{
					ServiceType.LauncherSecurity => $"s_{licenseInfo.Key.EnterpriseCode}_security${domain}",
					_ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unrecognized service type."),
				};
			}

			return $"-{serviceType}Username={GetUserName()}";
		}

		static string CreateExeArgument(ServiceType serviceType, string directoryName, string prcLocation)
		{
			string GetLocation()
			{
				return serviceType switch
				{
					ServiceType.ProcessController => prcLocation,
					ServiceType.LauncherSecurity => Path.Combine(directoryName, "net8.0", "CargoWise.ServiceManager.Next.Launcher.exe"),
					_ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unrecognized service type."),
				};
			}
			return $"-{serviceType}Exe={GetLocation()}";
		}

		protected IList<string> CreateServiceInstallerArguments(IServiceManagerHostOptions startupOptions)
		{
			var installerArgs = new List<string>();
			var licenseInfo = GetLicenseInfo();

			installerArgs.Add($"-ServerName={startupOptions.ServerName}");
			installerArgs.Add($"-DatabaseName={startupOptions.DatabaseName}");

			var prcUsername = startupOptions.Username;
			installerArgs.Add(CreateUsernameArgument(ServiceType.ProcessController, licenseInfo, prcUsername));
			installerArgs.Add(CreateUsernameArgument(ServiceType.LauncherSecurity, licenseInfo, prcUsername));

			if (!string.IsNullOrEmpty(startupOptions.Password))
			{
				var encryptedPass = ProtectedData.Protect(
					Encoding.Unicode.GetBytes(startupOptions.Password),
					null,
					DataProtectionScope.CurrentUser);
				installerArgs.Add($"-{ServiceType.ProcessController}Password={Convert.ToBase64String(encryptedPass)}");
			}

			if (startupOptions.Automatic)
			{
				installerArgs.Add("-Automatic");
			}

			if (!string.IsNullOrEmpty(startupOptions.Config))
			{
				installerArgs.Add(string.Format("-Config={0}", startupOptions.Config));
			}

			var licenseInfoKey = licenseInfo.Key;
			installerArgs.Add($"-EnterpriseCode={licenseInfoKey.EnterpriseCode}");
			installerArgs.Add($"-ServerCode={licenseInfoKey.ServerCode}");

			var prcLocation = Assembly.GetExecutingAssembly().Location;
			var directoryName = Path.GetDirectoryName(prcLocation) ?? throw new InvalidOperationException($"Unable to find parent directory for {prcLocation}");
			var prcFullPath = Path.Combine(directoryName, HostExeName);
			installerArgs.Add(CreateExeArgument(ServiceType.ProcessController, directoryName, prcFullPath));
			installerArgs.Add(CreateExeArgument(ServiceType.LauncherSecurity, directoryName, prcFullPath));

			installerArgs.Add(prcLocation);

			return installerArgs;

			IProductRegistration GetLicenseInfo()
			{
				using (Db.DisposableActionForDbConnection())
				{
					return productRegistration;
				}
			}
		}

		public abstract int Execute();

		protected readonly IHostLogger hostLogger;
		protected readonly IServiceManagerHostOptions hostOptions;
		readonly IManagedInstallerAdapter managedInstallerHelper;
		readonly IProductRegistration productRegistration;

		const string HostExeName = ServiceManagerConstants.ServiceManagerHostExe;
	}
}
