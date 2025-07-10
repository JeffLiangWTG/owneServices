using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;

namespace Enterprise.RemotePrinting.Client
{
	public interface ICNSWClientApplicationSetting : ICustomseHubClientSetting
	{
		string SendFolder { get; }
		string ReceiveFolder { get; }
		string ErrorResponseFolder { get; }
		string ArchiveFolder { get; }
		string AcdaSendFolder { get; }
		string AcdaReceiveFolder { get; }
		string AcdaErrorResponseFolder { get; }
		string AcdaArchiveFolder { get; }
	}

	public class CNSWClientApplicationSettingWrapper : ICNSWClientApplicationSetting
	{
		public readonly CNSWClientSetting Instance;

		public CNSWClientApplicationSettingWrapper(CNSWClientSetting instance)
		{
			Instance = Argument.NotNull(instance, nameof(instance));
		}

		public string MachineName => Instance.MachineName;
		public string SendFolder => Instance.SendFolder;
		public string ReceiveFolder => Instance.ReceiveFolder;
		public string ErrorResponseFolder => Instance.ErrorResponseFolder;
		public string ArchiveFolder => Instance.ArchiveFolder;
		public string AcdaSendFolder => Instance.AcdaSendFolder;
		public string AcdaReceiveFolder => Instance.AcdaReceiveFolder;
		public string AcdaErrorResponseFolder => Instance.AcdaErrorResponseFolder;
		public string AcdaArchiveFolder => Instance.AcdaArchiveFolder;
		public string EHubClientID => Instance.EHubClientID;
		public string EHubClientStatus => Instance.EHubClientStatus;
		public string EHubClientPassword => Instance.DecryptedEHubClientPassword;
		public string EHubGatewayServerAddress => Instance.EHubGatewayServerAddress;
		public int RunningIntervalInSeconds => Instance.RunningIntervalInSeconds;
		public bool IsValid { get; set; }
	}
}
