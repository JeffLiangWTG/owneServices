using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
#if NET
using System.Threading.Tasks;
#endif
using Amazon.S3;
using Amazon.S3.Model;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Documents;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Documents
{
	public class ExternalStorageSizeRetrievalMock : ExternalStorageSizeRetrieval
	{
		List<ValueTuple<string, int>> cacheMock = new List<ValueTuple<string, int>>();
		readonly Dictionary<string, int> sizeLookup = new Dictionary<string, int>();
		readonly Dictionary<string, int> continuationTokenLookup = new Dictionary<string, int>();
		readonly Dictionary<string, int> startAfterLookup = new Dictionary<string, int>();

		public new string StorageDocsDbName { get => base.StorageDocsDbName; set => base.StorageDocsDbName = value; }
		public new string StorageDocsTableName { get => base.StorageDocsTableName; set => base.StorageDocsTableName = value; }

		public bool ShouldUseOriginalAmazonClient { get; set; }

		public void AddToMockObjectCache(string s3FileName, int s3FileSize)
		{
			cacheMock.Add((s3FileName, s3FileSize));
			sizeLookup.Add(s3FileName, s3FileSize);
		}

		public int LookupSize(string s3FileName)
		{
			return sizeLookup.ContainsKey(s3FileName) ? sizeLookup[s3FileName] : 0;
		}

		public void FinaliseMockObjectCache()
		{
			cacheMock = cacheMock.OrderBy(x => x.Item1).ToList();
			var index = 0;
			foreach (var fileObj in cacheMock)
			{
				startAfterLookup[fileObj.Item1] = index++;
			}
		}

		public void ClearMockObjectCache()
		{
			cacheMock.Clear();
			sizeLookup.Clear();
			continuationTokenLookup.Clear();
			startAfterLookup.Clear();
		}

		ListObjectsV2Response GetNextResponseFromMockObjectCache(ListObjectsV2Request request)
		{
			var continuationToken = request.ContinuationToken;
			var startAfter = request.StartAfter;
			var maxKeys = request.MaxKeys;

			var index = 0;
			if (!string.IsNullOrEmpty(continuationToken) && continuationTokenLookup.ContainsKey(continuationToken))
			{
				index = continuationTokenLookup[continuationToken];
			}
			if (!string.IsNullOrEmpty(startAfter) && startAfterLookup.ContainsKey(startAfter) && startAfterLookup[startAfter] > index)
			{
				index = startAfterLookup[startAfter];
			}

			var currentBlock = cacheMock
				.Skip(index)
				.Take(Math.Min(maxKeys, cacheMock.Count - index))
				.Select(tuple => new S3Object
				{
					Key = tuple.Item1,
					Size = tuple.Item2
				})
				.ToList();
			index += currentBlock.Count;

			var response = new ListObjectsV2Response
			{
				S3Objects = currentBlock,
				IsTruncated = currentBlock.Count == maxKeys,
				KeyCount = currentBlock.Count,
				ContinuationToken = continuationToken,
				NextContinuationToken = Guid.NewGuid().ToString(),
				StartAfter = startAfter
			};
			continuationTokenLookup[response.NextContinuationToken] = index;

			return response;
		}

		protected override AmazonS3Client GetAmazonS3ClientFromRegistry()
		{
			if (ShouldUseOriginalAmazonClient)
			{
				return base.GetAmazonS3ClientFromRegistry();
			}

			var mockS3Client = new Mock<AmazonS3Client>("bogusKeyId", "bogusSecret", new AmazonS3Config()
			{
				ServiceURL = "https://bogus.com",
				ForcePathStyle = true,
			});
			mockS3Client
#if NETFRAMEWORK
				.Setup(client => client.ListObjectsV2(It.IsAny<ListObjectsV2Request>()))
				.Returns((ListObjectsV2Request request) => GetNextResponseFromMockObjectCache(request));
#else
				.Setup(client => client.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), default(CancellationToken)))
				.Returns((ListObjectsV2Request request, CancellationToken cancellationToken) => Task.FromResult(GetNextResponseFromMockObjectCache(request)));
#endif
			return mockS3Client.Object;
		}
	}

	[TestedType(typeof(ExternalStorageSizeRetrieval))]
	class ExternalStorageSizeRetrievalTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => MockInstance;
		ExternalStorageSizeRetrievalMock MockInstance => mockInstance ??= new ExternalStorageSizeRetrievalMock();
		ExternalStorageSizeRetrievalMock mockInstance;

		readonly List<string> logMessages = new List<string>();
		readonly HashSet<string> notAwsNames = new HashSet<string>();

		const int DOCS_IN_AWS = 13337;
		const int DOCS_NOT_IN_AWS = 500;

		string EXPECTED_STORAGEDOCS_DB => $"{Db.DatabaseName}{Db.SDDatabaseAffix}001";
		const string EXPECTED_STORAGEDOCS_TABLE = "StorageDocs";
		const string EXPECTED_DETAILSMISSING_LOG = "Skipping ExternalStorageSizeRetrieval transformation as the S3 bucket details are missing or incomplete.";
		const string EXPECTED_COMPLETED_LOG = "\tCompleted: Retrieve document sizes from the connected AWS bucket for the StorageDocs table";

		protected override void PrepareTestData()
		{
			PrepareTestDataWithCount(DOCS_IN_AWS, DOCS_NOT_IN_AWS);
		}

		void PrepareTestDataWithCount(int awsCount, int notAwsCount)
		{
			MockInstance.ClearMockObjectCache();
			logMessages.Clear();

			DeleteAllRecordsIfTableExists(TestConnection, $"{EXPECTED_STORAGEDOCS_DB}.dbo.{EXPECTED_STORAGEDOCS_TABLE}");
			DeleteAllRecordsIfTableExists(TestConnection, "StorageMain");

			var random = new Random(123456);
			for (var i = 0; i < awsCount; i++)
			{
				var smGuid = CreateStorageMain(TestConnection, TestConnection.CurrentDatabase, Guid.NewGuid(), "", 1);
				var sdGuid = CreateStorageDoc(TestConnection, EXPECTED_STORAGEDOCS_DB, smGuid, "test.txt", "", DateTime.Now, Array.Empty<byte>());
				MockInstance.AddToMockObjectCache(sdGuid.ToString(), random.Next(1, 10000));
			}
			for (var i = 0; i < notAwsCount; i++)
			{
				var smGuid = CreateStorageMain(TestConnection, TestConnection.CurrentDatabase, Guid.NewGuid(), "", 1);
				var sdGuid = CreateStorageDoc(TestConnection, EXPECTED_STORAGEDOCS_DB, smGuid, "test.txt", "", DateTime.Now, Array.Empty<byte>());
				notAwsNames.Add(sdGuid.ToString());
			}

			SetRegistryString(TestConnection, "DocManagerStorageBucketName", "bogus");
			SetRegistryString(TestConnection, "EDocsStorageAccess", "KeyId=bogus;Secret=bogus");
			SetRegistryString(TestConnection, "EDocsStorageServiceUrl", "https://bogus.bogus");

			MockInstance.FinaliseMockObjectCache();
		}

		void AssertStartAfterDropped()
		{
			var statement = $"SELECT value FROM sys.extended_properties WHERE name = 'ExternalStorageSize_StartAfter'";
			var resultObj = TestConnection.ExecuteScalar(statement);
			Assert($"'ExternalStorageSize_StartAfter' should be cleared from 'sys.extended_properties' table after Transformation finishes successfully.", resultObj == DBNull.Value || resultObj == null);
		}

		void AssertStorageDocsTableMissing()
		{
			var query = $@"SELECT IIF(OBJECT_ID('{MockInstance.StorageDocsDbName}.dbo.{MockInstance.StorageDocsTableName}') IS NULL, 1, 0)";
			Assert("Pre-condition: StorageDocs table should be inaccessible prior to running this test.", Convert.ToBoolean(TestConnection.ExecuteScalar(query)));
		}

		protected override void AssertTransformationResults()
		{
			AssertTransformationResultsCore(DOCS_IN_AWS + DOCS_NOT_IN_AWS);
		}

		void AssertTransformationResultsCore(int expectedTotalDocuments)
		{
			var numSeen = 0;
			var failedCases = new List<ValueTuple<string, int, int>>();
			using (var command = TestConnection.Command(@$"SELECT SC_PK, SC_ExternalStorageSize FROM {EXPECTED_STORAGEDOCS_DB}.dbo.{EXPECTED_STORAGEDOCS_TABLE}", 240))
			{
				using (var reader = command.ExecuteReader())
				{
					string fileName;
					int actualFileSize, expectedFileSize;
					while (reader.Read())
					{
						fileName = reader.GetGuid(0).ToString();
						expectedFileSize = MockInstance.LookupSize(fileName);
						actualFileSize = reader.GetInt32(1);

						if (expectedFileSize != actualFileSize)
						{
							failedCases.Add((fileName, expectedFileSize, actualFileSize));
						}
						if (expectedFileSize > 0 || notAwsNames.Contains(fileName))
						{
							numSeen++;
						}
					}
				}
			}
			AssertEquals($"Failed to retrieve the expected {expectedTotalDocuments} documents from the database. Actual number retrieved was {numSeen}.", expectedTotalDocuments, numSeen);

			var failureMessage = $"The following {failedCases.Count} cases failed to match the expected filesize:\n"
				+ string.Join("\n", failedCases.Select(x => $"Filename: {x.Item1}, Expected: {x.Item2}, Actual: {x.Item3}"));
			AssertEquals(failureMessage, 0, failedCases.Count);
			AssertEquals("Transformation should raise one log message (a completed notification).", 1, logMessages.Count);
			AssertEquals("Transformation completed notification should match the expected format.", EXPECTED_COMPLETED_LOG, logMessages[0]);
			AssertStartAfterDropped();
		}

		protected override void RunTransformation()
		{
			// Clear the log messages buffer so that TestRunAndAssertResultsTwice works properly.
			logMessages.Clear();

			using (CheckNoTablesCreated())
			{
				IOnlineTransformation transformation = TransformationToTest;
				if (TransformationTestShouldBeRunAgainstNewInstance)
				{
					transformation = GetNewTestTransformationInstance();
				}
				transformation.Run(log => logMessages.Add(log), CancellationToken.None);
			}
		}

		protected override string ReasonNotToBeMapped => "A mocked sub-class of the actual DataTransformation is tested in this DataTransformationTestCase for implementation reasons. The actual DataTransformation has its mapping tested by TestIsBaseMapped.";
		public void TestIsBaseMapped()
		{
			var isMapped = IsTransformationMapped(new ExternalStorageSizeRetrieval());
			HtmlAssert("This transform is not mapped but it should be.<br/>" + SeeWikiMessage, isMapped);
		}

		public void TestRunWhenAwsIsEmpty()
		{
			// Arrange
			PrepareTestDataWithCount(0, DOCS_NOT_IN_AWS);
			AssertPreConditions();

			// Act
			RunTransformation();

			// Assert
			AssertTransformationResultsCore(DOCS_NOT_IN_AWS);
		}

		public void TestRunWhenAwsHasExactMultiple()
		{
			// Arrange
			PrepareTestDataWithCount(10000, DOCS_NOT_IN_AWS);
			AssertPreConditions();

			// Act
			RunTransformation();

			// Assert
			AssertTransformationResultsCore(10000 + DOCS_NOT_IN_AWS);
		}

		public void TestRunIsResilientToRestart()
		{
			// Arrange
			PrepareTestData();
			AssertPreConditions();
			var hasCompleted = false;
			var maxIterations = 100;

			// Act
			CancellationTokenSource cts;
			var iter = 0;
			while (!hasCompleted && iter < maxIterations)
			{
				cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(1000));
				try
				{
					((IOnlineTransformation)TransformationToTest).Run(log => logMessages.Add(log), cts.Token);
				}
				catch (OperationCanceledException)
				{
					iter++;
					continue;
				}
				hasCompleted = true;
			}

			// Assert
			Assert("Transformation should complete successfully despite arbitrary cancellation.", hasCompleted);
			Assert("Transformation should not complete on its first run.", iter > 0);
			AssertTransformationResults();
		}

		public void TestRunWhenStorageDocsDatabaseMissing()
		{
			// Arrange
			SetRegistryString(TestConnection, "DocManagerStorageBucketName", "bogus");
			SetRegistryString(TestConnection, "EDocsStorageAccess", "KeyId=bogus;Secret=bogus");
			SetRegistryString(TestConnection, "EDocsStorageServiceUrl", "https://bogus.bogus");

			var nonExistentDbName = "BogusNameToUseForAUnitTest_SD001";
			((ExternalStorageSizeRetrievalMock)TransformationToTest).StorageDocsDbName = nonExistentDbName;

			// Pre-condition
			AssertStorageDocsTableMissing();

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				$"Skipping ExternalStorageSizeRetrieval transformation as the {nonExistentDbName}.dbo.{EXPECTED_STORAGEDOCS_TABLE} table does not exist.",
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise two log messages (a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		public void TestRunWhenStorageDocsTableMissing()
		{
			// Arrange
			SetRegistryString(TestConnection, "DocManagerStorageBucketName", "bogus");
			SetRegistryString(TestConnection, "EDocsStorageAccess", "KeyId=bogus;Secret=bogus");
			SetRegistryString(TestConnection, "EDocsStorageServiceUrl", "https://bogus.bogus");

			var nonExistentTableName = "BogusNameToUseForAUnitTest";
			((ExternalStorageSizeRetrievalMock)TransformationToTest).StorageDocsTableName = nonExistentTableName;

			// Pre-condition
			AssertStorageDocsTableMissing();

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				$"Skipping ExternalStorageSizeRetrieval transformation as the {EXPECTED_STORAGEDOCS_DB}.dbo.{nonExistentTableName} table does not exist.",
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise two log messages (a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		public void TestRunWhenBucketNameMissing()
		{
			// Arrange
			PrepareTestDataWithCount(10000, DOCS_NOT_IN_AWS);
			DeleteRegistryString(TestConnection, "DocManagerStorageBucketName");
			MockInstance.ShouldUseOriginalAmazonClient = true;

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				"DocManagerStorageBucketName registry item missing during ExternalStorageSizeRetrieval transformation.",
				EXPECTED_DETAILSMISSING_LOG,
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise three log messages (a detailed issue, a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		public void TestRunWhenBucketAccessMissing()
		{
			// Arrange
			PrepareTestDataWithCount(10000, DOCS_NOT_IN_AWS);
			DeleteRegistryString(TestConnection, "EDocsStorageAccess");
			MockInstance.ShouldUseOriginalAmazonClient = true;

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				"EDocsStorageAccess registry item missing during ExternalStorageSizeRetrieval transformation.",
				EXPECTED_DETAILSMISSING_LOG,
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise three log messages (a detailed issue, a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		public void TestRunWhenBucketUrlMissing()
		{
			// Arrange
			PrepareTestDataWithCount(10000, DOCS_NOT_IN_AWS);
			DeleteRegistryString(TestConnection, "EDocsStorageServiceUrl");
			MockInstance.ShouldUseOriginalAmazonClient = true;

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				"EDocsStorageServiceUrl registry item missing during ExternalStorageSizeRetrieval transformation.",
				EXPECTED_DETAILSMISSING_LOG,
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise three log messages (a detailed issue, a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		public void TestRunWhenBucketAccessMalformedWithoutSeparator()
		{
			// Arrange
			PrepareTestDataWithCount(10000, DOCS_NOT_IN_AWS);
			SetRegistryString(TestConnection, "EDocsStorageAccess", "a");
			MockInstance.ShouldUseOriginalAmazonClient = true;

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				"EDocsStorageAccess registry item in invalid format during ExternalStorageSizeRetrieval transformation.",
				EXPECTED_DETAILSMISSING_LOG,
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise three log messages (a detailed issue, a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		public void TestRunWhenBucketAccessMalformedWithSeparator()
		{
			// Arrange
			PrepareTestDataWithCount(10000, DOCS_NOT_IN_AWS);
			SetRegistryString(TestConnection, "EDocsStorageAccess", "a;a");
			MockInstance.ShouldUseOriginalAmazonClient = true;

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				"EDocsStorageAccess registry item in invalid format during ExternalStorageSizeRetrieval transformation.",
				EXPECTED_DETAILSMISSING_LOG,
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise three log messages (a detailed issue, a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		public void TestRunWhenBucketDetailsInvalidAndStorageDocsTableMissing()
		{
			// Arrange
			DeleteRegistryString(TestConnection, "DocManagerStorageBucketName");
			DeleteRegistryString(TestConnection, "EDocsStorageAccess");
			DeleteRegistryString(TestConnection, "EDocsStorageServiceUrl");
			MockInstance.ShouldUseOriginalAmazonClient = true;

			var nonExistentDbName = "BogusNameToUseForAUnitTest_SD001";
			((ExternalStorageSizeRetrievalMock)TransformationToTest).StorageDocsDbName = nonExistentDbName;

			// Pre-condition
			AssertStorageDocsTableMissing();

			// Act
			RunTransformation();

			// Assert
			var expectedLogs = new string[]
			{
				$"Skipping ExternalStorageSizeRetrieval transformation as the {nonExistentDbName}.dbo.{EXPECTED_STORAGEDOCS_TABLE} table does not exist.",
				EXPECTED_COMPLETED_LOG
			};
			AssertContainsExactElementsInExactOrder("Transformation should raise two log messages (a skip warning, and a completed notification).", expectedLogs, logMessages);
			AssertStartAfterDropped();
		}

		#region SQL Definitions
		static void SetRegistryString(DbConnection connection, string key, string value)
		{
			var sqlText = $@"IF EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = '{key}')
BEGIN
    UPDATE dbo.StmData SET SD_BinaryValue = CAST(N'{value}' AS VARBINARY(MAX)) WHERE SD_Name = '{key}';
END
ELSE BEGIN
    INSERT INTO StmData (SD_PK, SD_BinaryValue, SD_Name, SD_Type) values (NEWID(), CAST(N'{value}' AS VARBINARY(MAX)), '{key}', N'STR');
END";

			using (var command = connection.Command(sqlText))
			{
				command.ExecuteNonQuery();
			}
		}

		static void DeleteRegistryString(DbConnection connection, string itemName)
		{
			var sqlText = $@"IF EXISTS (SELECT NULL FROM dbo.StmData WHERE SD_Name = '{itemName}')
BEGIN
    DELETE FROM dbo.StmData WHERE SD_Name = '{itemName}';
END";
			connection.ExecuteNonQuery(sqlText);
		}

		static void DeleteAllRecordsIfTableExists(DbConnection connection, string tableName)
		{
			if (TableExists(connection, tableName))
			{
				connection.ExecuteNonQuery(FormattableString.Invariant($"DELETE FROM {tableName}"));
			}
		}

		static bool TableExists(DbConnection connection, string tableName)
		{
			return TableExists(connection, connection.CurrentDatabase, tableName);
		}

		static bool TableExists(DbConnection connection, string dbName, string tableName, string tableSchema = null)
		{
			string sqlText = string.Format("\r\n\t\t\t\tIF (OBJECT_ID('[{0}].[{1}].[{2}]', 'U') is null)\r\n\t\t\t\t\tSELECT 0;\r\n\t\t\t\tELSE\r\n\t\t\t\t\tSELECT 1;", dbName, string.IsNullOrWhiteSpace(tableSchema) ? "dbo" : tableSchema, tableName);
			using DbCommand dbCommand = connection.Command(sqlText);
			return Convert.ToBoolean(dbCommand.ExecuteScalar());
		}

		Guid CreateStorageMain(DbConnection connection, string databaseName, Guid parentPK, string type, int db)
		{
			var pk = Guid.NewGuid();

			using (var command = connection.Command(string.Format(CreateStorageMainSql, databaseName)))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, parentPK);
				command.AddParameter("@type", SqlDbType.VarChar, StorageMainSchema.SM_Type.MaxLength, type);
				command.AddParameter("@db", SqlDbType.Int, db);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		Guid CreateStorageDoc(DbConnection connection, string databaseName, Guid storageMainPK, string fileName, string desc, DateTime date, byte[] data, string docType = "")
		{
			var pk = Guid.NewGuid();

			using (var command = connection.Command(string.Format(CreateStorageDocSql, databaseName)))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, storageMainPK);
				command.AddParameter("@fileName", SqlDbType.VarChar, StorageDocsSchema.SC_FileName.MaxLength, fileName);
				command.AddParameter("@date", SqlDbType.DateTime, date);
				command.AddParameter("@data", SqlDbType.VarBinary, data);
				command.AddParameter("@docType", SqlDbType.Char, docType);
				command.AddParameter("@desc", SqlDbType.VarChar, StorageDocsSchema.SC_Desc.MaxLength, desc);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		const string CreateStorageMainSql = @"
			insert into {0}.dbo.StorageMain(SM_PK, SM_ParentFK, SM_Type, SM_DB, SM_SystemCreateTimeUtc, SM_SystemCreateUser, SM_SystemLastEditTimeUtc, SM_SystemLastEditUser)
			values(@pk, @parentPK, @type, @db, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";

		const string CreateStorageDocSql = @"
			insert into {0}.dbo.StorageDocs(SC_PK, SC_SM, SC_FileName, SC_Date, SC_ImageData, SC_DocType, SC_Desc, SC_SystemCreateTimeUtc, SC_SystemCreateUser, SC_SystemLastEditTimeUtc, SC_SystemLastEditUser)
			values(@pk, @parentPK, @fileName, @date, @data, @docType, @desc, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
			";
		#endregion
	}
}
