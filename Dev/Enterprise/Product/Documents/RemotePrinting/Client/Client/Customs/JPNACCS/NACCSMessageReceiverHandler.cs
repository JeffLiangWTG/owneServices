using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Enterprise.xTMessaging.Shared;
using MailKit;
using MailKit.Net.Pop3;
using MailKit.Security;
using MimeKit;

namespace Enterprise.RemotePrinting.Client;

sealed class NACCSMessageReceiverHandler
{
	public NACCSMessageReceiverHandler(JPNACCSMessageReceiverController controller)
	{
		this.controller = Argument.NotNull(controller, nameof(controller));
		errorSender = controller.GetErrorSender();
		context = new MessageReceiverContext();
	}

	readonly JPNACCSMessageReceiverController controller;
	readonly INACCSErrorSender errorSender;
	readonly MessageReceiverContext context;

	public bool Receive(IJPNACCSClientApplicationSetting setting, DirectxTConnector connector, MailboxAddress mailboxAddress, MailBoxInfo mailboxInfo, bool enableSendError)
	{
		var result = false;

		var retries = 0;
		var maxRetries = 1;

		var errorMessage = new StringBuilder();
		Exception lastException = null;

		context.Domain = mailboxAddress.Domain;
		context.MailBoxInfo = mailboxInfo;

		var mailbox = context.MailBoxInfo.MailBox;

		while (maxRetries >= retries)
		{
			using (var messageClient = controller.GetMessageClient())
			{
				maxRetries = Math.Max(1, messageClient.CommandMaxRetries);

				try
				{
					result = TryAuthenticate(messageClient, enableSendError) && GetMessageCore(messageClient, connector);
					break;
				}
				catch (Pop3CommandException ex)
				{
					SendPOP3Error(mailbox, $"[1] {ex}", $"POP3 Execute Command Failed. Mailbox: {mailbox} Domain: {context.Domain}");
					ErrorReporter.ReportError($"[{nameof(NACCSMessageReceiverHandler)}] Can't receive messages from Mailbox: {mailbox} Domain: {context.Domain}.", ex);

					result = true;
					break;
				}
				catch (Exception ex) when (ex is TimeoutException || ex is ServiceNotConnectedException)
				{
					retries++;

					var delaySeconds = TimeSpan.FromSeconds(Math.Max(0.1, messageClient.Interval));

					LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] An exception [{ex.GetType().Name}] happen, pause {delaySeconds.TotalSeconds} second(s).");
					LogInformation(ex.ToString());

					Thread.Sleep(delaySeconds);

					errorMessage.AppendLine($"[{retries}] {ex}");
					errorMessage.AppendLine();

					lastException = ex;
				}
				finally
				{
					messageClient.Disconnect(true);
				}
			}
		}

		if (enableSendError && !result && errorMessage.Length > 0)
		{
			SendPOP3Error(mailbox, errorMessage.ToString(), $"POP3 Get Message Failed. Mailbox: {mailbox}", lastException is ServiceNotConnectedException ? "" : setting.MachineName);
		}

