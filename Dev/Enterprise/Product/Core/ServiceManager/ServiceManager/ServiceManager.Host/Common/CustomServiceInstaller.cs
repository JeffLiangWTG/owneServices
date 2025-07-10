using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.ServiceProcess;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Common.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class CustomServiceInstaller : Installer
	{
		public CustomServiceInstaller(IProcessFactory processWrapperFactory)
		{
			eventLogInstaller = new EventLogInstaller();
			eventLogInstaller.Log = "Application";
			eventLogInstaller.Source = "";
			eventLogInstaller.UninstallAction = UninstallAction.Remove;

			Installers.Add(eventLogInstaller);

			this.processWrapperFactory = processWrapperFactory ?? throw new ArgumentNullException(nameof(processWrapperFactory));
		}

		public override void Install(IDictionary stateSaver)
		{
			try
			{
				InstallService();

				if (!string.IsNullOrEmpty(Description))
				{
					UpdateServiceDescription();
				}

				stateSaver["installed"] = true;
			}
			finally
			{
				base.Install(stateSaver);
			}
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);
			StopService();
			RemoveService();
		}

		public override void Rollback(IDictionary savedState)
		{
			base.Rollback(savedState);

			object installed = savedState["installed"];
			if (installed == null || !(bool)installed)
			{
				return;
			}

			RemoveService();
		}

		protected override void OnCommitted(IDictionary savedState)
		{
			base.OnCommitted(savedState);
			SetRecoveryOptions();
		}

		void SetRecoveryOptions()
		{
			var args = $"failure {ServiceNameWithQuoteIfNeeded} reset= 0 actions= restart/60000";
			RunScProcess(args, $"Failed to config recovery options for {ServiceName}");
		}

		string GenerateCreateCommandArgs()
		{
			if (string.IsNullOrWhiteSpace(ExecutablePath))
			{
				throw new ArgumentException($"{nameof(ExecutablePath)} is invalid", nameof(ExecutablePath));
			}

			if (string.IsNullOrWhiteSpace(DisplayName) || DisplayName.Length > 255)
			{
				throw new ArgumentException($"{nameof(DisplayName)} is invalid", nameof(DisplayName));
			}

			var binPathParam = $"binpath= {CommandLineArgEncoder.EnquoteArgumentIfNeeded(ExecutablePath)}";

			var args = new List<string>() { "create", ServiceNameWithQuoteIfNeeded, binPathParam, "type= own" };

			if (StartType == ServiceStartMode.Automatic)
			{
				args.Add(DelayedAutoStart ? "start= delayed-auto" : "start= auto");
			}
			else
			{
				args.Add("start= demand");
			}

			args.Add($"displayname= {CommandLineArgEncoder.EnquoteArgumentIfNeeded(DisplayName)}");

			if (ServicesDependedOn is { Count: > 0 })
			{
				args.Add($"depend= {CommandLineArgEncoder.EnquoteArgumentIfNeeded(string.Join("/", ServicesDependedOn))}");
			}

			switch (Account)
			{
				case ServiceAccount.LocalService:
					args.Add("obj= \"NT AUTHORITY\\LocalService\"");
					break;
				case ServiceAccount.NetworkService:
					args.Add("obj= \"NT AUTHORITY\\NetworkService\"");
					break;
				case ServiceAccount.User:
					args.Add($"obj= {CommandLineArgEncoder.EnquoteArgumentIfNeeded(Username)}");
					if (!string.IsNullOrWhiteSpace(Password))
					{
						args.Add($"password= {CommandLineArgEncoder.EnquoteArgumentIfNeeded(Password)}");
					}
					break;
			}

			return string.Join(" ", args);
		}

		static bool IsValidServiceName(string serviceName)
		{
			return !string.IsNullOrWhiteSpace(serviceName) &&
				   serviceName.Length <= 80 &&
				   serviceName.IndexOfAny(new char[] { '\\', '/' }) == -1;
		}

		void InstallService()
		{
			var createCommandArgs = GenerateCreateCommandArgs();
			RunScProcess(createCommandArgs, $"Failed to create service for {ServiceName}");
		}

		void UpdateServiceDescription()
		{
			// use sc description command to set service description as sc create command doesn't support adding description
			var descriptionCommandArgs = $"description {ServiceNameWithQuoteIfNeeded} {CommandLineArgEncoder.EnquoteArgumentIfNeeded(Description)}";
			RunScProcess(descriptionCommandArgs, $"Failed to config service description for {ServiceName}");
		}

		void RemoveService()
		{
			var args = $"delete {ServiceNameWithQuoteIfNeeded}";
			var exitCode = RunScProcess(args);

			if (exitCode == ErrorServiceDoesNotExist)
			{
				Context.LogMessage($"Service {ServiceName} does not exist, skip deletion");
				return;
			}

			if (exitCode != 0)
			{
				throw new Win32Exception($"Failed to delete service {ServiceName}, exit code {exitCode}");
			}
		}

		void StopService()
		{
			var args = $"stop {ServiceNameWithQuoteIfNeeded}";
			var exitCode = RunScProcess(args);

			if (exitCode == ErrorServiceDoesNotExist)
			{
				Context.LogMessage($"Service {ServiceName} does not exist, skip stopping");
				return;
			}

			if (exitCode == ErrorServiceNotStarted)
			{
				Context.LogMessage($"Service {ServiceName} is not started, skip stopping");
				return;
			}

			if (exitCode != 0)
			{
				throw new Win32Exception($"Failed to stop service {ServiceName}, exit code {exitCode}");
			}
		}

		void RunScProcess(string arguments, string errorMessage)
		{
			var exitCode = RunScProcess(arguments);
			if (exitCode != 0)
			{
				throw new Win32Exception($"{errorMessage}, exit code is {exitCode}");
			}
		}

		int RunScProcess(string arguments)
		{
			var startInfo = new ProcessStartInfo()
			{
				FileName = "sc",
				WindowStyle = ProcessWindowStyle.Hidden,
				Arguments = arguments,
			};

			using var process = processWrapperFactory.Create(startInfo);
			process.Start();
			// wait indefinitely
			process.WaitForExit(-1);
			return (int)process.ExitCode;
		}

		public string ServiceName
		{
			get => serviceName;
			set
			{
				serviceName = IsValidServiceName(value)
					? value
					: throw new ArgumentException($"{nameof(ServiceName)} is invalid", nameof(ServiceName));
				eventLogInstaller.Source = value;
			}
		}
		public string ExecutablePath { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
		public IReadOnlyList<string> ServicesDependedOn { get; set; }
		public ServiceStartMode StartType { get; set; }
		public bool DelayedAutoStart { get; set; }
		public ServiceAccount Account { get; set; }
		[Browsable(false)]
		public string Username { get; set; }
		[Browsable(false)]
		public string Password { get; set; }

		string ServiceNameWithQuoteIfNeeded => CommandLineArgEncoder.EnquoteArgumentIfNeeded(ServiceName);

		const int ErrorServiceDoesNotExist = 1060;
		const int ErrorServiceNotStarted = 1062;

		string serviceName;
		readonly EventLogInstaller eventLogInstaller;
		readonly IProcessFactory processWrapperFactory;
	}
}
