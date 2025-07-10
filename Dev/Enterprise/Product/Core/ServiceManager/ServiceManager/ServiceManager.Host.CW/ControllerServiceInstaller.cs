using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Common;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostUtilities;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace ServiceManager.Host.CW
{
	[RunInstaller(true)]
	public partial class ControllerServiceInstaller : Installer
	{
		public ControllerServiceInstaller()
		{
			logFilesAccessInstaller = new LogFilesAccessInstaller();
			prcHttpAccessInstaller = new HttpAccessInstaller();
			secHttpAccessInstaller = new HttpAccessInstaller();
			prcCustomServiceInstaller = new CustomServiceInstaller(new ProcessWrapperFactory());
			secCustomServiceInstaller = new CustomServiceInstaller(new ProcessWrapperFactory());
			tcpIpRegistrySettingsInstaller = new TcpIpRegistrySettingsInstaller();

			InitializeComponent();
		}

		protected override void OnBeforeInstall(IDictionary savedState)
		{
			base.OnBeforeInstall(savedState);
			AddInstallers(savedState, uninstall: false);
			SetStartParameters();
		}

		protected override void OnBeforeUninstall(IDictionary savedState)
		{
			base.OnBeforeUninstall(savedState);
			AddInstallers(savedState, uninstall: true);
		}

		void AddInstallers(IDictionary savedState, bool uninstall)
		{
			serverName = Context.Parameters["ServerName"];
			databaseName = Context.Parameters["DatabaseName"];
			enterpriseCode = Context.Parameters["EnterpriseCode"];
			serverCode = Context.Parameters["ServerCode"];
			ServiceType[] serviceTypes;
			if (uninstall)
			{
				serviceTypes = (savedState is not null) && savedState.Contains("serviceTypes")
					? (ServiceType[])savedState["serviceTypes"]
					: Enum.GetValues(typeof(ServiceType))
						.Cast<ServiceType>()
						.Where(x => x != ServiceType.ProcessController)
						.ToArray();
			}
			else
			{
				serviceTypes = ServiceManagerHelper.GetExtraServiceTypes();
				savedState["serviceTypes"] = serviceTypes;
			}

			Installers.Add(logFilesAccessInstaller);
			Installers.Add(prcHttpAccessInstaller);
			Installers.Add(prcCustomServiceInstaller);
			if (serviceTypes.Contains(ServiceType.LauncherSecurity))
			{
				Installers.Add(secHttpAccessInstaller);
				Installers.Add(secCustomServiceInstaller);
			}
			Installers.Add(tcpIpRegistrySettingsInstaller);
			ConfigureInstallers();
			return;

			void ConfigureInstallers()
			{
				ConfigureLogFilesAccessInstaller(logFilesAccessInstaller);
				ConfigureCustomServiceInstaller(prcCustomServiceInstaller, ServiceType.ProcessController);
				ConfigureHttpAccessInstaller(prcHttpAccessInstaller, ServiceType.ProcessController);
				ConfigureCustomServiceInstaller(secCustomServiceInstaller, ServiceType.LauncherSecurity);
				ConfigureHttpAccessInstaller(secHttpAccessInstaller, ServiceType.LauncherSecurity);
			}

			void ConfigureHttpAccessInstaller(HttpAccessInstaller installer, ServiceType serviceType)
			{
				installer.Url = ServiceManagerHelper.GetListenerStrongBinding(serviceType, enterpriseCode, serverCode);
			}

			void ConfigureLogFilesAccessInstaller(LogFilesAccessInstaller installer)
			{
				installer.LogDirectoryName = ServiceManagerHelper.GetLogFilesDirectory(serverName, databaseName);
			}

			void ConfigureCustomServiceInstaller(CustomServiceInstaller installer, ServiceType serviceType)
			{
				installer.ServiceName = ServiceHostProcess.GetServiceName(serviceType, serverName, databaseName);
				installer.DisplayName = ServiceManagerHelper.GetDisplayName(serviceType, serverName, databaseName);
				installer.Description = ServiceManagerHelper.GetServiceDescription(serviceType);
			}
		}

		void SetStartParameters()
		{
			var prcAccount = GetAccount(ServiceType.ProcessController);
			var prcSid = (SecurityIdentifier)prcAccount.Translate(typeof(SecurityIdentifier));
			var secAccount = GetAccount(ServiceType.LauncherSecurity);

			SetLogFilesAccessInstallerStartParameters(logFilesAccessInstaller, prcSid);
			SetHttpAccessInstallerStartParameters(prcHttpAccessInstaller, prcSid);
			if (Installers.Contains(secHttpAccessInstaller))
			{
				var secSid = (SecurityIdentifier)secAccount.Translate(typeof(SecurityIdentifier));
				SetHttpAccessInstallerStartParameters(secHttpAccessInstaller, secSid);
			}

			var isAutomatic = Context.Parameters.ContainsKey("Automatic");
			var prcExtraArgs = new List<string>();
			var sDirArg = System.Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith(HostCommandLineOptions.ServerDirectoryForEnterpriseArgumentPrefix, StringComparison.OrdinalIgnoreCase));
			if (sDirArg != null)
			{
				prcExtraArgs.Add(CommandLineArgEncoder.EnquoteArgumentIfNeeded(sDirArg));
			}
			SetCustomServiceInstallerStartParameters(prcCustomServiceInstaller, ServiceType.ProcessController, isAutomatic, prcAccount, prcExtraArgs);
			if (Installers.Contains(secCustomServiceInstaller))
			{
				var secExtraArgs = new List<string>()
				{
					$"-EnterpriseCode:{enterpriseCode}",
					$"-ServerCode:{serverCode}",
				};
				SetCustomServiceInstallerStartParameters(secCustomServiceInstaller, ServiceType.LauncherSecurity, isAutomatic, secAccount, secExtraArgs);
			}
			return;

			void SetHttpAccessInstallerStartParameters(HttpAccessInstaller installer, SecurityIdentifier sid)
			{
				installer.Sid = sid;
			}

			void SetLogFilesAccessInstallerStartParameters(LogFilesAccessInstaller installer, SecurityIdentifier sid)
			{
				installer.Sid = sid;
			}

			void SetCustomServiceInstallerStartParameters(
				CustomServiceInstaller customServiceInstaller,
				ServiceType serviceType,
				bool automatic,
				NTAccount account,
				IEnumerable<string> extraServiceArgs)
			{
				SetStartMode(customServiceInstaller, automatic);
				SetAccount(customServiceInstaller, serviceType, account);
				SetExecutablePath(customServiceInstaller, serviceType, extraServiceArgs);
			}

			void SetStartMode(CustomServiceInstaller customServiceInstaller, bool automatic)
			{
				if (automatic)
				{
					customServiceInstaller.StartType = ServiceStartMode.Automatic;
					customServiceInstaller.DelayedAutoStart = true;
				}
				else
				{
					customServiceInstaller.StartType = ServiceStartMode.Manual;
				}
			}

			NTAccount GetAccount(ServiceType serviceType)
			{
				var usernameKey = $"{serviceType}Username";
				var username = Context.Parameters[usernameKey];
				if (string.IsNullOrWhiteSpace(username))
				{
					throw new InvalidOperationException($"Username not found with {usernameKey}");
				}

				return new NTAccount(username);
			}

			void SetAccount(CustomServiceInstaller customServiceInstaller, ServiceType serviceType, NTAccount account)
			{
				var sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
				if (sid.IsWellKnown(WellKnownSidType.LocalSystemSid))
				{
					customServiceInstaller.Account = ServiceAccount.LocalSystem;
				}
				else if (sid.IsWellKnown(WellKnownSidType.LocalServiceSid))
				{
					customServiceInstaller.Account = ServiceAccount.LocalService;
				}
				else if (sid.IsWellKnown(WellKnownSidType.NetworkServiceSid))
				{
					customServiceInstaller.Account = ServiceAccount.NetworkService;
				}
				else
				{
					customServiceInstaller.Account = ServiceAccount.User;
					customServiceInstaller.Username = account.Value;
					var password = Context.Parameters[$"{serviceType}Password"];
					if (password != null)
					{
						password = Encoding.Unicode.GetString(ProtectedData.Unprotect(Convert.FromBase64String(password), null, DataProtectionScope.CurrentUser));
						customServiceInstaller.Password = password;
					}
				}
			}

			void SetExecutablePath(CustomServiceInstaller customServiceInstaller, ServiceType serviceType, IEnumerable<string> extraServiceArgs)
			{
				var executableKey = $"{serviceType}Exe";
				var serviceArgs = new List<string> { Context.Parameters[executableKey] };
				serviceArgs.AddRange(extraServiceArgs);
				serviceArgs.Add(serverName);
				serviceArgs.Add(databaseName);
				customServiceInstaller.ExecutablePath = string.Join(" ", serviceArgs);
			}
		}

		readonly TcpIpRegistrySettingsInstaller tcpIpRegistrySettingsInstaller;
		readonly CustomServiceInstaller prcCustomServiceInstaller;
		readonly CustomServiceInstaller secCustomServiceInstaller;
		readonly HttpAccessInstaller prcHttpAccessInstaller;
		readonly HttpAccessInstaller secHttpAccessInstaller;
		readonly LogFilesAccessInstaller logFilesAccessInstaller;
		string serverName;
		string databaseName;
		string serverCode;
		string enterpriseCode;
	}
}
