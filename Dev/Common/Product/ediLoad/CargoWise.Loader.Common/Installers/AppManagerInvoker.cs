using System;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public abstract class AppManagerInvoker : InstallationItem
	{
		protected AppManagerInvoker(Installation installation)
			: base(installation)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		public static string RemotingErrorMessage
		{
			get
			{
				return "An error has occurred while attempting to communicate with the " + AppManagerConfiguration.Instance.ServiceLongName +
					" service." + Environment.NewLine + @"Please check that the service is running. You can do so by accessing Control Panel\System and Maintenance\Administrative Tools\Services." +
					Environment.NewLine + "If the service is already running, please try restarting it.";
			}
		}

		protected int RetryCount { get; private set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		public static string ServerBeingUpgradedErrorMessage
		{
			get
			{
				return "The " + AppManagerConfiguration.Instance.ServiceLongName +
					" service is being upgraded. Please try again in a few minutes.";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		public static string ServerBusyErrorMessage
		{
			get
			{
				return "The " + AppManagerConfiguration.Instance.ServiceLongName +
					" is busy processing requests for other clients. Please try again in a few minutes. If the problem persists, please try restarting the service.";
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		public static string TimeoutErrorMessage
		{
			get
			{
				return "A request to the " + AppManagerConfiguration.Instance.ServiceLongName +
					" service has timed out. Please try again in a few minutes. If the problem persists, please try restarting the service.";
			}
		}

		protected virtual InstallationResult HandleAppManagerException(Exception ex)
		{
			return null;
		}

		bool IsRemoteExecutionExceptionType(Type type)
		{
			if (type == null)
			{
				return false;
			}

			if (type.Name == "RemotingException")
			{
				return true;
			}

			return IsRemoteExecutionExceptionType(type.BaseType);
		}

		protected InstallationResult InvokeAppManager<T>(object state, MutexRequest request) where T : IAppManagerInvocable
		{
			Argument.NotNull(state, nameof(state));
			return InvokeAppManager(typeof(T), state, request);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Error message")]
		protected virtual InstallationResult InvokeAppManager(Type type, object state, MutexRequest request)
		{
			Argument.NotNull(type, nameof(type));
			if (!typeof(IAppManagerInvocable).IsAssignableFrom(type))
			{
				throw new ArgumentException("The type must implement IAppManagerInvocable.", nameof(type));
			}

			InstallationResult unknownInstallationResult = InstallationResult.Error("The " + AppManagerConfiguration.Instance.ServiceLongName + " service returned an unknown status. This application cannot proceed.");
			InstallationResult exceptionResult = null;
			try
			{
				AppManagerResult result;
				using (var appManager = Installation.Configuration.GetNewAppManagerClient())
				{
					result = appManager.Invoke(
						type.Assembly.Location,
						type.FullName,
						state,
						request);
				}
				switch (result.Status)
				{
					case AppManagerResultStatus.Error:
						return InstallationResult.Error(result.Message);

					case AppManagerResultStatus.Retry:
						RetryCount++;
						return Retry(type, state, request);

					case AppManagerResultStatus.Success:
						return string.IsNullOrEmpty(result.Message) ? InstallationResult.OK() : InstallationResult.Warning(result.Message);

					case AppManagerResultStatus.TimedOut:
						return IgnoreMutexErrors ? InstallationResult.OK() : InstallationResult.Error(TimeoutErrorMessage);

					case AppManagerResultStatus.WaitingForUpgrade:
						return IgnoreMutexErrors ? InstallationResult.OK() : InstallationResult.Error(ServerBeingUpgradedErrorMessage);
				}
				return unknownInstallationResult;
			}
			catch (Exception ex) when (IsRemoteExecutionExceptionType(ex.GetType()))
			{
				if (ex.Message.Contains(CodeContractExceptionProcessing.CodeContractExceptionTag))
				{
					return InstallationResult.Error(ex.Message);
				}
				else
				{
					return InstallationResult.Error(RemotingErrorMessage);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException() && (exceptionResult = HandleAppManagerException(ex)) != null)
			{
				return exceptionResult;
			}
		}

		protected bool IgnoreMutexErrors
		{
			get;
			set;
		}

		protected virtual InstallationResult Retry(Type type, object state, MutexRequest request)
		{
			Argument.NotNull(type, nameof(type));
			if (!typeof(IAppManagerInvocable).IsAssignableFrom(type))
			{
				throw new ArgumentException("Invalid argument.", nameof(type));
			}

			if (RetryCount > 1)
			{
				return InstallationResult.Error(ServerBusyErrorMessage);
			}
			else
			{
				return NeedsToInstall() ? InvokeAppManager(type, state, request) : InstallationResult.OK();
			}
		}
	}
}

