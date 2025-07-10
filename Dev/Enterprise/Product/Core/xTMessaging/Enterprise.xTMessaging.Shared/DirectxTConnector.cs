using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.eHub.Common;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Core.Utils;
using Xware.Xt.Grpc.Application;

namespace Enterprise.xTMessaging.Shared
{
	public class DirectxTConnector : IDisposable
	{
		public DirectxTConnector(
			IMsgClientProvider msgClientProvider,
			IDirectxTMessagingConfig directxTMessagingConfig,
			ILogger logger,
			ISubmitMsgAttributeModifier msgAttributeModifier = null,
			IReceiveHandler receiveHandler = null
		)
		{
			this.logger = logger;
			this.receiveHandler = receiveHandler;
			this.msgAttributeModifier = msgAttributeModifier;
			this.msgClientProvider = msgClientProvider;
			xtToObjFilter = msgClientProvider.XtToObjFilter;
			messageTimeout = msgClientProvider.XtServerMessageTimeout;
			this.directxTMessagingConfig = directxTMessagingConfig;
		}

		readonly ILogger logger;
		readonly IReceiveHandler receiveHandler;
		readonly ISubmitMsgAttributeModifier msgAttributeModifier;
		readonly IMsgClientProvider msgClientProvider;
		IMsgClient msgClient;
		readonly IDirectxTMessagingConfig directxTMessagingConfig;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove private member as the value assigned to it is never read", Justification = "Value is Called through msgClientProvider")]
		readonly string xtToObjFilter;
		readonly TimeSpan messageTimeout;

		public bool InitializeWithFullLogging()
		{
			var sw = Stopwatch.StartNew();
			logger.Log(LogType.Debug, "Start Initialize DirectxT Connector.");

			var result = InitializeIfNeeded();
			if (!result.Item1)
			{
				logger.Log(LogType.Error, result.Item2);
			}

			sw.Stop();
			logger.Log(LogType.Debug, FormattableString.Invariant($"Finish Initialize DirectxT Connector. Total Time: {sw.Elapsed.TotalSeconds} second(s)"));
			return result.Item1;
		}

		public (bool, string) InitializeIfNeeded()
		{
			var errorMsg = string.Empty;
			if (msgClient == null)
			{
				msgClient = msgClientProvider?.MsgClient;
			}

			var result = msgClient != null;

			errorMsg = msgClientProvider?.ErrorMessage ?? errorMsg;

			if (result)
			{
				logger.Log(LogType.Information, "Initialization completed - connect to GRPC client");
			}

			return (result, errorMsg);
		}

		public virtual void StartTransaction(DateTime? deadline, CancellationToken cancellationToken)
		{
			try
			{
				logger.Debug($"IsCancellationRequested before StartTransaction: {cancellationToken.IsCancellationRequested}");
				_ = msgClient.StartTransactionAsync(new StartTransactionMessage(), deadline, cancellationToken).GetAwaiter().GetResult();
			}
			catch (Exception ex) when (ex is not MsgSessionTimeoutException)
			{
				throw new XtTransactionException($"StartTransaction Failed(IsCancellationRequested={cancellationToken.IsCancellationRequested})", ex);
			}
		}

		public virtual void EndTransaction(bool commit, DateTime? deadline, CancellationToken cancellationToken)
		{
			try
			{
				logger.Debug($"IsCancellationRequested before EndTransaction: {cancellationToken.IsCancellationRequested}");
				_ = msgClient.EndTransactionAsync(new EndTransactionMessage { Commit = commit }, deadline, cancellationToken).GetAwaiter().GetResult();
			}
			catch (Exception ex) when (ex is not MsgSessionTimeoutException)
			{
				throw new XtTransactionException($"EndTransaction Failed(IsCancellationRequested={cancellationToken.IsCancellationRequested})", ex);
			}
		}

		#region Sending

