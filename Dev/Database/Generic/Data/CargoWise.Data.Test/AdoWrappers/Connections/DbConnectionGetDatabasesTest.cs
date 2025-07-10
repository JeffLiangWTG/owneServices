using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	#region GetDatabases tests

	class DbConnectionGetDatabasesTests : TestCase
	{
		#region Tests including reference databases when no synonyms exist in the main database

		public void TestGetDatabasesWithAllFlagNoSynomyms()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				auditDatabase,
				edwDatabase,
				singleSharedRefDb,
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var allDatabases = connection.GetDatabases(DatabaseType.All);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabases);
			}
		}

		public void TestGetDatabasesWithAllLessBiAndSingleSharedRefFlagsCombinationNoSynomyms()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var allDatabasesExceptSingleRefDbAndBiDbs = connection.GetDatabases(DatabaseType.All & ~(DatabaseType.BI | DatabaseType.SingleSharedRef));

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabasesExceptSingleRefDbAndBiDbs);
			}
		}

		public void TestGetDatabasesWithAllLessSingleSharedRefFlagsCombinationNoSynomyms()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				auditDatabase,
				edwDatabase,
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var allDatabasesExceptSingleRefDb = connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabasesExceptSingleRefDb);
			}
		}

		public void TestGetDatabasesWithExclusiveOrSharedRefFlagsCombinationNoSynomyms()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var allExclusiveOrSharedRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef | DatabaseType.SharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allExclusiveOrSharedRefDbs);
			}
		}

		public void TestGetDatabasesWithSharedRefFlagNoSynonyms()
		{
			// Arrange
			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var sharedRefDbs = connection.GetDatabases(DatabaseType.SharedRef);

				// Assert
				AssertEquals("There can be no shared reference databases in the list when no synonyms are specified.", 0, sharedRefDbs.Count());
			}
		}

		public void TestGetDatabasesWithExclusiveRefFlagNoSynonyms()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var exclusiveRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, exclusiveRefDbs);
			}
		}

		public void TestGetDatabasesWithExclusiveRefFlagNoSynonymsReturnsTheSameSetTwiceEvenWhenMoreDatabasesAreCreatedInBetween()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				var exclusiveRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef);
				AssertContainsExactElementsInAnyOrder(expectedDatabases, exclusiveRefDbs);

				using (AdoTestUtils.CreateDbDropExistingDisposable(exclusiveRefDb3, mainDatabaseName))
				{
					// Act
					exclusiveRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef);

					// Assert
					AssertContainsExactElementsInAnyOrder($"Should only contain databases '{exclusiveRefDb1}' and '{exclusiveRefDb2}'.", expectedDatabases, exclusiveRefDbs);
				}
			}
		}

		#endregion Tests including reference databases when no synonyms exist in the main database

		#region Single database type tests

		public void TestGetDatabasesWithMainFlag()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var mainDbs = connection.GetDatabases(DatabaseType.Main);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, mainDbs);
			}
		}

		public void TestGetDatabasesWithSDFlagRetrievesBothWritableAndReadonlyDbs()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				sdDatabase1,
				sdDatabase2
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var sdDbs = connection.GetDatabases(DatabaseType.SD);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, sdDbs);
			}
		}

		public void TestGetDatabasesWithUserRepositoryFlag()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				userRepositoryDatabase,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var userRepositoryDbs = connection.GetDatabases(DatabaseType.UserRepository);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, userRepositoryDbs);
			}
		}

		public void TestGetDatabasesWithAuditFlag()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				auditDatabase,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var auditDbs = connection.GetDatabases(DatabaseType.Audit);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, auditDbs);
			}
		}

		public void TestGetDatabasesWithEdwFlag()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				edwDatabase,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var edwDbs = connection.GetDatabases(DatabaseType.EDW);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, edwDbs);
			}
		}

		public void TestGetDatabasesWithSingleSharedRefFlag()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				singleSharedRefDb,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				// Act
				var singleSharedRefDbs = connection.GetDatabases(DatabaseType.SingleSharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, singleSharedRefDbs);
			}
		}

		#endregion Single database type tests

		#region Tests including reference databases when synonyms exist to one or more exclusive ref dbs in the main database

		public void TestGetDatabasesWithAllFlagAndSynomymToExclusiveRefDb()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				auditDatabase,
				edwDatabase,
				singleSharedRefDb,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName2, exclusiveRefDb2);

				// Act
				var allDatabases = connection.GetDatabases(DatabaseType.All);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabases);
			}
		}

		public void TestGetDatabasesWithAllLessBiAndSingleSharedRefFlagsCombinationAndSynomymToExclusiveRefDb()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName2, exclusiveRefDb2);

				// Act
				var allDatabasesExceptSingleRefDbAndBiDbs = connection.GetDatabases(DatabaseType.All & ~(DatabaseType.BI | DatabaseType.SingleSharedRef));

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabasesExceptSingleRefDbAndBiDbs);
			}
		}

		public void TestGetDatabasesWithAllLessSingleSharedRefFlagsCombinationAndSynomymsToExclusiveRefDbs()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				auditDatabase,
				edwDatabase,
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, exclusiveRefDb1);
				CreateSynonym(connection, RefDbSynonymName2, exclusiveRefDb2);

				// Act
				var allDatabasesExceptSingleRefDb = connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabasesExceptSingleRefDb);
			}
		}

		public void TestGetDatabasesWithExclusiveOrSharedRefFlagsCombinationAndSynonymsToExclusiveRefDbs()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				exclusiveRefDb1,
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, exclusiveRefDb1);
				CreateSynonym(connection, RefDbSynonymName2, exclusiveRefDb2);

				// Act
				var allExclusiveOrSharedRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef | DatabaseType.SharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allExclusiveOrSharedRefDbs);
			}
		}

		public void TestGetDatabasesWithSharedRefFlagAndSynonymToExclusiveRefDb()
		{
			// Arrange
			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName2, exclusiveRefDb2);

				// Act
				var sharedRefDbs = connection.GetDatabases(DatabaseType.SharedRef);

				// Assert
				AssertEquals("There can be no shared reference databases in the list when synonyms are specified to exclusive databases only.", 0, sharedRefDbs.Count());
			}
		}

		public void TestGetDatabasesWithExclusiveRefFlagAndSynonymToExclusiveRefDb()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName2, exclusiveRefDb2);

				// Act
				var exclusiveRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, exclusiveRefDbs);
			}
		}

		public void TestGetDatabasesWithExclusiveRefFlagAndSynonymToExclusiveRefDbReturnsTheSameDatabasesTwiceEvenWhenMoreDatabasesAreAddedInBetween()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				exclusiveRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName2, exclusiveRefDb2);
				var exclusiveRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef);
				AssertContainsExactElementsInAnyOrder(expectedDatabases, exclusiveRefDbs);

				CreateSynonym(connection, RefDbSynonymName1, exclusiveRefDb1);

				// Act
				exclusiveRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef);

				// Assert
				AssertContainsExactElementsInAnyOrder($"Should contain a single reference database '{exclusiveRefDb2}'.", expectedDatabases, exclusiveRefDbs);
			}
		}

		#endregion Tests including reference databases when synonyms exist to one or more exclusive ref dbs in the main database

		#region Tests including reference databases when synonym exist to one or more shared ref db in the main database

		public void TestGetDatabasesWithAllFlagAndSynomymToSharedRefDb()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				auditDatabase,
				edwDatabase,
				singleSharedRefDb,
				sharedRefDb1,
				sharedAGRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, sharedRefDb1);
				CreateSynonym(connection, RefDbSynonymName2, sharedAGRefDb2);

				// Act
				var allDatabases = connection.GetDatabases(DatabaseType.All);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabases);
			}
		}

		public void TestGetDatabasesWithAllLessBiAndSingleSharedRefFlagsCombinationAndSynomymToSharedRefDb()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				sharedRefDb1,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, sharedRefDb1);

				// Act
				var allDatabasesExceptSingleRefDbAndBiDbs = connection.GetDatabases(DatabaseType.All & ~(DatabaseType.BI | DatabaseType.SingleSharedRef));

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabasesExceptSingleRefDbAndBiDbs);
			}
		}

		public void TestGetDatabasesWithAllLessSingleSharedRefFlagsCombinationAndSynomymsToSharedRefDbs()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				mainDatabaseName,
				sdDatabase1,
				sdDatabase2,
				userRepositoryDatabase,
				auditDatabase,
				edwDatabase,
				sharedRefDb1,
				sharedRefDb2,
				sharedAGRefDb1,
				sharedAGRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, sharedRefDb1);
				CreateSynonym(connection, RefDbSynonymName2, sharedRefDb2);
				CreateSynonym(connection, AGRefDbSynonymName1, sharedAGRefDb1);
				CreateSynonym(connection, AGRefDbSynonymName2, sharedAGRefDb2);

				// Act
				var allDatabasesExceptSingleRefDb = connection.GetDatabases(DatabaseType.All & ~DatabaseType.SingleSharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allDatabasesExceptSingleRefDb);
			}
		}

		public void TestGetDatabasesWithExclusiveOrSharedRefFlagsCombinationAndSynonymsToSharedRefDbs()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				sharedRefDb1,
				sharedRefDb2,
				sharedAGRefDb1,
				sharedAGRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, sharedRefDb1);
				CreateSynonym(connection, RefDbSynonymName2, sharedRefDb2);
				CreateSynonym(connection, AGRefDbSynonymName1, sharedAGRefDb1);
				CreateSynonym(connection, AGRefDbSynonymName2, sharedAGRefDb2);

				// Act
				var allExclusiveOrSharedRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef | DatabaseType.SharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, allExclusiveOrSharedRefDbs);
			}
		}

		public void TestGetDatabasesWithExclusiveRefFlagAndSynonymToSharedRefDb()
		{
			// Arrange
			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, sharedRefDb1);

				// Act
				var exclusiveRefDbs = connection.GetDatabases(DatabaseType.ExclusiveRef);

				// Assert
				AssertEquals("There can be no EXCLUSIVE reference databases in the list when there no synonyms specified to exclusive reference dbs BUT there are synonyms specified to SHARED reference databases.", 0, exclusiveRefDbs.Count());
			}
		}

		public void TestGetDatabasesWithSharedRefFlagAndSynonymToSharedRefDb()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				sharedRefDb1,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, RefDbSynonymName1, sharedRefDb1);

				// Act
				var sharedRefDbs = connection.GetDatabases(DatabaseType.SharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedDatabases, sharedRefDbs);
			}
		}

		public void TestGetDatabasesWithSharedRefFlagAndSynonymToSharedRefDbReturnsTheSameDatabasesTwiceEvenWhenMoreDatabasesAreAddedInBetween()
		{
			// Arrange
			var expectedDatabases = new[]
			{
				sharedAGRefDb2,
			};

			using (var connection = Db.NewExtraRestrictedWriterConnection(Db.ServerName, mainDatabaseName))
			{
				CreateSynonym(connection, AGRefDbSynonymName2, sharedAGRefDb2);
				var sharedRefDbs = connection.GetDatabases(DatabaseType.SharedRef);
				AssertContainsExactElementsInAnyOrder(expectedDatabases, sharedRefDbs);

				CreateSynonym(connection, RefDbSynonymName1, sharedRefDb1);

				sharedRefDbs = connection.GetDatabases(DatabaseType.SharedRef);

				// Assert
				AssertContainsExactElementsInAnyOrder($"Should only contain '{sharedAGRefDb2}'.", expectedDatabases, sharedRefDbs);
			}
		}

		#endregion Tests including reference databases when synonym exist to one or more shared ref db in the main database

		#region Implementation

		const string databaseWithRandomName = "84a1e7e6679c4c";
		const string mainDatabaseName = "A7c9af43ddc18";

		readonly string auditDatabase = $"{mainDatabaseName}{Db.AuditDatabaseSuffix}";
		readonly string edwDatabase = $"{mainDatabaseName}{Db.EdwDatabaseSuffix}";
		readonly string userRepositoryDatabase = $"{mainDatabaseName}{DbUserRepository.RepositoryDbSuffix}";
		readonly string sdDatabase1 = $"{mainDatabaseName}{Db.SDDatabaseAffix}001";
		readonly string sdDatabase2 = $"{mainDatabaseName}{Db.SDDatabaseAffix}101";
		readonly string exclusiveRefDb1 = $"{mainDatabaseName}_{RefDbTableNameResolver.RefDbAffix}_Test1";
		readonly string exclusiveRefDb2 = $"{mainDatabaseName}_{RefDbTableNameResolver.RefDbAffix}_Test2";
		readonly string exclusiveRefDb3 = $"{mainDatabaseName}_{RefDbTableNameResolver.RefDbAffix}_Test3";
		readonly string sharedRefDb1 = $"{RefDbTableNameResolver.SharedRefDbPrefix}{mainDatabaseName}Test1";
		readonly string sharedRefDb2 = $"{RefDbTableNameResolver.SharedRefDbPrefix}{mainDatabaseName}Test2";
		readonly string sharedAGRefDb1 = $"{RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix}{mainDatabaseName}Test1";
		readonly string sharedAGRefDb2 = $"{RefDbTableNameResolver.SharedAvailabilityGroupRefDbPrefix}{mainDatabaseName}Test2";

		readonly string singleSharedRefDb = RefDbTableNameResolver.DefaultSingleRefDbName;

		void CreateSynonym(DbConnection connection, string synonymName, string dbName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				connection.ExecuteNonQuery($@"
DROP TABLE IF EXISTS Test
CREATE TABLE Test (val int)
");
			}

			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			{
				DropSynonymIfExists(connection, synonymName);
				connection.ExecuteNonQuery($"CREATE SYNONYM [{synonymName}] FOR [{dbName}]..[Test]");
			}
		}

		const string RefDbSynonymName1 = "RefDb1Test";
		const string RefDbSynonymName2 = "RefDb2Test";
		const string AGRefDbSynonymName1 = "RefDbAG1Test";
		const string AGRefDbSynonymName2 = "RefDbAG2Test";

		void DropSynonymIfExists(DbConnection connection, string synonymName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(mainDatabaseName))
			{
				if (connection.Exists(
					$"FROM sys.synonyms WHERE name = @synonymName",
					cmd => cmd.AddParameter("@synonymName", System.Data.SqlDbType.NVarChar, 128, synonymName)))
				{
					connection.ExecuteNonQuery($"DROP SYNONYM {synonymName.QuoteName()};");
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			using (var connection = Db.NewAdminConnection())
			{
				DropSynonymIfExists(connection, RefDbSynonymName1);
				DropSynonymIfExists(connection, RefDbSynonymName2);

				DropSynonymIfExists(connection, AGRefDbSynonymName1);
				DropSynonymIfExists(connection, AGRefDbSynonymName2);
			}
		}

		protected override void MasterSetUp()
		{
			base.MasterSetUp();

			using (var adminConnection = Db.NewAdminConnection())
			{
				foreach (var database in GetDatabases())
				{
					AdoTestUtils.CreateDbDropExisting(adminConnection, database, mainDatabaseName);
				}

				adminConnection.ExecuteNonQuery($@"
SELECT * INTO [{mainDatabaseName}].dbo.StmData FROM dbo.StmData
WHERE SD_Name not in ('DATABASE_SCHEMA_VERSION', 'DATABASE_MINOR_SCHEMA_VERSION')
");
				adminConnection.ExecuteNonQuery($@"
-- Set one database to readonly to check that it will also be retrieved
ALTER DATABASE {sdDatabase2.QuoteName()} SET READ_ONLY WITH NO_WAIT
");
			}
		}

		protected override void FinalTearDown()
		{
			using (var adminConnection = Db.NewAdminConnection())
			{
				foreach (var database in GetDatabases())
				{
					AdoTestUtils.DropDbIfExists(adminConnection, database, mainDatabaseName);
				}
			}

			base.FinalTearDown();
		}

		IEnumerable<string> GetDatabases()
		{
			return new[]
			{
					mainDatabaseName,
					auditDatabase,
					edwDatabase,
					userRepositoryDatabase,
					sdDatabase1,
					sdDatabase2,
					exclusiveRefDb1,
					exclusiveRefDb2,
					sharedRefDb1,
					sharedRefDb2,
					sharedAGRefDb1,
					sharedAGRefDb2,
					databaseWithRandomName,
				};
		}

		#endregion Implementation
	}

	#endregion GetDatabases test

}
