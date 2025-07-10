using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[UseSnapshotProtection]
	internal abstract class BaseEDocsContentToExternalStorageProcessingTaskTest<T> : ServiceTaskTestCase<T> where T : ServiceProviderImpl
	{
		public BaseEDocsContentToExternalStorageProcessingTaskTest()
		{
		}

		protected internal void Prepare(bool addDuplication = false, DateTime? duplicatedDocumentDate = null)
		{
			TestCaseHelper.ClearTable(StorageMain.Schema.TableName);

			if (!testDbHelper.DatabaseExists(1))
			{
				testDbHelper.CreateDatabase(1);
			}

			var doc1DatabaseName = testDbHelper.GetDatabaseName(1);
			TestCaseHelper.ClearTable($"{doc1DatabaseName}..{StorageDocs.Schema.TableName}");

			SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);
			SetStorageAccess("KeyId=idfortest;Secret=secretfortest");
			SetStorageServiceUrl("https://a.b.c");
			SetStorageBucketName("testbucket");

			var masterFactory1 = new DbBackendDocumentFactory(Factory);
			var parentDB1 = masterFactory1.New<StorageMain>();
			parentDB1.SM_DB = 1;
			parentDB1.SM_ParentFK = ZGuid.NewZGuid();
			parentDB1.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			doc1DB1 = parentDB1.Documents.AddNew();
			doc1DB1.SC_DocType = "AAA";
			doc1DB1.SC_DataType = "XLS";
			doc1DB1.SC_Desc = "AAA Doc 11";
			doc1DB1.SC_IsPublished = false;
			doc1DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			doc2DB1 = parentDB1.Files.AddNew();
			doc2DB1.SC_DocType = "AAA";
			doc2DB1.SC_Desc = "AAA Doc 12";
			doc2DB1.SC_IsPublished = true;
			doc2DB1.SC_ImageData = new byte[] { 1, 2, 3 };
			masterFactory1.Save();

			if (!testDbHelper.DatabaseExists(2))
			{
				testDbHelper.CreateDatabase(2);
			}

			var masterFactory2 = new DbBackendDocumentFactory(Factory);
			var parentDB2 = masterFactory2.New<StorageMain>();
			parentDB2.SM_DB = 2;
			parentDB2.SM_ParentFK = ZGuid.NewZGuid();
			parentDB2.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			doc1DB2 = parentDB2.Files.AddNew();
			doc1DB2.SC_DocType = "AAA";
			doc1DB2.SC_Desc = "AAA Doc 21";
			doc1DB2.SC_IsPublished = false;
			doc1DB2.SC_ImageData = new byte[] { 1, 2, 3 };
			doc2DB2 = parentDB2.Documents.AddNew();
			doc2DB2.SC_DocType = "AAA";
			doc2DB2.SC_Desc = "AAA Doc 22";
			doc2DB2.SC_IsPublished = true;
			doc2DB2.SC_ImageData = new byte[] { 1, 2, 3 };

			var parentDB2_2 = masterFactory2.New<StorageMain>();
			parentDB2_2.SM_DB = 2;
			parentDB2_2.SM_ParentFK = ZGuid.NewZGuid();
			parentDB2_2.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			doc3DB2 = parentDB2_2.Documents.AddNew();
			doc3DB2.SC_DocType = "AAA";
			doc3DB2.SC_Desc = "AAA Doc 32";
			doc3DB2.SC_IsPublished = false;
			doc3DB2.SC_ImageData = new byte[] { 1, 2, 3 };
			doc4DB2 = parentDB2_2.Files.AddNew();
			doc4DB2.SC_DocType = "AAA";
			doc4DB2.SC_Desc = "AAA Doc 42";
			doc4DB2.SC_IsPublished = true;
			doc4DB2.SC_ImageData = new byte[] { 1, 2, 3 };

			masterFactory2.Save();

			if (addDuplication)
			{
				var sc_date = duplicatedDocumentDate.HasValue ? $"'{duplicatedDocumentDate.Value.ToString("s")}'" : "sysutcdatetime()";
				// Add duplications to SD001
				var insertDuplicatedStorageDocsSql = $@"
INSERT [{testDbHelper.GetDatabaseName(1)}]..StorageDocs (SC_PK, SC_SM, SC_Date, SC_SystemCreateTimeUtc, SC_SystemLastEditTimeUtc, SC_ImageData)
VALUES ('{doc1DB2.PK}', '{parentDB2.PK}', {sc_date}, sysutcdatetime(), sysutcdatetime(), 0x010203);";

				Db.Connection.ExecuteNonQuery(insertDuplicatedStorageDocsSql);
			}
			Db.Connection.CommitTransaction();
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected StorageDocs doc1DB1;
		protected StorageFile doc2DB1;
		protected StorageFile doc1DB2;
		protected StorageDocs doc2DB2;
		protected StorageDocs doc3DB2;
		protected StorageFile doc4DB2;

		protected DocManagerDBHelperTestClass testDbHelper = new DocManagerDBHelperTestClass();

		protected void Reset()
		{
			SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.DB);
			SetStorageAccess(string.Empty);
		}

		protected void Cleanup()
		{
			var helper = new DocManagerDBHelper();
			var doc1DatabaseName = helper.GetDatabaseName(1);
			var doc2DatabaseName = helper.GetDatabaseName(2);
			Db.Connection.ExecuteNonQuery($"DELETE {doc1DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc1DB1.PK}', '{doc2DB1.PK}')");
			Db.Connection.ExecuteNonQuery($"DELETE {doc1DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc1DB2.PK}', '{doc2DB2.PK}')");
			Db.Connection.ExecuteNonQuery($"DELETE {doc1DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc3DB2.PK}', '{doc4DB2.PK}')");
			Db.Connection.ExecuteNonQuery($"DELETE {doc2DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc1DB2.PK}', '{doc2DB2.PK}')");
			Db.Connection.ExecuteNonQuery($"DELETE {doc2DatabaseName}..{StorageDocs.Schema.TableName} WHERE {StorageDocs.Schema.PK} IN ('{doc3DB2.PK}', '{doc4DB2.PK}')");
			if (!Db.Connection.IsInTransaction)
			{
				Db.Connection.BeginTransaction();
			}
		}

		protected void TestProcessor_StopWhenS3NotEnabled(T processingTask)
		{
			try
			{
				Prepare();

				// Disable S3
				SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB);

				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);

				AssertContains($"Error|S3 Storage is not enabled, the process will be stopped.", logger.ToString());
			}
			finally
			{
				Reset();
				Cleanup();
			}
		}

		protected void TestProcessor_StopWhenAnyReadOnly(T processingTask, string expectedSubject)
		{
			try
			{
				EnvProxy.SetHostedLocationForTest("SYD");
				var testDbHelper = new DocManagerDBHelperTestClass();
				Prepare();

				// Create an unused SD003 readonly
				if (!testDbHelper.DatabaseExists(3))
				{
					testDbHelper.CreateDatabase(3);
				}
				Db.Connection.AlterDbWriteableStateForDocManager(testDbHelper.GetDatabaseName(3), false);

				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);

				var expectedErrorMessage = BaseEDocsContentToExternalStorageProcessor.DocumentDBReadOnlyMessage;
				AssertContains($"Error|{expectedErrorMessage}", logger.ToString());

				// Load notification email by subject and verify body.
				var mailItem = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains(expectedSubject));
				AssertNotNull(mailItem);
				AssertContains(expectedErrorMessage, mailItem.Body);
			}
			finally
			{
				Reset();
				Cleanup();
				EnvProxy.SetHostedLocationForTest("");
				testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(3));
			}
		}

		public void TestTaskDoesNotRunIfExclusiveS3Lock()
		{
			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				var gotLock = otherConnection.TryGetLock(BaseEDocsContentExternalStorageTask.S3ModificationLockName, out SqlApplicationLock appLock);
				Assert("Precondition", gotLock);

				using (appLock)
				{
					var testIsAbleToRunWhileExclusiveLock = false;
					try
					{
						TestProcessor();
						testIsAbleToRunWhileExclusiveLock = true;
					}
					// When the lock can't be aquired the processor won't run, so we expect the processor test on the dervied test class to fail
					catch (AssertionFailedError) { }
					catch (MockException) { }

					Assert(!testIsAbleToRunWhileExclusiveLock);

					Cleanup();
				}
			}
		}

		protected bool DocumentExist(string databaseName, ZGuid id)
		{
			return Db.Connection.Exists($"FROM {databaseName}..StorageDocs WHERE SC_PK='{id}'");
		}

		protected bool IsDocumentContentEmpty(string databaseName, ZGuid id)
		{
			return Db.Connection.Exists($"FROM {databaseName}..StorageDocs WHERE SC_PK='{id}' AND SC_ImageDataHasValue = 0");
		}

		protected bool IsDocumentUploadedAndEncrypted(string databaseName, ZGuid id)
			=> Db.Connection.Exists($"FROM {databaseName}..StorageDocs WHERE SC_PK='{id}' AND SC_ImageDataHasValue = 0 AND SC_SCK_MasterKey IS NOT NULL AND SC_EncryptedDataKey IS NOT NULL");

		protected bool IsDocumentUploadedAndEncrypted(StorageDocsWithS3Support doc)
			=> doc.SC_ImageDataFromDb.IsEmpty && !doc.SC_EncryptedDataKey.IsEmpty && !doc.SC_SCK_MasterKey.IsEmpty;

		protected static void SetStorageAccess(string credentials)
		{
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, credentials);
		}

		protected static void SetStorageServiceUrl(string serviceUrl)
		{
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceUrl);
		}

		protected static void SetStorageProvider(string provider)
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, provider);
		}

		protected static void SetStorageBucketName(string bucketName)
		{
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bucketName);
		}

		protected void TestServiceStopsIfNoAccessToExternalStorage(T processingTask)
		{
			var persisiterProvider = new Mock<IExternalPersisterProvider>();
			var persister = new Mock<IExternalPersister>();
			persisiterProvider.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persister.Object);

			ObjectFactory.DisposeSubstitutions();
			using (ObjectFactory.Substitute(persisiterProvider.Object))
			{
				persister.Setup(x => x.AllowWrite).Returns(false);

				var logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);
				AssertContains("Error|This service task has stopped because it is unable to access external storage.", logger.ToString());

				logger.ClearLog();
				persister.Setup(x => x.AllowWrite).Throws(new Exception("Unable to access S3 bucket.", new Exception("This is inner message.")));
				logger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);
				AssertContains(@"Error|Exception: Unable to access S3 bucket. Inner Exception: This is inner message.
Error|This service task has stopped because it is unable to access external storage.", logger.ToString());
			}
		}

		public abstract void TestProcessor();

		protected override void RunTest()
		{
			using (ExternalPersisterProviderTestHelper.MockExternalPersisterForS3(
				supportUpload: true, supportDownload: false, disableCheckAllowWriteToExternalStorage: true))
			{
				base.RunTest();
			}
		}

		public abstract void TestEDocsServiceTaskShouldNotHandleExternalStorageException();
	}
}
