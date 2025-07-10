using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;

namespace Enterprise.RemotePrinting.Client
{
	public class CLSMSClientApplicationSettingWrapper : ICLSMSClientApplicationSetting
	{
		public CLSMSClientSetting Instance { get; }

		public CLSMSClientApplicationSettingWrapper(CLSMSClientSetting instance)
		{
			Instance = Argument.NotNull(instance, nameof(instance));
		}

		public string MachineName => Instance.MachineName;

		public string EHubClientStatus => string.Empty;

		public string EHubClientID => string.Empty;

		public string EHubClientPassword => string.Empty;

		public string EHubGatewayServerAddress => string.Empty;

		public string ServerAddress => Instance.ServerAddress;

		public string CertificateForTheServer => Instance.ServerCertificate;

		public string CW1LicenseKey => Instance.RegistrationKey;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public int RunningIntervalInSeconds => Instance.RunningIntervalInSeconds;

		public bool IsValid { get; set; }

		public string ApplicationNodeName => Instance.ApplicationNodeName;

		public string ApplicationNodePassword => Instance.DecryptedApplicationNodePassword;

		public string SendFolder => Instance.SendFolder;

		public string UnknownFolder => Instance.UnknownFolder;

		public string InvalidFolder => Instance.InvalidFolder;

		public string RejectedFolder => Instance.RejectedFolder;

		public string ReceiveFolder => Instance.ReceiveFolder;

		public string AcceptedFolder => Instance.AcceptedFolder;
	}

	public interface ICLSMSClientApplicationSetting : ICustomseHubClientSetting
	{
		string CW1LicenseKey { get; }
		string ServerAddress { get; }
		string CertificateForTheServer { get; }
		string ApplicationNodeName { get; }
		string ApplicationNodePassword { get; }
		string SendFolder { get; }
		string UnknownFolder { get; }
		string InvalidFolder { get; }
		string RejectedFolder { get; }
		string ReceiveFolder { get; }
		string AcceptedFolder { get; }
	}
}
