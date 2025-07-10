using System;
using System.Collections.Generic;

namespace Enterprise.RemotePrinting.Client
{
	public class TWNCATKClientApplicationSettingManager : CustomseHubClientSettingManager
	{
		public TWNCATKClientApplicationSettingManager(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient) { }

		protected override Func<WebClient, ICustomseHubClientSetting> GetEhubClientSetting => client => client.GetTWNCATKClientApplicationSetting(LocalMachineName);

		protected override string SettingNameInCW1 => "NCATK";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Path specifies NCATK message settings for Taiwan Customs.")]
		protected override string SettingPathInCW1 => "Registry > Customs > Taiwan > NCATK Message Sending Configuration";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Name aligns with Taiwan Customs system.")]
		protected override string CustomsName => "Taiwan Customs";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Validation ensures necessary folder access for NCATK client.")]
		protected override IEnumerable<string> ValidatePaths(ICustomseHubClientSetting setting)
		{
			if (setting is ITWNCATKClientApplicationSetting twSetting)
			{
				yield return CheckDictoryAccess("Send To Folder", twSetting.SendToFolder);
			}
		}

		protected override IEnumerable<string> GetExtraSettingInfos(ICustomseHubClientSetting setting)
		{
			if (setting is ITWNCATKClientApplicationSetting twSetting)
			{
				yield return $"\tSendToFolder:{twSetting.SendToFolder}";
			}
		}
	}
}
