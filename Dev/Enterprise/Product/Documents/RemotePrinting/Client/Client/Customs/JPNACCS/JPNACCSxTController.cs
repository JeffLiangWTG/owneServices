using System.Threading;
using Enterprise.Integration;
using Enterprise.xTMessaging.Shared;
using MailKit;

namespace Enterprise.RemotePrinting.Client
{
	public abstract class JPNACCSxTController : CustomsMessageController
	{
		protected JPNACCSxTController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
		{
		}

		protected virtual IMsgClientProvider MsgClientProvider { get; }

		string ManagerKey { get; set; }

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
		{
			ManagerKey = $"{machineName}+{ConfigSetting.WebServiceUrl}";
			return JPNACCSClientApplicationSettingManagerProvider.GetSettingManager(ManagerKey, machineName, WebServiceClient);
		}

		public override void Stop()
		{
			if (!string.IsNullOrWhiteSpace(ManagerKey))
			{
				JPNACCSClientApplicationSettingManagerProvider.Remove(ManagerKey);
			}

			base.Stop();
		}

		protected ILogger xTLogger => (SettingManager.CurrentSetting as IJPNACCSClientApplicationSetting)?.Verbose ?? false ? new LoggerxTAdaptor(this) : new NullLogger();

		protected IProtocolLogger ProcotolLogger => (SettingManager.CurrentSetting as IJPNACCSClientApplicationSetting)?.Verbose ?? false ? new ProtocolLoggerAdaptor(this) : new NullProtocolLogger();

		public INACCSErrorSender GetErrorSender() => GetErrorSenderCore();

		protected abstract INACCSErrorSender GetErrorSenderCore();

		public void Log(LogType type, string message)
		{
			switch (type)
			{
				case LogType.Error:
					OnShowError(message);
					break;
				default:
					OnShowInformation(message);
					break;
			}
		}
	}
}
