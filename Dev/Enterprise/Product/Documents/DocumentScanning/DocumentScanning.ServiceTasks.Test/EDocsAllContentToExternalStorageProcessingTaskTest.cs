using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[TestedType(typeof(EDocsAllContentToExternalStorageProcessingTask))]
	internal class EDocsAllContentToExternalStorageProcessingTaskTest : BaseEDocsContentToExternalStorageProcessingTaskTest<EDocsAllContentToExternalStorageProcessingTask>
	{
		public void TestRequireFieldsAreUpdatedAfterUploaded()
		{
			try
			{
				Prepare();

				doc1DB2.SC_UncompressedSize = 0;
				doc2DB2.SC_UncompressedSize = 0;
				doc3DB2.SC_UncompressedSize = 0;
				doc4DB2.SC_UncompressedSize = 0;
				doc1DB2.MasterFactory.Save();

				AssertEquals(0, doc1DB2.SC_UncompressedSize);
				AssertEquals(0, doc2DB2.SC_UncompressedSize);
				AssertEquals(0, doc3DB2.SC_UncompressedSize);
				AssertEquals(0, doc4DB2.SC_UncompressedSize);

				AssertEquals($"{doc1DB2.Name} is not encypted before upload", false, IsDocumentUploadedAndEncrypted(doc1DB2));
				AssertEquals($"{doc2DB2.Name} is not encypted before upload", false, IsDocumentUploadedAndEncrypted(doc2DB2));
				AssertEquals($"{doc3DB2.Name} is not encypted before upload", false, IsDocumentUploadedAndEncrypted(doc3DB2));
				AssertEquals($"{doc4DB2.Name} is not encypted before upload", false, IsDocumentUploadedAndEncrypted(doc4DB2));

				var processingTask = new EDocsAllContentToExternalStorageProcessingTask();

				InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);
				Reset();

				var sd001DbName = new DocManagerDBHelper().GetDatabaseName(DocManagerDBHelper.InitialStorageDocsDatabaseNumber);

				using (var sd001Connection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, sd001DbName))
				{
					var factory = new DbBackendDocumentFactory(new BusinessObjectFactory(sd001Connection) { RefreshEnabled = false }, sd001Connection, doc1DB2.MasterFactory, true);
					var doc1 = factory.Load<StorageDocs>(doc1DB2.PK);
					var doc2 = factory.Load<StorageDocs>(doc2DB2.PK);
					var doc3 = factory.Load<StorageDocs>(doc3DB2.PK);
					var doc4 = factory.Load<StorageDocs>(doc4DB2.PK);

					AssertNotNull(doc1);
					AssertNotNull(doc2);
					AssertNotNull(doc3);
					AssertNotNull(doc4);

					AssertEquals(3, doc1.SC_UncompressedSize);
					AssertEquals(3, doc2.SC_UncompressedSize);
					AssertEquals(3, doc3.SC_UncompressedSize);
					AssertEquals(3, doc4.SC_UncompressedSize);

					AssertEquals($"{doc1.Name} is encypted", true, IsDocumentUploadedAndEncrypted(doc1));
					AssertEquals($"{doc2.Name} is encypted", true, IsDocumentUploadedAndEncrypted(doc2));
					AssertEquals($"{doc3.Name} is encypted", true, IsDocumentUploadedAndEncrypted(doc3));
					AssertEquals($"{doc4.Name} is encypted", true, IsDocumentUploadedAndEncrypted(doc4));
				}
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "DES", hostedServiceAttribute.Code);
				AssertEquals("Description", "All External Storage Processing Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", true, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1hour", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		[UseSnapshotProtection]
		public void TestServiceTaskCanRunInAnyBranch()
		{
			try
			{
				TestCaseHelper.ClearTable(AutoStorageMain.Schema.TableName);

				if (!testDbHelper.DatabaseExists(1))
				{
					testDbHelper.CreateDatabase(1);
				}

				if (!testDbHelper.DatabaseExists(2))
				{
					testDbHelper.CreateDatabase(2);
				}

				var databaseName = testDbHelper.GetDatabaseName(2);
				TestCaseHelper.ClearTable($"{databaseName}..{AutoStorageDocs.Schema.TableName}");

				SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);
				SetStorageAccess("KeyId=idfortest;Secret=secretfortest");
				SetStorageServiceUrl("https://a.b.c");
				SetStorageBucketName("testbucket");

				var documentFactory = new DbBackendDocumentFactory(Factory);
				var storageMain = documentFactory.New<StorageMain>();
				storageMain.SM_DB = 2;
				storageMain.SM_ParentFK = ZGuid.NewZGuid();
				storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				documentFactory.Save();

				var insertStorageDocsSql = $@"
INSERT [{databaseName}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData, SC_DocType)
VALUES ('{Guid.NewGuid()}', '{storageMain.PK}', sysutcdatetime(), sysutcdatetime(), sysutcdatetime(), 0x010203, 'AAA');";

				TestConnection.ExecuteNonQuery(insertStorageDocsSql);
				TestConnection.CommitTransaction();
				TestConnection.BeginTransaction();

				using (EnvProxy.Instance.TemporaryServiceTaskContext("DES", canRunInAnyBranch: true))
				{
					var serviceLogger = new TestServiceLogger();
					var processor = new EDocsAllContentToExternalStorageProcessor(1);
					var processingTask = new EDocsAllContentToExternalStorageProcessingTask { Processor = processor, ServiceLogger = serviceLogger };
					processingTask.RunTask();
				}

				AssertEquals("Should not have any errors reported", string.Empty, ErrorReporter.LastMessageReported);
			}
			finally
			{
				Reset();

				if (TestConnection.AppTransactionCount == 0)
				{
					TestConnection.BeginTransaction();
				}
			}
		}

		public override void TestEDocsServiceTaskShouldNotHandleExternalStorageException()
		{
			try
			{
				ExternalPersisterProviderTestHelper.CleanMocks();

				using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
				{
					Prepare();

					var task = new EDocsAllContentToExternalStorageProcessingTask();
					var logger = InitialiseTaskSchedule(task);
					AssertExceptionThrown<HostedServiceException>("Should throw HostedServiceException ", () => task.RunTask());
				}
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestFailedEDocsShouldNotMoveToSD001()
		{
			try
			{
				ExternalPersisterProviderTestHelper.CleanMocks();

				using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3WithThrowAmazonS3Exception())
				{
					Prepare();

					var helper = new DocManagerDBHelper();
					var doc1DatabaseName = helper.GetDatabaseName(1);
					var doc2DatabaseName = helper.GetDatabaseName(2);
					var eDocsCountInSD001Query = $"SELECT COUNT(1) FROM {doc1DatabaseName}..StorageDocs";
					var eDocsCountInSD002Query = $"SELECT COUNT(1) FROM {doc2DatabaseName}..StorageDocs";
					var eDocsCountInSD001 = Db.Connection.ExecuteScalar<int>(eDocsCountInSD001Query);
					var eDocsCountInSD002 = Db.Connection.ExecuteScalar<int>(eDocsCountInSD002Query);
					AssertEquals("Original eDocs count in SD001", 2, eDocsCountInSD001);
					AssertEquals("Original eDocs count in SD002", 4, eDocsCountInSD002);

					var storageMainOfSD002CountQuery = "SELECT COUNT(1) FROM dbo.StorageMain WHERE SM_DB = 2";
					var storageMainOfSD002Count = Db.Connection.ExecuteScalar<int>(storageMainOfSD002CountQuery);
					AssertEquals("Original StorageMain count for SD002", 2, storageMainOfSD002Count);

					var task = new EDocsAllContentToExternalStorageProcessingTask();
					InitialiseTaskSchedule(task);
					AssertExceptionThrown<HostedServiceException>("Should throw HostedServiceException", () => RunTaskSchedule(task));

					eDocsCountInSD001 = Db.Connection.ExecuteScalar<int>(eDocsCountInSD001Query);
					eDocsCountInSD002 = Db.Connection.ExecuteScalar<int>(eDocsCountInSD002Query);
					AssertEquals("No eDocs were moved to SD001.", 2, eDocsCountInSD001);
					AssertEquals("All edocs were still stored in sd002 when AWS errors occurred.", 4, eDocsCountInSD002);

					storageMainOfSD002Count = Db.Connection.ExecuteScalar<int>(storageMainOfSD002CountQuery);
					AssertEquals("No StorageMain was updated.", 2, storageMainOfSD002Count);
				}
			}
			finally
			{
				Reset();
				Cleanup();
				ErrorReporter.Clear();
			}
		}

		public void TestProcessStorageDocsWouldNotCauseDeadLoop()
		{
			try
			{
				Prepare();
				SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=123;Secret=abc");
				var processor = new EDocsAllContentToExternalStorageProcessor();
				var logger = new TestServiceLogger();

				using (var cancellationTokenSource = new CancellationTokenSource())
				{
					cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(1));
					processor.Run(logger, cancellationTokenSource.Token);

					AssertEquals("DES service task should be finished in the specified time for the current workload", false, cancellationTokenSource.IsCancellationRequested);
				}
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestProcessorShouldProcessAllStorageMain()
		{
			try
			{
				Prepare();
				var processor = new EDocsAllContentToExternalStorageProcessor(1);
				var processingTask = new EDocsAllContentToExternalStorageProcessingTask() { Processor = processor };
				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);

				var sd002Name = testDbHelper.GetDatabaseName(2);
				AssertProcessed(sd002Name, doc1DB2, true);

				var expectedLog = $@"Information|Started to process StorageMain batch with size 1
Information|Processed 2 StorageDocs in DB {sd002Name}.
Information|Processed 0 StorageDocs in DB {sd002Name}.
Information|Finished processing StorageMain batch with size 1
Information|Started to process StorageMain batch with size 1
Information|Processed 2 StorageDocs in DB {sd002Name}.
Information|Processed 0 StorageDocs in DB {sd002Name}.
Information|Finished processing StorageMain batch with size 1
";
				AssertEquals(expectedLog, logger.ToString());
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestDESHostedServiceRequirement()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				var checkResult = EDocsAllContentToExternalStorageProcessingTask.CheckEDocsStorageProvider();
				AssertEquals("The registry setting 'System -> DocManager -> eDocs Storage' requires a value equal to 'S3'.", checkResult);
			}

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var checkResult = EDocsAllContentToExternalStorageProcessingTask.CheckEDocsStorageProvider();
				AssertEquals("", checkResult);
			}
		}

		public void TestCheckEDocsDatabase()
		{
			if (testDbHelper.DatabaseExists(2))
			{
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(2));
			}
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3);

			var checkResult = EDocsAllContentToExternalStorageProcessingTask.CheckEDocsDatabase();
			AssertEquals("This service task is not required to run as there is only one eDocs database (SD001) present.", checkResult);

			if (!testDbHelper.DatabaseExists(2))
			{
				testDbHelper.CreateDatabase(2);
			}

			checkResult = EDocsAllContentToExternalStorageProcessingTask.CheckEDocsDatabase();
			AssertEquals(string.Empty, checkResult);
		}

		public void TestProcessorWithOrphan()
		{
			try
			{
				Prepare(true);
				var processingTask = new EDocsAllContentToExternalStorageProcessingTask();

				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);
				Reset();

				AssertContains($"Information|Finished processing StorageMain batch", logger.ToString());
				var doc2DatabaseName = testDbHelper.GetDatabaseName(2);
				AssertProcessed(doc2DatabaseName, doc1DB2, true);
				AssertProcessed(doc2DatabaseName, doc2DB2, true);
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestProcessorWithSqlInsertError()
		{
			ErrorReporter.Clear();
			var triggerName = "TG_UNITTEST_BLOCKINSERT_StorageDocs";
			try
			{
				Prepare();

				// add trigger to emulate insert failue on doc2DB2
				using (var sdDbConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, testDbHelper.GetDatabaseName(1)))
				{
					var addTrigger = $@"
IF OBJECT_ID(N'{triggerName}') IS NOT NULL
	DROP TRIGGER {triggerName};

EXEC(N'CREATE TRIGGER {triggerName} 
ON [{testDbHelper.GetDatabaseName(1)}]..[StorageDocs] FOR INSERT
AS
BEGIN
	IF EXISTS(SELECT * FROM inserted WHERE SC_PK = ''{doc2DB2.PK}'')
	BEGIN
		RAISERROR(''Emulate fail insert.'', 16, 1)
		ROLLBACK TRANSACTION
	END
	RETURN
END')";
					sdDbConnection.ExecuteNonQuery(addTrigger);
				}

				var processingTask = new EDocsAllContentToExternalStorageProcessingTask();

				var logger = InitialiseTaskSchedule(processingTask);
				AssertExceptionThrown<SqlException>(() => RunTaskSchedule(processingTask));
				Reset();

				AssertEquals("doc1DB2 should not be moved to SD001", false, DocumentExist(testDbHelper.GetDatabaseName(1), doc1DB2.PK));
				AssertEquals("doc2DB2 should not be moved to SD001", false, DocumentExist(testDbHelper.GetDatabaseName(1), doc2DB2.PK));
				AssertEquals("doc1DB2 should still remain in SD002", true, DocumentExist(testDbHelper.GetDatabaseName(2), doc1DB2.PK));
				AssertEquals("doc2DB2 should still remain in SD002", true, DocumentExist(testDbHelper.GetDatabaseName(2), doc2DB2.PK));

				var errorMessage = $"Unable to copy StorageDocs with SM_PK '{doc2DB2.SC_SM}' from {testDbHelper.GetDatabaseName(2)} to SD001";
				AssertContains($"Error|{errorMessage}", logger.ToString());
				AssertNotContains($"Information|Finished processing StorageMain batch", logger.ToString());
				AssertEquals("Should report", 1, ErrorReporter.TotalErrorCount);
				AssertEquals("Should report key", "StorageDocs_Copy_Error", ErrorReporter.LastKeyReported);
				AssertEquals("Should report message", errorMessage, ErrorReporter.LastMessageReported);
			}
			finally
			{
				Reset();
				Cleanup();
				ErrorReporter.Clear();

				using (var sdDbConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, testDbHelper.GetDatabaseName(1)))
				{
					var dropTrigger = $@"IF OBJECT_ID(N'{triggerName}') IS NOT NULL DROP TRIGGER {triggerName};";
					sdDbConnection.ExecuteNonQuery(dropTrigger);
				}
			}
		}

		public override void TestProcessor()
		{
			try
			{
				PrepareWith3rdDB(true);
				var processingTask = new EDocsAllContentToExternalStorageProcessingTask();

				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);
				Reset();
				AssertNotContains($"Error", logger.ToString());
				AssertContains($"Information|Finished processing StorageMain batch", logger.ToString());

				var doc2DatabaseName = testDbHelper.GetDatabaseName(2);
				var doc3DatabaseName = testDbHelper.GetDatabaseName(3);
				AssertProcessed(doc2DatabaseName, doc1DB2, true);
				AssertProcessed(doc2DatabaseName, doc2DB2, true);
				AssertProcessed(doc3DatabaseName, doc1DB3, true);
				AssertProcessed(doc3DatabaseName, doc2DB3, true);
			}
			finally
			{
				Reset();
				CleanupWith3rdDB();
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(3));
			}
		}

		protected internal void PrepareWith3rdDB(bool addDuplication = false)
		{
			Prepare(addDuplication);

			if (!testDbHelper.DatabaseExists(3))
			{
				testDbHelper.CreateDatabase(3);
			}

			var doc3DatabaseName = testDbHelper.GetDatabaseName(3);
			TestCaseHelper.ClearTable($"{doc3DatabaseName}..{StorageDocs.Schema.TableName}");

			var masterFactory3 = new DbBackendDocumentFactory(Factory);
			var parentDB3 = masterFactory3.New<StorageMain>();
			parentDB3.SM_DB = 3;
			parentDB3.SM_ParentFK = ZGuid.NewZGuid();
			parentDB3.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			doc1DB3 = parentDB3.Documents.AddNew();
			doc1DB3.SC_DocType = "AAA";
			doc1DB3.SC_Desc = "AAA Doc 31";
			doc1DB3.SC_IsPublished = false;
			doc1DB3.SC_ImageData = new byte[] { 1, 2, 3 };
			doc2DB3 = parentDB3.Files.AddNew();
			doc2DB3.SC_DocType = "AAA";
			doc2DB3.SC_Desc = "AAA Doc 32";
			doc2DB3.SC_IsPublished = true;
			doc2DB3.SC_ImageData = new byte[] { 1, 2, 3 };

			masterFactory3.Save();

			if (addDuplication)
			{
				// Add duplications to SD001
				var insertDuplicatedStorageDocsSql = $@"
			INSERT [{testDbHelper.GetDatabaseName(1)}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData)
			VALUES ('{doc1DB3.PK}', '{parentDB3.PK}', sysutcdatetime(), sysutcdatetime(), sysutcdatetime(), 0x010203);";

				Db.Connection.ExecuteNonQuery(insertDuplicatedStorageDocsSql);
			}
		}

		protected internal StorageDocs doc1DB3;
		protected internal StorageFile doc2DB3;

		protected internal void CleanupWith3rdDB()
		{
			var doc1DatabaseName = testDbHelper.GetDatabaseName(1);
			var doc3DatabaseName = testDbHelper.GetDatabaseName(3);
			Db.Connection.ExecuteNonQuery($"DELETE {doc1DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc1DB3.PK}', '{doc2DB3.PK}')");
			Db.Connection.ExecuteNonQuery($"DELETE {doc3DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc1DB3.PK}', '{doc2DB3.PK}')");

			Cleanup();
		}

		public void TestProcessor_MissingDB()
		{
			try
			{
				Prepare();

				var doc10DatabaseName = testDbHelper.GetDatabaseName(10);
				if (!testDbHelper.DatabaseExists(10))
				{
					testDbHelper.CreateDatabase(10);
				}

				var masterFactory = new DbBackendDocumentFactory(Factory);
				for (var index = 0; index < 100; index++)
				{
					var parentDB_NonExisting = masterFactory.New<StorageMain>();
					parentDB_NonExisting.SM_DB = 10;
					parentDB_NonExisting.SM_ParentFK = ZGuid.NewZGuid();
					parentDB_NonExisting.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				}
				masterFactory.Save();

				var db10 = testDbHelper.GetDatabaseName(10);
				testDbHelper.DropDatabase(db10);

				var processingTask = new EDocsAllContentToExternalStorageProcessingTask();
				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);

				var expectedLog = $@"Error|Moving eDocs to external storage has encountered an error due to missing Document Databases.
Please inform your System Administrator that the following document databases are missing:
{doc10DatabaseName}";
				AssertContains(expectedLog, logger.ToString());
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestProcessor_StopWhenS3NotEnabled()
		{
			var processingTask = new EDocsAllContentToExternalStorageProcessingTask();
			TestProcessor_StopWhenS3NotEnabled(processingTask);
		}

		public void TestProcessor_StopWhenOnlyOneSDDB()
		{
			TestCaseHelper.ClearTable(StorageMain.Schema.TableName);

			if (!testDbHelper.DatabaseExists(1))
			{
				testDbHelper.CreateDatabase(1);
			}

			if (testDbHelper.DatabaseExists(2))
			{
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(2));
			}

			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3);

			var processingTask = new EDocsAllContentToExternalStorageProcessingTask();
			var logger = InitialiseTaskSchedule(processingTask);
			RunTaskSchedule(processingTask);

			AssertContains($"Warning|This service task is not required to run as there is only one eDocs database (SD001) present.", logger.ToString());
			AssertNotContains($"Information|Finished processing StorageMain batch", logger.ToString());
		}

		public void TestProcessor_StopWhenAnyReadOnly()
		{
			var processingTask = new EDocsAllContentToExternalStorageProcessingTask();
			base.TestProcessor_StopWhenAnyReadOnly(processingTask, "DES service failed to execute");
		}

		public void TestServiceStopsIfNoAccessToExternalStorage()
		{
			var processingTask = new EDocsAllContentToExternalStorageProcessingTask();
			base.TestServiceStopsIfNoAccessToExternalStorage(processingTask);
		}

		public void TestProcessor_SkipLockedDb()
		{
			try
			{
				PrepareWith3rdDB();

				var db2 = testDbHelper.GetDatabaseName(2);
				using (var sdDbConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, db2))
				{
					if (sdDbConnection.TryGetLock("DESProcessingLock", out var db2Mutex, db2))
					{
						using (db2Mutex)
						{
							var processingTask = new EDocsAllContentToExternalStorageProcessingTask();
							var logger = InitialiseTaskSchedule(processingTask);
							RunTaskSchedule(processingTask);
							AssertNotContains($"Error", logger.ToString());
							AssertContains($"Information|Finished processing StorageMain batch", logger.ToString());
						}
					}
				}

				var sd002 = testDbHelper.GetDatabaseName(2);
				var sd003 = testDbHelper.GetDatabaseName(3);
				AssertProcessed(sd002, doc1DB2, false);
				AssertProcessed(sd002, doc2DB2, false);
				AssertProcessed(sd003, doc1DB3, true);
				AssertProcessed(sd003, doc2DB3, true);
			}
			finally
			{
				Reset();
				CleanupWith3rdDB();
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(3));
			}
		}

		public void TestProcessor_SkipLockedDbV2()
		{
			Db.Connection.CommitTransaction();
			var docs = new List<(int, StorageDocs)>();
			try
			{
				// we create 4 SD DBs with some docs in them
				docs.AddRange(CreateStorageDocs(1, 3));
				docs.AddRange(CreateStorageDocs(2, 3));
				docs.AddRange(CreateStorageDocs(3, 3));
				docs.AddRange(CreateStorageDocs(4, 3));

				SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);
				SetStorageServiceUrl("https://jmk.awesome");
				SetStorageAccess("KeyId=dude;Secret=top");
				SetStorageBucketName("test");

				// and lock sd002
				var db2 = testDbHelper.GetDatabaseName(2);
				using (var sdDbConnection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, db2))
				{
					if (sdDbConnection.TryGetLock("DESProcessingLock", out var db2Mutex, db2))
					{
						using (db2Mutex)
						{
							var processingTask = new EDocsAllContentToExternalStorageProcessingTask();
							var logger = InitialiseTaskSchedule(processingTask);
							RunTaskSchedule(processingTask);
							AssertNotContains($"Error", logger.ToString());
							AssertContains($"Information|Finished processing StorageMain batch", logger.ToString());
						}
					}
				}

				AssertProcessed(docs, 2, false);
				AssertProcessed(docs, 3, true);
				AssertProcessed(docs, 4, true);
			}
			finally
			{
				Reset();
				CleanupStorageDocs(docs);
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(2));
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(3));
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(4));
				Db.Connection.BeginTransaction();
			}
		}

		IEnumerable<(int, StorageDocs)> CreateStorageDocs(int dbNumber, int numOfDocs)
		{
			var testDbHelper = new DocManagerDBHelperTestClass();
			if (!testDbHelper.DatabaseExists(dbNumber))
			{
				testDbHelper.CreateDatabase(dbNumber);
			}

			var docDbName = testDbHelper.GetDatabaseName(dbNumber);
			TestCaseHelper.ClearTable($"{docDbName}..{StorageDocs.Schema.TableName}");

			var masterFactory = new DbBackendDocumentFactory(Factory);
			var storageMain = masterFactory.New<StorageMain>();
			storageMain.SM_DB = dbNumber;
			storageMain.SM_ParentFK = ZGuid.NewZGuid();
			storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			for (var i = 0; i < numOfDocs; i++)
			{
				var storageDoc = storageMain.Documents.AddNew();
				storageDoc.SC_DocType = "TTT";
				storageDoc.SC_DataType = "SSS";
				storageDoc.SC_Desc = $"Test Doc {i} of SD00{dbNumber}";
				storageDoc.SC_IsPublished = true;
				storageDoc.SC_ImageData = new byte[] { 1, 2, 3 };
				yield return (dbNumber, storageDoc);
			}

			masterFactory.Save();
		}

		void CleanupStorageDocs(IEnumerable<(int dbNumber, StorageDocs storageDoc)> docs)
		{
			var sd001 = testDbHelper.GetDatabaseName(1);
			foreach (var doc in docs)
			{
				var sdDbName = testDbHelper.GetDatabaseName(doc.dbNumber);
				Db.Connection.ExecuteNonQuery($"DELETE {sdDbName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} = '{doc.storageDoc.PK}'");

				if (doc.dbNumber > 1)
				{
					// delete sd that has been copied to sd001
					Db.Connection.ExecuteNonQuery($"DELETE {sd001}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} = '{doc.storageDoc.PK}'");
				}
			}
		}

		void AssertProcessed(string dbName, StorageDocsBase doc, bool processed)
		{
			var sd001 = testDbHelper.GetDatabaseName(1);

			AssertEquals($"{doc.Name} should {(processed ? "not " : "")}exist in {dbName}.", !processed, DocumentExist(dbName, doc.PK));
			AssertEquals($"{doc.Name} should {(processed ? "" : "not ")}be copied to SD001.", processed, DocumentExist(sd001, doc.PK));
			if (processed)
			{
				AssertEquals($"{doc.Name} should be cleared in SD001", true, IsDocumentContentEmpty(sd001, doc.PK));
				AssertEquals($"{doc.Name} should be encrypted", true, IsDocumentUploadedAndEncrypted(sd001, doc.PK));
			}
			else
			{
				AssertEquals($"{doc.Name} should not be cleared in {dbName}", false, IsDocumentContentEmpty(dbName, doc.PK));
			}
		}

		void AssertProcessed(IEnumerable<(int dbNumber, StorageDocs storageDoc)> docs, int dbNumber, bool processed)
		{
			var dbName = testDbHelper.GetDatabaseName(dbNumber);

			Assert("sd001 is not exist", testDbHelper.DatabaseExists(1));
			Assert($"{dbName} is not exist", testDbHelper.DatabaseExists(dbNumber));

			CombineAssertions($"{dbName} should {(processed ? "" : "not ")}be processed", () =>
			{
				foreach (var doc in docs.Where(d => d.dbNumber == dbNumber))
				{
					AssertProcessed(dbName, doc.storageDoc, processed);
				}
			});
		}
	}
}
