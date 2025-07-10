using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Testing;
using Enterprise.Integration;
using Enterprise.MailManager.Business;
using Enterprise.ZArchitecture.Core;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace Enterprise.MailManager.ExternalMailInterface
{
	public class MailKitMailSender : ISmtpSender
	{
		public MailKitMailSender(
			SmtpConfiguration smtpConfiguration,
			IOAuth2Configuration oAuth2Configuration,
			string senderAddress = null,
			ILogger logger = null,
			bool useMailKitSMTPProtocolLogging = false)
			: this(smtpConfiguration, senderAddress, logger, useMailKitSMTPProtocolLogging)
		{
			this.oAuth2Configuration = oAuth2Configuration;
			shouldUseOauth2 = true;
		}

		public MailKitMailSender(
			SmtpConfiguration smtpConfiguration,
			UserPasswordAuthConfiguration userPasswordAuthConfiguration = null,
			string senderAddress = null,
			ILogger logger = null,
			bool useMailKitSMTPProtocolLogging = false)
			: this(smtpConfiguration, senderAddress, logger, useMailKitSMTPProtocolLogging)
		{
			this.userPasswordAuthConfiguration = userPasswordAuthConfiguration;
		}

		MailKitMailSender(SmtpConfiguration smtpConfiguration, string senderAddress = null, ILogger logger = null, bool useMailKitSMTPProtocolLogging = false)
		{
			this.smtpConfiguration = smtpConfiguration;
			this.logger = logger;
			smtpFromAddress = senderAddress;

			DisposableLeakListener.Instance.RegisterDisposable(this);

			if (logger != null && useMailKitSMTPProtocolLogging)
			{
				var loggerStream = ObjectFactory.Get<LoggerStream>(nameof(LoggerStream), logger);
				SmtpClientImpl = new SmtpClientImpl(new ProtocolLogger(loggerStream));
			}
			else
			{
				SmtpClientImpl = new SmtpClientImpl();
			}
#if DEBUG
			this.logger?.Log(LogType.Information, $"{nameof(MailKitMailSender)} constructed."); // To test logger does be passed here.
#endif
		}

		internal readonly ILogger logger;

		readonly SmtpConfiguration smtpConfiguration;
		readonly UserPasswordAuthConfiguration userPasswordAuthConfiguration;
		readonly IOAuth2Configuration oAuth2Configuration;
		readonly string smtpFromAddress;
		readonly bool shouldUseOauth2;

		internal ISmtpClientImpl SmtpClientImpl;
		static readonly object smtpMutex = new object();

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing)
			{
				SmtpClientImpl?.Close();
				SmtpClientImpl?.Dispose();
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
			disposed = true;
		}
		bool disposed;

		~MailKitMailSender()
		{
			Dispose(false);
		}

		public void Send(MailItem mailItem)
		{
			lock (smtpMutex)
			{
				logger?.Log(LogType.Debug,
				$"{nameof(MailKitMailSender)} Connect Smtp -- Host = {smtpConfiguration.Server} Port = {smtpConfiguration.Port} User = {userPasswordAuthConfiguration?.UserName} SenderAddress = {smtpFromAddress} SecureConnectionType = {smtpConfiguration.SecureConnectionType}");

				var retriesMax = 3;
				for (var retries = 0; retries < retriesMax; ++retries)
				{
					try
					{
						Connect();
						SendMimeMessage(mailItem);
						return;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						if (retries == retriesMax - 1)
						{
							throw;
						}
					}
				}
			}
		}

		internal void Connect()
		{
			if (!SmtpClientImpl.IsConnected)
			{
				try
				{
					SmtpClientImpl.Connect(smtpConfiguration);
				}
				catch (NotSupportedException exception) when (exception.Message.Contains(ExceptionHandlingHelper.StartTls, StringComparison.OrdinalIgnoreCase) && smtpConfiguration.SecureConnectionType == SecureConnectionTypes.TLS)
				{
					Close();
					ExceptionHandlingHelper.ThrowStartTlsNotSupportedException(smtpConfiguration);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					try
					{
						Close();
						Thread.Sleep(200);
						SmtpClientImpl.Connect(smtpConfiguration);
					}
					catch (Exception e2) when (!e2.IsCriticalException())
					{
						Close();
						ExceptionHandlingHelper.ThrowFailedToConnectException(e, e2);
					}
				}

				if (!string.IsNullOrEmpty(userPasswordAuthConfiguration?.UserName) && !string.IsNullOrEmpty(userPasswordAuthConfiguration?.Password) || shouldUseOauth2)
				{
					if (SmtpClientImpl.Capabilities.HasFlag(SmtpCapabilities.Authentication))
					{
						try
						{
							if (shouldUseOauth2)
							{
								var oAuth = oAuth2Configuration.GetSaslMechanism();
								SmtpClientImpl.Authenticate(oAuth);

								var username = oAuth.Credentials?.UserName;
								senderProfile = $"From: {username} Server: {smtpConfiguration.Server} with OAuth2";
							}
							else
							{
								senderProfile = $"From: {userPasswordAuthConfiguration.UserName} Server: {smtpConfiguration.Server}";
								SmtpClientImpl.Authenticate(userPasswordAuthConfiguration.UserName, userPasswordAuthConfiguration.Password);
							}
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							Close();
							ExceptionHandlingHelper.ThrowFailedToAuthenticateException(ex);
						}
					}
					else
					{
						Close();
						ExceptionHandlingHelper.ThrowAuthLoginNotSupportedException(smtpConfiguration);
					}
				}
			}
		}

		void SendMimeMessage(MailItem mailItem)
		{
			var mimeMessage = mailItem.BuildMimeMessage();

			string GetCommonLogInfo()
			{
				return $@"SMTP Command Error occurring when sending email with subject '{mailItem.MI_Subject}'.
MessageFromAddress: {mailItem.MI_From}
MessageToAddress: {mailItem.AllRecipients}
RejectedRecipients: {string.Join(",", SmtpClientImpl.RejectedRecipients.Select(x => $"Address:{x.Address},ErrorCode:{x.ErrorCode},ErrorMessage:{x.ErrorMessage}"))}";
			}

			try
			{
				SendMimeMessageInternal();
			}
			catch (SmtpCommandException smtpCommandException)
			{
				try
				{
					var messageFromAddress = mimeMessage.From.OfType<MailboxAddress>().First().Address;
					if (string.IsNullOrEmpty(smtpFromAddress) || messageFromAddress.Equals(smtpFromAddress, StringComparison.InvariantCultureIgnoreCase))
					{
						throw; // If message from address is same with smtp from address, no need another try.
					}

					if (!SmtpClientImpl.IsConnected || !SmtpClientImpl.IsAuthenticated)
					{
						Connect();
					}

					logger?.Log(LogType.Warning, FormattableString.Invariant($"{GetCommonLogInfo()}\r\nError Message: {smtpCommandException.Message}\r\nWill try to deliver the email again using the system email address as the From sender:   From: {smtpFromAddress}."));
					SendMimeMessageInternal(true);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					logger?.Log(LogType.Warning, FormattableString.Invariant($"{GetCommonLogInfo()}\r\nError Message: {e.Message}"));
					CloseAndThrowFailedToSendException(e);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				CloseAndThrowFailedToSendException(e);
			}

			void SendMimeMessageInternal(bool replaceFromAddress = false)
			{
				if (!string.IsNullOrEmpty(smtpFromAddress))
				{
					if (replaceFromAddress)
					{
						SmtpClientImpl.RejectedRecipients.Clear();
						mimeMessage.From.Clear();
						mimeMessage.From.Add(InternetAddress.Parse(smtpFromAddress));
					}

					SmtpClientImpl.Send(mimeMessage, MailboxAddress.Parse(smtpFromAddress), GetMessageRecipients(mimeMessage));
				}
				else
				{
					SmtpClientImpl.Send(mimeMessage);
				}

				rejectedRecipients = SmtpClientImpl.RejectedRecipients.ToArray();
			}

			IList<MailboxAddress> GetMessageRecipients(MimeMessage message)
			{
				var recipients = new List<MailboxAddress>();
				recipients.AddRange(message.To.Mailboxes);
				recipients.AddRange(message.Cc.Mailboxes);
				recipients.AddRange(message.Bcc.Mailboxes);
				return recipients;
			}

			void CloseAndThrowFailedToSendException(Exception e)
			{
				Close();
				ExceptionHandlingHelper.ThrowFailedToSendMessageException(e);
			}
		}

		void Close()
		{
			try
			{
				SmtpClientImpl.Close();
			}
			catch (Exception e) when (!e.IsCriticalException()) //Can't be more specific - it's literally Exception.
			{
				try
				{
					SmtpClientImpl.Close();
				}
				catch (Exception e2) when (!e2.IsCriticalException())
				{
					logger?.Log(LogType.Warning, $"Close connection failed.\r\nError Message: {e2.Message}");
				}
			}
		}

		public RejectedRecipientInfo[] GetRejectedRecipients()
		{
			return rejectedRecipients;
		}

		public string GetSendingInfo() => senderProfile ?? (NoResString)"From: Empty Profile";

		RejectedRecipientInfo[] rejectedRecipients;
		string senderProfile;
	}
}
