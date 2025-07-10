using System;
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
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[TestedType(typeof(EDocsRegularContentToExternalStorageProcessingTask))]
	internal class EDocsRegularContentToExternalStorageProcessingTaskTest :
		BaseEDocsContentToExternalStorageProcessingTaskTest<EDocsRegularContentToExternalStorageProcessingTask>
	{
		public void TestOnlyOneInstanceCanRun()
		{
			try
			{
				Prepare();

				SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);
				SetStorageServiceUrl("https://jmk.awesome");
				SetStorageAccess("KeyId=dude;Secret=top");
				SetStorageBucketName("test");

				using var mainConnection = Db.NewExtraConnectionToMainDb();
				var sd001DBName = new DocManagerDBHelper().GetDatabaseName(DocManagerDBHelper.InitialStorageDocsDatabaseNumber);
				using var sd001Connection = Db.NewExtraConnectionWithMainDbCredentials(Db.Connection.ServerName, sd001DBName);
				if (sd001Connection.TryGetLock("DERDoubleLock", out var lockObject, sd001DBName))
				{
					using (lockObject)
					{
						var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
						var logger = InitialiseTaskSchedule(processingTask);
						mainConnection.Dispose(); //simulate main connection reset

						RunTaskSchedule(processingTask);
						AssertNullOrEmpty("Process should not be run if connection is locked",
							logger.ToString());
					}
				}
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestRequireFieldsAreUpdatedAfterUploaded()
		{
			try
			{
				Prepare();
				doc1DB1.SC_UncompressedSize = 0;
				doc2DB1.SC_UncompressedSize = 0;
				doc1DB1.MasterFactory.Save();

				AssertEquals(0, doc1DB1.SC_UncompressedSize);
				AssertEquals(0, doc2DB1.SC_UncompressedSize);
				AssertEquals($"{doc1DB1.Name} is not encypted before uploaded", false,
					IsDocumentUploadedAndEncrypted(doc1DB1));
				AssertEquals($"{doc2DB1.Name} is not encypted before uploaded", false,
					IsDocumentUploadedAndEncrypted(doc2DB1));

				var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();

				InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);
				Reset();

				doc1DB1.Reload();
				doc2DB1.Reload();
				AssertEquals(3, doc1DB1.SC_UncompressedSize);
				AssertEquals(3, doc2DB1.SC_UncompressedSize);
				AssertEquals($"{doc1DB1.Name} is encypted", true, IsDocumentUploadedAndEncrypted(doc1DB1));
				AssertEquals($"{doc2DB1.Name} is encypted", true, IsDocumentUploadedAndEncrypted(doc2DB1));
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestUploadMultiLargeSizeEDocs()
		{
			Prepare();

			var masterFactory = new DbBackendDocumentFactory(Factory);
			var parentDB1 = masterFactory.New<StorageMain>();
			parentDB1.SM_DB = 1;
			parentDB1.SM_ParentFK = ZGuid.NewZGuid();
			parentDB1.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			for (int i = 0; i < 100; i++)
			{
				var storageDocs = parentDB1.Documents.AddNew();
				storageDocs.SC_DocType = "AAA";
				storageDocs.SC_DataType = "XLS";
				storageDocs.SC_Desc = $"TestDoc {i}";
				storageDocs.SC_IsPublished = false;
				storageDocs.SC_ImageData = new byte[] { 1, 2, 3, 4, 5 };
			}

			masterFactory.Save();

			try
			{
				ExternalPersisterProviderTestHelper.CleanMocks();

				using (DbBackendDocumentFactory.SetActionBeforeLoadForTest(ActionBeforeLoadForTest))
				using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(
				supportUpload: true, supportDownload: false, disableCheckAllowWriteToExternalStorage: true))
				{
					var loggerMock = new Mock<ILogger>();
					var task = new EDocsRegularContentToExternalStorageProcessingTask()
					{
						ServiceLogger = loggerMock.Object
					};
					AssertNoExceptionThrown("Should not throw exception.", () => task.RunTask());

					AssertEquals("All eDocs are processed.", false,
						Db.Connection.Exists(
							$"FROM {testDbHelper.GetDatabaseName(1)}..StorageDocs WHERE SC_ImageDataHasValue = 1"));
				}
			}
			finally
			{
				Db.Connection.ExecuteNonQuery(
					$"DELETE {new DocManagerDBHelper().GetDatabaseName(1)}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.SC_SM} = '{parentDB1.PK}'");

				Reset();
				Cleanup();
			}

			void ActionBeforeLoadForTest(ZQuery zQuery)
			{
				if (zQuery.MaximumRows > 20)
				{
					throw new OutOfMemoryException(
						"Throw OutOfMemoryException if batch size is greater than 20 to simulate failure in loading multiple large binary records.");
				}
			}
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "DER", hostedServiceAttribute.Code);
				AssertEquals("Description", "Regular External Storage Processing Task",
					hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "10minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
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

				var databaseName = testDbHelper.GetDatabaseName(1);
				TestCaseHelper.ClearTable($"{databaseName}..{AutoStorageDocs.Schema.TableName}");

				SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);
				SetStorageAccess("KeyId=idfortest;Secret=secretfortest");
				SetStorageServiceUrl("https://a.b.c");
				SetStorageBucketName("testbucket");

				var documentFactory = new DbBackendDocumentFactory(Factory);
				var storageMain = documentFactory.New<StorageMain>();
				storageMain.SM_DB = 1;
				storageMain.SM_ParentFK = ZGuid.NewZGuid();
				storageMain.SM_Type = Core.Constants.DocManagerCodes.Shipment;
				documentFactory.Save();

				var insertStorageDocsSql = $@"
INSERT [{databaseName}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData, SC_DocType)
VALUES ('{Guid.NewGuid()}', '{storageMain.PK}', sysutcdatetime(), sysutcdatetime(), sysutcdatetime(), 0x010203, 'AAA');";

				TestConnection.ExecuteNonQuery(insertStorageDocsSql);
				TestConnection.CommitTransaction();
				TestConnection.BeginTransaction();

				using (EnvProxy.Instance.TemporaryServiceTaskContext("DER", canRunInAnyBranch: true))
				{
					var serviceLogger = new TestServiceLogger();
					var processingTask = new EDocsRegularContentToExternalStorageProcessingTask { ServiceLogger = serviceLogger };
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
					var loggerMock = new Mock<ILogger>();
					var task = new EDocsRegularContentToExternalStorageProcessingTask() { ServiceLogger = loggerMock.Object };

					AssertExceptionThrown<HostedServiceException>("Should throw HostedServiceException", () => task.RunTask());
				}
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestAuditColumnInfoShouldNotBeChangedWhenMovingEDocsToExternalStorage()
		{
			Prepare();
			try
			{
				var doc1DatabaseName = new DocManagerDBHelper().GetDatabaseName(1);
				Db.Connection.ExecuteNonQuery($"Update {doc1DatabaseName}..StorageDocs set SC_Date = '2021-11-20 00:00:00',SC_SystemLastEditTimeUtc = '2021-11-20 00:00:00' where SC_PK= '{doc1DB1.PK}'");

				doc1DB1.Reload();
				var originalDate = doc1DB1.SC_Date;
				var lastEditDate = doc1DB1.SC_SystemLastEditTimeUtc;

				var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
				InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);

				doc1DB1.Reload();
				AssertEquals("SC_Date", originalDate, doc1DB1.SC_Date);
				AssertEquals("SC_SystemLastEditTimeUtc", lastEditDate, doc1DB1.SC_SystemLastEditTimeUtc);
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestIsMovingToExternalStorageShouldBeTrueWhenRunningTask()
		{
			Prepare();
			try
			{
				BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
				{
					var storageDocs = (factory as IBusinessObjectFactoryInternals).AllBusinessObjects.OfType<StorageDocsBase>().Where(doc => doc.ParentMain.SM_DB == 1);
					foreach (var storageDoc in storageDocs)
					{
						Assert("We are moving edoc to S3.", storageDoc.IsMovingToExternalStorage);
					}
				});

				var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
				InitialiseTaskSchedule(processingTask);
				var loggerMock = new Mock<ILogger>();
				processingTask.ServiceLogger = loggerMock.Object;
				RunTaskSchedule(processingTask);
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestEDocsContentToExternalStorageWithConcurrencyError()
		{
			Prepare();

			try
			{
				using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
				{
					var processor = new EDocsRegularContentToExternalStorageProcessor();
					var logger = new TestServiceLogger();
					var token = new CancellationToken(false);

					bool doCleanUp = true;
					BusinessObjectFactory.SetOnFactorySaveHookForTest(factory =>
					{
						if (doCleanUp)
						{
							var helper = new DocManagerDBHelper();
							var doc1DatabaseName = helper.GetDatabaseName(1);
							Db.Connection.Command($"DELETE {doc1DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc1DB1.PK}', '{doc2DB1.PK}')").ExecuteNonQuery();
							doCleanUp = false;
						}
					});

					AssertNoExceptionThrown(() => processor.Run(logger, token));

					var expectedMessage = $@"Information|Run Start.
Information|Loading new batch of StorageDocs.
Information|Started processing batch of 2 StorageDocs.
Information|Uploaded 2 StorageDocs to external storage.
Information|Processed 2 StorageDocs. PKs are: {doc1DB1.PK},{doc2DB1.PK}
Information|Loading new batch of StorageDocs.
Information|No StorageDocs were loaded.
Information|Run Completed, total processed 2 StorageDocs.";
					AssertEquals(expectedMessage, logger.ToString().Trim());
				}
			}
			finally
			{
				Cleanup();
			}
		}

		public void TestDERHostedServiceRequirement()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				var checkResult = EDocsRegularContentToExternalStorageProcessingTask.CheckEDocsStorageProvider();
				AssertEquals("The registry setting 'System -> DocManager -> eDocs Storage' requires a value other than 'DB'.", checkResult);
			}

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var checkResult = EDocsRegularContentToExternalStorageProcessingTask.CheckEDocsStorageProvider();
				AssertEquals("", checkResult);
			}
		}

		public override void TestProcessor()
		{
			Prepare();
			var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
			InitialiseTaskSchedule(processingTask);
			var loggerMock = new Mock<ILogger>();
			processingTask.ServiceLogger = loggerMock.Object;
			RunTaskSchedule(processingTask);
			Reset();

			loggerMock.Verify(l => l.Log(LogType.Information, $"Processed 2 StorageDocs. PKs are: {doc1DB1.PK},{doc2DB1.PK}"), Times.Once);
			var helper = new DocManagerDBHelper();
			var doc1DatabaseName = helper.GetDatabaseName(1);
			Assert("Processed doc1DB1", IsDocumentContentEmpty(doc1DatabaseName, doc1DB1.PK));
			Assert("Processed doc2DB1", IsDocumentContentEmpty(doc1DatabaseName, doc2DB1.PK));
			AssertEquals("IsDocumentUploadedAndEncrypted doc1DB1", true, IsDocumentUploadedAndEncrypted(doc1DatabaseName, doc1DB1.PK));
			AssertEquals("IsDocumentUploadedAndEncrypted doc2BD1", true, IsDocumentUploadedAndEncrypted(doc1DatabaseName, doc2DB1.PK));

			var doc2DatabaseName = helper.GetDatabaseName(2);
			AssertEquals("Not processed doc1DB2 in SD002", false, IsDocumentContentEmpty(doc2DatabaseName, doc1DB2.PK));
			AssertEquals("doc1DB2 should remain in SD002", true, DocumentExist(doc2DatabaseName, doc1DB2.PK));
			AssertEquals("Not processed doc2DB2 in SD002", false, IsDocumentContentEmpty(doc2DatabaseName, doc2DB2.PK));
			AssertEquals("doc2DB2 should remain in SD002", true, DocumentExist(doc2DatabaseName, doc2DB2.PK));
			Cleanup();
		}

		public void TestProcessor_WithDelayUpload()
		{
			try
			{
				SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 3);
				Prepare();
				// set doc1DB1 to be older than EDocsDBMinimumStorageDays so it will be processed
				doc1DB1.SC_Date = ZDateTime.Now.AddDays(-1 - SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.Value);
				doc1DB1.Factory.Save();

				var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
				InitialiseTaskSchedule(processingTask);

				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);
				Reset();

				// Only one eDoc that is older than EDocsDBMinimumStorageDays will be processed
				AssertContains($"Information|Processed 1 StorageDocs. PKs are: {doc1DB1.PK}", logger.ToString());
				var helper = new DocManagerDBHelper();
				var doc1DatabaseName = helper.GetDatabaseName(1);
				AssertEquals("doc1DB1 that is older should be processed", true, IsDocumentContentEmpty(doc1DatabaseName, doc1DB1.PK));
				AssertEquals("doc2DB1 that is newer should not be processed", false, IsDocumentContentEmpty(doc1DatabaseName, doc2DB1.PK));
			}
			finally
			{
				Reset();
				Cleanup();
				ErrorReporter.Clear(); // clear DeveloperNotificationException report message
			}
		}

		public void TestProcessor_WithOrphan()
		{
			try
			{
				Prepare(true);
				// Make this doc to be orphan and it should not be processed
				var parentFactory = doc1DB1.ParentMain.Factory;
				doc1DB1.SC_SM = ZGuid.NewZGuid();
				parentFactory.Save();

				var helper = new DocManagerDBHelper();
				var doc1DatabaseName = helper.GetDatabaseName(1);

				AssertEquals("Pre: Orphan doc1DB1 exitsts in SD001", true, DocumentExist(doc1DatabaseName, doc1DB1.PK));
				AssertEquals("Pre: Duplicated orphan doc1DB2 exists in SD001", true, DocumentExist(doc1DatabaseName, doc1DB2.PK));

				var loggerMock = new Mock<ILogger>();
				var task = new EDocsRegularContentToExternalStorageProcessingTask() { ServiceLogger = loggerMock.Object };
				task.RunTask();

				loggerMock.Verify(l => l.Log(LogType.Information, $"Uploaded 1 StorageDocs to external storage."), Times.Once);
				loggerMock.Verify(l => l.Log(LogType.Information, $"Processed 1 StorageDocs. PKs are: {doc2DB1.PK}"), Times.Once);
				loggerMock.Verify(l => l.Log(LogType.Information, $"2 orphan StorageDocs found in the batch and deleted."), Times.Once);

				AssertEquals("Normal doc should be processed", true, IsDocumentContentEmpty(doc1DatabaseName, doc2DB1.PK));
				AssertEquals("Orphan should not be processed", false, IsDocumentContentEmpty(doc1DatabaseName, doc1DB1.PK));

				// orphans should be deleted
				AssertEquals("Orphan doc1DB1 in SD001 should be deleted", false, DocumentExist(doc1DatabaseName, doc1DB1.PK));
				AssertEquals("Duplicated orphan doc1DB2 in SD001 should be deleted", false, DocumentExist(doc1DatabaseName, doc1DB2.PK));
				// non-orphan should remain
				AssertEquals("Non-orphan doc1DB2 in SD002 should not be deleted", true, DocumentExist(helper.GetDatabaseName(2), doc1DB2.PK));

				// Deleted orphan's pk should not be added to StorageDocsToDelete
				AssertEquals("Deleted orphan's pk should not be added to StorageDocsToDelete: doc1DB1", expected: false, Db.Connection.Exists($"FROM dbo.StorageDocsToDelete WHERE SCD_StorageDocIdentifier = '{doc1DB1.PK}'"));
				AssertEquals("Deleted orphan's pk should not be added to StorageDocsToDelete: doc1DB2", expected: false, Db.Connection.Exists($"FROM dbo.StorageDocsToDelete WHERE SCD_StorageDocIdentifier = '{doc1DB2.PK}'"));
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		/// <summary>
		/// This test asserts error 208 behaviour (same as missing database)
		/// </summary>
		public void TestDeletedTableReturnsZeroRows()
		{
			var helper = new DocManagerDBHelper();
			var doc1DatabaseName = helper.GetDatabaseName(1);

			Db.Connection.ExecuteNonQuery("DROP TABLE " + doc1DatabaseName + ".dbo.StorageDocs");

			IHostedServiceQueueProvider provider = new EDocsExternalStorageDataQueue();
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3);
			AssertEquals(QueueResult.Error.QueueSize, provider.QueueResult.QueueSize);
			AssertEquals(QueueResult.Error.MaximumItemAge, provider.QueueResult.MaximumItemAge);
		}

		public void TestEDocsExternalStorageDataQueue()
		{
			IHostedServiceQueueProvider provider = new EDocsExternalStorageDataQueue();
			AssertEquals(0, provider.QueueResult.QueueSize);
			AssertEquals(TimeSpan.Zero, provider.QueueResult.MaximumItemAge);

			var documentDate = new DateTime(2021, 11, 1);
			Prepare(true, documentDate);
			var helper = new DocManagerDBHelper();
			var doc1DatabaseName = helper.GetDatabaseName(1);
			var doc1DB1IsEmpty = IsDocumentContentEmpty(doc1DatabaseName, doc1DB1.PK);

			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);
			Assert("There is eDoc with SC_ImageData content", !doc1DB1IsEmpty);
			AssertEquals(0, provider.QueueResult.QueueSize);
			AssertEquals(TimeSpan.Zero, provider.QueueResult.MaximumItemAge);

			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3);
			SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			AssertEquals(3, provider.QueueResult.QueueSize);
			var expectedAge = DateTime.UtcNow - documentDate;
			NUnit.Framework.Assert.That((int)provider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));

			var storageDays = 3;
			SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, storageDays);
			expectedAge = DateTime.UtcNow - documentDate - TimeSpan.FromDays(storageDays);
			NUnit.Framework.Assert.That((int)provider.QueueResult.MaximumItemAge.TotalSeconds, NUnit.Framework.Is.EqualTo((int)expectedAge.TotalSeconds).Within(60));

			Cleanup();
		}

		public void TestProcessStorageDocsWouldNotCauseDeadLoop()
		{
			try
			{
				Prepare();
				SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KeyId=123;Secret=abc");
				var processor = new EDocsRegularContentToExternalStorageProcessor();
				var logger = new TestServiceLogger();

				using (var cancellationTokenSource = new CancellationTokenSource())
				{
					cancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(1));
					processor.Run(logger, cancellationTokenSource.Token);

					AssertEquals("DER service task should be finished in the specified time for the current workload", false, cancellationTokenSource.IsCancellationRequested);
				}
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		public void TestProcessor_StopWhenS3NotEnabled()
		{
			var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
			TestProcessor_StopWhenS3NotEnabled(processingTask);
		}

		public void TestProcessor_StopWhenNoDocDatabase()
		{
			try
			{
				var testDbHelper = new DocManagerDBHelperTestClass();
				for (int i = 1; i <= 3; i++)
				{
					if (testDbHelper.DatabaseExists(i))
					{
						testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(i));
					}
				}

				var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);

				var expectedErrorMessage = EDocsRegularContentToExternalStorageProcessor.NoDocumentDatabaseMessage;
				AssertContains($"Warning|{expectedErrorMessage}", logger.ToString());
			}
			finally
			{
				Reset();
			}
		}

		public void TestProcessor_StopWhenAnyReadOnly()
		{
			var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
			base.TestProcessor_StopWhenAnyReadOnly(processingTask, "DER service failed to execute");
		}

		public void TestServiceStopsIfNoAccessToExternalStorage()
		{
			var processingTask = new EDocsRegularContentToExternalStorageProcessingTask();
			base.TestServiceStopsIfNoAccessToExternalStorage(processingTask);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			// for test purpose, all new docs should be processed
			SystemDataRegistry.Instance.EDocsDBMinimumStorageDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
		}
	}
}
