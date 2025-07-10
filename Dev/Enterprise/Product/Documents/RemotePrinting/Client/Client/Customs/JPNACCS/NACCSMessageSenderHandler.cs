using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.xTMessaging.Shared;
using MailKit;
using MailKit.Net.Smtp;
using MimeKit;
using Xware.Xt.Grpc.Application;
using ResultOperation = Enterprise.xTMessaging.Shared.Constants.MessageHandlingResultOperation;

namespace Enterprise.RemotePrinting.Client;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer")]
sealed class NACCSMessageSenderHandler : IReceiveHandler
{
	public NACCSMessageSenderHandler(JPNACCSMessageSenderController controller)
	{
		this.controller = Argument.NotNull(controller, nameof(controller));

		errorSender = controller.GetErrorSender();
		messageProcessingResults = [];
	}

	readonly JPNACCSMessageSenderController controller;
	readonly INACCSErrorSender errorSender;
	readonly Dictionary<MsgIdUri, MessageHandlingResult> messageProcessingResults;

	IDictionary<MsgIdUri, MessageHandlingResult> IReceiveHandler.MessageProcessingResults => messageProcessingResults;

	void IReceiveHandler.HandleReceivedMessageBatch(ICollection<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaDataForHandling, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
	{
		messageProcessingResults.Clear();

		var lastResultOperation = ResultOperation.Success;
		var hasErrorMessage = false;

		foreach (var msgId in msgIds)
		{
			// “IReceiveHandler” runs inside the Scope of the DxT Connector.
			// If there are a very large number of messages that fail to be processed and are retried multiple times, then it will cause the connection to the DxT Connector to time out.
			// So we need to save these successfully processed messages and process these remaining messages in the next run.

			var currentResultOperation = lastResultOperation == ResultOperation.Success
				? HandleReceivedMessageBatchCore(msgId, getMessageMetaDataForHandling, getMsgData, loadReplyIntoMemory)
				: ResultOperation.LeaveItToNextRun;

			if (currentResultOperation == ResultOperation.LeaveItToNextRun)
			{
				if (!hasErrorMessage)
				{
					currentResultOperation = ResultOperation.Error;
					hasErrorMessage = true;

					LogInformation($@"[{nameof(NACCSMessageSenderHandler)}] The message - {msgId.Msgid} is marked failed.");
				}
				else
				{
					LogInformation($"[{nameof(NACCSMessageSenderHandler)}] The message - {msgId.Msgid} is left for the next run.");
				}
			}

			lastResultOperation = currentResultOperation;
			messageProcessingResults[msgId] = new MessageHandlingResult(currentResultOperation, 0);
		}

		if (lastResultOperation != ResultOperation.Success)
		{
			errorSender.SettingManager.ClearCachedSetting();
		}
	}

	ResultOperation HandleReceivedMessageBatchCore(MsgIdUri msgId, GetMessageMetaDataForHandling getMessageMetaDataForHandling, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
	{
		using (var message = TryLoadMimeMessage(msgId, getMsgData, loadReplyIntoMemory))
		{
			if (TrySendMessage(message, msgId, getMessageMetaDataForHandling))
			{
				var bodyData = Encoding.GetEncoding(NACCSConstants.Encodings.DefaultEncoding).GetBytes(message.TextBody);
				var (businessCode, userID, messageReference) = NACCSUtils.ExtractOutboundHeaderInformation(bodyData);

				var fromAddress = message.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty;
				LogInformation($"[Success] {userID} sent message {messageReference} for procedure {businessCode} from {fromAddress}");

				return ResultOperation.Success;
			}
			else
			{
				return ResultOperation.LeaveItToNextRun;
			}
		}
	}

	#region Implementation

	ISendMessageClient TryBuildMessageClient(MimeMessage message, MsgIdUri msgId, GetMessageMetaDataForHandling getMessageMetaDataForHandling)
	{
		var setting = GetOrLoadSetting();
		if (setting == null)
		{
			LogInformation(NACCSConstants.ErrorMessages.ErrorSettingLog);
			return null;
		}

		var errorMessage = new StringBuilder();
		var retries = 0;

		var messageClient = controller.GetMessageClient();
		var maxRetries = Math.Max(1, messageClient.ConnectMaxRetries);
		var domainName = string.Empty;

		while (retries <= maxRetries)
		{
			try
			{
				messageClient ??= controller.GetMessageClient();

				if (messageClient is NACCSSMTPClient stmpClient)
				{
					stmpClient.LocalDomain = setting.DomainName;
				}

				var naccsMailbox = MailboxAddress.Parse(setting.NACCSMailbox);
				domainName = naccsMailbox.Domain;
				messageClient.Connect(domainName);

				errorMessage.Clear();
				break;
			}
			catch (SmtpCommandException ex)
			{
				SendSMTPError(setting, $"[1] {ex}", msgId, $"SMTP Execute Command Failed. Domain: {domainName} MsgId: {msgId.Msgid}", getMessageMetaDataForHandling);
				ErrorReporter.ReportError($"[{nameof(NACCSMessageSenderHandler)}] Can't connect Domain: {domainName}.", ex);

				ReleaseMessageClient(messageClient);
				messageClient = null;

				errorMessage.Clear();
				break;
			}
			catch (Exception ex)
			{
				retries++;

				var delaySeconds = TimeSpan.FromSeconds(Math.Max(0.1, messageClient.Interval));

				LogInformation($"[{nameof(NACCSMessageSenderHandler)}] An exception [{ex.GetType().Name}] happen, pause {delaySeconds.TotalSeconds} second(s).");
				LogInformation(ex.ToString());

				Thread.Sleep(delaySeconds);

				errorMessage.AppendLine($"[{retries}] {ex}");
				errorMessage.AppendLine();

				if (retries >= maxRetries)
				{
					setting = GetOrLoadSetting(true);
				}

				ReleaseMessageClient(messageClient);
				messageClient = null;
			}
		}

		if (errorMessage.Length > 0)
		{
			var errorTitle = $"SMTP Connect Failed. Mailbox Domain: {domainName} Local Domain: {setting.DomainName}";
			var errorDescription = errorMessage.ToString();

			if (setting != null)
			{
				var mailBox = message.From.Mailboxes.FirstOrDefault()?.Address ?? string.Empty;

				var companyCode = string.Empty;
				var sourceMessageMetaData = getMessageMetaDataForHandling.Invoke(msgId);
				string senderId = sourceMessageMetaData.TryGetValue(xTMessaging.Shared.Constants.CustomMsgAttributes.SourceParty, out senderId)
					? senderId : string.Empty;
				if (senderId.Length == 9)
				{
					companyCode = senderId.Substring(3, 3);
				}
				var prefixTips = $@"SMTP connection fails. Please check the followings: 
	1.You are connected to the NACCS internet via a NACCS router. 
	2.The domain {setting.DomainName} is valid. To update this value, visit CargoWise > Maintain > System > Registry > Customs > Country or Region Specific > Japan > NACCS Messaging > Remote WebPrint Client Configurations. 
	3.The mailbox {mailBox} is valid. To update this value, visit CargoWise > Maintain > User Admin > Companies > {companyCode} > Brokerage > NACCS Mailbox. 
If the issue persists. Please raise an eRequest (Product: CargoWise; Module: Customs; Country: Japan) and provide the following exception message to the support. You can raise an eRequest by pressing F1 in CargoWise.";
				SendSMTPError(setting, prefixTips + "\n\n" + errorDescription, msgId, errorTitle, getMessageMetaDataForHandling, machineNameIsRequired: true);
			}
			else
			{
				ShowErrorWithoutSetting(errorTitle, errorDescription);
			}
		}

		return messageClient;
	}

	void ReleaseMessageClient(ISendMessageClient messageClient)
	{
		messageClient?.Disconnect(true);
		messageClient?.Dispose();
	}

	MimeMessage TryLoadMimeMessage(MsgIdUri msgId, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
	{
		try
		{
			using (var msgData = getMsgData(new GetMsgDataMessage { Id = msgId }))
			using (var resultStream = loadReplyIntoMemory(msgData))
			{
				LogInformation($"xT Message:{msgId} received.");

				resultStream.Seek(0, SeekOrigin.Begin);
				return MimeMessage.Load(resultStream);
			}
		}
		catch (Exception ex)
		{
			LogInformation($"xT Message:{msgId} receive failed. Error: {ex}");
			return null;
		}
	}

	bool TrySendMessage(MimeMessage message, MsgIdUri msgId, GetMessageMetaDataForHandling getMessageMetaDataForHandling)
	{
		if (message == null)
		{
			return false;
		}

		var setting = GetOrLoadSetting();
		if (setting == null)
		{
			LogInformation(NACCSConstants.ErrorMessages.ErrorSettingLog);
			return false;
		}

		var result = false;
		var errorMessage = new StringBuilder();
		Exception lastException = null;

		var messageClient = TryBuildMessageClient(message, msgId, getMessageMetaDataForHandling);
		if (messageClient == null)
		{
			return false;
		}

		var retries = 0;
		var maxRetries = Math.Max(1, messageClient.CommandMaxRetries);

		try
		{
			while (retries <= maxRetries)
			{
				try
				{
					messageClient ??= TryBuildMessageClient(message, msgId, getMessageMetaDataForHandling);
					messageClient?.Send(message);

					result = messageClient != null;

					break;
				}
				catch (SmtpCommandException ex)
				{
					SendSMTPError(setting, $"[1] {ex}", msgId, $"SMTP Execute Command Failed. Mailbox: {setting.NACCSMailbox} MsgId: {msgId.Msgid}", getMessageMetaDataForHandling);
					ErrorReporter.ReportError($"[{nameof(NACCSMessageSenderHandler)}] Can't send message {msgId.Msgid}.", ex);

					break;
				}
				catch (Exception ex)
				{
					retries++;

					var delaySeconds = TimeSpan.FromSeconds(Math.Max(0.1, messageClient.Interval));

					LogInformation($"[{nameof(NACCSMessageSenderHandler)}] An exception [{ex.GetType().Name}] happen, pause {delaySeconds.TotalSeconds} second(s).");
					LogInformation(ex.ToString());

					Thread.Sleep(delaySeconds);

					errorMessage.AppendLine($"[{retries}] {ex}");
					errorMessage.AppendLine();

					lastException = ex;

					ReleaseMessageClient(messageClient);
					messageClient = null;
				}
			}
		}
		finally
		{
			ReleaseMessageClient(messageClient);
		}

		if (!result && errorMessage.Length > 0)
		{
			var errorTitle = $"SMTP Send Message Failed. Mailbox: {setting.NACCSMailbox} MsgId: {msgId.Msgid}";
			var errorDescription = errorMessage.ToString();

			if (setting != null)
			{
				SendSMTPError(setting, errorDescription, msgId, errorTitle, getMessageMetaDataForHandling, lastException is not ServiceNotConnectedException);
			}
			else
			{
				ShowErrorWithoutSetting(errorTitle, errorDescription);
			}
		}

		return result;
	}

	void SendSMTPError(IJPNACCSClientApplicationSetting setting, string errorDescription, MsgIdUri msgId, string message, GetMessageMetaDataForHandling getMessageMetaDataForHandling, bool machineNameIsRequired = false)
	{
		var errorInfo = new ErrorInfo
		{
			SenderID = machineNameIsRequired ? $"{NACCSConstants.WebPrintParty}_{setting.MachineName}" : setting.NACCSMailbox,
			RecipientID = setting.NACCSMailbox,
			ErrorType = NACCSConstants.ErrorTypes.TransmissionError,
			ErrorDescription = errorDescription,
			Parameters = new KeyValuePair<string, string>[]
			{
				new (NACCSConstants.Attributes.Domain, setting.DomainName),
				new (NACCSConstants.Attributes.MachineID, setting.MachineName),
				new (NACCSConstants.Attributes.ServerMailbox, setting.NACCSMailbox),
				new (NACCSConstants.Attributes.MessageId, msgId.Msgid.ToString())
			}
		};

		LogInformation($"[{nameof(NACCSMessageSenderHandler)}] {message}");
		errorSender.Send(errorInfo, msgId, getMessageMetaDataForHandling);
	}

	void ShowErrorWithoutSetting(string titile, string description)
	{
		LogInformation($"[{nameof(NACCSMessageSenderHandler)}] Can't load a valid setting and send the below message.");
		LogInformation($"[{nameof(NACCSMessageSenderHandler)}] Message: {titile}");
		LogInformation($"[{nameof(NACCSMessageSenderHandler)}] Error Type: {NACCSConstants.ErrorTypes.TransmissionError}");
		LogInformation($"[{nameof(NACCSMessageSenderHandler)}] Error Description: {description}");
	}

	IJPNACCSClientApplicationSetting GetOrLoadSetting(bool clearCachedSetting = false)
	{
		if (clearCachedSetting)
		{
			errorSender.SettingManager.ClearCachedSetting();
			LogInformation($"[{nameof(NACCSMessageSenderHandler)}] Clear Cached Setting.");
		}

		LogInformation($"[{nameof(NACCSMessageSenderHandler)}] Load Setting.");
		return errorSender.SettingManager.GetOrLoadSetting();
	}

	void LogInformation(string message) => controller.Log(LogType.Information, message);

	#endregion
}
