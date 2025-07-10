using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.AlwaysOnHelper;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public delegate void NotificationEvent(string message);
	public delegate void MergeProgressEvent(DbMergeInfoCollection dbInfoCollection, int percentageComplete);

	public class DocumentDbMerger : NonPersistentBusinessObject, IObsoleteValidation, ILogger
	{
		public DocumentDbMerger(DocumentFactory factory, ILowPriorityProcessPauserFactory priorityProcessPauserFactory = null)
			: base(factory)
		{
			this.priorityProcessPauserFactory = priorityProcessPauserFactory ?? new LowPriorityProcessPauserFactory();
		}

		public void Initialise(ISynchronizeInvoke syncInvoke)
		{
			GetDbMergeInfoCollectionAndRefreshUserFeedback();
			this.syncInvoke = syncInvoke;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		public void GetDbMergeInfoCollectionAndRefreshUserFeedback()
		{
			try
			{
				storageDbSizes = GetDbMergeInfoCollection(((IDbConnected)Factory).Connection);
				FireRefreshMergeProgress();
			}
			catch (SqlException ex)
			{
				var dbError = new DbErrorMatch(ex);

				if (dbError.ExceptionType == DbErrorType.DatabaseInSingleUserModeAndAlreadyOpen)
				{
					FireOnProcessFailed(Res.GetString("5dda2334-0f20-4352-b184-972764a75c13", "Please correct any errors and try again.\r\n{0}", ex.Message));
				}
				else if (dbError.ExceptionType != DbErrorType.InvalidObjectName)
				{
					throw;
				}
			}
		}

		public void Stop()
		{
			stopMerge = true;
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		public void Run()
		{
			using (var semaphoreHandle = EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(new DocDbManagerSemaphore()))
			{
				if (semaphoreHandle.Success)
				{
					StartMerge();
				}
				else
				{
					var message = Res.GetString(
						"da79b888-d881-49d7-9f19-f3e84cb62c75",
						"Cannot merge databases now, as {0} is currently making changes to the eDocs databases.",
						DocDbManagerSemaphore.GetUserHoldingSemaphore(semaphoreHandle, new BusinessObjectFactory()));
					FireOnNoMergeRequired(message);
				}
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		void StartMerge()
		{
			stopMerge = false;
			ShowMessage(Res.GetString("191be3da-74d1-46de-a70f-982fbf71e756", "DocManager database merge started."));

			try
			{
				var firstWritableDbWithFreeSpace = MergeDatabases();

				if (stopMerge)
				{
					FireOnProcessStopped(Res.GetString("a7cf0346-3eab-4a9d-aaab-21aad59dce26", "Merge was stopped by user."));
				}
				else
				{
					FireOnNoMergeRequired(Res.GetString("07441e2e-7c82-4805-bad1-f73d8539cb68", "Merge has been completed successfully."));

					if (firstWritableDbWithFreeSpace > 1)
					{
						var dbHelper = new DocManagerDBHelper();
						var warningMessage = Res.GetString("c4532f1e-a7fa-478e-884f-c793734b018d",
							@"Last writable database is {0}.
Asterisk(*) indicate read-only database.
Databases that are read-only or larger than or equal to the threshold size set in the Registry ( {1} ) will not be merged.", dbHelper.GetDatabaseName(firstWritableDbWithFreeSpace), ((IMultilingualRegistryItem)SystemDataRegistry.Instance.DocManagerDataFileSizeThresholdGb).LocationMultilingual);
						FireOnNoMergeRequired(string.Empty);
						FireOnNoMergeRequired(warningMessage);
					}
				}
			}
			catch (Exception ex)
			{
				if (!(ex is SqlException || ex is DocDbManagerException || ex is BacklogWaiterTimeoutException))
				{
					throw;
				}

				FireOnProcessFailed(Res.GetString("67fe82b8-4f73-446a-bec7-9b46925e7ecd", "Merge has stopped with an error, please fix it and try again.\r\n{0}", ex.Message));
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		protected internal virtual int MergeDatabases()
		{
			var pauserTimeout = TimeSpan.FromMinutes(5);
			using (var connection = Db.NewAdminConnection())
			{
				initialQuantityMbsNecessaryToMove = 0;
				storageDbSizes = GetDbMergeInfoCollection(connection);
				FireRefreshMergeProgress();
				initialQuantityMbsNecessaryToMove = QuantityMbsNecessaryToMove(storageDbSizes);

				ILowPriorityProcessPauser sourcePauser = null;
				ILowPriorityProcessPauser destinationPauser = null;

				while (!stopMerge && storageDbSizes.CountWritableWithFreeSpace > 1)
				{
					var sourceDatabase = storageDbSizes.LastWritableDatabase;

					if (sourceDatabase.Number >= lastDroppedDbNumber)
					{
						if (sourceDatabase.Number < lastSkippedDbNumber)
						{
							ShowMessage(Res.GetString("a835c392-75d6-4ed0-bc7c-06e5d079b539", "Skipped further merging of database [{0}] as it appears to be in use", sourceDatabase.Number));
							lastSkippedDbNumber = sourceDatabase.Number;
						}
						sourceDatabase = storageDbSizes.GetLastWritableDatabaseBeforeDbNumber(lastDroppedDbNumber);
					}

					sourcePauser = priorityProcessPauserFactory.Create(new IBacklogInfoProvider[]
					{
						new LogFullnessProvider(sourceDatabase.Name),
						new AlwaysOnDelayProvider(sourceDatabase.Name),
					});

					var destinationDatabase = storageDbSizes.FirstWritableDbWithFreeSpace;
					if (sourceDatabase.Number <= destinationDatabase.Number)
					{
						break;
					}

					destinationPauser = priorityProcessPauserFactory.Create(new IBacklogInfoProvider[]
					{
						new LogFullnessProvider(destinationDatabase.Name),
						new AlwaysOnDelayProvider(destinationDatabase.Name),
					});

					sourcePauser.Wait(this, pauserTimeout);
					destinationPauser.Wait(this, pauserTimeout);

					MoveDocumentsOrDropEmptySourceDb(connection, sourceDatabase, destinationDatabase);
					FireRefreshMergeProgress();
				}

				return storageDbSizes.FirstWritableDbWithFreeSpace.Number;
			}
		}
		DbMergeInfoCollection storageDbSizes;

		/// <summary>
		/// A StorageDocs database is regarded as empty if it has no documents linked to StorageMain.
		/// </summary>
		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		void DropEmptySourceDb(AdminConnection connection, DbMergeInfo sourceDb, out bool isSourceDbEmpty)
		{
			isSourceDbEmpty = false;
			try
			{
				using (Db.DisposableActionForDbConnection())
				{
					connection.AlterDbWriteableStateForDocManager(sourceDb.Name, false);
				}

				if (GetDbDocumentGroupCount(connection, sourceDb.Number) == 0)
				{
					isSourceDbEmpty = true;
					if (AlwaysOn.IsDbPartOfAlwaysOn(connection, sourceDb.Name))
					{
						var lockResult = AlwaysOnHelper.RunActionWithAlwaysOnLock(connection, _ =>
						{
							ShowMessage(Res.GetString("88b3b3ee-a890-469f-82b3-39d27eceef06", "Removing database [{0}] from the AlwaysOn setup.", sourceDb.Name));
							var alwaysOnReplicas = AlwaysOnHelper.GetAlwaysOnSecondaryReplicaNames(sourceDb.Name, useCache: false);
							if (!AlwaysOn.RemoveDatabaseFromAlwaysOnSetup(sourceDb.Name))
							{
								ShowMessage(Res.GetString("968a4abf-8a6a-4c4b-aa0e-0531baf93795", "Database [{0}] cannot be removed from AlwaysOn setup", sourceDb.Number));
								return;
							}

							foreach (var replicaName in alwaysOnReplicas)
							{
								ShowMessage(Res.GetString("dae63738-9604-4b53-9b70-dfb0eb8340bd", "Dropping database [{0}] on AlwaysOn replica [{1}]", sourceDb.Number, replicaName));

								using (var connectionToReplica = Db.NewAdminConnection(replicaName, Db.SqlMasterDb))
								{
									while (true)
									{
										if (stopMerge)
										{
											return;
										}

										if (IsDatabaseReadyToDrop(connectionToReplica, sourceDb.Name))
										{
											DropDbOnSecondaryReplica(connectionToReplica, sourceDb.Number);
											break;
										}

										Thread.Sleep(3000);
									}
								}
							}
							DropDatabaseOnPrimary(sourceDb.Number);
						});

						if (lockResult != LockedProcessResult.Completed)
						{
							throw new FailedToObtainAppLockException(sourceDb.Name, lockResult);
						}
					}
					else
					{
						DropDatabaseOnPrimary(sourceDb.Number);
					}
				}
			}
			finally
			{
				using (Db.DisposableActionForDbConnection())
				{
					if (connection.DatabaseExists(sourceDb.Name))
					{
						connection.AlterDbWriteableStateForDocManager(sourceDb.Name, true);
					}
				}
			}

			void DropDatabaseOnPrimary(int dbNumber)
			{
				ShowMessage(Res.GetString("493bc471-b3aa-45e5-bfc6-2199edbd0f6a", "Dropping database [{0}] on server [{1}]", sourceDb.Number, Db.ServerName));
				DropDbWithLock(connection, dbNumber);
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		void MoveDocumentsOrDropEmptySourceDb(AdminConnection connection, DbMergeInfo sourceDb, DbMergeInfo destinationDb)
		{
			DropEmptySourceDb(connection, sourceDb, out var isSourceDbEmpty);

			if (!isSourceDbEmpty)
			{
				MoveDocuments(connection, sourceDb, destinationDb);
				if (storageDbSizes.UpdateInternalAttributesSuccessfully(sourceDb, destinationDb, MaxDbSizeMb))
				{
					return;
				}
			}

			storageDbSizes = GetDbMergeInfoCollection(connection);
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		protected internal virtual bool IsDatabaseReadyToDrop(DbConnection connection, string dbName)
		{
			try
			{
				var dbDescription = connection.DatabaseStateDescription(dbName);
				return string.IsNullOrWhiteSpace(dbDescription) || dbDescription == "RESTORING";
			}
			catch (InvalidOperationException ex)
			{
				FireOnProcessFailed(Res.GetString("736D2869-24CD-42EA-B0DC-1D04D96E2D7D", "Cannot open connection when retrieving database status. Database name: {0}, Error message: {1}", dbName, ex.Message));
				return true;
			}
			catch (NullReferenceException ex)
			{
				FireOnProcessFailed(Res.GetString("736D2869-24CD-42EA-B0DC-1D04D96E2D7D", "Cannot open connection when retrieving database status. Database name: {0}, Error message: {1}", dbName, ex.Message));
				return true;
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		void MoveDocuments(DbConnection connection, DbMergeInfo sourceDb, DbMergeInfo destinationDb)
		{
			if (lastMovedFromDbNumber != sourceDb.Number || lastMovedToDbNumber != destinationDb.Number)
			{
				lastMovedFromDbNumber = sourceDb.Number;
				lastMovedToDbNumber = destinationDb.Number;
				ShowMessage(Res.GetString("7e4f16ac-d059-4b3f-8143-966f8ddd4c15", "Moving documents from database [{0}] to database [{1}]", lastMovedFromDbNumber, lastMovedToDbNumber));
			}

			var sqlText = $@"EXEC ep_StorageDocsMerge_MoveDocumentGroup {lastMovedFromDbNumber}, {lastMovedToDbNumber}";

			using (var transactionManager = connection.BeginTransactionWithManager())
			{
				connection.ExecuteNonQuery(sqlText);
				transactionManager.CommitTransaction();
			}
		}

		int GetDbDocumentGroupCount(DbConnection connection, int dbNumber)
		{
			var sqlText = $"SELECT count(*) FROM {StorageMainSchema.Constants.SqlSchemaName}.{StorageMainSchema.Constants.TableName} WHERE {StorageMainSchema.Constants.SM_DB} = {dbNumber}";
			var count = Convert.ToInt32(connection.ExecuteScalar(sqlText));
			return count;
		}

		protected internal virtual void DropDbWithLock(DbConnection connection, int dbNumber)
		{
			using (var anotherConn = Db.NewExtraConnectionToMainDb())
			using (anotherConn.BeginTransactionWithManager())
			{
				anotherConn.ExecuteNonQuery(FormattableString.Invariant($"SELECT count(*) FROM dbo.StorageMain WITH (UPDLOCK, SERIALIZABLE) WHERE SM_DB = {dbNumber}"));
				DropDb(connection, dbNumber);
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		protected virtual void DropDbOnSecondaryReplica(DbConnection connection, int dbNumber)
		{
			const string sqlText = @"
SET nocount ON

DECLARE @DbName varchar(128)
DECLARE @SqlText nvarchar(1000)

SET @DbName =
	@MainDbName + '_SD' +
	replicate('0', 3 - len(convert(varchar(3), @DbNumber))) +
	+ convert(varchar(3), @DbNumber)

IF (NOT EXISTS(SELECT null FROM sys.databases WHERE name = @DbName))
BEGIN
	SELECT -1
END
ELSE
BEGIN
	SET @SqlText = 'DROP DATABASE [' + @DbName + ']'
	EXEC sp_executesql @SqlText
	SELECT 0
END
";

			lastDroppedDbNumber = dbNumber;

			try
			{
				using (connection.UseMasterDb())
				{
					var status = connection.ExecuteScalar(sqlText, command =>
					{
						command.AddParameter("@MainDbName", SqlDbType.VarChar, 128, Db.DatabaseName);
						command.AddParameter("@DbNumber", SqlDbType.Int, dbNumber);
					});

					ShowMessage(GetResultMessageFromDropDbReturnStatus(status, dbNumber, connection.ServerName));
				}
			}
			catch (SqlException ex)
			{
				throw new DocDbManagerException($"Failed to drop database [{dbNumber}] on server [{connection.ServerName}]. Details: {ex.Message}", ex);
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI009:ServiceTaskLogsInEnglishOnlyRule", Justification = "It is not a service task")]
		protected virtual void DropDb(DbConnection connection, int dbNumber)
		{
			var sqlText = $@"DECLARE @return_status int;
EXEC @return_status = ep_StorageDocsMerge_DropEmptyDb {dbNumber};
SELECT @return_status;";

			lastDroppedDbNumber = dbNumber;

			try
			{
				var status = connection.ExecuteScalar(sqlText);
				ShowMessage(GetResultMessageFromDropDbReturnStatus(status, dbNumber, connection.ServerName));
			}
			catch (SqlException ex)
			{
				throw new DocDbManagerException($"Failed to drop database [{dbNumber}] on server [{connection.ServerName}]. Details: {ex.Message}", ex);
			}
		}

		string GetResultMessageFromDropDbReturnStatus(object status, int dbNumber, string serverName)
		{
			int resultStatus;
			string resultMessage;

			if (status == null || status == DBNull.Value || (resultStatus = (int)status) == 0)
			{
				resultMessage = Res.GetString("ad41eaaf-d9f3-4f7e-984d-980f6a223fad", "Empty database [{0}] on server [{1}] has been dropped", dbNumber, serverName);
			}
			else if (resultStatus == 1)
			{
				resultMessage = Res.GetString("d0478272-5aa7-420d-8eae-4e2f131863c7", "Cannot drop database [{0}] on server [{1}] - it is not empty", dbNumber, serverName);
			}
			else if (resultStatus == -1)
			{
				resultMessage = Res.GetString("3228f5a2-4301-46fb-bc95-9cb4cd3d6250", "Cannot drop database [{0}] on server [{1}] - it does not exist", dbNumber, serverName);
			}
			else
			{
				resultMessage = Res.GetString("fde87f6f-fe24-4131-a26d-5de7dbefec31", "Dropping database [{0}] on server [{1}] - unknown status", dbNumber, serverName);
			}

			return resultMessage;
		}

#if DEBUG
		public
#else
		internal
#endif
		int QuantityMbsNecessaryToMove(DbMergeInfoCollection dbInfoArray)
		{
			var quantity = 0;
			var sizes = new List<int>();
			var maxDbSizeMb = MaxDbSizeMb;

			foreach (DbMergeInfo dbInfo in dbInfoArray)
			{
				if (!dbInfo.IsReadOnly && dbInfo.Number < lastDroppedDbNumber)
				{
					sizes.Add(dbInfo.Size);
				}
			}

			var lowIndex = 0;
			var highIndex = sizes.Count - 1;
			var freeDbCount = sizes.Count(size => size < MaxDbSizeMb);

			while (lowIndex < highIndex && freeDbCount > 1)
			{
				if (sizes[lowIndex] < maxDbSizeMb)
				{
					var freeSpace = maxDbSizeMb - sizes[lowIndex];

					if (freeSpace >= sizes[highIndex])
					{
						sizes[lowIndex] += sizes[highIndex];
						quantity += sizes[highIndex];
						sizes.RemoveAt(sizes.Count - 1);
						--highIndex;
						--freeDbCount;
					}
					else
					{
						sizes[lowIndex] = maxDbSizeMb;

						if (sizes[highIndex] < maxDbSizeMb + freeSpace)
						{
							++freeDbCount;
						}

						sizes[highIndex] -= freeSpace;
						quantity += freeSpace;
					}
				}
				else
				{
					++lowIndex;
					--freeDbCount;
				}
			}

			return quantity;
		}

		DbMergeInfoCollection GetDbMergeInfoCollection(DbConnection connection) => DbMergeInfoCollection.New(connection, MaxDbSizeMb);

		#region Firing Events

		void ShowMessage(string message)
		{
			if (OnShowMessage != null)
			{
				if (syncInvoke == null)
				{
					OnShowMessage(message);
				}
				else
				{
					syncInvoke.BeginInvoke(new NotificationEvent(OnShowMessage), new object[] { message });
				}
			}
		}

		void FireRefreshMergeProgress()
		{
			if (OnRefreshMergeProgress != null)
			{
				var percent = GetPercentageComplete(storageDbSizes);

				if (syncInvoke == null)
				{
					OnRefreshMergeProgress(storageDbSizes, percent);
				}
				else
				{
					syncInvoke.BeginInvoke(new MergeProgressEvent(OnRefreshMergeProgress), new object[] { storageDbSizes, percent });
				}
			}
		}

		int GetPercentageComplete(DbMergeInfoCollection dbInfoArray)
		{
			var percent = 0;

			if (initialQuantityMbsNecessaryToMove > 0)
			{
				var currentQuantityMbsNecessaryToMove = QuantityMbsNecessaryToMove(dbInfoArray);
				var mbsMovedSoFar = initialQuantityMbsNecessaryToMove - currentQuantityMbsNecessaryToMove;
				percent = (int)((double)mbsMovedSoFar / initialQuantityMbsNecessaryToMove * 100);
				percent = percent < 0 ? 0 : percent > 100 ? 100 : percent;
			}

			return percent;
		}

		void FireOnProcessStopped(string message)
		{
			if (OnProcessStopped != null)
			{
				if (syncInvoke == null)
				{
					OnProcessStopped(message);
				}
				else
				{
					syncInvoke.BeginInvoke(new NotificationEvent(OnProcessStopped), new object[] { message });
				}
			}
		}

		void FireOnProcessFailed(string message)
		{
			if (OnProcessFailed != null)
			{
				if (syncInvoke == null)
				{
					OnProcessFailed(message);
				}
				else
				{
					syncInvoke.BeginInvoke(new NotificationEvent(OnProcessFailed), new object[] { message });
				}
			}
		}

		void FireOnNoMergeRequired(string message)
		{
			if (OnNoMergeRequired != null)
			{
				if (syncInvoke == null)
				{
					OnNoMergeRequired(message);
				}
				else
				{
					syncInvoke.BeginInvoke(new NotificationEvent(OnNoMergeRequired), new object[] { message });
				}
			}
		}

		void ILogger.Log(LogType type, string message)
		{
			if (type != LogType.Debug)
			{
				ShowMessage($"{type}: {message}");
			}
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			if (type != LogType.Debug)
			{
				ShowMessage($@"{type}: {message}.
{ex}");
			}
		}

		#endregion

#if DEBUG
		virtual
#endif
		public int MaxDbSizeMb
		{
			get
			{
				return SystemDataRegistry.Instance.DocManagerDataFileSizeThresholdGb.Value * 1024;
			}
		}

		ISynchronizeInvoke syncInvoke;
		readonly ILowPriorityProcessPauserFactory priorityProcessPauserFactory;
		int initialQuantityMbsNecessaryToMove;
		volatile bool stopMerge;

		int lastMovedFromDbNumber;
		int lastMovedToDbNumber;
		int lastSkippedDbNumber = int.MaxValue;
		int lastDroppedDbNumber = int.MaxValue;

		public event NotificationEvent OnShowMessage;
		public event NotificationEvent OnProcessFailed;
		public event NotificationEvent OnProcessStopped;
		public event NotificationEvent OnNoMergeRequired;
		public event MergeProgressEvent OnRefreshMergeProgress;
	}
}
