using System;

namespace Enterprise.RemotePrinting.Client
{
	public interface IUpdateConfigurationProvider
	{
		string MachineName { get; }

		WebClientConfiguration SystemConfiguration { get; }

		WebClientUpdateConfiguration UpdateConfiguration { get; }

		void SaveUpdateTrackingConfiguration(WebClientUpdateConfiguration updateConfiguration);

		void SendNotification(string message);
	}

	public class UpdateConfigurationProvider : IUpdateConfigurationProvider
	{
		readonly ConnectionRegistryManager registryManager;
		readonly string configurationName;
		readonly Action<string, string> sendNotificationAction;

		public UpdateConfigurationProvider(ConnectionRegistryManager registryManager, string configurationName, string machineName, Action<string, string> sendNotificationAction)
		{
			this.registryManager = registryManager;
			this.configurationName = configurationName;
			MachineName = machineName;
			this.sendNotificationAction = sendNotificationAction;
		}

		public string MachineName { get; }

		public WebClientConfiguration SystemConfiguration
		{
			get => registryManager.GetWebClientConfiguration(configurationName);
		}

		public WebClientUpdateConfiguration UpdateConfiguration
		{
			get => SystemConfiguration.UpdateConfiguration;
		}

		public void SaveUpdateTrackingConfiguration(WebClientUpdateConfiguration updateConfiguration)
		{
			registryManager.SaveRemotePrintingUpdateTrackingConfigRegistryValues(configurationName, updateConfiguration);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "sendNotificationAction")]
		public void SendNotification(string message)
		{
			sendNotificationAction?.Invoke("Remote Printing Client Update", message);
		}
	}
}