		public string GetAndValidateOutgoingMessageMetaData(IXtMessageInfo interchange, out Dictionary<string, string> metadataDict)
		{
			var errorMsg = new StringBuilder();
			metadataDict = [];
			try
			{
				if (interchange.MessageTrackingID == Guid.Empty.ToString())
				{
					Utils.AppendError(errorMsg, "MessageTrackingID (EI_SessionGUID) must be a valid GUID.");
				}
				metadataDict[Constants.CustomMsgAttributes.MessageTrackingID] = interchange.MessageTrackingID;

				metadataDict.AddToDictionaryIfValid(Constants.CustomMsgAttributes.ApplicationCode, interchange.ApplicationCode);
				metadataDict.AddToDictionaryIfValid(Constants.CustomMsgAttributes.SourceParty, interchange.SourceParty);
				metadataDict.AddToDictionaryIfValid(Constants.CustomMsgAttributes.DestinationParty, interchange.DestinationParty);
				metadataDict.AddToDictionaryIfValid(Constants.CustomMsgAttributes.MessageType, interchange.MessageType);

				metadataDict.AddRangeToDictionaryIfValid(interchange.XTMessageAttributes);
			}
			catch (Exception ex)
			{
				Utils.AppendError(errorMsg, $"Exception occurred during processing: {ex.Message}.");
			}

			return errorMsg.Length > 0 ? errorMsg.ToString() : string.Empty;
		}

