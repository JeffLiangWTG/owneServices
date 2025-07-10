namespace Enterprise.DbUpgrader.Shared
{
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using System.Text;
	using System.Text.RegularExpressions;
	using CargoWise.Data;

	public class EmptyDbCreator : IDbCreator
	{
		public EmptyDbCreator(string dbName, string dataPath = null, bool mapDbLogins = false)
		{
			this.dbName = dbName;
			this.dataPath = dataPath;
			this.mapDbLogins = mapDbLogins;
		}

		#region IDbCreator Members

		void IDbCreator.CreateIfNotExists(AdminConnection conn)
		{
			CreateIfNotExistsCore(conn);
		}

		void IDbCreator.Drop(AdminConnection conn)
		{
			DatabaseRemover.Drop(conn);
		}

		void IDbCreator.CreateDropExisting(AdminConnection conn)
		{
			CreateDropExistingCore(conn);
		}

		#endregion

		void CreateIfNotExistsCore(AdminConnection conn)
		{
			if (!DbObjectCreator.DatabaseExists(conn, dbName))
			{
				CreateDb(conn);
			}
		}

		void CreateDropExistingCore(AdminConnection conn)
		{
			DatabaseRemover.Drop(conn);
			CreateIfNotExistsCore(conn);
		}

		void CreateDb(AdminConnection conn)
		{
			var dbCreated = false;
			var stopwatch = Stopwatch.StartNew();
			var log = new StringBuilder();
			do
			{
				try
				{
					CreateDbCore(conn);
					dbCreated = true;
				}
				catch (SqlException ex)
				{
					var errorHandler = new DbErrorMatch(ex);
					if (errorHandler.ExceptionType == DbErrorType.CannotCreateFileBecauseItAlreadyExists)
					{
						if (stopwatch.Elapsed > TimeSpan.FromMinutes(1))
						{
							throw new TimeoutException("Timeout trying to delete existing database files" + Environment.NewLine + log.ToString(), ex);
						}
						var fileFullName = errorHandler.GetFileFullNameOnCannotCreateFileError();
						log.AppendLine(ex.Message + " - Deleting " + fileFullName);
						try
						{
							DeletePreviousDbFilesIfExist(conn, fileFullName);
						}
						catch (Exception ex2)
						{
							throw new AggregateException(ex, ex2);
						}
					}
					else
					{
						throw;
					}
				}
			}
			while (!dbCreated);
		}

		#if DEBUG
		protected virtual
		#endif
		void CreateDbCore(AdminConnection conn)
		{
			conn.CreateDatabase(dbName, dataPath, logPath: dataPath, mapDbLogins: mapDbLogins);
		}

		#region Handle Conflicting Database File

		void DeletePreviousDbFilesIfExist(DbConnection conn, string conflictingFilePath)
		{
			var sqlText = String.Format(CultureInfo.InvariantCulture,
				"EXEC [{0}]..[{1}] '{2}', '1';",
				GetDbNameForDeleteFileProcExecution(conn),
				DeleteFileProcName,
				conflictingFilePath);
			conn.ExecuteNonQuery(sqlText);
		}

		/// <summary>
		/// If main database exists (i.e. connected to the main server), use it.
		/// Otherwise (e.g: connected to data warehouse server), gets the database name from the auxiliary database name.
		/// </summary>
		string GetDbNameForDeleteFileProcExecution(DbConnection conn)
		{
			string result = conn.CurrentDatabase;

			if (conn.DatabaseExists(Db.DatabaseName))
			{
				result = Db.DatabaseName;
			}
			else
			{
				var dbNameMatch = Regex.Match(dbName, @"_(?<DBNAME>" + Db.DatabaseName + @"_\w+)$", RegexOptions.IgnoreCase);

				if (dbNameMatch != null && dbNameMatch.Success && dbNameMatch.Groups["DBNAME"] != null)
				{
					result = dbNameMatch.Groups["DBNAME"].Value;
				}
			}

			return result;
		}

		#if DEBUG
		protected virtual
		#endif
		string DeleteFileProcName
		{
			get { return "CLRDeleteFile"; }
		}

		#endregion

		#region DbRemover

		DbRemover DatabaseRemover
		{
			get { return remover_UsePtyInstead ?? (remover_UsePtyInstead = new DbRemover(dbName)); }
		}
		DbRemover remover_UsePtyInstead;

		#endregion

		protected readonly string dbName;
		protected readonly string dataPath;
		protected readonly bool mapDbLogins;
	}
}
