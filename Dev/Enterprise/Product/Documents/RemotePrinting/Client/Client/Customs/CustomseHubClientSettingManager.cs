using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Client
{
	[ImmutableObject(true)]
	public abstract class CustomseHubClientSettingManager
	{
		protected readonly WebClient WebServiceClient;

		ICustomseHubClientSetting cachedEHubClientSetting;
		DateTime? cachedEHubClientSettingTimeStamp;

		protected virtual int IntervalToRefreshInMilliseconds => 20 * 60 * 1000;

		public event EventHandler<LogEventArgs> LogInformation;
		public Action<ICustomseHubClientSetting> OnSettingDownloaded;

		protected CustomseHubClientSettingManager(string localMachineName, WebClient webServiceClient)
		{
			LocalMachineName = localMachineName;
			WebServiceClient = webServiceClient;
		}

		public string LocalMachineName { get; }

		#region eHubClientSetting get&clear

		protected abstract Func<WebClient, ICustomseHubClientSetting> GetEhubClientSetting { get; }

		public ICustomseHubClientSetting CurrentSetting
		{
			get
			{
				if (cachedEHubClientSetting == null || !cachedEHubClientSettingTimeStamp.HasValue || (DateTime.Now - cachedEHubClientSettingTimeStamp.Value).TotalMilliseconds >= IntervalToRefreshInMilliseconds)
				{
					cachedEHubClientSetting = GetNewSetting();
					cachedEHubClientSettingTimeStamp = DateTime.Now;
				}
				return cachedEHubClientSetting;
			}
		}

		ICustomseHubClientSetting GetNewSetting()
		{
			var setting = GetEhubClientSetting(WebServiceClient);
			OnSettingDownloaded?.Invoke(setting);
			ShowDownloadedSettings(setting);

			OnLogInformation($"Validating {SettingNameInCW1} client application setting:");

			var validationResult = ValidateEHubSetting(setting).Where(c => !string.IsNullOrWhiteSpace(c));
			if (validationResult.Any())
			{
				foreach (var message in validationResult)
				{
					OnLogInformation("\t" + message);
				}
			}
			else
			{
				setting.IsValid = true;
				OnLogInformation($"\t{SettingNameInCW1} client application setting is valid");
			}

			return setting;
		}

		public void ClearCachedSetting()
		{
			cachedEHubClientSetting = default;
		}

		#endregion

		#region Show Infomations

		void OnLogInformation(string message)
		{
			LogInformation?.Invoke(this, new LogEventArgs(message));
		}

		void ShowDownloadedSettings(ICustomseHubClientSetting setting)
		{
			foreach (var info in GetBasicSettingInfos(setting).Union(GetExtraSettingInfos(setting)))
			{
				OnLogInformation(info);
			}
		}

		IEnumerable<string> GetBasicSettingInfos(ICustomseHubClientSetting setting)
		{
			yield return $"{SettingNameInCW1} client application setting downloaded:";
			yield return $"\tMachineName:{setting.MachineName}";
			yield return $"\tRunningIntervalInSeconds:{setting.RunningIntervalInSeconds.ToString(CultureInfo.InvariantCulture)}";
			yield return $"\tEHubGatewayServerAddress:{setting.EHubGatewayServerAddress}";
			yield return $"\tEHubClientID:{setting.EHubClientID}";
			yield return $"\tEHubClientStatus:{setting.EHubClientStatus}";
			yield return $"\tEHubClientPassword:**********";
		}

		protected virtual IEnumerable<string> GetExtraSettingInfos(ICustomseHubClientSetting setting)
		{
			return Enumerable.Empty<string>();
		}

		#endregion

		#region Validations

		protected virtual CustomseHubServiceClientProxy CreateEHubServiceClient(string serverAddress, string clientId, string password)
		{
			return new CustomseHubServiceClientProxy(serverAddress, clientId, password);
		}

		public CustomseHubServiceClientProxy CreateEHubServiceClient(ICustomseHubClientSetting setting)
		{
			return CreateEHubServiceClient(setting.EHubGatewayServerAddress, setting.EHubClientID, setting.EHubClientPassword);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Ensures critical settings are not empty.")]
		readonly string EmptySettingMessageFormat = "{0} should not be empty. Please open Cargo Wise One, go to {1} and set it.";

		protected abstract string SettingNameInCW1 { get; }

		protected abstract string SettingPathInCW1 { get; }

		protected abstract string CustomsName { get; }

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Validates necessary eHub settings for proper operation.")]
		protected virtual IEnumerable<string> ValidateEHubSetting(ICustomseHubClientSetting setting)
		{
			var errorMessages = new List<string>();

			if (string.IsNullOrEmpty(setting.MachineName))
			{
				errorMessages.Add(string.Format(CultureInfo.InvariantCulture, "{0} Setting is empty. If you need the feature of sending message to {1}, please open Cargo Wise One, go to {2} and set it.", SettingNameInCW1, CustomsName, SettingPathInCW1));
			}
			else
			{
				var eHubServiceSettingValid = true;
				if (string.IsNullOrEmpty(setting.EHubGatewayServerAddress))
				{
					eHubServiceSettingValid = false;
					errorMessages.Add("eHub Gateway Server Address should not be empty, please open Cargo Wise One, go to Registry > eServices > eHub > eHub Gateway Server Address and set it.");
				}
				if (string.IsNullOrEmpty(setting.EHubClientID))
				{
					eHubServiceSettingValid = false;
					errorMessages.Add(string.Format(CultureInfo.InvariantCulture, EmptySettingMessageFormat, "eHub Client ID", SettingPathInCW1));
				}
				if (string.IsNullOrEmpty(setting.EHubClientPassword))
				{
					eHubServiceSettingValid = false;
					errorMessages.Add("eHub Client Password should not be empty, please contact your system administrator.");
				}
				if (setting.EHubClientStatus != "OK")
				{
					eHubServiceSettingValid = false;
					errorMessages.Add("eHub Client Status is invalid, please contact your system administrator.");
				}

				if (eHubServiceSettingValid)
				{
					using (var client = CreateEHubServiceClient(setting.EHubGatewayServerAddress, setting.EHubClientID, setting.EHubClientPassword))
					{
						errorMessages.Add(client.Ping());
					}
				}

				errorMessages.AddRange(ValidatePaths(setting));
			}

			return errorMessages.Where(x => !string.IsNullOrEmpty(x));
		}

		protected abstract IEnumerable<string> ValidatePaths(ICustomseHubClientSetting setting);

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		protected string CheckDictoryAccess(string folderName, string path)
		{
			var result = string.Empty;

			if (string.IsNullOrEmpty(path))
			{
				result = string.Format(CultureInfo.InvariantCulture, EmptySettingMessageFormat, folderName, SettingPathInCW1);
			}
			else
			{
				try
				{
					Directory.GetAccessControl(path);
				}
				catch (Exception ex)
				{
					result = $"Access to {folderName} '{path}' failed, " + Regex.Replace(ex.Message, @"[\r\n]+", " ");
				}
			}
			return result;
		}

		#endregion
	}
}
