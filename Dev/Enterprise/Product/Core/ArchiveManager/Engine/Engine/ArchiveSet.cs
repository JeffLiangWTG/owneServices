using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.ArchiveManager.Engine.ArchiveSetExceptionHelpers;

namespace Enterprise.ArchiveManager.Engine
{
	public class ArchiveSet : IArchiveSet
	{
		public ArchiveSet(IArchiveSystemDescriptor systemDescriptor, string stageName, Guid scheduleIdentifier, ArchiveItem mainItem, ArchiveableType mainArchiveableType, ZQuery mainArchiveableTypeFilter)
			: this(systemDescriptor, stageName, scheduleIdentifier, mainItem, mainArchiveableType, mainArchiveableTypeFilter, null)
		{
			SystemDescriptor = systemDescriptor;
			StageName = stageName;
			ScheduleIdentifier = scheduleIdentifier;
		}

		public IArchiveSystemDescriptor SystemDescriptor { get; }

		public string StageName { get; }

		public string SanitizedStageName => StageName.Replace(" ", "").Replace("_", "");

		public Guid ScheduleIdentifier { get; }

		public ArchiveSet(IArchiveSystemDescriptor systemDescriptor, string stageName, Guid scheduleIdentifier, ArchiveItem mainItem, ArchiveableType mainArchiveableType, ZQuery mainArchiveableTypeFilter, string mainArchiveItemNK)
		{
			SystemDescriptor = systemDescriptor;
			StageName = stageName;
			ScheduleIdentifier = scheduleIdentifier;
			MainArchiveItem = mainItem;
			MainArchiveItemNK = mainArchiveItemNK;
			MainArchiveableTypeFilter = mainArchiveableTypeFilter;
			this.mainArchiveableType = mainArchiveableType;

			itemDictionary = new Dictionary<Guid, IArchiveItem>();
		}

		protected ArchiveSet(ArchiveItem mainItem)
		{
			MainArchiveItem = mainItem;
		}

		public IArchiveItem MainArchiveItem { get; private set; }

		public string MainArchiveItemNK { get; private set; }

		public ZQuery MainArchiveableTypeFilter { get; private set; }

		public Guid IgnoredPK { get; set; }

		public int Count
			=> itemDictionary.Count;

		public static int loadArchiveSetTimeout
			=> SystemDataRegistry.Instance.LoadArchiveSetTimeout.Value;

		public ReadOnlyCollection<IArchiveItem> GetArchiveItems()
		{
			if (itemCollection == null)
			{
				var itemList = new List<IArchiveItem>(itemDictionary.Values);
				itemCollection = new ReadOnlyCollection<IArchiveItem>(itemList);
			}

			return itemCollection;
		}

		public IArchiveItem GetArchiveItem(Guid itemPK)
		{
			_ = itemDictionary.TryGetValue(itemPK, out var item);
			return item;
		}

		[Serializable]
		public class ArchiveSetLoadException : Exception
		{
			public ArchiveSetLoadException(string errorMessage) : base(errorMessage)
			{ }

#if NETFRAMEWORK
			protected ArchiveSetLoadException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
			{ }
#endif
		}

		Guid mainItemFK;

