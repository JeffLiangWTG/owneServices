using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	public class DbUserRepositoryTest : TestCase
	{
		public void TestSynchroniseSynonyms_WhenCurrentDbIsMain()
			=> AssertSynchroniseSynonyms(useMaster: false);

		public void TestSynchroniseSynonyms_WhenCurrentDbIsMaster()
			=> AssertSynchroniseSynonyms(useMaster: true);

		void AssertSynchroniseSynonyms(bool useMaster)
		{
			var userRepository = new DbUserRepository();

			using (var adminConnection = Db.NewAdminConnection(Db.ServerName, useMaster ? Db.SqlMasterDb : Db.DatabaseName))
			{
				AdoTestUtils.DropDbIfExists(adminConnection, UserRepositoryDbName);

				// SynchroniseSynonyms shouldn't do anything as the User Repository DB doesn't exist
				bool anyChangesApplied = userRepository.SynchroniseSynonyms(adminConnection);
				AssertEquals("SynchroniseSynonyms applied any changes?", false, anyChangesApplied);
				AssertEquals("User Repository database exists?", false, adminConnection.DatabaseExists(UserRepositoryDbName));

				AdoTestUtils.CreateDbIfNotExists(adminConnection, UserRepositoryDbName);

				using (var conn = Db.NewExtraConnectionToMainDb())
				{
					conn.BeginTransaction();

					try
					{
						PrepareTestObjects(conn);
						using (useMaster ? ((ICurrentDbControl)conn).UseDatabase(Db.SqlMasterDb) : null)
						{
							anyChangesApplied = userRepository.SynchroniseSynonyms(conn);
						}
						AssertEquals("SynchroniseSynonyms applied any changes?", true, anyChangesApplied);
						AssertSynonymsSynchronised(conn);
					}
					finally
					{
						conn.RollbackTransaction();
					}
				}
			}
		}

		void PrepareTestObjects(DbConnection connection)
		{
			connection.ExecuteNonQuery("CREATE TABLE [Test-SYN-MATCH-OBJ] (Col1 int)");
			connection.ExecuteNonQuery("CREATE SYNONYM [Test-SYN-MATCH-SYN] FOR [SomeDb].[dbo].[Test-SYN-MATCH-SYN]");
			connection.ExecuteNonQuery("CREATE VIEW [Test-OBJ-CONFLICT-OBJ] AS SELECT 1 Col1");
			connection.ExecuteNonQuery("CREATE SYNONYM [Test-OBJ-CONFLICT-SYN] FOR [SomeOtherDb].[dbo].[Test-OBJ-CONFLICT-SYN]");
			connection.ExecuteNonQuery("CREATE TABLE [Test-NEW_TAB] (Col1 int)");
			connection.ExecuteNonQuery("CREATE VIEW [Test-NEW_VIEW] AS SELECT 1 Col1");
			connection.ExecuteNonQuery("CREATE FUNCTION [Test-NEW_IF_FUNC]() RETURNS TABLE AS RETURN SELECT 1 Col1");
			connection.ExecuteNonQuery("CREATE FUNCTION [Test-NEW_TF_FUNC]() RETURNS @T TABLE(Col1 int) AS BEGIN RETURN END");
			connection.ExecuteNonQuery("CREATE FUNCTION [Test-NEW_FN_FUNC]() RETURNS int AS BEGIN RETURN 1 END");
			connection.ExecuteNonQuery("CREATE SYNONYM [Test-NEW_SYN] FOR [YetAnotherDb].[dbo].[Test-NEW_SYN]");
			connection.ExecuteNonQuery("CREATE PROCEDURE [Test-NEW_PROC] AS BEGIN SELECT 1 END");
			connection.ExecuteNonQuery("IF NOT EXISTS (SELECT null FROM sys.schemas WHERE name = 'cdc')	EXEC (N'CREATE SCHEMA [cdc]'); EXEC (N'CREATE VIEW [cdc].[Test-OBJ-MS-SCHEMA] AS SELECT 1 Col1');");

			connection.ExecuteNonQuery("CREATE FUNCTION [ClientTest-NEW_IF_FUNC]() RETURNS TABLE AS RETURN SELECT 1 Col1");
			connection.ExecuteNonQuery("CREATE PROCEDURE [ClientTest-NEW_PROC] AS BEGIN SELECT 1 END");

			using (((ICurrentDbControl)connection).UseDatabase(UserRepositoryDbName))
			{
				connection.ExecuteNonQuery("CREATE SYNONYM [Test-SYN-MATCH-OBJ] FOR [{0}].[dbo].[Test-SYN-MATCH-OBJ]");
				connection.ExecuteNonQuery("CREATE SYNONYM [Test-SYN-MATCH-SYN] FOR [SomeDb].[dbo].[Test-SYN-MATCH-SYN]");
				connection.ExecuteNonQuery("CREATE SYNONYM [Test-SYN-NO_MATCH] FOR [{0}].[dbo].[Test-SYN-NO_MATCH]");
				connection.ExecuteNonQuery("CREATE SYNONYM [Test-OBJ-MS-SCHEMA] FOR [{0}].[cdc].[Test-OBJ-MS-SCHEMA]");
				connection.ExecuteNonQuery("CREATE TABLE [Test-OBJ-CONFLICT-OBJ] (Col1 int)");
				connection.ExecuteNonQuery("CREATE VIEW [Test-OBJ-CONFLICT-SYN] AS SELECT 1 Col1");
				connection.ExecuteNonQuery("CREATE FUNCTION [Test-OBJ-NO_CONFLICT]() RETURNS TABLE AS RETURN SELECT 1 Col1");
			}
		}

		void AssertSynonymsSynchronised(DbConnection connection)
		{
			AssertSynonymExistInRepositoryDb(connection, "Test-SYN-MATCH-OBJ", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-SYN-MATCH-SYN", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-SYN-NO_MATCH", expected: false);

			AssertSynonymExistInRepositoryDb(connection, "Test-OBJ-CONFLICT-OBJ", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-OBJ-CONFLICT-SYN", expected: true);

			// Synonym mapped to reserved schema objects removed
			AssertSynonymExistInRepositoryDb(connection, "Test-OBJ-MS-SCHEMA", expected: false);

			// Conflicting objects renamed
			string sqlText = string.Format("SELECT count(*) FROM [{0}].sys.objects WHERE name like '[_]RENAMED[_]%'", UserRepositoryDbName);
			AssertEquals("Number of renamed objects", 2, (int)connection.ExecuteScalar(sqlText));

			// Non-conflicting object kept
			sqlText = string.Format("SELECT count(*) FROM [{0}].sys.objects WHERE name = 'Test-OBJ-NO_CONFLICT' AND type = 'IF'", UserRepositoryDbName);
			AssertEquals("Inline table function [Test-OBJ-NO_CONFLICT] kept?", true, (int)connection.ExecuteScalar(sqlText) == 1);

			// Synonyms created to map new objects
			AssertSynonymExistInRepositoryDb(connection, "Test-NEW_TAB", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-NEW_VIEW", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-NEW_IF_FUNC", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-NEW_TF_FUNC", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-NEW_FN_FUNC", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-NEW_SYN", expected: true);
			AssertSynonymExistInRepositoryDb(connection, "Test-NEW_PROC", expected: true);

			// ASSERT Client* does NOT exist
			sqlText = $"SELECT count(*) FROM [{UserRepositoryDbName}].sys.synonyms WHERE name like 'Client%'";
			AssertEquals("Synonyms starting with Client%", 0, (int)connection.ExecuteScalar(sqlText));
		}

		void AssertSynonymExistInRepositoryDb(DbConnection connection, string synonymName, bool expected)
		{
			string sqlText = string.Format(
				"SELECT count(*) FROM [{0}].sys.synonyms WHERE name = '{1}'",
				UserRepositoryDbName, synonymName);
			AssertEquals("Synonym [" + synonymName + "] exist?", expected, (int)connection.ExecuteScalar(sqlText) == 1);
		}

		public void TestCreateRepositoryDatabase()//
		{
			var userRepository = new DbUserRepository();

			DropUserRepositoryDatabase();
			AssertEquals("User Repository database exists?", false, Db.Connection.DatabaseExists(UserRepositoryDbName));

			new DbUserRepository().CreateRepositoryDatabase();
			AssertEquals("User Repository database exists?", true, Db.Connection.DatabaseExists(UserRepositoryDbName));

			string sqlText = string.Format("SELECT count(*) FROM [{0}].sys.synonyms", UserRepositoryDbName);
			AssertEquals("Synonyms created?", true, (int)Db.Connection.ExecuteScalar(sqlText) > 0);
		}

		public void TestIsRepositoryDatabase()
		{
			AssertEquals("Is NULL a repository database?", false, DbUserRepository.IsRepositoryDatabase(null, "MainDb_UserRepository"));
			AssertEquals("Is NULL a repository database?", false, DbUserRepository.IsRepositoryDatabase("MainDb", null));
			AssertEquals("Is MainDb_UserRepository a repository database?", false, DbUserRepository.IsRepositoryDatabase("AAA", "MainDb_UserRepository"));
			AssertEquals("Is MainDb_UserRepository a repository database?", true, DbUserRepository.IsRepositoryDatabase("MainDb", "MainDb_UserRepository"));
			AssertEquals("Is MainDb_UserRepositoryAbcd a repository database?", false, DbUserRepository.IsRepositoryDatabase("MainDb", "MainDb_UserRepositoryAbcd"));
			AssertEquals("Is AnyOtherDbName a repository database?", false, DbUserRepository.IsRepositoryDatabase("MainDb", "AnyOtherDbName"));
		}

		public static void DropUserRepositoryDatabase()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				AdoTestUtils.DropDbIfExists(adminConnection, UserRepositoryDbName);
			}
		}

		static readonly string UserRepositoryDbName = Db.DatabaseName + DbUserRepository.RepositoryDbSuffix;
	}
}
