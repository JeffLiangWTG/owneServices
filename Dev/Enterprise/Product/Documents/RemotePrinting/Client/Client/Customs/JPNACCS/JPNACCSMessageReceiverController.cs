using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.xTMessaging.Shared;
using MailKit;
using MailKit.Net.Pop3;
using MimeKit;

namespace Enterprise.RemotePrinting.Client
{
	public class JPNACCSMessageReceiverController : JPNACCSxTController
	{
		public JPNACCSMessageReceiverController(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "WebPrint Client has no access to ZDateTime")]
		protected override int ProcessCore(ICustomseHubClientSetting setting)
		{
			var currentTime = DateTime.Now;

			if (setting is IJPNACCSClientApplicationSetting naccsSetting && (currentTime < naccsSetting.DownTimeStart || currentTime > naccsSetting.DownTimeEnd))
			{
				ReceiveNACCSMessages(naccsSetting);
			}

			return 0;
		}

		void ReceiveNACCSMessages(IJPNACCSClientApplicationSetting setting)
		{
			var naccsMailbox = MailboxAddress.Parse(setting.NACCSMailbox);
			ReceiveNACCSMessagesCore(setting, naccsMailbox, setting.Mailboxes.ToDictionary(c => c, _ => 0));
		}

		void ReceiveNACCSMessagesCore(IJPNACCSClientApplicationSetting setting, MailboxAddress naccsMailbox, Dictionary<MailBoxInfo, int> mailboxInfoRecords)
		{
			var failedMailboxInfos = new Dictionary<MailBoxInfo, int>();

			using (var connector = new DirectxTConnector(MsgClientProvider ?? NACCSUtils.GetMsgClientProvider(setting), setting.DirectxTMessagingConfig, xTLogger, NACCSUtils.GetSubmitMsgAttributeModifier(setting)))
			{
				connector.InitializeWithFullLogging();

				var handler = new NACCSMessageReceiverHandler(this);

				foreach (var mailboxInfoRecord in mailboxInfoRecords)
				{
					var mailboxInfo = mailboxInfoRecord.Key;
					var mailboxInfoRetries = mailboxInfoRecord.Value;
					var enableSendError = mailboxInfoRetries >= setting.MaxRetries;

					if (!handler.Receive(setting, connector, naccsMailbox, mailboxInfo, enableSendError) && !enableSendError)
					{
						failedMailboxInfos.Add(mailboxInfo, mailboxInfoRetries + 1);
					}
				}
			}

			if (failedMailboxInfos.Count > 0)
			{
				var delaySeconds = TimeSpan.FromSeconds(Math.Max(0.1, setting.RetryInterval));
				OnShowInformation($"[Info] Try reconnecting {failedMailboxInfos.Count} mailbox(es) and then receiving mail after {delaySeconds} seconds");

				Thread.Sleep(delaySeconds);

				ReceiveNACCSMessagesCore(setting, naccsMailbox, failedMailboxInfos);
			}
		}

		protected override INACCSErrorSender GetErrorSenderCore() => new NACCSErrorSender(ProtocolType.POP3, SettingManager as JPNACCSClientApplicationSettingManager, MsgClientProvider, xTLogger);

		public IReceiveMessageClient GetMessageClient() => GetMessageClientCore();

		protected virtual IReceiveMessageClient GetMessageClientCore() => new NACCSPOP3Client(ProcotolLogger);

		protected override int GetRunningIntervalCore(ICustomseHubClientSetting setting) => (setting as IJPNACCSClientApplicationSetting).ReceivingInterval;
	}

	sealed class NACCSPOP3Client : Pop3Client, IReceiveMessageClient
	{
		public NACCSPOP3Client(IProtocolLogger protocolLogger)
			: base(protocolLogger)
		{
		}

		double IReceiveMessageClient.Interval => 5;

		int IReceiveMessageClient.ConnectMaxRetries => 3;

		int IReceiveMessageClient.AuthenticateMaxRetries => 3;

		int IReceiveMessageClient.CommandMaxRetries => 3;

		bool IReceiveMessageClient.IsAuthenticated => base.IsAuthenticated;

		void IReceiveMessageClient.Connect(string host) => base.Connect(host);

		void IReceiveMessageClient.Authenticate(string userName, string password) => base.Authenticate(userName, password);

		MimeMessage IReceiveMessageClient.GetMessage(int index) => base.GetMessage(index);

		void IReceiveMessageClient.DeleteMessage(int index) => base.DeleteMessage(index);

		void IReceiveMessageClient.Disconnect(bool quit) => base.Disconnect(quit);
	}
}