		public Guid MainItemFK
		{
			get
			{
				FetchMainItemFKFromDB();
				ReportErrorIfMainItemFKNotFound();

				return mainItemFK;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Querying tables with no bizo")]
		void FetchMainItemFKFromDB()
		{
			Guid? result = null;
			var schedulePKString = ScheduleIdentifier != Guid.Empty
				? (NoResString)"AIM_S5_ParentSchedule = @SchedulePK" : (NoResString)"AIM_S5_ParentSchedule IS NULL";

			if (mainItemFK == Guid.Empty)
			{
				var sqlForFetchingForeignKeyValue = $@"SELECT TOP 1 AIM_PK FROM dbo.ArchiveMainItemQueue WHERE
					AIM_ParentID = @RootPK AND {schedulePKString} AND AIM_StageName = @StageName";

				using (var cmd = Db.Connection.Command(sqlForFetchingForeignKeyValue))
				{
					cmd.AddParameter("@RootPK", SqlDbType.UniqueIdentifier, MainArchiveItem.PK);

					if (ScheduleIdentifier != Guid.Empty)
					{
						cmd.AddParameter("@SchedulePK", SqlDbType.UniqueIdentifier, ScheduleIdentifier);
					}

					cmd.AddParameter("@StageName", SqlDbType.NVarChar, 125, StageName);

					result = cmd.ExecuteScalar() as Guid?;

					if (result.HasValue)
					{
						mainItemFK = result.Value;
					}
				}
			}
		}

		void ReportErrorIfMainItemFKNotFound()
		{
			if (mainItemFK == Guid.Empty)
			{
				var doesMainRecordExistInItsActualTable = DoesMainRecordStillExistInItsActualTable();
				var schedulePKString = ScheduleIdentifier != Guid.Empty
					? (NoResString)"AIM_S5_ParentSchedule = @SchedulePK" : (NoResString)"AIM_S5_ParentSchedule IS NULL";
				var exMessage = $@"The expected corresponding record in ArchiveMainItemQueue was not found.
					MainArchiveItem.PK was: '{MainArchiveItem.PK}'. MainArchiveItem Table was: '{MainArchiveItem.PKColumn.TableName}'. MainArchiveItem still exists in its table: {doesMainRecordExistInItsActualTable}.
					StageName: '{StageName}'. SchedulePK: '{ScheduleIdentifier}'. SchedulePKString: {schedulePKString}.";
				var ex = new Exception(exMessage);
				throw ex;
			}
		}

		public string DoesMainRecordStillExistInItsActualTable()
		{
			bool doesMainRecordExistInItsActualTable;
			try
			{
				doesMainRecordExistInItsActualTable = QueryMainTableToFindRecord();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				var exMessage = $"An exception was encountered while trying to determine if the top-level record being archived still exists: '{e.Message}'.";
				return exMessage;
			}

			return doesMainRecordExistInItsActualTable.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Querying tables which may not have a bizo")]
		protected virtual bool QueryMainTableToFindRecord()
		{
			var queryToFindRecordInActualTable = $"SELECT COUNT(*) FROM {MainArchiveItem.PKColumn.TableSchema.SqlSchemaName}.{MainArchiveItem.PKColumn.TableName}" +
				$" WHERE {MainArchiveItem.PKColumn.Name} = @MainArchiveItemPK";

			using (var cmd = Db.Connection.Command(queryToFindRecordInActualTable))
			{
				cmd.AddParameter("@MainArchiveItemPK", SqlDbType.UniqueIdentifier, MainArchiveItem.PK);
				return (cmd.ExecuteScalar() as int?).Value > 0;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Querying temp tables")]
		public virtual void LoadUnsafe(string suffix)
		{
			var resolver = new EnterpriseSchemaResolver();

			lock (this)
			{
				int returnValue = -1;

				var procedureName = SystemDescriptor?.Code == "HAR" ? "LoadArchiveSetHAR" : "LoadArchiveSet";

				using (DbCommand command = Db.Connection.Command(procedureName))
				{
					command.CommandType = System.Data.CommandType.StoredProcedure;
					command.AddParameter("@StartingArchiveableTypeName", System.Data.SqlDbType.NVarChar, 125, mainArchiveableType.PKColumn.TableName);
					command.AddParameter("@StartingArchiveableTableName", System.Data.SqlDbType.NVarChar, 125, MainArchiveItem.PKColumn.TableName);
					command.AddParameter("@StartingArchiveablePK", System.Data.SqlDbType.UniqueIdentifier, MainArchiveItem.PK);
					command.AddParameter("@MainArchiveableTypeFilter", System.Data.SqlDbType.NVarChar, -1, MainArchiveableTypeFilter.LiteralTextSqlFormatted);
					command.AddParameter("@MainArchiveableTypeName", System.Data.SqlDbType.NVarChar, 125, mainArchiveableType.PKColumn.TableName);
					command.AddParameter("@SystemSpecificSuffix", System.Data.SqlDbType.NVarChar, 125, suffix);
					command.AddParameter("@SchedulePK", System.Data.SqlDbType.UniqueIdentifier, ScheduleIdentifier);
					command.AddParameter("@StageName", System.Data.SqlDbType.NVarChar, 125, SanitizedStageName);
					command.AddParameter("@IgnoredPK", SqlDbType.UniqueIdentifier, Guid.Empty);
					command.GetParameter("@IgnoredPK").Direction = ParameterDirection.Output;
					command.CommandTimeout = loadArchiveSetTimeout * 60;

					returnValue = command.ExecuteProcedureWithReturnValue();
					IgnoredPK = (Guid)command.GetParameterValue("@IgnoredPK");
				}

				if (returnValue == 0)
				{
					try
					{
						var sqlForFetchFromRelatedItemQueue = $@"SELECT COUNT(*) FROM dbo.ArchiveRelatedItemQueue WHERE ARQ_AIM_MainItem = @MainItemPK;
SELECT ARQ_LoadedID as PK, ARQ_LoadedTableCode as TableCode,
ARQ_ParentTableCode as ParentTableCode, ARQ_ParentID as ParentPK, ARQ_IsReversed as IsReversed
FROM dbo.ArchiveRelatedItemQueue WHERE ARQ_AIM_MainItem = @MainItemPK;";
						var mainItemPK = MainItemFK;
						using (var command = Db.Connection.Command(sqlForFetchFromRelatedItemQueue))
						{
							command.AddParameter("@MainItemPK", System.Data.SqlDbType.UniqueIdentifier, mainItemPK);
							using (var reader = command.ExecuteReader())
							{
								if (reader.Read())
								{
									int count = reader.GetInt32(0);
									itemDictionary = new Dictionary<Guid, IArchiveItem>(count + 1);
									itemDictionary.Add(MainArchiveItem.PK, MainArchiveItem);
								}
								else
								{
									throw new ArchiveSetLoadException("count expect to be returned");
								}

								if (reader.NextResult())
								{
									while (reader.Read())
									{
										var pk = reader.GetGuid(reader.GetOrdinal("PK"));
										var tableCode = reader.GetString(reader.GetOrdinal("TableCode"));
										var parentPK = reader.GetGuid(reader.GetOrdinal("ParentPK"));
										var parentTableCode = reader.GetString(reader.GetOrdinal("ParentTableCode"));
										var isReversed = reader.GetBoolean(reader.GetOrdinal("IsReversed"));

										var schema = resolver.GetTableSchemaFromColumnNamePrefix(tableCode);
										var parentSchema = resolver.GetTableSchemaFromColumnNamePrefix(parentTableCode);
										var pkCol = schema.PK;
										var parentPkCol = parentSchema.PK;
										itemDictionary.Add(pk, new ArchiveItem(pkCol, pk, parentPkCol, parentPK, isReversed, tableCode));
									}
								}
								else
								{
									throw new ArchiveSetLoadException("2nd result set with archive items expected");
								}

								reader.Close();
							}
						}
					}
					catch (SqlException sqlEx) when (!sqlEx.IsCriticalException())
					{
						if (new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.TimeoutExpired)
						{
							ErrorReporter.ReportOnce("ArchiveSetItemRetrievalTimeout",
								$"Timed out while retrieving items from the ArchiveSet.");
						}

						throw;
					}
				}
				else if (returnValue == 2)
				{
					itemDictionary.Clear();
				}
			}
		}

		public virtual IArchiveStepResult Load(IArchiveLogger logger)
		{
			var stepResult = new ArchiveStepResult();
			itemCollection = null;

			var numberOftimesToTryBeforeGivingUp = 3;
			while (true)
			{
				string errorMessage;
				string errorKey;

				using (var transactionManager = Db.Connection.BeginTransactionWithManager())
				{
					try
					{
						LoadUnsafe(ArchiveManager.SystemSpecificSuffix);
						transactionManager.CommitTransaction();
						break;
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						try
						{
							transactionManager.RollbackTransaction();
						}
						catch (Exception rollbackException) when (!rollbackException.IsCriticalException())
						{
							errorMessage = rollbackException.Message;
							logger.LogAndReportError("ArchiveSetLoadRollbackException", SystemDescriptor.Code, errorMessage, rollbackException);
							stepResult.ErrorsEncountered.Add(errorMessage);

							break;
						}

						if (numberOftimesToTryBeforeGivingUp-- == 0 || IsDoNotRetryException(e))
						{
							itemDictionary.Clear();

							MarkToIgnoreForThisRun();

							if (IsSqlTimeoutException(e))
							{
								var humanReadableNameWithFallback = MainArchiveItem.HumanReadableName.IsNullOrEmpty() ? MainArchiveItem.PKColumn.TableName : MainArchiveItem.HumanReadableName;

								errorKey = "ArchiveSetLoadTimeout";
								errorMessage = $"Timed out while loading Archive Set. MainArchiveableTypeName was '{mainArchiveableType.PKColumn.TableName}'. "
									+ $"MainArchiveableTypeFilter was '{MainArchiveableTypeFilter.LiteralTextSqlFormatted}'. The main item was '{humanReadableNameWithFallback}' with PK '{MainArchiveItem.PK}'. "
									+ $"A summary of items that were successfully retrieved before timeout: {GetSummaryOfAllItemsRetrievedBeforeTimeout()}";
							}
							else if (IsMainRecordMissingException(e))
							{
								errorKey = "ArchiveQueueMainRecordNotFound";
								errorMessage = e.Message;
							}
							else if (IsLockRequestTimeoutException(e))
							{
								errorKey = "ArchiveSetLockRequestTimeoutPeriodExceeded";
								errorMessage = GetLockRequestTimeoutExceptionMessage(e);
							}
							else
							{
								errorKey = "ArchiveSetLoadException";
								errorMessage = $"Unable to load ArchiveSet for main record type: {MainArchiveItem.PKColumn.TableName}, table: {mainArchiveableType.PKColumn.TableName}, pk: {MainArchiveItem.PK}\n"
									+ $"This set will be skipped. Full error below:\n\n{e}";
							}

							logger.LogAndReportError(errorKey, SystemDescriptor.Code, errorMessage, e);
							stepResult.ErrorsEncountered.Add(errorMessage);
							break;
						}
					}
				}
			}

			return stepResult;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Archive Queues Have No Associated BizO")]
		public string GetSummaryOfAllItemsRetrievedBeforeTimeout()
		{
			var dictOfTypenameToCount = new Dictionary<string, int>();
			var sql = @"SELECT ARQ_TypeName as TypeName
FROM dbo.ArchiveRelatedItemQueue WHERE ARQ_AIM_MainItem = @MainItemFK";

			try
			{
				using (var cmd = Db.Connection.Command(sql))
				{
					_ = cmd.AddParameter("@MainItemFK", SqlDbType.UniqueIdentifier, MainItemFK);

					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var typeName = reader.GetString(0);

							if (dictOfTypenameToCount.TryGetValue(typeName, out var value))
							{
								dictOfTypenameToCount[typeName] = ++value;
							}
							else
							{
								dictOfTypenameToCount[typeName] = 1;
							}
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex.Message;
			}

			var sb = new StringBuilder();

			foreach (var kvp in dictOfTypenameToCount.ToArray())
			{
				_ = sb.AppendLine($"{kvp.Key}: {kvp.Value}");
			}

			return sb.ToString();
		}

		public void MarkToIgnoreForThisRun()
			=> ArchiveTableHelper.MarkPKAsIgnored(MainArchiveItem.PK);

		public ReadOnlyDictionary<string, ITableProcessingInfo> GetTotalDocumentsDeletedFromArchiveItems()
		{
			var totalDocumentsDeleted = new Dictionary<string, ITableProcessingInfo>();
			var itemsWithDocumentsDeleted = GetArchiveItems().Where(item => item.TotalDocumentsDeleted > 0);

			if (itemsWithDocumentsDeleted.Any())
			{
				var totalStorageDocsDeleted = itemsWithDocumentsDeleted.Sum(item => item.TotalDocumentsDeleted);
				var totalStorageMainsDeleted = itemsWithDocumentsDeleted.Count();

				totalDocumentsDeleted.Add(StorageDocsSchema.Constants.TableName, new TableProcessingInfo(totalStorageDocsDeleted));
				totalDocumentsDeleted.Add(StorageMainSchema.Constants.TableName, new TableProcessingInfo(totalStorageMainsDeleted));
			}

			this.totalDocumentsDeleted = new ReadOnlyDictionary<string, ITableProcessingInfo>(totalDocumentsDeleted);
			return this.totalDocumentsDeleted;
		}

		ReadOnlyDictionary<string, ITableProcessingInfo> totalDocumentsDeleted;

		ReadOnlyCollection<IArchiveItem> itemCollection;
		protected Dictionary<Guid, IArchiveItem> itemDictionary;
		public readonly ArchiveableType mainArchiveableType;

		public int TotalNumberOfDocumentsGeneratedInSet { get; set; }

		public long TimeTakenToDelete { get; set; }
		public long TimeTakenToGenerateDocuments { get; set; }
		public long TimeTakenToDeleteAllDocuments { get; set; }
	}
}
