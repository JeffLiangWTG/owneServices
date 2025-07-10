using System;
using System.Threading;
using Enterprise.xTMessaging.Shared;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace Enterprise.RemotePrinting.Client
{
	public class JPNACCSMessageSenderController : JPNACCSxTController
	{
		public JPNACCSMessageSenderController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "WebPrint Client has no access to ZDateTime")]
		protected override int ProcessCore(ICustomseHubClientSetting setting)
		{
			var currentTime = DateTime.Now;

			if (setting is IJPNACCSClientApplicationSetting jpNACCSSetting && (currentTime < jpNACCSSetting.DownTimeStart || currentTime > jpNACCSSetting.DownTimeEnd))
			{
				SendNACCSMessages(jpNACCSSetting);
			}

			return 0;
		}

		void SendNACCSMessages(IJPNACCSClientApplicationSetting setting)
		{
			using var connector = new DirectxTConnector(MsgClientProvider ?? NACCSUtils.GetMsgClientProvider(setting), setting.DirectxTMessagingConfig, xTLogger, null, new NACCSMessageSenderHandler(this));
			if (connector.InitializeWithFullLogging())
			{
				var cancellationToken = GetCancellationTokenCore();
				connector.Receive(cancellationToken);
			}
		}

		protected virtual CancellationToken GetCancellationTokenCore() => CancellationToken.None;

		protected override INACCSErrorSender GetErrorSenderCore() => new NACCSErrorSender(ProtocolType.SMTP, SettingManager as JPNACCSClientApplicationSettingManager, MsgClientProvider, xTLogger);

		public ISendMessageClient GetMessageClient() => GetMessageClientCore();

		protected virtual ISendMessageClient GetMessageClientCore() => new NACCSSMTPClient(ProcotolLogger);

		protected override int GetRunningIntervalCore(ICustomseHubClientSetting setting) => (setting as IJPNACCSClientApplicationSetting).SendingInterval;
	}

	sealed class NACCSSMTPClient : SmtpClient, ISendMessageClient
	{
		public NACCSSMTPClient(IProtocolLogger protocolLogger) : base(protocolLogger)
		{
		}

		double ISendMessageClient.Interval => 5;

		int ISendMessageClient.ConnectMaxRetries => 3;

		int ISendMessageClient.CommandMaxRetries => 3;

		void ISendMessageClient.Connect(string host) => base.Connect(host);

		void ISendMessageClient.Disconnect(bool quit) => base.Disconnect(quit);

		void ISendMessageClient.Send(MimeMessage message) => base.Send(message);

		protected override void Prepare(FormatOptions options, MimeMessage message, EncodingConstraint constraint, int maxLineLength)
		{
			// do not try to recode message, because the message is already optimized for NACCS transmission
			// this fixes Content-Transfer-Encoding as "8bit"
		}
	}
}
