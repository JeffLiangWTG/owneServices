using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.DataTools.DbBackupAndRestore.Business.Security;
using Microsoft.Extensions.DependencyInjection;
using WTG.Foundation.Cryptography.UserSecrets;
using WTG.StaticAnalysis.Annotation;
using static System.FormattableString;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class DbSecurityManager : DbTaskManager
	{
		#region CreateEnterpriseDbLogin

		public class DatabaseEntryCollection
		{
			// map of DatabaseName -> DataWarehouseServer
			readonly Dictionary<string, string> dbMap = new Dictionary<string, string>();

			public int Count => dbMap.Count;

			public void Add(string dbName, string dwServer)
			{
				dbMap.Add(dbName, dwServer);
			}

			public string[] GetDatabaseList()
			{
				return dbMap.Keys.ToArray();
			}

			public string GetDataWarehouseServer(string dbName) => dbMap.TryGetValue(dbName, out var result) ? result : null;
		}

		public DatabaseEntryCollection GetServerMainDbList(string dbServer, string saValue)
		{
			var result = new DatabaseEntryCollection();
			if (saValue == null)
			{
				saValue = Db.SaValue;
			}

			try
			{
				FireOnTaskStarted(Invariant($"Getting List of Databases\r\n\r\nServer: {dbServer}"));

				var sqlText = @"
								DECLARE cursor_databases CURSOR FOR
								SELECT db.name
								FROM sys.databases db LEFT JOIN sys.dm_hadr_availability_replica_states nonPrimaryReplica
									ON nonPrimaryReplica.replica_id = db.replica_id
									AND nonPrimaryReplica.is_local = 1
									AND nonPrimaryReplica.[role] <> 1
								WHERE db.[name] not in ('master', 'tempdb', 'model', 'msdb')
									AND db.[name] not like '%[''.,_-]%'
									AND db.[state] = 0
									AND db.is_in_standby = 0
									AND db.is_read_only = 0
									AND nonPrimaryReplica.replica_id is null
									ORDER BY db.[name];

								OPEN cursor_databases

								DECLARE @result table (DatabaseName nvarchar(max),BiDataWarehouseServer nvarchar(max))
								DECLARE @dbName nvarchar(200)
								FETCH NEXT FROM cursor_databases INTO @dbName

								WHILE @@FETCH_STATUS = 0
								BEGIN
									DECLARE
										@res bit = 0,
										@dwName nvarchar(max) = null,
										@QueryText NVARCHAR(MAX) =
											'SELECT @res = 1 FROM [' + @dbName + '].sys.tables WHERE [name] in (''GlbStaff'', ''JobShipment'', ''StmData'', ''RefCountry'') HAVING count(*) = 4
											IF @res = 1
											BEGIN
											DECLARE @QueryText nvarchar(max) = ''
												IF EXISTS (SELECT null from [' + @dbName + '].dbo.StmData where SD_Name = ''''BiDataWarehouseServer'''')
													SELECT @dwName = CONVERT(NVARCHAR(MAX), SD_BinaryValue) from [' + @dbName + '].dbo.StmData where SD_Name = ''''BiDataWarehouseServer''''
												ELSE
													SET @dwName = NULL''
												EXECUTE sp_executesql @QueryText , N''@dwName nvarchar(max) OUTPUT'' ,@dwName = @dwName OUTPUT
											END'

									BEGIN TRY
										EXECUTE sp_executesql @QueryText, N'@res bit OUTPUT, @dwName nvarchar(max) OUTPUT', @res = @res OUTPUT , @dwName = @dwName OUTPUT
										if @res=1
										BEGIN
											Insert @result values (@dbName,@dwName)
										END
									END TRY
									BEGIN CATCH
									END CATCH

									FETCH NEXT FROM cursor_databases INTO @dbName
								END;
								CLOSE cursor_databases;
								DEALLOCATE cursor_databases;

								SELECT * FROM @result
								";

				using (Db.DisableSchemaVersionCheck())
				using (var conn = GetNewAdminConnectionToMasterDbWithFallbackToSa(dbServer, saValue))
				{
					CreateAdminLoginIfRequired(conn, dbServer);

					conn.ExecuteReader(sqlText, reader =>
					{
						result.Add(
							reader["DatabaseName"] as string,
							reader["BiDataWarehouseServer"] as string);
					});
				}

				FireOnTaskCompleted(Invariant($"Finished getting list of main databases (Total = {result.Count})."));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				FireOnTaskFailed("Error getting list of databases\r\n\r\n" + ex.Message);
			}

			return result;
		}

		void CreateAdminLoginIfRequired(DbConnection connectionWithAdminRights, string serverName)
		{
			// If it is an AdminConnection, OdysseyAdmin already exists and has sysadmin rights.
			if (!(connectionWithAdminRights is AdminConnection))
			{
				/// TODO:
				/// DbBackupAndRestore should have a global service provider that we can retrieve the service from
				/// This is temporary untill we completely convert DbBakcupAndRestore.

				var serviceCollection = new ServiceCollection();
				serviceCollection.ConfigureProtectedDataFactoryServices();
				serviceCollection.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
				serviceCollection.ConfigureProtectedDataAdministrationServices();
				serviceCollection.ConfigureProtectedDataSqlServerAdministrationServices();
				serviceCollection.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, BackupAndRestoreSqlAdministrationContextManager>();
				using var serviceProvider = serviceCollection.BuildServiceProvider();

				FireOnTaskStarted(Invariant($"Creating {OdysseyAdminCredentials.AdminUserName} sql login."));

				var initService = serviceProvider.GetRequiredService<ISqlServerSecurityInitializationService>();
				var contextManager = (BackupAndRestoreSqlAdministrationContextManager)serviceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();

				try
				{
					contextManager.UseConnection(((IDbConnectionInternals)connectionWithAdminRights).SqlConnection, serverName);
					initService.InitializeServer(serverName);
				}
				finally
				{
					contextManager.ReleaseConnection();
				}

				FireOnTaskCompleted(Invariant($"Created {OdysseyAdminCredentials.AdminUserName} sql login."));
			}
		}

		#endregion

		#region Reset CW1 sysadmin account

		public void ResetCW1Sysadmin(string serverName, string dbName, string newPassword)
		{
			try
			{
				FireOnTaskStarted("Reactivating and resetting CW1 sysadmin account");
				DoResetCW1Sysadmin(serverName, dbName, newPassword);
				FireOnTaskCompleted("Finished reactivating and resetting CW1 sysadmin account");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				FireOnTaskFailed("Failed to reactivate and reset CW1 sysadmin account\n" + ex.Message);
			}
		}

		void DoResetCW1Sysadmin(string serverName, string dbName, string newPassword)
		{
			using (Db.DisableSchemaVersionCheck())
			using (var adminConn = Db.NewAdminConnection(serverName, dbName))
			{
				var sql = "Select CAST(GS_PK as varchar(36)) From [" + dbName + "].dbo.GlbStaff Where GS_LoginName = 'sysadmin'";
				var pk = (string)adminConn.ExecuteScalar(sql);

				var (_, hash, salt, iterations) = UserSecretsContext.DefaultContext.DeriveRawComponents(newPassword, UserSecretHashAlgorithmExtensions.PreferredAlgorithm, 200000);

				sql = $@"Update [{dbName}].dbo.GlbStaff Set
GS_PasswordHash = @passwordHash,
GS_PasswordSalt = @passwordSalt,
GS_PasswordHashIterations = @passwordHashIterations,
GS_IsActive = @isActive,
GS_SystemLastEditTimeUtc = GetUtcDate(),
GS_SystemLastEditUser = '~BP'
Where GS_LoginName = @loginName";

				using (var cmd = adminConn.Command(sql))
				{
					cmd.AddParameter("@passwordHash", SqlDbType.VarBinary, hash);
					cmd.AddParameter("@passwordSalt", SqlDbType.VarBinary, salt);
					cmd.AddParameter("@passwordHashIterations", SqlDbType.Int, iterations);
					cmd.AddParameter("@isActive", SqlDbType.Bit, true);
					cmd.AddParameter("@loginName", SqlDbType.VarChar, "sysadmin");

					var recordsUpdated = cmd.ExecuteNonQuery();

					if (recordsUpdated == 0)
					{
						FireOnTaskFailed(string.Format(CultureInfo.InvariantCulture, "Error updating sysadmin password. {0} records updated", recordsUpdated));
					}
					else
					{
						FireOnShowInfoMessage("sysadmin Password updated (" + newPassword + ")");
					}
				}
			}
		}

		#endregion

		#region Reset All Staff Local Password

		public void ResetAllStaffLocalPassword(string serverName, string dbName)
		{
			try
			{
				FireOnTaskStarted("Resetting all staff's local password");
				DoResetAllStaffLocalPassword(serverName, dbName);
				FireOnTaskCompleted("Finished resetting staff's local password");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				FireOnTaskFailed("Failed to reset staff's local password\n" + ex.Message);
			}
		}

		void DoResetAllStaffLocalPassword(string serverName, string dbName)
		{
			using (Db.DisableSchemaVersionCheck())
			using (var adminConn = Db.NewAdminConnection(serverName, dbName))
			{
				var sql = $@"
UPDATE [{dbName}].dbo.GlbStaff
SET
	GS_PasswordHash = NULL,
	GS_PasswordSalt = NULL,
	GS_PasswordHashIterations = 0,
	GS_SystemLastEditTimeUtc = GetUtcDate(),
	GS_SystemLastEditUser = '~BP'
WHERE
	GS_IsSystemAccount = 0";
				adminConn.ExecuteNonQuery(sql);
			}
		}

		#endregion

		#region Connection

		DbConnection GetNewSaConnectionToMasterDatabase(string serverName, string saValue)
		{
#if DEBUG
			if (ZArchitecture.Environment.Globals.IsTest)
			{
				return Db.NewAdminConnection(serverName, Db.SqlMasterDb);
			}
#endif
			return Db.NewExtraConnection(serverName, Db.SqlMasterDb, Db.SysAdminUserLogin, saValue);
		}

		DbConnection GetNewAdminConnectionToMasterDbWithFallbackToSa(string serverName, string saValue)
		{
			try
			{
				var adminConnection = Db.NewAdminConnection(serverName, Db.SqlMasterDb);

				using (var command = adminConnection.Command("SELECT IS_SRVROLEMEMBER('sysadmin');"))
				{
					if (Convert.ToBoolean(command.ExecuteScalar()))
					{
						return adminConnection;
					}
					// This will be caught by the catch block, and force a connection with 'sa'
					throw new InvalidOperationException();
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// If fails to open an AdminConnection, connects using 'sa'
				return GetNewSaConnectionToMasterDatabase(serverName, saValue);
			}
		}

		#endregion
	}

	[CodeAlive("Used Code")]
	public enum DbSecurityActionEnum
	{
		Undefined,
		ResetSysadmin,
		ResetAllStaffLocalPassword,
	}
}
