using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business
{
	public enum DbWriteableState
	{
		NonExistent,
		Writeable,
		ReadOnly
	}

	public class DocManagerDBHelper : IDocManagerDBHelper
	{
		public const int InitialStorageDocsDatabaseNumber = 1;

		public string GetTableNameWithDatabasePrefix(int dbNumber, string tableName)
		{
			return new DocManagerDBHelper().GetDatabaseName(dbNumber) + ".dbo." + tableName;
		}

		public string GetDatabaseName(int databaseNumber)
		{
			return GetDatabaseNameFromNumber(databaseNumber);
		}

		public static string GetDatabaseNameFromNumber(int databaseNumber)
		{
			return (databaseNumber == 0) ? Db.DatabaseName.Trim() : StorageDocsDatabaseRootName + GetDBNumberAsString(databaseNumber);
		}

		public IEnumerable<int> GetStorageDocDbNumbersIncludingMainDb()
		{
			var result = GetStorageDocDbNumbers().ToList();
			result.Add(0);
			return result;
		}

		IEnumerable<int> GetStorageDocDbNumbers() => Db.Connection.GetDatabases(DatabaseType.SD).Select(n => GetDatabaseNumber(n));

		public static int GetDatabaseNumber(string dbName) => int.Parse(dbName.Substring(dbName.Length - 3));

		public string[] GetDBNamesMissing()
		{
			var result = new List<int>();
			var command = Db.Connection.Command("SELECT DISTINCT (" + StorageMainSchema.Constants.SM_DB + ") FROM " + StorageMainSchema.Constants.SqlSchemaName + "." + StorageMainSchema.Constants.TableName + " "); // there is no Z equivalent for a select distinct(...)
			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader.GetInt32(0));
				}
			}

			return result.Except(GetStorageDocDbNumbersIncludingMainDb())
				.Select(referencedDB => StorageDocsDatabaseRootName + referencedDB.ToString().PadLeft(3, '0')).ToArray();
		}

		IEnumerable<int> GetUncreatedStorageDocDbNumbers()
		{
			var exists = GetStorageDocDbNumbers().ToHashSet();

			for (var i = 1; i < HighestSDNumber; ++i)
			{
				if (!exists.Contains(i))
				{
					yield return i;
				}
			}
		}

		/// <summary>
		/// Return the number of the StorageDocuments database with free space.
		/// If no appropriate databases found, returns the number of the newly created one.
		/// If customer chooses external storage, it will always return 1 because corresponding service task
		/// </summary>
		public int LastWritableDatabaseWithFreeSpace(int sizeRequiredInMB = 0, bool forceNewDatabase = false, StringBuilder builder = null)
		{
			int[] numbersToTry = null;
			var createInitialDB = false;

			if (SystemDataRegistry.Instance.EDocsStorageProvider.Value != Core.Constants.EDocsStorageProviders.Code.DB)
			{
				if (DatabaseExists(InitialStorageDocsDatabaseNumber))
				{
					return InitialStorageDocsDatabaseNumber;
				}

				createInitialDB = true;
			}

			var counter = 0;
			var counter_max = 12;

			while (counter < counter_max)
			{
				var freeDbNumber = 0;
				var backupDbNumber = 0;

				if (!createInitialDB)
				{
					var dbNumbers = GetStorageDocDbNumbers().OrderByDescending(x => x).ToList();

					foreach (int dbNumber in dbNumbers)
					{
						try
						{
							if (GetDbWriteableState(dbNumber) == DbWriteableState.Writeable)
							{
								backupDbNumber = Math.Max(backupDbNumber, dbNumber);
								var dbSizeMb = GetDatabaseSizeMB(dbNumber);

								if (dbSizeMb > 0 && dbSizeMb + sizeRequiredInMB < DatabaseFileSizeThresholdInMb)
								{
									freeDbNumber = dbNumber;
									break;
								}
							}
						}
						catch (SqlException ex)
						{
							builder?.AppendLine(string.Format(CultureInfo.InvariantCulture, (NoResString)"SqlException occurred while trying to access database {0}.", GetDatabaseName(dbNumber)))
								.AppendLine(ex.ToString());
						}
					}
				}

				//only try to make a new database if there's absolutely no writable database or if we explicitly are trying to make one
				if (!(forceNewDatabase && freeDbNumber == 0) && backupDbNumber != 0)
				{
					return freeDbNumber > 0 ? freeDbNumber : backupDbNumber;
				}

				var number = -1;
				int numberToTry;

				try
				{
					if (numbersToTry == null)
					{
						numbersToTry = createInitialDB ? new[] { InitialStorageDocsDatabaseNumber } : GetUncreatedStorageDocDbNumbers().Take(5).ToArray();
					}

					numberToTry = numbersToTry[0];

					if (counter >= 8)
					{
						//for later attempts, try our higher numbers - but don't try to try OoB (1000+)
						numberToTry = numbersToTry[Math.Min(numbersToTry.Length - 1, counter - 7)];
					}
				}
				catch (IndexOutOfRangeException)
				{
					//we've exhausted all SDs from 000 to 999 - game over - must be handled specially since most functions will throw OdysseyException for 1000+
					numbersToTry = new[] { HighestSDNumber };
					break;
				}

				try
				{
					number = CreateDatabase(numberToTry);
				}
				catch (CreateDocManagerDatabaseException e)
				{
					//even in case of deadlock/file already exists, try again, possibly with higher number
					if (counter >= counter_max - 1)
					{
						throw;
					}

					builder?.AppendLine(Res.GetString("9f5d695e-eaf2-4e0b-9172-b7541346d429", "Exception occurred while trying to make database number {0}, but attempted continued afterwards:", numberToTry));
					builder?.AppendLine(e.ToString());
				}

				if (number == -1)
				{
					counter++;
					Thread.Sleep(counter > 4 ? 250 : 100);
				}
				else
				{
					return number;
				}
			}

			var firstNumber = numbersToTry.Any() ? numbersToTry.First() : HighestSDNumber;

			if (firstNumber >= HighestSDNumber)
			{
				throw new CanNotCreateSDDatabaseException(Res.GetString("6c6c72e0-45fb-4db9-a53f-5becf48ea0e9",
@"Database with Number = {0} cannot be created as it exceeded maximum number of databases. Please contact your system administrator.", firstNumber));
			}

			var message = Res.GetString("F225FF4A-536D-442E-A1B3-35CB89485F29", @"Databases with Numbers = ({0}) cannot be created. Please try in few minutes or contact your system administrator.
Last Error(s):
{1}
Additional information:
'Count of document databases': {2};
{3}
Create DocManager Database: {4};
DB log File Path: {5};
Db Data File Path: {6};",
				numbersToTry.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y),//0
				string.Join("\r\n\r\n", GetLastCreationExceptionArray().ToArray()),//1
				GetStorageDocDbNumbers().Count(),//2
				string.Join("\r\n", numbersToTry.Select(GetDataBaseInfo)),//3
				createDocManagerDatabase,//4
				SystemDataRegistry.Instance.DocManagerDBLogFilePath.Value,//5
				SystemDataRegistry.Instance.DocManagerDBDataFilePath.Value//6
				);

			throw string.IsNullOrEmpty(cannotAcquireLockMessage)
				? new CanNotCreateSDDatabaseException(message)
				: new CanNotAcquireLockForCreatingSDDatabaseException(message);
		}

		string GetDataBaseInfo(int num)
		{
			var name = GetDatabaseName(num);
			var exist = Db.Connection.DatabaseExists(name);
			var message = Res.GetString("2faec441-007b-4574-a39b-711a15641068", "{0} Exists: {1}", name, exist);
			if (exist)
			{
				var filePath = Db.Connection.GetDBDataFiles(name);
				message += Res.GetString("7b69b356-7f28-4cf9-93d6-96fc8b6cea5d", "Files: {0}",
					string.Join("\r\n\t", filePath.Select(f =>
					{
						var fileInfo = new FileInfo(filePath.FirstOrDefault());
						return Res.GetString("b6494924-31b4-4dd3-920b-ca4f09c1dbb2", "{0} Create Time: {1}", fileInfo.FullName, fileInfo.CreationTime);
					})));//0
			}
			return message;
		}

		public string GetLastWritableDatabaseName()
		{
			return GetDatabaseName(LastWritableDatabaseWithFreeSpace());
		}

		internal List<string> GetLastCreationExceptionArray()
		{
			return ConstructExceptionArray(LastCreationException);
		}

		internal Exception LastCreationException;
		string cannotAcquireLockMessage;

		List<string> ConstructExceptionArray(Exception ex)
		{
			List<string> errors = new List<string>();
			while (ex != null)
			{
				errors.Add(ex.Message);
				ex = ex.InnerException;
			}

			if (errors.Count == 0 && !string.IsNullOrEmpty(cannotAcquireLockMessage))
			{
				errors.Add(cannotAcquireLockMessage);
			}

			return errors;
		}

		public DbWriteableState GetDbWriteableState(int dbNumber)
		{
			var dbName = GetDatabaseName(dbNumber);
			if (!Db.Connection.DatabaseExists(dbName))
			{
				return DbWriteableState.NonExistent;
			}

			return DocManagerUtils.IsDbWriteableForDocManager(GetDatabaseName(dbNumber))
				? DbWriteableState.Writeable
				: DbWriteableState.ReadOnly;
		}

		internal virtual int GetDatabaseSizeMB(int dbNumber)
		{
			string dbName = GetDatabaseName(dbNumber);
			DbCommand command = Db.Connection.Command("ep_DbUsedSize");
			command.CommandType = CommandType.StoredProcedure;
			command.AddParameter("@DbName", SqlDbType.VarChar, 128, dbName);
			command.AddOutputParameter("@DbSizeMb", SqlDbType.Int, 0, 0, 0, 0);
			command.ExecuteNonQuery();
			return (int)command.GetParameterValue("@DbSizeMb");
		}

		protected virtual int DatabaseFileSizeThresholdInMb
		{
			get { return SystemDataRegistry.Instance.DocManagerDataFileSizeThresholdGb.Value * 1024; }
		}

		public bool DatabaseExists(int dBNumber)
		{
			string dBName = GetDatabaseName(dBNumber);

			return Db.Connection.DatabaseExists(dBName);
		}

		#region Implementation

		protected static string StorageDocsDatabaseRootName
		{
			get
			{
				if (storageDocsDatabaseRootName == null)
				{
					if (string.IsNullOrWhiteSpace(Db.DatabaseName))
					{
						throw new Exception("Invalid StorageDocsDatabaseRootName. Main DB name is blank.");
					}

					storageDocsDatabaseRootName = Db.DatabaseName.Trim() + "_SD";
				}

				return storageDocsDatabaseRootName;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Baseline")]
		static string storageDocsDatabaseRootName;

		const int HighestSDNumber = 1000;

		static string GetDBNumberAsString(int databaseNumber)
		{
			if (databaseNumber >= 0 && databaseNumber < HighestSDNumber)
			{
				return string.Format("{0:000}", databaseNumber);
			}

			throw new OdysseyException("Database number out of bounds; possibly run out of DBs. DBNumber: " + databaseNumber);
		}

		// Returns the number of the newly created Storage Documents database.
		protected int CreateDatabase(int dBNumber)
		{
			if (!(dBNumber >= 1 && dBNumber < HighestSDNumber))
			{
				var ex = new OdysseyException("Exceeded maximum number of databases. Request <" + dBNumber.ToString() + ">");
				LastCreationException = ex;
				throw ex;
			}

			var databaseName = GetDatabaseName(dBNumber);

			if (!CreateDatabase_Raw(databaseName))
			{
				return -1;
			}

			if (!SkipRefreshDbReaderRolePermissionsForTest)
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					var security = new DbSecurity();
					security.RefreshDbReaderRoleOnDatabase(adminConnection, ((ICurrentDbControl)adminConnection).InitialDatabase.Trim(), databaseName, false, _ => { });
				}
			}

			return dBNumber;
		}

		protected bool SkipRefreshDbReaderRolePermissionsForTest { get; set; }

		void CreateSqlServerLockInfoFile(string aFilePath)
		{
			string sqlText = @"
				SELECT
					l.request_session_id AS spid, l.resource_database_id AS database_id,
					d.name AS database_name, l.resource_associated_entity_id AS associated_entity_id,
					isnull(p.index_id, 0) AS index_id, l.resource_type, l.resource_description,
					l.request_mode, l.request_status
				FROM
					sys.dm_tran_locks l
					inner join sys.databases d ON d.database_id = l.resource_database_id
					left join sys.partitions p ON p.hobt_id = l.resource_associated_entity_id";

			var ds = new DataSet("LockInformation"); // This is a DataSet name
			var command = Db.Connection.Command(sqlText);
			using (var dataAdapter = command.NewDataAdapter())
			{
				dataAdapter.Fill(ds);
				ds.WriteXml(aFilePath);
			}
		}

		bool CreateDatabase_Raw(string databaseName)
		{
			using (AdminConnection createDbConnection = Db.NewAdminConnection())
			{
				LastCreationException = null;
				cannotAcquireLockMessage = string.Empty;
				var result = false;
				SqlApplicationLock dbLock;
				var locked = createDbConnection.TryGetLock(MutexIDs.StorageDocsDBBeingCreated.Name, out dbLock);
				using (dbLock)
				{
					try
					{
						if (locked)
						{
							if (!createDbConnection.DatabaseExists(databaseName))
							{
								CreateDatabase_Raw_UsingADifferentConnection(databaseName, createDbConnection);
							}
							result = true;
						}
						else
						{
							cannotAcquireLockMessage = Res.GetString("C90FDE00-A55C-4744-87FE-658866CFD562", "Could not acquire lock for creating database {0}.", databaseName);
						}
					}
					catch (Exception ex)
					{
						LastCreationException = ex;
						cannotAcquireLockMessage = string.Empty;
						HandleCreateDBException(databaseName, ex);
					}
				}
				return result;
			}
		}

		#region HandleCreateDBException

		void HandleCreateDBException(string databaseName, Exception ex)
		{
			var msg = Res.GetString("b9e36a73-ed9f-4705-9efd-a7a6f3841d30", "Error creating database [{0}]:\r\n{1}", databaseName, ex.Message) + "\r\n";
			var suppressException = false;

			if (ex is SqlException sqlEx)
			{
				var errorHandler = new DbErrorHandler(sqlEx, Db.Connection);

				if (errorHandler.ExceptionType == DbErrorType.DeadlockError || errorHandler.ExceptionType == DbErrorType.TimeoutExpired)
				{
					// get lock info
					msg = HandleDeadlockError(ex, msg);
				}
				else if (errorHandler.ExceptionType == DbErrorType.ObjectAlreadyExists || errorHandler.ExceptionType == DbErrorType.CannotCreateFileBecauseItAlreadyExists)
				{
					suppressException = true;
				}
			}
			if (!suppressException)
			{
				try
				{
					Thread.Sleep(10);
					DropDatabase(databaseName);
				}
				catch
				{
					// throw away errors, since we're kind of stuffed anyway.
				}

				msg += "\r\n " + Res.GetString("e7888b57-681d-405c-898b-9937c78f020b", "Contact your administrator.");

				throw new CreateDocManagerDatabaseException(msg, ex);
			}
		}

		string HandleDeadlockError(Exception ex, string msg)
		{
			string retrieveLockError = "";
			string diagnosticsFile = "";
			try
			{
				Guid tempPK = Guid.NewGuid();

				string fPath = Path.Combine(Env.TempPath, "LockInfo_" + tempPK.ToString() + ".xml"); // this isn't a schema column, it's a filename
				CreateSqlServerLockInfoFile(fPath);
				diagnosticsFile = fPath;
			}
			catch (Exception ex2)
			{
				retrieveLockError = ex2.GetType().ToString() + ": " + ex.Message;
			}

			if (!string.IsNullOrEmpty(diagnosticsFile))
			{
				msg += (NoResString)" lock information available in file : " + diagnosticsFile;
			}
			else
			{
				msg += (NoResString)" Attempt to retrieve lock information failed " + retrieveLockError;
			}
			return msg;
		}

		#endregion

		protected void CreateDatabase_Raw_UsingADifferentConnection(string databaseName, AdminConnection createDbConnection)
		{
			string dataPathFromRegistry = SystemDataRegistry.Instance.DocManagerDBDataFilePath.Value.Trim();
			string logPathFromRegistry = SystemDataRegistry.Instance.DocManagerDBLogFilePath.Value.Trim();
			string createDbObjectScript = DbScriptManager.DocManagerSchemaScript;
			int eDocDbAutoGrowthSizeMB = 500;
			int eDocDbLogFileInitialSizeMB = 500;
			int eDocDbLogFileAutoGrowthSizeMB = 200;

			createDbConnection.CreateDatabase(databaseName, dataPathFromRegistry, logPathFromRegistry, DbInitialSizeMb, eDocDbLogFileInitialSizeMB,
				eDocDbAutoGrowthSizeMB, eDocDbLogFileAutoGrowthSizeMB, actionToPerformAfterCreatingDb: (tempDatabaseName) =>
				{
					// set auto create stats to OFF
					createDbConnection.AlterDbAutoCreateStats(tempDatabaseName, isON: false);

					if (createDbConnection.IsDbUsingSnapshotIsolation(Db.DatabaseName))
					{
						// If main database uses snapshot isolation, new database should also use it.
						var snapshotManager = new SnapshotIsolationManager();
						snapshotManager.EnableSnapshotIsolationForDatabase(createDbConnection, tempDatabaseName);
					}

					using (((ICurrentDbControl)createDbConnection).UseDatabase(tempDatabaseName))
					{
						createDocManagerDatabase = createDbConnection.CurrentDatabase;
						// Create schema objects (tables, indexes, constraints)
						createDbConnection.ExecuteNonQuery(createDbObjectScript);

						// Create script objects (views, functions, procedures, triggers)
						// => There are currently no script objects in the DocManager databases
						//     If one is needed, create a DocManagerScriptIndex class in Enterprise.Build.Database.Script.
					}

					// Grant EnterpriseDbUser Permissions
					#if DEBUG
						// When querying GlbStaff, use existing Db.Connection instead of new connection, as GlbStaff may have been locked in existing Db.Connection and never committed in Test.
						new DbUserManager().GrantStaffPermissionsToNewDatabase(createDbConnection, tempDatabaseName, Db.Connection);
					#else
						new DbUserManager().GrantStaffPermissionsToNewDatabase(createDbConnection, tempDatabaseName);
					#endif
				});

			// This part needs to happen AFTER AdminConnection.CreateDatabase renames the database and returns,
			// Because it logs, sends emails and creates files using the current DB name.
			// (Possibly there's a smarter way to do this?)
			// Backup newly created database and add it to existing Always On availability group (if any)
			var sqlAlwaysOnAuto = ObjectFactory.Get<ISqlAlwaysOnAutomation>();
			sqlAlwaysOnAuto.BackupNewDatabase(databaseName);
			sqlAlwaysOnAuto.AddDatabaseToAlwaysOnGroup(databaseName);
		}

		protected virtual int? DbInitialSizeMb
		{
			get { return SystemDataRegistry.Instance.DocManagerInitialDataFileSizeGb.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) * 1024; }
		}

		protected void DropDatabase(string aDatabaseName)
		{
			using (DbConnection dropDbConnection = Db.NewAdminConnection())
			{
				using (DbCommand command = dropDbConnection.Command("DROP DATABASE [" + aDatabaseName + "]"))
				{
					command.ExecuteNonQuery();
				}
			}
		}

		ScriptManager DbScriptManager => dbScriptManager ?? (dbScriptManager = new ScriptManager());
		ScriptManager dbScriptManager;
		string createDocManagerDatabase;

		#endregion
	}
}
