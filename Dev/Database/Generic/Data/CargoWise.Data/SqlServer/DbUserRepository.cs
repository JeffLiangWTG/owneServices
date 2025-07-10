using System;
using System.Globalization;
using System.Text;
using CargoWise.Common;
using CargoWise.Database.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	public class DbUserRepository
	{
		public const string RepositoryDbSuffix = "_UserRepository";
		public const string StaffDbLoginPrefix = "EnterpriseDbUser";
		public static string GetStaffDbLoginFullPrefix(string dbName)
		{
			Argument.NotNullOrEmpty(dbName, nameof(dbName));

			var result = string.Format(CultureInfo.InvariantCulture, "{0}_{1}_", StaffDbLoginPrefix, dbName);
			return result;
		}

		public void CreateRepositoryDatabase()
		{
			var databaseName = Db.DatabaseName + RepositoryDbSuffix;

			using (var createDbConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb))
			{
				createDbConnection.CreateDatabase(databaseName);
				SynchroniseSynonyms(createDbConnection);
			}

			// Backup newly created database and add it to existing Always On availability group (if any)
			var sqlAlwaysOnAuto = GlobalServiceProvider.Instance.GetRequiredService<ISqlAlwaysOnAutomation>();
			sqlAlwaysOnAuto.BackupNewDatabase(databaseName);
			sqlAlwaysOnAuto.AddDatabaseToAlwaysOnGroup(databaseName);
		}

		/// <summary>
		/// Synchronises synonyms in the User Repository database.
		/// ---------------------------------------------------------------------
		/// * For existing synonyms in User Repository DB:
		///   - IF synonym base object points to valid homonymic object
		///     => NO ACTION TAKEN (synonym is kept)
		///   - ELSE
		///     => DROP SYNONYM
		///     
		/// * For other existing objects in User Repository DB:
		///   - IF object name conflicts with a main DB object
		///     => RENAME OBJECT (_RENAMED_newguid_objname)
		///   - ELSE
		///     => NO ACTION TAKEN (object is kept)
		///     
		/// * For objects in the main databases:
		///   - IF already mapped to a synonym in the User Repository DB
		///     => NO ACTION TAKEN (existing synonym kept)
		///   - ELSE
		///     => CREATE SYNONYM
		/// ---------------------------------------------------------------------
		/// Object types in the main DB which are considered:
		///   - U  = USER_TABLE
		///   - V  = VIEW
		///   - P  = SQL_STORED_PROCEDURE
		///   - IF = SQL_INLINE_TABLE_VALUED_FUNCTION
		///   - TF = SQL_TABLE_VALUED_FUNCTION
		///   - FN = SQL_SCALAR_FUNCTION
		///   - FS = CLR_SCALAR_FUNCTION
		///   - AF = AGGREGATE_FUNCTION
		///   - SN = SYNONYM (base object used as synonym chaining not allowed)
		/// </summary>
		/// <returns>
		/// TRUE  = if any changes were applied
		/// FALSE = no changes required
		/// </returns>
		public bool SynchroniseSynonyms(DbConnection connection)
		{
			Argument.NotNull(connection, nameof(connection));

			var mainDbName = Db.DatabaseName;
			if (!connection.DatabaseExists(mainDbName + RepositoryDbSuffix))
			{
				return false;
			}

			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				SELECT
					CASE
						WHEN repositoryobj.ObjType = 'SN' AND repositoryobj.ThreePartName != isnull(baseobj.ThreePartName, '')
							THEN 'DROP SYNONYM [' + repositoryobj.ObjName + '];'
						WHEN repositoryobj.ObjType != 'SN' AND baseobj.ObjName is not null
							THEN 'EXEC sp_rename ''' + repositoryobj.ObjName + ''', ''' + left('_RENAMED_' + convert(char(36), newid()) + '_' + repositoryobj.ObjName, 128) + ''';'
					END DropOrRenameOld,
					CASE
						WHEN isnull(repositoryobj.ThreePartName, '') != baseobj.ThreePartName
							THEN 'CREATE SYNONYM [' + baseobj.ObjName + '] FOR ' + baseobj.ThreePartName + ';'
					END CreateNew
				FROM
					(
						SELECT
							o.name ObjName,
							CASE o.type
								WHEN 'SN' THEN sn.base_object_name
								ELSE '[{0}].[' + sch.name + '].[' + o.name + ']'
							END ThreePartName
						FROM
							[{0}].sys.objects o
							INNER JOIN [{0}].sys.schemas sch ON sch.schema_id = o.schema_id
							LEFT JOIN [{0}].sys.synonyms sn ON sn.object_id = o.object_id
						WHERE
							o.is_ms_shipped = 0
							AND o.name not like 'Client%'
							AND sch.name not in ('{2}')
							AND o.type in ('U', 'V', 'P', 'IF', 'TF', 'FN', 'FS', 'AF', 'SN')
					) baseobj
					FULL OUTER JOIN
					(
						SELECT o.name ObjName, o.type ObjType, sn.base_object_name ThreePartName
						FROM [{0}{1}].sys.objects o
						LEFT JOIN [{0}{1}].sys.synonyms sn ON sn.object_id = o.object_id
						WHERE o.is_ms_shipped = 0
					) repositoryobj ON repositoryobj.ObjName = baseobj.ObjName
				WHERE
					isnull(baseobj.ThreePartName, '') != isnull(repositoryobj.ThreePartName, '')
					AND isnull(baseobj.ThreePartName, repositoryobj.ThreePartName) is not null",
				mainDbName,
				RepositoryDbSuffix,
				string.Join("','", Db.SqlReservedSchemas));

			var synonymSyncCmdBuilder = new StringBuilder();

			using (var reader = connection.Command(sqlText).ExecuteReader())
			{
				while (reader.Read())
				{
					if (reader[0] != DBNull.Value)
					{
						synonymSyncCmdBuilder.Append(reader[0]);
					}

					if (reader[1] != DBNull.Value)
					{
						synonymSyncCmdBuilder.Append(reader[1]);
					}
				}
			}

			var anyChangesApplied = false;
			var synonymSyncCmd = synonymSyncCmdBuilder.ToString();
			if (synonymSyncCmd.Length > 0)
			{
				synonymSyncCmd = string.Format(CultureInfo.InvariantCulture, "EXEC [{0}{1}]..sp_executesql N'{2}'", // sql command format string
					mainDbName, RepositoryDbSuffix, DataUtils.EscapeSingleQuotes(synonymSyncCmd));

				connection.ExecuteNonQuery(synonymSyncCmd);
				anyChangesApplied = true;
			}

			return anyChangesApplied;
		}

		public static bool IsRepositoryDatabase(string mainDbName, string dbName)
		{
			return
				mainDbName != null
				&& dbName != null
				&& dbName.Equals(string.Format(CultureInfo.InvariantCulture, "{0}{1}", mainDbName, RepositoryDbSuffix), StringComparison.OrdinalIgnoreCase)
				;
		}
	}
}
