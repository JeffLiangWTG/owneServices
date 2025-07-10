using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.RemotePrinting.Client
{
	public class CLSMSClientApplicationSettingManager : CustomseHubClientSettingManager
	{
		public CLSMSClientApplicationSettingManager(string localMachineName, WebClient webServiceClient) : base(localMachineName, webServiceClient) { }

		protected override Func<WebClient, ICustomseHubClientSetting> GetEhubClientSetting => client => client.GetCLSMSClientApplicationSetting(LocalMachineName);

		protected override string SettingNameInCW1 => "CLSMS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Specifies the setting path in CW1.")]
		protected override string SettingPathInCW1 => "Registry > Customs > Chile > SMS Client Application Settings";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Specifies the customs name for Chile.")]
		protected override string CustomsName => "Chile Customs";

		public new ICLSMSClientApplicationSetting CurrentSetting => (ICLSMSClientApplicationSetting)base.CurrentSetting;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Analyzer warning is not relevant.")]
		protected override IEnumerable<string> ValidatePaths(ICustomseHubClientSetting setting)
		{
			if (setting is ICLSMSClientApplicationSetting clSetting)
			{
				yield return CheckDictoryAccess("Send To Folder", clSetting.SendFolder);
				yield return CheckDictoryAccess("Receive Folder", clSetting.ReceiveFolder);
				yield return CheckDictoryAccess("Invalid Folder", clSetting.InvalidFolder);
				yield return CheckDictoryAccess("Rejected Folder", clSetting.RejectedFolder);
				yield return CheckDictoryAccess("Unknown Folder", clSetting.UnknownFolder);
				yield return CheckDictoryAccess("Accepted Folder", clSetting.AcceptedFolder);
			}
		}

		protected override IEnumerable<string> GetExtraSettingInfos(ICustomseHubClientSetting setting)
		{
			if (setting is ICLSMSClientApplicationSetting clSetting)
			{
				yield return $"\tSendFolder:{clSetting.SendFolder}";
				yield return $"\tReceiveFolder:{clSetting.ReceiveFolder}";
				yield return $"\tInvalidFolder:{clSetting.InvalidFolder}";
				yield return $"\tRejectedFolder:{clSetting.RejectedFolder}";
				yield return $"\tUnknownFolder:{clSetting.UnknownFolder}";
				yield return $"\tAcceptedFolder:{clSetting.AcceptedFolder}";
			}
		}

		protected override IEnumerable<string> ValidateEHubSetting(ICustomseHubClientSetting setting)
		{
			var errorMessages = new List<string>();
			if (!string.IsNullOrEmpty(setting.MachineName))
			{
				errorMessages.AddRange(ValidatePaths(setting));
			}

			return errorMessages.Where(x => !string.IsNullOrEmpty(x));
		}
	}
}
