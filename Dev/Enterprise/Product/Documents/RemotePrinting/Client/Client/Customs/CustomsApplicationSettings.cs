namespace Enterprise.RemotePrinting.Client.RemotePrintServer
{
	public abstract class CustomsApplicationSetting
	{
		public bool IsDecrypted { get; set; }
	}

	partial class CNSWClientSetting : CustomsApplicationSetting
	{
		public string DecryptedEHubClientPassword { get; set; }
	}

	partial class JPNACCSClientSetting : CustomsApplicationSetting
	{
		public string DecryptedxTPassword { get; set; }
	}

	partial class MailBoxInfo
	{
		public string DecryptedMailBoxPassword { get; set; }
	}

	partial class TWNCATKClientSetting : CustomsApplicationSetting
	{
		public string DecryptedEHubClientPassword { get; set; }
	}

	partial class CLSMSClientSetting : CustomsApplicationSetting
	{
		public string DecryptedApplicationNodePassword { get; set; }
	}
}
