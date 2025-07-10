using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Enterprise.RemotePrinting.Client
{
	[ImmutableObject(true)]
	public class CNSWClientApplicationSettingManager : CustomseHubClientSettingManager
	{
		public CNSWClientApplicationSettingManager(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient) { }

		protected override Func<WebClient, ICustomseHubClientSetting> GetEhubClientSetting
		{
			get
			{
				return webClient => webClient.GetCNSWClientApplicationSetting(LocalMachineName);
			}
		}

		protected override string SettingNameInCW1 => "CNSW";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Defines the registry path for Single Window Client settings.")]
		protected override string SettingPathInCW1 => "Registry > Customs > China > Single Window Client Application Settings";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Defines China customs integration.")]
		protected override string CustomsName => "China Customs Single Window";

		public new ICNSWClientApplicationSetting CurrentSetting => (ICNSWClientApplicationSetting)base.CurrentSetting;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Ensures folder paths for China customs.")]
		protected override IEnumerable<string> ValidatePaths(ICustomseHubClientSetting setting)
		{
			if (setting is ICNSWClientApplicationSetting cnSetting)
			{
				yield return CheckDictoryAccess("Send Folder", cnSetting.SendFolder);
				yield return CheckDictoryAccess("Receive Folder", cnSetting.ReceiveFolder);
				yield return CheckDictoryAccess("Error Response Folder", cnSetting.ErrorResponseFolder);
				yield return CheckDictoryAccess("Archive Folder", cnSetting.ArchiveFolder);

				if (HasAcdaPaths(cnSetting))
				{
					yield return CheckDictoryAccess("ACDA Send Folder", cnSetting.AcdaSendFolder);
					yield return CheckDictoryAccess("ACDA Receive Folder", cnSetting.AcdaReceiveFolder);
					yield return CheckDictoryAccess("ACDA Error Response Folder", cnSetting.AcdaErrorResponseFolder);
					yield return CheckDictoryAccess("ACDA Archive Folder", cnSetting.AcdaArchiveFolder);
				}
			}
		}

		protected override IEnumerable<string> GetExtraSettingInfos(ICustomseHubClientSetting setting)
		{
			if (setting is ICNSWClientApplicationSetting cnSetting)
			{
				yield return $"\tArchiveFolder:{cnSetting.ArchiveFolder}";
				yield return $"\tReceiveFolder:{cnSetting.ReceiveFolder}";
				yield return $"\tSendFolder:{cnSetting.SendFolder}";
				yield return $"\tErrorResponseFolder:{cnSetting.ErrorResponseFolder}";

				if (HasAcdaPaths(cnSetting))
				{
					yield return $"\tAcdaSendFolder:{cnSetting.AcdaSendFolder}";
					yield return $"\tAcdaReceiveFolder:{cnSetting.AcdaReceiveFolder}";
					yield return $"\tAcdaErrorResponseFolder:{cnSetting.AcdaErrorResponseFolder}";
					yield return $"\tAcdaArchiveFolder:{cnSetting.AcdaArchiveFolder}";
				}
			}
		}

		static bool HasAcdaPaths(ICNSWClientApplicationSetting setting)
		{
			return !(string.IsNullOrWhiteSpace(setting.AcdaSendFolder) && string.IsNullOrWhiteSpace(setting.AcdaReceiveFolder) && string.IsNullOrWhiteSpace(setting.AcdaErrorResponseFolder) && string.IsNullOrWhiteSpace(setting.AcdaArchiveFolder));
		}
	}
}
