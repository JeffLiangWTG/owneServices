using System;
using System.Data;
using System.Text;
using BorderWise.Sync;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.Integration;
using Newtonsoft.Json;

namespace Enterprise.AuditDataServices.BorderWise.Subscribers
{
	public abstract class BorderWiseSubscriberBase : ActualDataChangesAuditSubscriber
	{
		public override bool NotifyInsert => true;
		public override bool NotifyUpdate => true;
		public override bool NotifyDelete => true;
		public override Action<DataRow> CustomFilter => null;

		public override bool IsRequired()
		{
			return true;
		}

		public virtual string MessageKey => "EdiProd2BWUPM-Sync"; // Use constant key to ensure all messages are put into same partition (even if there are multiple partitions)

		protected abstract string MessageSource { get; }

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			logger.Log(LogType.Debug, $"BorderWise Received DataRows: {changeTable.Rows.Count}"); // Log message
			using (var publisher = ObjectFactory.Get<IBorderWiseChangesPublisher>())
			{
				foreach (DataRow changeRow in changeTable.Rows)
				{
					var changeRowValue = GetDataRowDetails(changeRow);
					logger.Log(LogType.Debug, $"BorderWise Subscriber Processing DataRow, Change State: {changeRow.RowState}; Value: {changeRowValue}"); // Log message

					try
					{
						PublishChange(logger, changeRow, publisher);
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						var message = GetDataRowDetailsCore(changeRow, changeRow.RowState == DataRowState.Deleted ? DataRowVersion.Original : DataRowVersion.Current);
						throw new Exception(message, ex);
					}
				}
			}
		}

		static string GetDataRowDetails(DataRow changeRow)
		{
			switch (changeRow.RowState)
			{
				case DataRowState.Added:
					return GetDataRowDetailsCore(changeRow, DataRowVersion.Current);
				case DataRowState.Deleted:
					return GetDataRowDetailsCore(changeRow, DataRowVersion.Original);
				case DataRowState.Modified:
					return $"Original Version: {GetDataRowDetailsCore(changeRow, DataRowVersion.Original)}; Current Version: {GetDataRowDetailsCore(changeRow, DataRowVersion.Current)}"; // Log message
				case DataRowState.Detached:
					break;
				case DataRowState.Unchanged:
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(changeRow), changeRow.RowState, "Invalid DataRowState value.");
			}
			return string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer exception message")]
		static string GetDataRowDetailsCore(DataRow changeRow, DataRowVersion dataRowVersion)
		{
			var message = new StringBuilder();
			message.Append("Data row details");

			foreach (DataColumn column in changeRow.Table.Columns)
			{
				var value = changeRow[column, dataRowVersion];
				var valueAsString = value is Array arr
					? FormattableString.Invariant($"{arr.GetType().Name} length {arr.Length}")
					: value?.ToString();

				message.AppendLine();
				message.Append(FormattableString.Invariant($"{column.ColumnName}: {valueAsString}"));
			}

			return message.ToString();
		}

		protected abstract void PublishChange(ILogger logger, DataRow changeRow, IBorderWiseChangesPublisher publisher);

		protected virtual ChangeType GetChangeType(ILogger logger, DataRow changeRow)
		{
			ChangeType changeType;

			try
			{
				changeType = changeRow.RowState.ToChangeType();
			}
			catch (ArgumentException ex)
			{
				logger.Log(LogType.Warning, FormattableString.Invariant($"Cannot process changed {Table.TableName} row with RowState = {changeRow.RowState}"), ex);
				changeType = ChangeType.None;
			}

			return changeType;
		}

		protected ChangeSequenceDataObject GetChangeSequence(DataRow changeRow, DataRowVersion rowVersion)
		{
			return new ChangeSequenceDataObject
			{
				TransactionLsn = (byte[])changeRow[AuditFieldNames.StartLsnFieldName, rowVersion],
				SequenceValue = (byte[])changeRow[AuditFieldNames.SeqValFieldName, rowVersion],
				CommandId = (int)changeRow[AuditFieldNames.CommandIdFieldName, rowVersion]
			};
		}

		protected BusinessObjectFactory DataFactory => dataFactory ?? (dataFactory = new BusinessObjectFactory { RefreshEnabled = false });
		BusinessObjectFactory dataFactory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "json convert setting")]
		const string SerializerDateFormat = "yyyy-MM-dd HH:mm:ss";
		protected string SerializeObject(object obj)
		{
			var jsonSettings = new JsonSerializerSettings
			{
				DateFormatString = SerializerDateFormat,
				Formatting = Formatting.Indented
			};
			return JsonConvert.SerializeObject(obj, jsonSettings);
		}
	}
}
