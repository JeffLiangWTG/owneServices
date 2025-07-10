using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Enterprise.RemotePrinting.Client
{
	public static class JPNACCSClientApplicationSettingManagerProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule")]
		static readonly ConcurrentDictionary<string, JPNACCSClientApplicationSettingManager> Managers = new ();

		public static JPNACCSClientApplicationSettingManager GetSettingManager(string key, string localMachineName, WebClient client)
		{
			return Managers.GetOrAdd(key, (s) => new JPNACCSClientApplicationSettingManager(localMachineName, client));
		}

		public static void Remove(string key)
		{
			Managers.TryRemove(key, out var _);
		}
	}

	[ImmutableObject(true)]
	public class JPNACCSClientApplicationSettingManager : CustomseHubClientSettingManager
	{
		public JPNACCSClientApplicationSettingManager(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient)
		{
		}

		public new IJPNACCSClientApplicationSetting CurrentSetting => base.CurrentSetting as IJPNACCSClientApplicationSetting;

		public IJPNACCSClientApplicationSetting GetOrLoadSetting()
		{
			var result = CurrentSetting;

			if (result == null)
			{
				ClearCachedSetting();

				var retries = 0;

				while (retries < 3)
				{
					result = CurrentSetting;

					if (result != null)
					{
						break;
					}
					else
					{
						retries++;
						Thread.Sleep(TimeSpan.FromSeconds(5));
					}
				}
			}

			return result;
		}

		protected override Func<WebClient, ICustomseHubClientSetting> GetEhubClientSetting => (webClient) => webClient.GetJPNACCSClientApplicationSetting(LocalMachineName);

		protected override string SettingNameInCW1 => "JPNACCS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Path is specific to Japan NACCS settings.")]
		protected override string SettingPathInCW1 => "Registry > Customs > Country or Region Specific > Japan > NACCS Messaging > Remote WebPrint Client Configurations";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Name reflects specific Japan NACCS system.")]
		protected override string CustomsName => "JAPAN CUSTOMS NACCS SYSTEM";

		protected override IEnumerable<string> ValidateEHubSetting(ICustomseHubClientSetting setting)
		{
			if (setting is IJPNACCSClientApplicationSetting applicationSetting)
			{
				if (string.IsNullOrWhiteSpace(applicationSetting.xTPassword))
				{
					yield return NACCSConstants.ErrorMessages.ErrorSettingLog;
				}

				yield return CheckIsValidSetting(nameof(applicationSetting.NACCSMailbox), applicationSetting.NACCSMailbox);
				yield return CheckIsValidSetting(nameof(applicationSetting.xTServerAddress), applicationSetting.xTServerAddress);
				yield return CheckIsValidSetting(nameof(applicationSetting.xTServerCertificate), applicationSetting.xTServerCertificate);
				yield return CheckIsValidSetting(nameof(applicationSetting.xTApplicationNode), applicationSetting.xTApplicationNode);
			}
		}

		string CheckIsValidSetting(string name, string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return $"{name} should not be empty.";
			}

			return string.Empty;
		}

		protected override IEnumerable<string> ValidatePaths(ICustomseHubClientSetting setting)
		{
			yield break;
		}
	}
}
