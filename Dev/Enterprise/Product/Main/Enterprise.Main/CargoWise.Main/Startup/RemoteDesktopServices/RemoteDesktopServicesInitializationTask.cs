using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using Enterprise.Integration.Licensing;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	class RemoteDesktopServicesInitializationTask : AbstractApplicationStartupTask
	{
		public override string TaskDescription => Res.GetString("3254237a-f51b-466b-ac4c-43fb95696d8f", "Initializing Remote Desktop Services");

		protected override bool GetShouldExecute(CommandLineArguments arguments)
		{
#if DEBUG
			if ((bool)arguments[ApplicationArguments.OptionTestAdapter])
			{
				return false;
			}
#endif
			return true;
		}

		protected virtual IProductRegistrationKey GetRegistrationKey()
		{
			return ObjectFactory.Get<IProductRegistration>().Key;
		}

		protected override bool DoExecute(CommandLineArguments arguments)
		{
			var terminalService = ObjectFactory.Get<TerminalService>();

			if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode != RemoteConnectingModes.ServerOnly
				&& terminalService.IsWTSSession
				&& terminalService.IsRemoteAppSession)
			{
				var initialized = InitializeChannel(terminalService.IsCitrixICA);
				var isReconnection = (bool)arguments.OptionalArgs[ApplicationArguments.OptionReconnect];

				if (!EnterpriseChannel.Instance.CreatedNewMutex && isReconnection)
				{
					EnterpriseChannel.Instance.Close();
					return false;
				}

				return initialized;
			}

			return true;
		}

#if DEBUG
		internal
#endif
		bool InitializeChannel(bool isCitrix)
		{
			ChannelInitialized = false;
			EnterpriseChannel.Initialize();
			MessageHandlers.Register(EnterpriseChannelMessageTypes.EdiEntUrl, new RemoteDesktopServicesEnterpriseUrlHandler());

			var remoteVersion = InitializationMessageHandler.RemoteVersion;

			if (remoteVersion == null && isCitrix)
			{
				// Try the old citrix client
				EnterpriseChannel.Instance.Close();
				EnterpriseChannel.Initialize(useLegacyCitrix: true);
				remoteVersion = InitializationMessageHandler.RemoteVersion;
			}

			if (remoteVersion == null)
			{
				EnterpriseChannel.Instance.Close();
				if (DataRegistry.Instance.RemoteAppAllowEDocAccessWithoutConnectorMode == RemoteConnectingModes.ConnectorOnly)
				{
					Globals.Message.ShowWarning(ZTerminalService.ClientPluginApplicationNotInstalledWarning);
				}
				return true;
			}

			if (GetServerVersion(isCitrix) > remoteVersion)
			{
				var productName = isCitrix ? ClientCitrixVersion.ProductName : ClientVersion.ProductName;
				var result = Globals.Message.Show(Res.GetString("1bcc7e88-3c85-48ac-9b3b-97cddf18d19f", "A new version of {0} is available and will be installed now.", productName), productName, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

				if (result == DialogResult.Cancel)
				{
					EnterpriseChannel.Instance.Close();
					Globals.Message.ShowWarning(ZTerminalService.ClientPluginApplicationNotInstalledWarning);
					return true;
				}

				if (RemoteDesktopServicesPostLoginTask.Upgrade())
				{
					EnterpriseChannel.Instance.Close();
					return false;
				}
			}
			ChannelInitialized = true;
			return true;
		}

		protected virtual Version GetServerVersion(bool isCitrix)
		{
			return isCitrix
				? ClientCitrixVersion.Version
				: ClientVersion.Version;
		}

		protected Version GetExecutionResultCallbackVersion(bool isCitrix)
		{
			return isCitrix ? new Version(1, 7, 7) : new Version(4, 12, 7);
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Exposing state across different startup tasks")]
		public static bool ChannelInitialized { get; private set; }

		public string SharedMutexName => string.Format(CultureInfo.InvariantCulture, "CargoWiseOne_Citrix_Session_{0}", Process.GetCurrentProcess().SessionId);

		public override int FailureExitCode => ExitCodes.RemoteDesktopServicesInitializationTaskError;
	}
}
