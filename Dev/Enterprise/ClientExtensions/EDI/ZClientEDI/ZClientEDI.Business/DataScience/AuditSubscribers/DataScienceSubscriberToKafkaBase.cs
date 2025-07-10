using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;
using Confluent.Kafka;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Client.EDI.DataScience.Business.Extensions;
using Enterprise.Integration;
using Newtonsoft.Json;
using WTG.Serialization.DataScience.Audit;
using WTG.Serialization.DataScience.Audit.ObjectModel;
using WTG.Serialization.DataScience.Extensions;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers
{
	public abstract class DataScienceSubscriberToKafkaBase : ActualDataChangesAuditSubscriber, IDataScienceSubscriberToKafka
	{
		public const string ProducerNamePrefix = "Data Science Audit Subscriber ";
		const int kafkaMessageSizeOverhead = 100_000;
		readonly StringBuilder reusableBuffer = new StringBuilder();

		protected TimeSpan kafkaFlushTimeout = TimeSpan.FromSeconds(120);

		public override string Description => $"Data Science {Table.TableName} subscriber";
		public override IEnumerable<SchemaColumn> SpecificColumns => null;

		public override Action<DataRow> CustomFilter => null;

		public abstract ISubscriberDataSchema DataSchema { get; }

		public IReadOnlyList<ColumnInfo> ColumnInfos => columnInfos ?? (columnInfos =
			DataSchema.BizObjColumns.Concat(DataSchema.LegacyColumns)
				.Select(sc => new ColumnInfo(columnName: sc.Name, sqlType: sc.SqlDbTypeDeclaration, isNullable: sc.IsNullable))
				.Concat(DataSchema.NonBizObjColumns)
				.ToList());
		IReadOnlyList<ColumnInfo> columnInfos;

		protected abstract string Topic { get; }
		protected abstract IProducer<string, string> GetOrCreateKafkaProducer(ILogger logger);
		protected abstract object KafkaTopicLock { get; }

		protected virtual string MessageKey { get; } = "ediProd";

		protected virtual int MaxChangesCount { get; } = 100;

		protected virtual bool UseTransactions { get; }

		protected virtual int MaxKafkaMessageSize { get; } = 1_000_000;  // Defaults to 1Mb

		protected ProducerDetails ProducerDetails => producerDetails ?? (producerDetails =
			new ProducerDetails(
				producerName: ProducerNamePrefix + Code,
				producerAssemblyVersion: FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion,
				producerHost: System.Environment.MachineName));
		ProducerDetails producerDetails;

		protected DataSource DataSource => dataSource ?? (dataSource =
			new DataSource(
				dataSchemaVersion: DataSchema.DataSchemaVersion,
				database: Db.DatabaseName,
				server: Db.ServerName,
				table: Table.SqlSchemaName + "." + Table.TableName,
				columns: ColumnInfos));
		DataSource dataSource;

		public virtual bool ShouldSendAuditRow(AuditRow auditRow) => true;

		JsonSerializer JsonSerializer { get; } = AuditMessageJsonSerializerFactory.CreateJsonSerializer();

		int InitialStringBuilderCapacity { get; } = 2048;

		IAuditSubscriberWrapper IAuditSubscriber.GetWrapper(DbConnection auditConnection, ILogger logger) => new DataScienceSubscriberToKafkaBaseWrapper(this, auditConnection, logger);

		public Guid SessionId { get; set; } = Guid.Empty;

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			if (UseTransactions)
			{
				logger.Information($"[Session {SessionId}] ProcessChanges - using Kafka transactions");
				lock (KafkaTopicLock)
				{
					var kafkaProducer = GetOrCreateKafkaProducer(logger);
					kafkaProducer.BeginTransaction();
					var abortTransactionOnError = true;
					try
					{
						var lastAuditRowProduced = ProduceInBatches(kafkaProducer,  logger, changeTable);
						abortTransactionOnError = false; // Beyond this point any exception throw should not cause the completed transaction to abort

						if (lastAuditRowProduced == null)
						{
							kafkaProducer.AbortTransaction();
						}
						else
						{
							kafkaProducer.CommitTransaction();
						}

						var librdkafkaOutQueueLength = kafkaProducer.Flush(kafkaFlushTimeout);
						if (librdkafkaOutQueueLength > 0)
						{
							throw new KafkaFlushTimeoutException($"Librdkafka out queue length is {librdkafkaOutQueueLength} after flush timeout. We probably lost messages!");
						}

						if (lastAuditRowProduced == null)
						{
							logger.Information($"All change records were filtered; no audit rows were produced");
						}
						else
						{
							logger.Information($"Committed up to startLsn {lastAuditRowProduced.StartLsn.ToHexString()}, sequenceValue {lastAuditRowProduced.SequenceValue.ToHexString()}");
						}
					}
					catch (Exception error)
					{
						if (!error.IsCriticalException() && abortTransactionOnError)
						{
							kafkaProducer.AbortTransaction();
						}

						logger.Error($"Kafka transaction aborted as {GetType().FullName}.ProcessChanges() failed with exception [{error.GetType().FullName}]({error.Message})", error);
						throw;
					}
				}
			}
			else
			{
				logger.Information($"[Session {SessionId}] ProcessChanges - not using Kafka transactions");
				try
				{
					AuditRow lastAuditRowProduced;
					lock (KafkaTopicLock)
					{
						var kafkaProducer = GetOrCreateKafkaProducer(logger);
						lastAuditRowProduced = ProduceInBatches(kafkaProducer, logger, changeTable);
						var librdkafkaOutQueueLength = kafkaProducer.Flush(kafkaFlushTimeout);
						if (librdkafkaOutQueueLength > 0)
						{
							throw new KafkaFlushTimeoutException($"Librdkafka out queue length is {librdkafkaOutQueueLength} after flush timeout. We probably lost messages!");
						}

						if (lastAuditRowProduced == null)
						{
							logger.Information($"All change records were filtered; no audit rows were produced");
						}
						else
						{
							logger.Information($"Published up to startLsn {lastAuditRowProduced.StartLsn.ToHexString()}, sequenceValue {lastAuditRowProduced.SequenceValue.ToHexString()}");
						}
					}
				}
				catch (Exception error)
				{
					logger.Error($"{GetType().FullName}.ProcessChanges() failed with exception [{error.GetType().FullName}]({error.Message})", error);
					throw;
				}
			}
		}

		AuditRow ProduceInBatches(IProducer<string, string> kafkaProducer, ILogger logger, DataTable changeTable)
		{
			reusableBuffer.Capacity = MaxKafkaMessageSize;
			if (MaxChangesCount < 1)
			{
				throw new InvalidOperationException(nameof(MaxChangesCount));
			}

			var auditRows = ConvertChangeTableToAuditRows(logger, changeTable);

			var filteredAuditRows = (from auditRow in auditRows where ShouldSendAuditRow(auditRow) select auditRow).ToList();

			logger.Information($"Producing in batches {filteredAuditRows.Count} audit rows from {changeTable.Rows.Count} change table rows, into Kafka topic {Topic}.");
			AuditRow lastAuditRowEverProduced = null;
			var messageRowIndexBegin = 0;
			while (messageRowIndexBegin < filteredAuditRows.Count)
			{
				var lastAuditRowJustProduced = AggregateAndProduceNextAuditMessage(
					kafkaProducer, filteredAuditRows, logger, MaxChangesCount, messageRowIndexBegin, out var messageRowIndexEnd);
				if (lastAuditRowJustProduced == null)
				{
					break;
				}

				lastAuditRowEverProduced = lastAuditRowJustProduced;
				messageRowIndexBegin = messageRowIndexEnd;
			}

			return lastAuditRowEverProduced;
		}

		AuditRow AggregateAndProduceNextAuditMessage(
			IProducer<string, string> kafkaProducer, List<AuditRow> auditRows, ILogger logger, int maxChangesCount, int messageRowIndexBegin, out int messageRowIndexEnd)
		{
			var auditMessage = AggregateNextAuditMessage(auditRows, maxChangesCount, messageRowIndexBegin, out messageRowIndexEnd, out var isOutsized);
			if (auditMessage == null)
			{
				return null;
			}

			if (isOutsized)
			{
				// We estimated that the audit message is too large. We will produce it in multiple parts.
				return ProduceOutsizedAuditMessageInMultipleParts(kafkaProducer, auditMessage, logger);
			}
			else
			{
				var kafkaMessage = new Message<string, string>()
				{
					Key = MessageKey,
					Value = SerialiseAuditMessage(auditMessage),
				};

				try
				{
					kafkaProducer.ProduceWithQHandling(logger, Topic, kafkaMessage, null);
					var lastAuditRowProduced = auditMessage.Changes[auditMessage.Changes.Count - 1];
					return lastAuditRowProduced;
				}
				catch (KafkaException produceError) when (produceError.Error.Code == ErrorCode.MsgSizeTooLarge)
				{
					// Kafka reported a message exceeding the size limit despite our prior overly optimistic estimate.
					if (auditMessage.Changes.Count > 1)
					{
						// What we are gonna do now is to redo the aggregation with a more conservative parameter
						var reducedMaxChangesCount = Math.Max(1, (1 + auditMessage.Changes.Count) / 2);
						Debug.Assert(reducedMaxChangesCount < maxChangesCount);
						return AggregateAndProduceNextAuditMessage(kafkaProducer, auditRows, logger, reducedMaxChangesCount, messageRowIndexBegin, out messageRowIndexEnd);
					}
					else
					{
						// There is only one audit record in the message. We will produce the audit message in multiple parts.
						return ProduceOutsizedAuditMessageInMultipleParts(kafkaProducer, auditMessage, logger, priorError: produceError);
					}
				}
				catch (KafkaException produceError)
				{
					logger?.Error($"{GetType().FullName}.{nameof(AggregateAndProduceNextAuditMessage)}() failed with error [{produceError.GetType().FullName}]({produceError.Message})", produceError);
					throw;
				}
			}
		}

		AuditRow ProduceOutsizedAuditMessageInMultipleParts(IProducer<string, string> kafkaProducer, AuditMessage auditMessage, ILogger logger, int additionalOverhead = 0, KafkaException priorError = null)
		{
			Debug.Assert(auditMessage.Changes.Count == 1);
			var theOnlyAuditRow = auditMessage.Changes.Single();
			var messageEncoding = Encoding.UTF8;
			var messageId = Guid.NewGuid();
			var partialMessages = PartialAuditMessage.SplitFrom(
				auditMessage,
				maxMessageSize: MaxKafkaMessageSize,
				kafkaMessageOverhead: kafkaMessageSizeOverhead + messageEncoding.GetByteCount(MessageKey) + additionalOverhead,
				messageSerialiser: JsonSerializer,
				messageId: messageId);

			logger.Information(
				string.Concat(
					$"Producing large audit message in multiple {partialMessages.Length} parts",
					$" so as to not exceed the Kafka message limit ({MaxKafkaMessageSize}).",
					$" MESSAGE ID={messageId},",
					$" LSN={theOnlyAuditRow.StartLsn?.ToHexString() ?? "<null>"},",
					$" SEQVAL={theOnlyAuditRow.SequenceValue?.ToHexString() ?? "<null>"},",
					$" OPERATION={theOnlyAuditRow.Operation},",
					$" TRANSACTION END TIME={theOnlyAuditRow.TransactionEndTimeUtc: O}."));

			try
			{
				foreach (var partialMessage in partialMessages)
				{
					var kafkaMessage = new Message<string, string>()
					{
						Key = MessageKey,
						Value = SerialisePartialAuditMessage(partialMessage),
					};

					kafkaProducer.ProduceWithQHandling(logger, Topic, kafkaMessage, null);
					logger.Debug($"Produced part {partialMessage.MultipartTrackingInfo.PartIndex} out of {partialMessage.MultipartTrackingInfo.PartsCount} for multi-part message {partialMessage.MultipartTrackingInfo.MessageId}");
				}

				return theOnlyAuditRow;
			}
			catch (KafkaException produceError) when (produceError.Error.Code == ErrorCode.MsgSizeTooLarge)
			{
				if (additionalOverhead < 500_000)
				{
					// Let's split the audit message into even smaller parts and try produce them
					return ProduceOutsizedAuditMessageInMultipleParts(kafkaProducer, auditMessage, logger, additionalOverhead + 100_000);
				}

				// By this point there really is not much more we could do...
				throw new OversizeChangeRecordException(
					auditRow: auditMessage.Changes.Single(),
					subscriber: this,
					actualMessageSize: kafkaMessageSizeOverhead + Encoding.UTF8.GetByteCount(MessageKey) + Encoding.UTF8.GetByteCount(SerialiseAuditMessage(auditMessage)),
					maxMessageSize: MaxKafkaMessageSize,
					innerException: priorError);
			}
		}

		AuditMessage AggregateNextAuditMessage(List<AuditRow> auditRows, int maxMessageRowCount, int messageRowIndexBegin, out int messageRowIndexEnd, out bool isOutsized)
		{
			isOutsized = false;
			var messageEncoding = Encoding.UTF8;
			var auditMessage = new AuditMessage(ProducerDetails, DataSource);
			var auditMessageJSON = SerialiseAuditMessage(auditMessage);
			var minMessageSize = kafkaMessageSizeOverhead + messageEncoding.GetByteCount(MessageKey) + messageEncoding.GetByteCount(auditMessageJSON);
			var messageSize = minMessageSize;
			var messageRowCount = 0;
			messageRowIndexEnd = messageRowIndexBegin;
			for (int i = messageRowIndexBegin, n = auditRows.Count; i < n; ++i)
			{
				var auditRow = auditRows[i];
				var auditRowJSON = SerialiseAuditRow(auditRow);
				var rowSize = Encoding.UTF8.GetByteCount(auditRowJSON);
				if (auditMessage.Changes.Count > 0)
				{
					rowSize += Encoding.UTF8.GetByteCount(",");
				}

				var nextMessageSize = messageSize + rowSize;
				var nextMessageRowCount = messageRowCount + 1;
				if (messageRowCount > 0 && (nextMessageSize > MaxKafkaMessageSize || nextMessageRowCount > maxMessageRowCount))
				{
					messageRowIndexEnd = messageRowIndexBegin + messageRowCount;
					isOutsized = messageSize > MaxKafkaMessageSize;
					return auditMessage;
				}

				auditMessage.Changes.Add(auditRow);
				messageSize = nextMessageSize;
				messageRowCount = nextMessageRowCount;
			}

			if (auditMessage.Changes.Count > 0)
			{
				messageRowIndexEnd = messageRowIndexBegin + messageRowCount;
				isOutsized = messageSize > MaxKafkaMessageSize;
				return auditMessage;
			}

			return null;
		}

		List<AuditRow> ConvertChangeTableToAuditRows(ILogger logger, DataTable changeTable)
		{
			return changeTable.Rows.Cast<DataRow>().SelectMany(row =>
			{
				if (!IsRowStateAndCdcOperationConsistent(row))
				{
					throw new InvalidOperationException(
						string.Concat(
							$"Invalid {nameof(CdcOperation)} in change table row: ",
							$"Original StartLsn {(row[AuditFieldNames.StartLsnFieldName, DataRowVersion.Original] as byte[])?.ToHexString() ?? "<null>"}, ",
							$"Original SeqVal {(row[AuditFieldNames.SeqValFieldName, DataRowVersion.Original] as byte[])?.ToHexString() ?? "<null>"}, ",
							$"Current StartLsn {(row[AuditFieldNames.StartLsnFieldName, DataRowVersion.Current] as byte[])?.ToHexString() ?? "<null>"}, ",
							$"Current SeqVal {(row[AuditFieldNames.SeqValFieldName, DataRowVersion.Current] as byte[])?.ToHexString() ?? "<null>"}, ",
							$"RowState {row.RowState}, ",
							$"Original {nameof(CdcOperation)} {(row[AuditFieldNames.OperationFieldName, DataRowVersion.Original] as CdcOperation?)?.ToString() ?? "<null>"}, ",
							$"Current {nameof(CdcOperation)} {(row[AuditFieldNames.OperationFieldName, DataRowVersion.Current] as CdcOperation?)?.ToString() ?? "<null>"}"));
				}

				switch (row.RowState)
				{
					case DataRowState.Deleted:
						return new[] { (ChangeRow: row, RowVersion: DataRowVersion.Original, Operation: CdcOperation.Delete) };
					case DataRowState.Added:
						return new[] { (ChangeRow: row, RowVersion: DataRowVersion.Current, Operation: CdcOperation.Insert) };
					case DataRowState.Modified:
						return new[] {
							(ChangeRow: row, RowVersion: DataRowVersion.Original, Operation: CdcOperation.PreUpdate),
							(ChangeRow: row, RowVersion: DataRowVersion.Current, Operation: CdcOperation.PostUpdate)
						};
					default:
						logger.Error(
							string.Concat(
								$"Unexpected change table RowState; ignoring row: ",
								$"DataRowVersion.Default StartLsn {(row[AuditFieldNames.StartLsnFieldName, DataRowVersion.Default] as byte[])?.ToHexString() ?? "<null>"}, ",
								$"DataRowVersion.Default SeqVal {(row[AuditFieldNames.SeqValFieldName, DataRowVersion.Default] as byte[])?.ToHexString() ?? "<null>"}, ",
								$"RowState {row.RowState}"));
						return Enumerable.Empty<(DataRow ChangeRow, DataRowVersion RowVersion, CdcOperation Operation)>();
				}
			})
			.Select(cr => new AuditRow(
				startLsn: (byte[])cr.ChangeRow[AuditFieldNames.StartLsnFieldName, cr.RowVersion],
				sequenceValue: (byte[])cr.ChangeRow[AuditFieldNames.SeqValFieldName, cr.RowVersion],
				operation: cr.Operation,
				transactionEndTimeUtc: (DateTime)cr.ChangeRow[AuditFieldNames.TranEndTimeUtc, cr.RowVersion],
				columnValues: ColumnInfos.ToDictionary(c => c.ColumnName, c => cr.ChangeRow[c.ColumnName, cr.RowVersion])
			))
			.ToList();
		}

		static bool IsRowStateAndCdcOperationConsistent(DataRow changeRow)
		{
			CdcOperation OriginalCdcOperation()
			{
				return (CdcOperation)changeRow[AuditFieldNames.OperationFieldName, DataRowVersion.Original];
			}

			CdcOperation CurrentCdcOperation()
			{
				return (CdcOperation)changeRow[AuditFieldNames.OperationFieldName, DataRowVersion.Current];
			}

			return
					changeRow.RowState == DataRowState.Deleted && OriginalCdcOperation() == CdcOperation.Delete
				|| changeRow.RowState == DataRowState.Added && CurrentCdcOperation() == CdcOperation.Insert
				|| changeRow.RowState == DataRowState.Modified
						&& (OriginalCdcOperation() == CdcOperation.PreUpdate || OriginalCdcOperation() == CdcOperation.PostUpdate)
						&& (CurrentCdcOperation() == CdcOperation.PreUpdate || CurrentCdcOperation() == CdcOperation.PostUpdate);
		}

		string SerialiseAuditMessage(AuditMessage auditMessage)
		{
			reusableBuffer.Clear();
			using var textWriter = new StringWriter(reusableBuffer);
			using var jsonWriter = new JsonTextWriter(textWriter);
			JsonSerializer.Serialize(jsonWriter, auditMessage);
			return reusableBuffer.ToString();
		}

		string SerialiseAuditRow(AuditRow auditRow)
		{
			reusableBuffer.Clear();
			using var textWriter = new StringWriter(reusableBuffer);
			using var jsonWriter = new JsonTextWriter(textWriter);
			JsonSerializer.Serialize(jsonWriter, auditRow);
			return reusableBuffer.ToString();
		}

		string SerialisePartialAuditMessage(PartialAuditMessage partialAuditMessage)
		{
			reusableBuffer.Clear();
			using var textWriter = new StringWriter(reusableBuffer);
			using var jsonWriter = new JsonTextWriter(textWriter);
			JsonSerializer.Serialize(jsonWriter, partialAuditMessage);
			return reusableBuffer.ToString();
		}
	}

	sealed class DataScienceSubscriberToKafkaBaseWrapper : ActualDataChangesAuditSubscriberWrapper
	{
		readonly DataScienceSubscriberToKafkaBase dataScienceSubscriber;

		public DataScienceSubscriberToKafkaBaseWrapper(DataScienceSubscriberToKafkaBase subscriber, DbConnection auditConnection, ILogger logger)
			: base(subscriber, auditConnection, logger)
		{
			dataScienceSubscriber = subscriber;
		}

		Guid SessionId
		{
			get => dataScienceSubscriber.SessionId;
			set => dataScienceSubscriber.SessionId = value;
		}

		public override bool FetchDataAndProcessChanges()
		{
			SessionId = Guid.NewGuid();
			LogBeginSession();
			try
			{
				return base.FetchDataAndProcessChanges();
			}
			finally
			{
				LogEndSession();
				SessionId = Guid.Empty;
			}
		}

		public override bool HasChangesToProcess()
		{
			LogHasChangesToProcessParameters();
			var hasChanges = base.HasChangesToProcess();
			LogHasChangesToProcessOutputs(hasChanges);
			return hasChanges;
		}

		public override void UpdateLsnHighWaterMark()
		{
			LogUpdateLsnHighWaterMarkParameters();
			base.UpdateLsnHighWaterMark();
		}

		protected override DataTable FilterQueryResult(DataTable queryResult)
		{
			LogUnprocessedChangesRange($"{nameof(FilterQueryResult)} - Unprocessed", queryResult);
			var processedChanges = base.FilterQueryResult(queryResult);
			LogProcessedChangesRange(processedChanges);
			return processedChanges;
		}

		protected override DataTable ExecuteFetchChangesQuery()
		{
			LogFetchDataParameters();
			var rawChanges = base.ExecuteFetchChangesQuery();
			LogUnprocessedChangesRange($"{nameof(ExecuteFetchChangesQuery)} - Result", rawChanges);
			return rawChanges;
		}

		void LogFetchDataParameters()
		{
			Logger.Debug(
				$"[Session {SessionId}] Fetch parameters" +
				$": @SubscriberTableName = {Table.TableName}" +
				$", @BatchSize = {BatchSize}" +
				$", @LatestLsn = {MaxLsn?.ToHexString() ?? "<null>"}" +
				$", @LsnPeriod = {NextPeriodHighWaterMark}" +
				$", @LsnHighWaterMark = {NextLsnHighWaterMark?.ToHexString() ?? "<null>"}" +
				$", @SeqValHighWaterMark = {NextSeqValHighWaterMark?.ToHexString() ?? "<null>"}" +
				$", @CommandId = {NextCommandIdHighWaterMark}" +
				$", @Operation = {NextOperationHighWaterMark}");
		}

		void LogHasChangesToProcessOutputs(bool hasChanges)
		{
			Logger.Debug(
				$"[Session {SessionId}] HasChangesToProcess outputs" +
				$": @lsnHighWaterMark = {NextLsnHighWaterMark?.ToHexString() ?? "<null>"}" +
				$", @seqValHighWaterMark = {NextSeqValHighWaterMark?.ToHexString() ?? "<null>"}" +
				$", @commandIdHighWaterMark = {NextCommandIdHighWaterMark}" +
				$", @operationHighWaterMark = {NextOperationHighWaterMark}" +
				$", @nextPeriodToProcess (NextPeriodHighWaterMark) = {NextPeriodHighWaterMark}" +
				$", HasChangesToProcess = {hasChanges}");
		}

		void LogHasChangesToProcessParameters()
		{
			Logger.Debug(
				$"[Session {SessionId}] HasChangesToProcess parameters" +
				$": @subscriberCode = {Code}" +
				$", @subscriberTableName = {Table.TableName}" +
				$", @subscriberSchemaName = {Table.SqlSchemaName}" +
				$", @latestPeriod = {MaxLsnPeriod}" +
				$", NextPeriodHighWaterMark = {NextPeriodHighWaterMark}");
		}

		void LogUpdateLsnHighWaterMarkParameters()
		{
			Logger.Debug(
				$"[Session {SessionId}] UpdateLsnHighWaterMark parameters" +
				$": @lsnHighWaterMark = {NextLsnHighWaterMark?.ToHexString() ?? "<null>"}" +
				$", @seqValHighWaterMark = {NextSeqValHighWaterMark?.ToHexString() ?? "<null>"}" +
				$", @periodHighWaterMark = {NextPeriodHighWaterMark}" +
				$", @commandIdHighWaterMark = {NextCommandIdHighWaterMark}" +
				$", @operationHighWaterMark = {NextOperationHighWaterMark}");
		}

		void LogUnprocessedChangesRange(string stageName, DataTable queryResult)
		{
			static void ExtractChangeRecordKeysForLogging(DataRow row, out byte[] lsn, out byte[] seqVal, out CdcOperation? operation)
			{
				lsn = row[AuditFieldNames.StartLsnFieldName] as byte[];
				seqVal = row[AuditFieldNames.SeqValFieldName] as byte[];
				operation = row[AuditFieldNames.OperationFieldName] switch
				{
					int i => (CdcOperation)i,
					CdcOperation op => op,
					_ => null
				};
			}

			var numRows = queryResult.Rows.Count;
			if (numRows > 0)
			{
				var firstRow = queryResult.Rows[0];
				var lastRow = queryResult.Rows[numRows - 1];
				ExtractChangeRecordKeysForLogging(firstRow, out var firstRowLsn, out var firstRowSeqVal, out var firstRowOp);
				ExtractChangeRecordKeysForLogging(lastRow, out var lastRowLsn, out var lastRowSeqVal, out var lastRowOp);

				Logger.Debug(
					$"[Session {SessionId}] {stageName}" +
					$": numRows = {numRows}" +
					$", table = [{Table.SqlSchemaName}].[{Table.TableName}]" +
					$", from = (LSN={firstRowLsn?.ToHexString() ?? "<null>"}, SEQVAL={firstRowSeqVal?.ToHexString() ?? "<null>"}, OPERATION={firstRowOp?.ToString() ?? "<null>"})" +
					$", to = (LSN={lastRowLsn?.ToHexString() ?? "<null>"}, SEQVAL={lastRowSeqVal?.ToHexString() ?? "<null>"}, OPERATION={lastRowOp?.ToString() ?? "<null>"})");
			}
			else
			{
				Logger.Debug($"[Session {SessionId}] {stageName} - empty unprocessed changes");
			}
		}

		void LogProcessedChangesRange(DataTable changeTable)
		{
			static void ExtractChangeRecordKeysForLogging(DataRow row, out byte[] lsn, out byte[] seqVal, out CdcOperation[] operations)
			{
				var rowState = row.RowState;
				var version = row.RowState switch
				{
					DataRowState.Deleted => DataRowVersion.Original,
					_ => DataRowVersion.Current
				};

				operations = rowState switch
				{
					DataRowState.Deleted => new [] { CdcOperation.Delete },
					DataRowState.Added => new [] { CdcOperation.Insert },
					DataRowState.Modified => new [] { CdcOperation.PreUpdate, CdcOperation.PostUpdate },
					_ => Array.Empty<CdcOperation>()
				};

				lsn = row[AuditFieldNames.StartLsnFieldName, version] as byte[];
				seqVal = row[AuditFieldNames.SeqValFieldName, version] as byte[];
			}

			var numRows = changeTable.Rows.Count;
			if (numRows > 0)
			{
				var firstRow = changeTable.Rows[0];
				var lastRow = changeTable.Rows[numRows - 1];
				var numChangeRecords = changeTable.Rows.Cast<DataRow>().Sum(r => r.RowState == DataRowState.Modified ? 2 : 1);
				ExtractChangeRecordKeysForLogging(firstRow, out var firstRowLsn, out var firstRowSeqVal, out var firstRowOps);
				ExtractChangeRecordKeysForLogging(lastRow, out var lastRowLsn, out var lastRowSeqVal, out var lastRowOps);

				Logger.Debug(
					$"[Session {SessionId}] FilterQueryResult - processed changes" +
					$": numRows = {numRows}" +
					$", numChangeRecords = {numChangeRecords}" +
					$", table = [{Table.SqlSchemaName}].[{Table.TableName}]" +
					$", from = (LSN={firstRowLsn?.ToHexString() ?? "<null>"}, SEQVAL={firstRowSeqVal?.ToHexString() ?? "<null>"}, OPERATION={string.Join("+", firstRowOps)})" +
					$", to = (LSN={lastRowLsn?.ToHexString() ?? "<null>"}, SEQVAL={lastRowSeqVal?.ToHexString() ?? "<null>"}, OPERATION={string.Join("+", lastRowOps)})");
			}
			else
			{
				Logger.Debug($"[Session {SessionId}] FilterQueryResult - empty processed changes");
			}
		}

		void LogBeginSession() => Logger.Information($"[Session {SessionId}] New session");
		void LogEndSession() => Logger.Information($"[Session {SessionId}] End of session");
	}

	public abstract class DataScienceSubscriberToKafkaBase<TConfig> : DataScienceSubscriberToKafkaBase, ISubscriberDataSchema
		where TConfig : KafkaRegistryConfigsBase<TConfig>, new()
	{
		readonly Lazy<TConfig> lazyKafkaConfigs = new (() => new ());

		public override bool NotifyInsert => true;
		public override bool NotifyUpdate => true;
		public override bool NotifyDelete => true;
		public override bool IsRequired() => lazyKafkaConfigs.Value.EnableSubscriber;
		protected override string Topic => lazyKafkaConfigs.Value.KafkaTopic;
		protected override bool UseTransactions => lazyKafkaConfigs.Value.UseTransaction;
		protected override IProducer<string, string> GetOrCreateKafkaProducer(ILogger logger) => lazyKafkaConfigs.Value.GetOrCreateProducer(logger);
		protected override object KafkaTopicLock => lazyKafkaConfigs.Value.KafkaTopicLock;
		public override ISubscriberDataSchema DataSchema => this;
		public abstract int DataSchemaVersion { get; }
		public virtual IEnumerable<SchemaColumn> BizObjColumns => Enumerable.Empty<SchemaColumn>();
		public virtual IEnumerable<SchemaColumn> LegacyColumns => Enumerable.Empty<SchemaColumn>();
		public virtual IEnumerable<ColumnInfo> NonBizObjColumns => Enumerable.Empty<ColumnInfo>();
	}
}
