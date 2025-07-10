using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceHostUtilities;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Shared
{
	public static class ServiceManagerHelper
	{
		public static string GetDisplayName(ServiceType serviceType, string serverName, string databaseName) =>
			$"{GetDisplayProcessName(serviceType)} ({ServiceHostProcess.GetDbServerName(serverName)} {ServiceHostProcess.GetDatabaseName(databaseName)})";

		public static string GetServiceDescription(ServiceType serviceType) => serviceType switch
		{
			ServiceType.ProcessController =>
				"Manages and controls background tasks such as Email Processing, Report Scheduling, Database Consistency checks and backups.",
			ServiceType.LauncherSecurity =>
				"Manages and controls background tasks such as access token generation.",
			_ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unrecognized service type."),
		};

		public static string GetListenerStrongBinding(ServiceType processName)
		{
			var productRegistrationKey = GetProductRegistrationKey();
			return GetListenerStrongBinding(processName, productRegistrationKey.EnterpriseCode, productRegistrationKey.ServerCode);
		}

		public static string GetListenerStrongBinding(ServiceType processName, string enterpriseCode, string serverCode)
		{
			var result = GetLocalBaseUri(processName, enterpriseCode, serverCode)
				.OriginalString
				.Replace("localhost", "+");
			return result;
		}

		public static Uri GetLocalBaseUri(ServiceType serviceType, string enterpriseCode, string serverCode) =>
			ServiceHostProcess.GetServiceUri(serviceType, null, enterpriseCode, serverCode);

		public static Uri GetLogFilesUri(string? serverName)
		{
			return GetUri(ServiceHostUriBuilder.GetLogFilesUri, serverName);
		}

		public static Uri GetStatusUri(string? serverName)
		{
			return GetUri(ServiceHostUriBuilder.GetStatusUri, serverName);
		}

		public static Uri GetTaskStatusUri(string? serverName)
		{
			return GetUri(ServiceHostUriBuilder.GetTaskStatusUri, serverName);
		}

		public static Uri GetIsAliveUri(string? serverName)
		{
			return GetUri(ServiceHostUriBuilder.GetIsAliveUri, serverName);
		}

		public static Uri GetCommandUri(string? serverName)
		{
			return GetUri(ServiceHostUriBuilder.GetCommandUri, serverName);
		}

		public static Uri GetQueueStatusUri(string? serverName)
		{
			return GetUri(ServiceHostUriBuilder.GetQueueStatusUri, serverName);
		}

		public static Uri GetBindingListUri(string? serverName)
		{
			return GetUri(ServiceHostUriBuilder.GetBindingListUri, serverName);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static Uri GetUri(Func<string?, string, string, Uri> func, string? serverName)
		{
			var productRegistrationKey = GetProductRegistrationKey();
			return func(serverName, productRegistrationKey.EnterpriseCode, productRegistrationKey.ServerCode);
		}

		static IProductRegistrationKey GetProductRegistrationKey()
		{
			return ObjectFactory.Get<IProductRegistration>().Key;
		}

		public static string GetLogFilesDirectory(string dbServer, string? databaseName)
		{
			return CommonProgramData.GetCargoWiseDirectory(
				"Process Controller",
				ServiceHostProcess.GetDbServerName(dbServer),
				ServiceHostProcess.GetDatabaseName(databaseName));
		}

		public static string GetHostName()
		{
			var hostname = Dns.GetHostName();
			try
			{
				return Dns.GetHostEntry(hostname).HostName.ToLower();
			}
			catch (SocketException)
			{
				return hostname.ToLower();
			}
		}

		public static bool AreSameHosts(string host1, string host2)
		{
			if (string.Equals(host1, host2, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			try
			{
				host1 = Dns.GetHostEntry(host1).HostName;
			}
			catch (SocketException)
			{
			}
			try
			{
				host2 = Dns.GetHostEntry(host2).HostName;
			}
			catch (SocketException)
			{
			}

			if (string.Equals(host1, host2, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}

			try
			{
				return (
					from address1 in Dns.GetHostAddresses(host1)
					from address2 in Dns.GetHostAddresses(host2)
					where address1.Equals(address2)
					select address1
				).Any();
			}
			catch (SocketException)
			{
				return false;
			}
		}
		public static bool IsLocalHost(string? dbServer)
		{
			var backslash = dbServer?.IndexOf('\\') ?? -1;
			if (backslash >= 0)
			{
				dbServer = dbServer!.Substring(0, backslash);
			}

			if (string.IsNullOrEmpty(dbServer) || dbServer == "." || dbServer == "(local)")
			{
				return true;
			}

			try
			{
				dbServer = Dns.GetHostEntry(dbServer).HostName;
				var localHostName = Dns.GetHostEntry(Dns.GetHostName()).HostName;

				return localHostName.Equals(dbServer, StringComparison.OrdinalIgnoreCase);
			}
			catch (SocketException)
			{
				return false;
			}
		}

		public static string SQLServiceName(string? dbServer)
		{
			var backslash = dbServer?.IndexOf('\\') ?? -1;
			return backslash < 0
				? "MSSQLServer"
				: "MSSQL$" + dbServer!.Substring(backslash + 1);
		}

		static string GetDisplayProcessName(ServiceType serviceType) => serviceType switch
		{
			ServiceType.ProcessController => "Process Controller",
			ServiceType.LauncherSecurity => "Process Launcher - Security",
			_ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unrecognized service type."),
		};

		public static ServiceType[] GetExtraServiceTypes()
		{
			return Enum.GetValues(typeof(ServiceType))
				.Cast<ServiceType>()
				.Where(IncludeExtraServiceType)
				.ToArray();
		}

		internal static bool IncludeExtraServiceType(ServiceType serviceType)
		{
			return serviceType switch
			{
				ServiceType.ProcessController => false,
				ServiceType.LauncherSecurity => IncludeLauncherSecurity(),
				_ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unrecognized service type."),
			};
		}

		static bool IncludeLauncherSecurity()
		{
			var protectionMechanism = SystemDataRegistry.Instance.SystemToSystemTrustDataProtectionMechanism.Value;
			var result = protectionMechanism != DataProtectionMechanisms.Codes.None;
			return result;
		}

		public static readonly int IsInitializedMask = BitVector32.CreateMask();
		public static readonly int IsActiveMask = BitVector32.CreateMask(IsInitializedMask);
		public static readonly int IsRunningMask = BitVector32.CreateMask(IsActiveMask);
		public static readonly int IsBlockedMask = BitVector32.CreateMask(IsRunningMask);
		public static readonly int IsLastRunFailedMask = BitVector32.CreateMask(IsBlockedMask);

		public const string StatusUnknown = "Not Running";
		public const string StatusIdle = "Idle";
		public const string StatusRunning = "Running";
		public const string StatusBlocked = "Not Running - Requires Registered Non-Trial Product";
		public const string StatusLastRunFailed = "Last Run Failed";

		public const string RunnerToHostCommunicationServiceTaskErrorCommandPrefix = "STER";
		public const string RunnerToHostCommunicationReenqueueTaskCommandPrefix = "REQU";
		public const string RunnerToHostCommunicationSetNextRuntimeForRetryTaskCommandPrefix = "RESC";
		public const string RunnerToHostCommunicationIdleEvent = "IDLE";
		public const string RunnerToHostGrpcPortLockAquiredCommandPrefix = "GRPC";
		public const string HostLoggerCode = "HOST";
		public const string WebTaskLoggerCode = "WEB";
	}
}
