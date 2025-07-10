using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class LockingMessageDequeuer
	{
		readonly ILockMechanism lockMechanism;
		readonly LoggingInformation logger;
		readonly IMessageKeySetExtractor keySetExtractor;

		public int MinQuickScanMessageCount { get; internal set; } = 1000;
		public int KeyGenerationBatchSize { get; internal set; } = 100;

		public LockingMessageDequeuer(ILockMechanism lockMechanism, LoggingInformation logger, IMessageKeySetExtractor keySetExtractor)
		{
			this.lockMechanism = lockMechanism;
			this.logger = logger;
			this.keySetExtractor = keySetExtractor;
		}

		public BaseMessageProcessor<EDIMessage>.DisposableBatch DequeueMessages(BusinessObjectFactory factory, ZQuery query)
		{
			LockedMessageBatch batch = DequeueMessages(query);
			try
			{
				var messages = batch.LoadMessages(factory);
				var disposableBatch = new BaseMessageProcessor<EDIMessage>.DisposableBatch(messages, new IgnoreNotifyDisposable(batch));
				// note: From this point onward, the owner of the external batch is responsible for releasing the locks.
				// note: And no exception can happen between assignment and return statement.
				batch = null;
				return disposableBatch;
			}
			finally
			{
				batch?.Dispose();
			}
		}

		LockedMessageBatch DequeueMessages(ZQuery query)
		{
			var messageKeySetCache = new Dictionary<ZGuid, MessageKeySet>();

			return DequeueMessages(query, messageKeySetCache, false)
				?? DequeueMessages(query, messageKeySetCache, true)
				?? new LockedMessageBatch(Array.Empty<ZGuid>(), null);
		}

		LockedMessageBatch DequeueMessages(ZQuery query, Dictionary<ZGuid, MessageKeySet> messageKeySetCache, bool deepScan)
		{
			logger.Log(LogType.Debug, deepScan ? "Starting deep scan." : "Starting quick scan.");

			var pkQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			pkQuery.AddToFilter(query);
			pkQuery.OrderBy = query.OrderBy;

			if (deepScan)
			{
				pkQuery.MaximumRows = null;
			}
			else
			{
				pkQuery.MaximumRows = Math.Max(MinQuickScanMessageCount, (query.MaximumRows ?? 1) * 2);
			}

			var lockedMessageQueue = new LockingMessageQueue(lockMechanism);
			try
			{
				var sw = Stopwatch.StartNew();

				var sqlLoader = new DirectSqlLoader();
				var messagePKs = sqlLoader.LoadPKs(pkQuery);

				var processableMessagePKs = new List<ZGuid>();
				int readMessages = 0;

				bool stopEarly = false;
				for (int mIdx = 0; mIdx < messagePKs.Count && !stopEarly; mIdx += KeyGenerationBatchSize)
				{
					var currentBatch = LoadBatch(messagePKs, mIdx, KeyGenerationBatchSize, messageKeySetCache);
					if (!deepScan)
					{
						foreach (var messageInfo in currentBatch)
						{
							messageKeySetCache[messageInfo.MessagePK] = messageInfo.MessageKeySet;
						}
					}

					foreach (var messageInfo in currentBatch)
					{
						readMessages++;

						if (lockedMessageQueue.Enqueue(messageInfo.MessageKeySet))
						{
							processableMessagePKs.Add(messageInfo.MessagePK);
							if (processableMessagePKs.Count >= query.MaximumRows)
							{
								stopEarly = true;
								break;
							}
						}
					}
				}

				logger.Log(LogType.Debug, FormattableString.Invariant(
					$"Selected {processableMessagePKs.Count} messages out of first {readMessages} messages in {sw.ElapsedMilliseconds}ms."
				));

				if (processableMessagePKs.Count == 0)
				{
					return null;
				}

				var batch = new LockedMessageBatch(processableMessagePKs, lockedMessageQueue);
				// note: From this point onward, the owner of the batch is responsible for releasing the locks.
				// note: And no exception can happen between assignment and return statement.
				lockedMessageQueue = null;
				return batch;
			}
			finally
			{
				lockedMessageQueue?.Dispose();
			}
		}

		List<(ZGuid MessagePK, MessageKeySet MessageKeySet)> LoadBatch(List<ZGuid> messagePKs, int firstMessageIndex, int batchSize, Dictionary<ZGuid, MessageKeySet> knownKeySets)
		{
			var batchMessagePKs = messagePKs.GetRange(firstMessageIndex, Math.Min(batchSize, messagePKs.Count - firstMessageIndex));

			var messageKeySetsByMessagePK = new Dictionary<ZGuid, MessageKeySet>();
			var messagePKsWithoutKeySet = new List<ZGuid>();

			foreach (var messagePK in batchMessagePKs)
			{
				if (knownKeySets.TryGetValue(messagePK, out var messageKeySet))
				{
					messageKeySetsByMessagePK.Add(messagePK, messageKeySet);
				}
				else
				{
					messagePKsWithoutKeySet.Add(messagePK);
				}
			}

			if (messagePKsWithoutKeySet.Count != 0)
			{
				var factory = new BusinessObjectFactory { RefreshEnabled = false };
				var batchQuery = new ZQuery(EDIMessageSchema.PK, messagePKsWithoutKeySet);
				batchQuery.IncludeBlob(EDIMessageSchema.EM_MessageText);

				var batchMessages = factory.Load<EDIMessage>(batchQuery);
				foreach (var message in batchMessages)
				{
					var messageKeys = ExtractKeysSafe(message);
					messageKeySetsByMessagePK.Add(message.PK, messageKeys);
				}
			}

			var result = new List<(ZGuid MessagePK, MessageKeySet MessageKeySet)>(batchMessagePKs.Count);

			foreach (var messagePK in batchMessagePKs)
			{
				if (!messageKeySetsByMessagePK.TryGetValue(messagePK, out var messageKeySet))
				{
					// message no longer exists
					logger.Log(LogType.Warning, FormattableString.Invariant($"Message with PK='{messagePK}' no longer exists."));
				}
				else
				{
					result.Add((messagePK, messageKeySet));
				}
			}

			return result;
		}

		MessageKeySet ExtractKeysSafe(EDIMessage message)
		{
			try
			{
				return keySetExtractor.ExtractKeys(message);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				LogAndReportExceptionOnce(FormattableString.Invariant($"Failed to extract keys for message (PK={message?.PK})."), ex);
				return MessageKeySet.Unknown;
			}
		}

		static readonly Overridable<bool> exceptionReported = new Overridable<bool>(false);

		void LogAndReportExceptionOnce(string message, Exception exception)
		{
			if (exceptionReported.Value)
			{
				return;
			}

			exceptionReported.Value = true;
			logger.Log(LogType.Error, FormatException(message, exception));
			ExceptionReporter.Instance.ReportDeveloperException(message, exception);
		}

		static string FormatException(string message, Exception exception)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(message);
			sb.Append(exception.GetType().FullName);
			sb.Append(": ");
			sb.AppendLine(exception.Message);
			sb.AppendLine(exception.StackTrace);
			return sb.ToString();
		}
	}
}
