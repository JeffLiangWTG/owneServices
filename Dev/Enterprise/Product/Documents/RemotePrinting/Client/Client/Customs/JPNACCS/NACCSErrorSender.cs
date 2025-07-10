using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using Enterprise.Integration;
using Enterprise.xTMessaging.Shared;
using Xware.Xt.Grpc.Application;

namespace Enterprise.RemotePrinting.Client;

public class NACCSErrorSender : INACCSErrorSender
{
	public NACCSErrorSender(ProtocolType protocolType, JPNACCSClientApplicationSettingManager settingManager, IMsgClientProvider msgClientProvider, ILogger logger)
	{
		SettingManager = settingManager;

		this.logger = logger;
		this.protocolType = protocolType;
		this.msgClientProvider = msgClientProvider;
	}

	readonly ILogger logger;
	readonly ProtocolType protocolType;
	readonly IMsgClientProvider msgClientProvider;

	public JPNACCSClientApplicationSettingManager SettingManager { get; }

	public void Send(ErrorInfo errorInfo)
	{
		var msgTrackingId = string.Empty;
		Send(msgTrackingId, errorInfo);
	}

	public void Send(ErrorInfo errorInfo, MsgIdUri msgId, GetMessageMetaDataForHandling getMessageMetaDataForHandling)
	{
		if (getMessageMetaDataForHandling == null)
		{
			logger.Information($"Can't pick source attributes without a GetMessageMetaDataForHandling.");
			return;
		}

		var sourceMessageMetaData = getMessageMetaDataForHandling.Invoke(msgId);
		string msgTrackingId = sourceMessageMetaData.TryGetValue(xTMessaging.Shared.Constants.CustomMsgAttributes.MessageTrackingID, out msgTrackingId)
			? msgTrackingId : string.Empty;

		Send(msgTrackingId, errorInfo);
	}

	void Send(string msgTrackingId, ErrorInfo errorInfo)
	{
		var setting = SettingManager.GetOrLoadSetting();

		if (setting == null)
		{
			logger.Information($"Can't send error message without a valid client application setting.");
			return;
		}

		var trackingId = string.IsNullOrWhiteSpace(msgTrackingId) || !Guid.TryParse(msgTrackingId, out _) ? NACCSConstants.EmptyTrackingId : msgTrackingId;
		var messageData = GetMessageData(errorInfo);

		using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(messageData)))
		using (var bodyReader = new BinaryReader(stream))
		using (var connector = new DirectxTConnector(msgClientProvider ?? NACCSUtils.GetMsgClientProvider(setting), setting.DirectxTMessagingConfig, logger, NACCSUtils.GetSubmitMsgAttributeModifier(setting)))
		{
			connector.InitializeWithFullLogging();

			stream.Seek(0, SeekOrigin.Begin);

			var messageInfo = GetMessageInfo(bodyReader);
			(var isSuccess, var msgId, var error) = connector.SendInterchange(messageInfo).GetAwaiter().GetResult();

			if (isSuccess)
			{
				logger.Information($"Send error message success. MsgTrackingId: {msgTrackingId}");
			}
			else
			{
				logger.Error($"Send error message failed. MsgTrackingId: {msgTrackingId} Error: {error}");
			}
		}

		BasicXtMessageInfo GetMessageInfo(BinaryReader bodyReader)
		{
			var result = new BasicXtMessageInfo
			(
				NACCSConstants.ApplicationCode,
				NACCSConstants.ErrorMessageType,
				NACCSConstants.WebPrintParty,
				NACCSUtils.GetReceiver(setting),
				trackingId,
				bodyReader
			);

			result.XTMessageAttributes[NACCSConstants.Attributes.ProtocolType] = protocolType.ToString();

			if (errorInfo.Parameters != null)
			{
				foreach (var kv in errorInfo.Parameters)
				{
					result.XTMessageAttributes[kv.Key] = kv.Value;
				}
			}

			return result;
		}
	}

	string GetMessageData(ErrorInfo errorInfo)
	{
		var senderId = errorInfo.SenderID;
		var recipientId = errorInfo.RecipientID;
		var errorType = errorInfo.ErrorType;
		var errorDescription = errorInfo.ErrorDescription;

		var notificationTime = GetNotificationTime();
		var variables = new { senderId, recipientId, notificationTime, errorType, errorDescription };

		var properties = variables.GetType().GetProperties();
		var result = MessageTemplate;

		foreach (var property in properties)
		{
			var sourceContent = property.GetValue(variables)?.ToString() ?? string.Empty;
			result = result.Replace($"{{{property.Name}}}", SecurityElement.Escape(sourceContent));
		}

		return result;
	}

	protected virtual string GetNotificationTime() => DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

	string MessageTemplate => messageTemplate ??= NACCSUtils.ReadEmbeddedResource("DeliveryResponseTemplate.xml");
	string messageTemplate;
}

public enum ProtocolType
{
	SMTP,
	POP3,
}

public sealed record ErrorInfo
{
	public string SenderID { get; set; }

	public string RecipientID { get; set; }

	public string ErrorType { get; set; }

	public string ErrorDescription { get; set; }

	public KeyValuePair<string, string>[] Parameters { get; set; }
}
