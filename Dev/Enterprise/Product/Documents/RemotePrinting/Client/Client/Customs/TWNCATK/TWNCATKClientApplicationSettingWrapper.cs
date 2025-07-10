using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;

namespace Enterprise.RemotePrinting.Client
{
	public interface ITWNCATKClientApplicationSetting : ICustomseHubClientSetting
	{
		string SendToFolder { get; }
	}

	public class TWNCATKClientApplicationSettingWrapper : ITWNCATKClientApplicationSetting
	{
		public TWNCATKClientSetting Instance { get; }

		public TWNCATKClientApplicationSettingWrapper(TWNCATKClientSetting instance)
		{
			Instance = Argument.NotNull(instance, nameof(instance));
		}

		public string MachineName => Instance.MachineName;

		public string EHubClientID => Instance.EHubClientID;

		public string EHubClientStatus => Instance.EHubClientStatus;

		public int RunningIntervalInSeconds => Instance.RunningIntervalInSeconds;

		public string SendToFolder => Instance.SendToFolder;

		public string EHubGatewayServerAddress => Instance.EHubGatewayServerAddress;

		public string EHubClientPassword => Instance.DecryptedEHubClientPassword;

		public bool IsValid { get; set; }
	}
}