		return result;
	}

	bool GetMessageCore(IReceiveMessageClient messageClient, DirectxTConnector connector)
	{
		var result = true;

		for (var i = 0; i < messageClient.Count; i++)
		{
			var message = messageClient.GetMessage(i);
			var businessCode = string.Empty;
			var userCode = string.Empty;

			using (var contentStream = new MemoryStream())
			{
				((MimePart)message.Body).WriteTo(contentStream, contentOnly: true);

				contentStream.Seek(0, SeekOrigin.Begin);
				(businessCode, userCode, var messageReference) = NACCSUtils.ExtractInboundHeaderInformation(contentStream);

				contentStream.Seek(0, SeekOrigin.Begin);
				LogInformation($"{userCode} received message {messageReference} for procedure {businessCode} from {context.MailBoxInfo.MailBox}.");
			}

			using (var messageStream = new MemoryStream())
			using (var bodyReader = new BinaryReader(messageStream))
			{
				message.WriteTo(messageStream);
				messageStream.Seek(0, SeekOrigin.Begin);

				var messageInfo = new BasicXtMessageInfo
				(
					NACCSConstants.ApplicationCode,
					businessCode.Substring(0, Math.Min(businessCode.Length, 3)),
					NACCSConstants.CustomsParty,
					userCode,
					Guid.NewGuid().ToString(),
					bodyReader
				);

				var (isSuccess, msgId, error) = connector.SendInterchange(messageInfo).GetAwaiter().GetResult();
				if (isSuccess)
				{
					LogInformation($"xT Message:{msgId} submitted");
					messageClient.DeleteMessage(i);
				}
				else
				{
					result = false;
					LogError(error);
				}
			}
		}

		return result;
	}

	bool TryConnectDomain(IReceiveMessageClient messageClient, bool enableSendError)
	{
		if (messageClient.IsConnected)
		{
			return true;
		}

		var result = false;
		var errorMessage = new StringBuilder();
		var retries = 0;

		var maxRetries = Math.Max(1, messageClient.ConnectMaxRetries);

		while (retries <= maxRetries)
		{
			try
			{
				messageClient.Connect(context.Domain);
				result = true;

				break;
			}
			catch (Pop3CommandException)
			{
				throw;
			}
			catch (Exception ex)
			{
				retries++;

				var delaySeconds = TimeSpan.FromSeconds(Math.Max(0.1, messageClient.Interval));

				LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] An exception [{ex.GetType().Name}] happen, pause {delaySeconds.TotalSeconds} second(s).");
				LogInformation(ex.ToString());

				Thread.Sleep(delaySeconds);

				errorMessage.AppendLine($"[{retries}] {ex}");
				errorMessage.AppendLine();

				if (retries >= maxRetries)
				{
					var setting = GetOrLoadSetting(true);
					if (setting != null && MailboxAddress.TryParse(setting.NACCSMailbox, out var domainMailbox))
					{
						context.Domain = domainMailbox.Domain;
					}
					else
					{
						errorMessage.AppendLine($"[{retries}] Can't load a valid setting or the NACCSMailbox [{setting.NACCSMailbox}] is not a valid MailboxAddress.");
						errorMessage.AppendLine();

						break;
					}
				}
			}
		}

		if (enableSendError && !result && errorMessage.Length > 0)
		{
			var setting = GetOrLoadSetting();

			var errorTitle = $"POP3 Connect Failed. Domain: {context.Domain}";
			var errorType = NACCSConstants.ErrorTypes.TransmissionError;
			var errorDescription = errorMessage.ToString();

			if (setting != null)
			{
				var mailbox = context.MailBoxInfo?.MailBox ?? string.Empty;
				var companyCode = context.MailBoxInfo?.CompanyCode ?? string.Empty;
				var prefixTips = $@"POP3 connection fails. Please check the followings: 
	1.You are connected to the NACCS internet via a NACCS router. 
	2.The mailbox {mailbox} is valid. To update this value, visit CargoWise > Maintain > User Admin > Companies > {companyCode} > Brokerage > NACCS Mailbox. 
If the issue persists. Please raise an eRequest (Product: CargoWise; Module: Customs; Country: Japan) and provide the following exception message to the support. You can raise an eRequest by pressing F1 in CargoWise.";
				SendPOP3Error(context.Domain, prefixTips + "\n\n" + errorDescription, errorTitle, setting.MachineName, errorType, [new(NACCSConstants.Attributes.Domain, context.Domain)]);
			}
			else
			{
				ShowErrorWithoutSetting(errorTitle, errorType, errorDescription);
			}
		}

		return messageClient.IsConnected;
	}

	bool TryAuthenticate(IReceiveMessageClient messageClient, bool enableSendError)
	{
		if (messageClient.IsConnected && messageClient.IsAuthenticated)
		{
			return true;
		}

		var result = false;

		var retries = 0;
		var errorMessage = new StringBuilder();

		var mailbox = context.MailBoxInfo.MailBox;
		var password = context.MailBoxInfo.DecryptedMailBoxPassword;

		var maxRetries = Math.Max(1, messageClient.AuthenticateMaxRetries);
		var lastErrorType = string.Empty;

		while (retries <= maxRetries)
		{
			try
			{
				if (TryConnectDomain(messageClient, enableSendError))
				{
					if (!messageClient.IsAuthenticated)
					{
						messageClient.Authenticate(mailbox, password);
					}

					result = true;
				}

				break;
			}
			catch (Pop3CommandException)
			{
				throw;
			}
			catch (Exception ex)
			{
				retries++;

				var delaySeconds = TimeSpan.FromSeconds(Math.Max(0.1, messageClient.Interval));

				LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] An exception [{ex.GetType().Name}] happen, pause {delaySeconds.TotalSeconds} second(s).");
				LogInformation(ex.ToString());

				Thread.Sleep(delaySeconds);

				lastErrorType = ex is ServiceNotAuthenticatedException || ex is AuthenticationException
					? NACCSConstants.ErrorTypes.Unauthorized
					: NACCSConstants.ErrorTypes.TransmissionError;

				errorMessage.AppendLine($"[{retries}] {ex}");
				errorMessage.AppendLine();

				var setting = GetOrLoadSetting(true);

				var latestMailboxInfo = setting?.Mailboxes?.FirstOrDefault(c => c.MailBox == mailbox);

				if (latestMailboxInfo != null)
				{
					context.MailBoxInfo = latestMailboxInfo;
					password = latestMailboxInfo.DecryptedMailBoxPassword;
				}
				else
				{
					errorMessage.AppendLine($"[{retries}] Can't load the mailbox [{mailbox}] from the latest setting.");
					errorMessage.AppendLine();

					break;
				}
			}
		}

		if (enableSendError && !result && errorMessage.Length > 0)
		{
			var action = lastErrorType == NACCSConstants.ErrorTypes.Unauthorized ? NACCSConstants.Actions.Authenticate : NACCSConstants.Actions.Transmit;
			var errorTitle = $"POP3 {action} Failed. Mailbox: {mailbox}";
			var errorDescription = errorMessage.ToString();

			var setting = GetOrLoadSetting();
			if (setting != null)
			{
				SendPOP3Error(mailbox, errorDescription, errorTitle, string.Empty, lastErrorType);
			}
			else
			{
				ShowErrorWithoutSetting(errorTitle, lastErrorType, errorDescription);
			}
		}

		return messageClient.IsConnected && messageClient.IsAuthenticated;
	}

	void SendPOP3Error(string recipientID, string errorDescription, string message, string machineName = "", string errorType = NACCSConstants.ErrorTypes.TransmissionError, KeyValuePair<string, string>[] parameters = null)
	{
		var errorInfo = new ErrorInfo
		{
			SenderID = string.IsNullOrEmpty(machineName) ? NACCSConstants.CustomsParty : $"{NACCSConstants.WebPrintParty}_{machineName}",
			RecipientID = recipientID,
			ErrorType = errorType,
			ErrorDescription = errorDescription,
			Parameters = parameters ?? new KeyValuePair<string, string>[]
			{
				new (NACCSConstants.Attributes.CompanyCode, context.MailBoxInfo.CompanyCode),
				new (NACCSConstants.Attributes.ClientMailbox, context.MailBoxInfo.MailBox)
			}
		};

		LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] {message}");
		errorSender.Send(errorInfo);
	}

	void ShowErrorWithoutSetting(string errorTitle, string errorType, string errorDescription)
	{
		LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] Can't load a valid setting and send the below message.");
		LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] Message: {errorTitle}");
		LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] Error Type: {errorType}");
		LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] Error Description: {errorDescription}");
	}

	IJPNACCSClientApplicationSetting GetOrLoadSetting(bool clearCachedSetting = false)
	{
		if (clearCachedSetting)
		{
			errorSender.SettingManager.ClearCachedSetting();
			LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] Clear Cached Setting.");
		}

		LogInformation($"[{nameof(NACCSMessageReceiverHandler)}] Load Setting.");
		return errorSender.SettingManager.GetOrLoadSetting();
	}

	void LogInformation(string information) => controller.Log(Integration.LogType.Information, information);

	void LogError(string error) => controller.Log(Integration.LogType.Error, error);
}

sealed record MessageReceiverContext
{
	public string Domain { get; set; }
	public MailBoxInfo MailBoxInfo { get; set; }
}
