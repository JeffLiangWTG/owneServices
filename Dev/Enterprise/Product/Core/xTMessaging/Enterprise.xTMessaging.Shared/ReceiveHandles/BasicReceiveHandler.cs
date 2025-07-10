using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Enterprise.Integration;
using Grpc.Core;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared
{
	public abstract class BasicReceiveHandler : IReceiveHandler
	{
		protected BasicReceiveHandler(ILogger logger)
		{
			this.logger = logger;
			MessageProcessingResults = new Dictionary<MsgIdUri, MessageHandlingResult>();
		}

		protected readonly ILogger logger;
		readonly Dictionary<MsgIdUri, (MetaDataHelper MetadataHelper, Stream Payload)> dataDictionary = new();
		readonly Dictionary<MsgIdUri, string> errorMessages = new();

		public IDictionary<MsgIdUri, MessageHandlingResult> MessageProcessingResults { get; }

		public void HandleReceivedMessageBatch(ICollection<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaDataForHandling, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
		{
			MessageProcessingResults.Clear();
			if (msgIds.Any())
			{
				LoadAttributesAndMessagesFromServer(msgIds, getMessageMetaDataForHandling, getMsgData, loadReplyIntoMemory);
				if (dataDictionary.Any())
				{
					HandleMessages();
					if (errorMessages.Any())
					{
						logger?.Log(LogType.Warning, $"{dataDictionary.Count} message(s) handled: {dataDictionary.Count - errorMessages.Count} received, {errorMessages.Count} failed.");
					}
					else
					{
						logger?.Log(LogType.Information, $"{dataDictionary.Count} message(s) received.");
					}

					ClearingMemory();
				}
			}
		}

		protected abstract (bool Success, string ErrorMessage) HandleMessage(MetaDataHelper metaDataHelper, Stream payload, long xTInternalMsgID);

		void LoadAttributesAndMessagesFromServer(IEnumerable<MsgIdUri> msgIds, GetMessageMetaDataForHandling getMessageMetaDataForHandling, GetMsgData getMsgData, LoadReplyIntoMemory loadReplyIntoMemory)
		{
			foreach (var msgId in msgIds)
			{
				Dictionary<string, string> metadata = null;
				try
				{
					metadata = getMessageMetaDataForHandling(msgId);
				}
				catch (Exception ex) when (ex is not MsgServerConnectionException)
				{
					OnFail(msgId, $"Error on reading metadata: {ex.Message}");
				}

				if (metadata != null)
				{
					Stream payload = null;
					try
					{
						payload = loadReplyIntoMemory(getMsgData(new GetMsgDataMessage { Id = msgId }));
					}
					catch (Exception ex) when (ex.InnerException is not RpcException)
					{
						OnFail(msgId, $"Error on reading message body: {ex.Message}", metadata);
					}

					if (payload != null)
					{
						try
						{
							dataDictionary.Add(msgId, (new MetaDataHelper(metadata), payload));
						}
						catch (Exception ex)
						{
							OnFail(msgId, $"Error on creating metadata helper: {ex.Message}", metadata);
						}
					}
				}
			}
		}

		void OnFail(MsgIdUri msgId, string message, Dictionary<string, string> metadata = null, Constants.MessageHandlingResultOperation processingResult = Constants.MessageHandlingResultOperation.Error)
		{
			MessageProcessingResults[msgId] = new MessageHandlingResult(processingResult, 0);
			var messageKeyInfo = metadata == null ? $"MsgId:{msgId.Msgid}" : GetMessageKeyInfoForLog(msgId, metadata);
			var fullMessage = $"{message} - {messageKeyInfo}";
			if (errorMessages.ContainsKey(msgId))
			{
				errorMessages[msgId] = fullMessage;
			}
			else
			{
				errorMessages.Add(msgId, fullMessage);
			}
			logger?.Log(LogType.Error, fullMessage);
		}

		void HandleMessages()
		{
			foreach (var entry in dataDictionary)
			{
				try
				{
					var handleResult = HandleMessage(entry.Value.MetadataHelper, entry.Value.Payload, (long)entry.Key.Msgid);
					if (handleResult.Success)
					{
						MessageProcessingResults[entry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Success, 0);
						logger?.Log(LogType.Information, $"Message {entry.Key.Msgid} received successfully.");
					}
					else
					{
						MessageProcessingResults[entry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.LeaveItToNextRun, 0);
						OnFail(entry.Key, $"Message handling error and will retry in next run: {handleResult.ErrorMessage}", entry.Value.MetadataHelper.MetaData);
					}
				}
				catch (Exception ex)
				{
					MessageProcessingResults[entry.Key] = new MessageHandlingResult(Constants.MessageHandlingResultOperation.Error, 0);
					OnFail(entry.Key, $"Message handling error and will not retry more: {ex.Message}", entry.Value.MetadataHelper.MetaData);
				}
			}
		}

		void ClearingMemory()
		{
			foreach (var data in dataDictionary)
			{
				data.Value.Payload?.Dispose();
			}
			dataDictionary.Clear();
			errorMessages.Clear();
		}

		static string GetMessageKeyInfoForLog(MsgIdUri msgId, IDictionary<string, string> metaData)
		{
			return $"MsgId: {msgId.Msgid}\r\n" +
					$"AppCode: {metaData.GetValueSafe(Constants.CustomMsgAttributes.ApplicationCode)}\r\n" +
					$"Sender: {metaData.GetValueSafe(Constants.CustomMsgAttributes.SourceParty)}\r\n" +
					$"Recipient: {metaData.GetValueSafe(Constants.CustomMsgAttributes.DestinationParty)}\r\n" +
					$"SessionID: {metaData.GetValueSafe(Constants.CustomMsgAttributes.MessageTrackingID)}";
		}
	}
}
