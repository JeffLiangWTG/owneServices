using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	public abstract class AuditSubscriberWrapper : IAuditSubscriberWrapper
	{
		public AuditSubscriberWrapper(IAuditSubscriber subscriber, DbConnection auditConnection, ILogger logger)
		{
			Subscriber = subscriber;
			this.auditConnection = auditConnection;
			Logger = logger;

			MaxLsn = BiMasterState.GetParameterAsByteArray(auditConnection, BiConstants.LastMaxLsnProcessed);
			MaxLsnPeriod = BiMasterState.GetLastMaxLsnProcessedPeriod(auditConnection).GetValueOrDefault();
			Logger.Log(LogType.Debug, $"> Audit DB HWM: (LSN: {ByteArrayToString(MaxLsn)}, Period: {MaxLsnPeriod})");
		}

		#region Properties and Fields

		public readonly byte[] MaxLsn;
		public readonly int MaxLsnPeriod;

		public readonly dynamic Subscriber;
		public DbConnection auditConnection { get; }
		public ILogger Logger { get; }

		public byte[] NextLsnHighWaterMark { get; set; }
		public byte[] NextSeqValHighWaterMark { get; set; }
		public int NextPeriodHighWaterMark { get; set; }
		public int NextCommandIdHighWaterMark { get; set; }
		public int NextOperationHighWaterMark { get; set; }

		public byte[] CurrentLsnHighWaterMark { get; set; }
		public int CurrentPeriodHighWaterMark { get; set; }

		public int BatchSize
		{
			get
			{
				if (batchSize == null)
				{
					batchSize = GetBatchSize();
				}
				return batchSize.Value;
			}
			protected set
			{
				batchSize = value;
			}
		}
		int? batchSize;

		public bool IsValid
		{
			get
			{
				if (isValid == null)
				{
					try
					{
						ValidateSubscriber();
						isValid = true;
					}
					catch (Exception)
					{
						isValid = false;
					}
				}
				return isValid.Value;
			}
		}
		bool? isValid;

		int GetBatchSize()
		{
			var bSizeString = BiMasterState.GetParameter(auditConnection, BiConstants.AspBatchSize);

			int bSizeInt;
			if (int.TryParse(bSizeString, out bSizeInt))
			{
				return bSizeInt;
			}
			else
			{
				return defaultBatchSize;
			}
		}
		const int defaultBatchSize = 5000;

		public string Code => Subscriber.Code;
		public string Description => Subscriber.Description;

		#endregion

#pragma warning disable CW1107 // Do Not Use Db.Connection Methods
		public void AddSubscriberToSubscriberControlTable()
		{
			using (var cmd = auditConnection.Command($"[{BiConstants.BiAdminSchemaName}].usp_AddSubscriberToSubscriberControlTable"))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.AddParameter("@SubscriberCode", SqlDbType.Char, 3, Code);
				cmd.AddParameter("@Description", SqlDbType.VarChar, 128, Description);
				cmd.AddParameter("@LatestLsn", SqlDbType.Binary, 10, 0, 0, MaxLsn);
				cmd.AddParameter("@LatestPeriod", SqlDbType.SmallInt, MaxLsnPeriod);

				cmd.ExecuteNonQuery();

				NextLsnHighWaterMark = MaxLsn;
				NextSeqValHighWaterMark = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
				NextCommandIdHighWaterMark = 2147483647;
				NextOperationHighWaterMark = 2147483647;
				NextPeriodHighWaterMark = MaxLsnPeriod;

				Logger.Log(LogType.Debug, $"> Added to subscriber control table (LSN: {ByteArrayToString(NextLsnHighWaterMark)}, SeqVal: {ByteArrayToString(NextSeqValHighWaterMark)}, Period: {NextPeriodHighWaterMark})");
			}
		}

		protected string ByteArrayToString(byte[] bytes)
		{
			var response = BitConverter.ToString(bytes).Replace("-", "");
			return response;
		}