		public async Task<(bool, long, string)> SendInterchange(IXtMessageInfo interchange, CancellationToken token = default)
		{
			var sentSuccessful = false;
			var errorNote = string.Empty;
			var msgId = 0L;

			await Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeededAsync(
				async () =>
				{
					if (msgClient != null)
					{
						errorNote = GetAndValidateOutgoingMessageMetaData(interchange, out var metadata);
						if (string.IsNullOrEmpty(errorNote))
						{
							using var dataStream = interchange.GetMessageData();
							(sentSuccessful, msgId, errorNote) = await SendCoreAsync(metadata, dataStream, token);
						}
					}
					else
					{
						errorNote = msgClientProvider?.ErrorMessage ?? errorNote;
					}
					return sentSuccessful;
				}, $"Send interchange(MessageTrackingID={interchange.MessageTrackingID})", messageTimeout);

			return (sentSuccessful, msgId, errorNote);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		async Task<(bool, long, string)> SendCoreAsync(Dictionary<string, string> metadata, BinaryReader dataStream, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var dataSubmitRef = await SendContentAsync(dataStream, token);
			var msgToSubmit = GetSubmitMessage(dataSubmitRef, metadata);
			msgAttributeModifier?.AddParserFields(msgToSubmit);

			logger.Debug($"IsCancellationRequested before SubmitMsgAsync: {token.IsCancellationRequested}");
			var submitReply = await msgClient.SubmitMsgAsync(msgToSubmit, cancellationToken: token);
			var sentSuccessful = submitReply.Errorcode == (int)ErrorCode.ErrOk;
			var msgId = sentSuccessful ? Utils.ConvertUnsignedLongToLong(submitReply.Ids.FirstOrDefault()?.Msgid ?? 0) : 0L;
			var errorNote = sentSuccessful ? " has been sent." : $"Rejected by xT Server. Error '{Enum.Parse(typeof(ErrorCode), submitReply.Errorcode.ToString())} - {xTError.GetErrorDescription(submitReply.Errorcode)} ({submitReply.Errorcode})' returned.";
			return (sentSuccessful, msgId, errorNote);
		}

		#region GetSubmitMessage

		static SubmitMsgMessage GetSubmitMessage(string dataSubmitRef, Dictionary<string, string> metaDataDict)
		{
			var msg = new SubmitMsgMessage()
			{
				Type = MsgType.Normal,
				Dataref = dataSubmitRef
			};
			msg.Msgattr.Add(metaDataDict);

			return msg;
		}

		async Task<string> SendContentAsync(BinaryReader messageStream, CancellationToken token)
		{
			var call = msgClient.WriteMsgDataStream(cancellationToken: token);
			var byteChunk = new ByteChunk();
			var array = new byte[directxTMessagingConfig.XTServerMessageChunkSizeWhenSendingValue * 1024];
			var task = Task.CompletedTask;
			while (messageStream.Read(array, 0, array.Length) is var read && read > 0)
			{
				byteChunk.Chunk = ByteString.CopyFrom(array.Take(read).ToArray(), 0, read);
				task.Wait();
				task = call.RequestStream.WriteAsync(byteChunk);
			}
			task.Wait();
			await call.RequestStream.CompleteAsync();
			var result = await call.ResponseAsync;
			return result.Ref;
		}

		#endregion

		#endregion

		#region Receiving

		Dictionary<MsgIdUri, Dictionary<string, string>> CachedMsgAttributes;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Message")]
		public void Receive(CancellationToken cancellationToken = default)
		{
			Utils.RunActionAndThrowMsgServerConnectionExceptionIfNeeded(
				() =>
				{
					RunActivityLoop(cancellationToken, () =>
					{
						var receivedMessageCount = 0;
						var incomingMessages = msgClient.WaitMsgAsync(new WaitMsgMessage() { Types_ = WaitMsgSpec.WaitAppAll }, cancellationToken: cancellationToken)
														.ConfigureAwait(false)
														.GetAwaiter()
														.GetResult();

						receivedMessageCount = incomingMessages?.Ids?.Count ?? 0;
						logger.Log(LogType.Information, FormattableString.Invariant($"{receivedMessageCount} message(s) is ready to be received"));

						if (receivedMessageCount > 0)
						{
							var interchangeCountPerBatch = directxTMessagingConfig.InterchangeCountPerBatchOnReceivingValue;
							var readyToBeReceivedIds = incomingMessages.Ids.Select(x => x.Id).OrderBy(x => x.Msgid).ToList();
							CachingMsgAttributes(readyToBeReceivedIds);
							var sw = new Stopwatch();
#if NET
							var ids = Enumerable.Chunk(readyToBeReceivedIds, interchangeCountPerBatch);
#else
							var ids = readyToBeReceivedIds.Chunk(interchangeCountPerBatch);
#endif
							foreach (var idInABatch in ids)
							{
								var idToBeProcessed = idInABatch.ToArray();
								sw.Restart();
								receiveHandler.HandleReceivedMessageBatch(idToBeProcessed, GetMessageMetaData, getMsg => msgClient.GetMsgData(getMsg), LoadReplyIntoMemory);
								ConfirmReceivingStatus(receiveHandler.MessageProcessingResults);
								sw.Stop();
								logger.Log(LogType.Information, FormattableString.Invariant($"Finish Receive {idToBeProcessed.Length} Interchange(s). Total Time: {sw.Elapsed.TotalSeconds} second(s)"));
							}
						}

						return receivedMessageCount > 0;
					});
					return true;
				}, "Receive", messageTimeout
			);
		}

		void CachingMsgAttributes(List<MsgIdUri> msgIds)
		{
			try
			{
				var getMsgAttributesListMessage = new GetMsgAttributesListMessage();
				getMsgAttributesListMessage.Ids.AddRange(msgIds);
				var msgAttr = msgClient.GetMsgAttributesList(getMsgAttributesListMessage).ResponseStream.ToListAsync().Result;
				CachedMsgAttributes = msgAttr.ToDictionary(m => m.Id, m => m.Msgattr?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>());
			}
			catch (Exception ex)
			{
				CachedMsgAttributes = new Dictionary<MsgIdUri, Dictionary<string, string>>();
				logger.Log(LogType.Warning, $"Error while caching message attributes: {ex.Message}");
			}
		}

		Dictionary<string, string> GetMessageMetaData(MsgIdUri msgId)
		{
			if (CachedMsgAttributes != null && CachedMsgAttributes.TryGetValue(msgId, out var metaData))
			{
				logger.Log(LogType.Information, $"Return metadata for message {msgId.Msgid} from pre-cached dictionary.");
				return metaData;
			}
			var msgAttr = msgClient.GetMsgAttributes(new GetMsgAttributesMessage() { Id = msgId })?.Msgattr;
			var msgAttrDict = msgAttr?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, string>();
			return msgAttrDict;
		}

		void ConfirmReceivingStatus(IDictionary<MsgIdUri, MessageHandlingResult> processingResults)
		{
			var msgIds = processingResults.Keys;
			foreach (var msgId in msgIds)
			{
				var ackMessage = new MsgSetStatusMessage { Id = msgId };
				var processingResult = processingResults[msgId];
				string logString;

				switch (processingResult.Operation)
				{
					case Constants.MessageHandlingResultOperation.Success:
						ackMessage.Cmd = MsgStatusCommand.StatusOk;
						logString = $"has been acknowledged with status {ackMessage.Cmd}";
						break;
					case Constants.MessageHandlingResultOperation.Error:
						ackMessage.Cmd = MsgStatusCommand.StatusFailed;
						logString = $"has been acknowledged with status {ackMessage.Cmd}";
						break;
					default:
						logString = $"has not been acknowledged and would retry in next run";
						ackMessage.Cmd = MsgStatusCommand.StatusNone;
						break;
				}

				if (processingResult.ReceivingRetryCount > 0)
				{
					ackMessage.Msgattr.Add(Constants.CustomMsgAttributes.ReceivingRetryCount, processingResult.ReceivingRetryCount.ToString());
				}

				var statusSetReply = msgClient.MsgSetStatus(ackMessage);
				logger.Log(LogType.Information, FormattableString.Invariant($"Msg:{ackMessage.Id.Msgid} {logString}/{statusSetReply.Errorcode}"));
			}
		}

		static Stream LoadReplyIntoMemory(AsyncServerStreamingCall<ResultByteChunk> msgData)
		{
			var messageStream = msgData.ResponseStream;
			var resultStream = new MemoryStream();
			while (messageStream.MoveNext().Result)
			{
				var currentChunk = messageStream.Current;
				var errorCode = currentChunk.Errorcode;
				if (errorCode != 0)
				{
					throw new Exception($"GetMsgData error: {errorCode}");
				}
				var chunkString = currentChunk.Chunk.ToByteArray();
				resultStream.Write(chunkString, 0, chunkString.Length);
			}
			resultStream.Seek(0, SeekOrigin.Begin);
			return resultStream;
		}

		#endregion

		public void RunActivityLoop(CancellationToken token, Func<bool> activityFunc)
		{
			var firstLoop = true;
			var idleStopWatch = new Stopwatch();
			while (!token.IsCancellationRequested)
			{
				if (activityFunc())
				{
					idleStopWatch.Reset();
				}
				else
				{
					if (!idleStopWatch.IsRunning)
					{
						idleStopWatch.Start();
					}
					if (firstLoop || idleStopWatch.Elapsed >= IdleConnectionKeepAliveInSeconds)
					{
						break;
					}
					logger.Log(LogType.Debug, FormattableString.Invariant($"The connection is idle but the keep alive has not yet expired. Pausing for {(int)IdleConnectionRetryPauseInSeconds.TotalSeconds} second(s) before we retry."));
					Task.Delay(IdleConnectionRetryPauseInSeconds).Wait(token);
				}

				firstLoop = false;
			}
		}

		TimeSpan IdleConnectionKeepAliveInSeconds => TimeSpan.FromSeconds(directxTMessagingConfig.XTIdleConnectionKeepAliveInSecondsValue);
		TimeSpan IdleConnectionRetryPauseInSeconds => TimeSpan.FromSeconds(directxTMessagingConfig.XTIdleConnectionRetryPauseInSecondsValue);

		public void Dispose()
		{
			msgClientProvider?.TearDown();
			if (msgClient != null)
			{
				logger.Log(LogType.Information, "Logged Off the GRPC client");
			}
		}
	}
}
