using System;
using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.xTMessaging.Shared;

namespace Enterprise.RemotePrinting.Client
{
	public sealed class JPNACCSClientApplicationSettingWrapper : IJPNACCSClientApplicationSetting
	{
		public JPNACCSClientApplicationSettingWrapper(JPNACCSClientSetting instance, string machineName)
		{
			MachineName = machineName;
			Instance = Argument.NotNull(instance, nameof(instance));
		}

		public JPNACCSClientSetting Instance { get; }

		public string MachineName { get; }

		string IJPNACCSClientApplicationSetting.DomainName => Instance.DomainName;

		string IJPNACCSClientApplicationSetting.NACCSMailbox => Instance.NACCSMailbox;

		string IJPNACCSClientApplicationSetting.xTServerAddress => Instance.xTServerAddress;

		string IJPNACCSClientApplicationSetting.xTServerCertificate => Instance.xTServerCertificate;

		string IJPNACCSClientApplicationSetting.xTApplicationNode => Instance.xTApplicationNode;

		string IJPNACCSClientApplicationSetting.xTPassword => Instance.DecryptedxTPassword;

		MailBoxInfo[] IJPNACCSClientApplicationSetting.Mailboxes => Instance.MailBoxInfos;

		IDirectxTMessagingConfig IJPNACCSClientApplicationSetting.DirectxTMessagingConfig => directxTMessagingConfig ??= new DirectxTMessagingConfig
		{
			XTIdleConnectionKeepAliveInSecondsValue = Instance.XTIdleConnectionKeepAliveInSecondsValue,
			XTIdleConnectionRetryPauseInSecondsValue = Instance.XTIdleConnectionRetryPauseInSecondsValue,
			InterchangeCountPerBatchOnReceivingValue = Instance.InterchangeCountPerBatchOnReceivingValue,
			XTServerMessageChunkSizeWhenSendingValue = Instance.XTServerMessageChunkSizeWhenSendingValue
		};
		IDirectxTMessagingConfig directxTMessagingConfig;

		bool IJPNACCSClientApplicationSetting.Verbose => Instance.Verbose;

		int IJPNACCSClientApplicationSetting.ReceivingInterval => (int)TimeSpan.FromMinutes(Instance.ReceivingInterval).TotalSeconds;

		int IJPNACCSClientApplicationSetting.SendingInterval => Instance.SendingInterval;

		double IJPNACCSClientApplicationSetting.RetryInterval => 5d;

		int IJPNACCSClientApplicationSetting.MaxRetries => 3;

		DateTime IJPNACCSClientApplicationSetting.DownTimeStart => Instance.DownTimeStart;

		DateTime IJPNACCSClientApplicationSetting.DownTimeEnd => Instance.DownTimeEnd;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration")]
		int ICustomseHubClientSetting.RunningIntervalInSeconds => -1;

		bool ICustomseHubClientSetting.IsValid { get; set; }

		string ICustomseHubClientSetting.EHubClientStatus => string.Empty;

		string ICustomseHubClientSetting.EHubClientID => string.Empty;

		string ICustomseHubClientSetting.EHubClientPassword => string.Empty;

		string ICustomseHubClientSetting.EHubGatewayServerAddress => string.Empty;
	}

	public interface IJPNACCSClientApplicationSetting : ICustomseHubClientSetting
	{
		string DomainName { get; }

		string NACCSMailbox { get; }

		MailBoxInfo[] Mailboxes { get; }

		string xTServerAddress { get; }

		string xTServerCertificate { get; }

		string xTApplicationNode { get; }

		string xTPassword { get; }

		IDirectxTMessagingConfig DirectxTMessagingConfig { get; }

		int ReceivingInterval { get; }

		int SendingInterval { get; }

		double RetryInterval { get; }

		int MaxRetries { get; }

		bool Verbose { get; }

		DateTime DownTimeStart { get; }

		DateTime DownTimeEnd { get; }
	}

	sealed class DirectxTMessagingConfig : IDirectxTMessagingConfig
	{
		public double XTIdleConnectionKeepAliveInSecondsValue { get; set; }

		public double XTIdleConnectionRetryPauseInSecondsValue { get; set; }

		public int InterchangeCountPerBatchOnReceivingValue { get; set; }

		public int XTServerMessageChunkSizeWhenSendingValue { get; set; }
	}
}