#pragma warning restore CW1107 // Do Not Use Db.Connection Methods

		public bool ExistsInSubscriberControlTable
		{
			get
			{
				var query = $"SELECT COUNT(*) FROM [{BiConstants.BiAdminSchemaName}].SubscriberControl WHERE SubscriberCode = @SubscriberCode";

				using (var cmd = auditConnection.Command(query))
				{
					cmd.AddParameter("@SubscriberCode", SqlDbType.Char, 3, Code);
					var result = (int)cmd.ExecuteScalar();
					return result > 0;
				}
			}
		}

		public abstract bool HasChangesToProcess();

		public abstract bool FetchDataAndProcessChanges();

		public virtual void UpdateLsnHighWaterMark()
		{
			UpdateSubscriberControlHighWaterMark(NextLsnHighWaterMark, NextSeqValHighWaterMark, NextPeriodHighWaterMark, NextCommandIdHighWaterMark, NextOperationHighWaterMark);
		}

		public void UpdateSubscriberControlHighWaterMark(byte[] nextLsnHighWaterMark, byte[] nextSeqValHighWaterMark, int nextPeriodHighWaterMark, int nextCommandIdHighWaterMark, int nextOperationHighWaterMark)
		{
			Argument.NotNull(nextLsnHighWaterMark, nameof(nextLsnHighWaterMark));
			Argument.NotNull(nextSeqValHighWaterMark, nameof(nextSeqValHighWaterMark));
			Argument.NotNull(nextPeriodHighWaterMark, nameof(nextPeriodHighWaterMark));

			using (var cmd = auditConnection.Command($"{BiConstants.BiAdminSchemaName}.usp_UpdateSubscriberHighWaterMark"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@subscriberCode", SqlDbType.Char, 3, Code);
				cmd.AddParameter("@lsnHighWaterMark", SqlDbType.Binary, 10, nextLsnHighWaterMark);
				cmd.AddParameter("@seqValHighWaterMark", SqlDbType.Binary, 10, nextSeqValHighWaterMark);
				cmd.AddParameter("@periodHighWaterMark", SqlDbType.SmallInt, nextPeriodHighWaterMark);
				cmd.AddParameter("@commandIdHighWaterMark", SqlDbType.Int, nextCommandIdHighWaterMark);
				cmd.AddParameter("@operationHighWaterMark", SqlDbType.Int, nextOperationHighWaterMark);
				cmd.ExecuteNonQuery();

				Logger.Log(LogType.Debug, $"> Updating Subscriber High Water Mark (LSN: {ByteArrayToString(nextLsnHighWaterMark)}, SeqVal: {ByteArrayToString(nextSeqValHighWaterMark)}, Period: {nextPeriodHighWaterMark})");
			}
		}

		protected static byte[] GetByteOutputParameter(DbCommand cmd, string paramName)
		{
			var objParamValue = cmd.GetParameterValue(paramName);
			return (objParamValue == DBNull.Value) ? null : (byte[])objParamValue;
		}

		protected static int GetIntOutputParameter(DbCommand cmd, string paramName)
		{
			var objParamValue = cmd.GetParameterValue(paramName);
			return (objParamValue == DBNull.Value) ? 0 : (int)objParamValue;
		}

		protected static int GetSmallintOutputParameter(DbCommand cmd, string paramName)
		{
			var objParamValue = cmd.GetParameterValue(paramName);
			return (objParamValue == DBNull.Value) ? 0 : (short)objParamValue;
		}

		public void SetLsnHighWaterMarkToMaxLsn()
		{
			UpdateSubscriberControlHighWaterMark(MaxLsn, new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, MaxLsnPeriod, 2147483647, 2147483647);
		}

		public abstract void ValidateSubscriber();

		public IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger)
		{
			return this;
		}

		public bool IsRequired()
		{
			return Subscriber.IsRequired();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "Baseline")]

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "System Notification")]
		public bool ShouldRunSubscriber()
		{
			var shouldRunSubscriber = false;
			try
			{
				if (IsRequired())
				{
					ValidateSubscriber();
					if (ExistsInSubscriberControlTable)
					{
						if (HasChangesToProcess())
						{
							shouldRunSubscriber = true;
						}
						else if (ShouldUpdateHighWaterMark)
						{
							SetLsnHighWaterMarkToMaxLsn();
						}
					}
					else
					{
						AddSubscriberToSubscriberControlTable();
					}
				}
			}
			catch (TimeoutException ex) when (ex.Message == "Timeout during IsRequired")
			{
				var errorMessage = $"Validation failed: Requirements check failed due to a timeout.";
				ErrorReporter.ReportOnce("AuditSubscriberWrapperValidationTimeout", errorMessage, ex);
				Logger.Error(errorMessage);
				shouldRunSubscriber = false;
			}
			catch (Exception ex)
			{
				var errorMessage = new StringBuilder();
				var innerEx = ex;
				while (innerEx != null)
				{
					errorMessage.AppendLine(innerEx.Message);
					innerEx = innerEx.InnerException;
				}
				ErrorReporter.ReportOnce("AuditSubscriberWrapperValidationFailure", errorMessage.ToString(), ex);
				Logger.Error($"Validation failed:\r\n{errorMessage}");
				shouldRunSubscriber = false;
			}

			return shouldRunSubscriber;
		}

		protected bool IsTableStateHwmGreaterThanSubscriberControlHwm(string tableName, Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache)
		{
			var result = false;
			var subscriberCodeIsValid = subscriberLsnHighWaterMarkCache.ContainsKey(Code);
			var tableStateHwmIsValid = tableStateLsnHighWaterMarkCache.ContainsKey(tableName) && tableStateLsnHighWaterMarkCache[tableName] != null;

			if (subscriberCodeIsValid && tableStateHwmIsValid)
			{
				var tableStateHwmExceedsSubscriberControlHwm = tableStateLsnHighWaterMarkCache[tableName].CompareTo(subscriberLsnHighWaterMarkCache[Code]) > 0 ||
					(
						tableStateLsnHighWaterMarkCache[tableName].CompareTo(subscriberLsnHighWaterMarkCache[Code]) == 0 &&
						subscriberCommandIdHighWaterMarkCache[Code] < int.MaxValue
					);
				result = tableStateHwmExceedsSubscriberControlHwm;
			}

			return result;
		}

		public abstract bool HasChangesToProcessFromCache(Dictionary<string, Lsn> tableStateLsnHighWaterMarkCache, Dictionary<string, Lsn> subscriberLsnHighWaterMarkCache, Dictionary<string, int> subscriberCommandIdHighWaterMarkCache);

		protected bool ShouldUpdateHighWaterMark { get; set; } = true;

		public static bool ThereAreRelevantChanges(DataTable changeTable)
		{
			return changeTable.Rows.Count > 0;
		}
	}
}
