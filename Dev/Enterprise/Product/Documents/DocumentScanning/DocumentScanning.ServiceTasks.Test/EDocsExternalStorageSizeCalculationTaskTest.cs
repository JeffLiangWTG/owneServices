using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.DocumentScanning.Business.AWSPersister;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[TestedType(typeof(EDocsExternalStorageSizeCalculationTask))]
	class EDocsExternalStorageSizeCalculationTaskTest : ServiceTaskTestCase<EDocsExternalStorageSizeCalculationTask>
	{
		[TestDate(2024, 11, 11, 11, 11, 11)]
		public void TestExternalStorageSizeCalculation_SaveStmDataOneBatch()
		{
			_ = PrepareFilesWithExternalStorageSizes([100, 1000]);
			var processingTask = new EDocsExternalStorageSizeCalculationTask();

			var serviceLogger = InitialiseTaskSchedule(processingTask);
			RunTaskSchedule(processingTask);

			var sizeData = Business.DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
			AssertNotNullOrEmpty("External storage size data should not be null", sizeData);

			var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
			AssertEquals(1100, sizeObject.CalculatedSize);

			var logs = serviceLogger.ToString();
			AssertContains("Information|Started calculating eDocs external storage size", logs);
			AssertContains("Information|Started calculating size for batch of 2 StorageDocs", logs);
			AssertContains("Information|End of calculating storage size. On 2024-11-11 11-11-11 UTC the calculated external storage size is 1 MB and S3 bucket size is 0 MB", logs);
			AssertContains("Information|Finished calculating eDocs external storage size", logs);
		}

		public void TestExternalStorageSizeCalculation_SaveStmDataTwoBatches()
		{
			var docs = PrepareFilesWithExternalStorageSizes([500, 2000]);
			docs[0].SC_Date = new ZDateTime(2024, 7, 1);
			docs[0].MasterFactory.Save();
			var processingTask = new EDocsExternalStorageSizeCalculationTask();

			var serviceLogger = InitialiseTaskSchedule(processingTask);
			RunTaskSchedule(processingTask);

			var sizeData = Business.DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
			AssertNotNullOrEmpty("External storage size data should not be null", sizeData);

			var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
			AssertEquals(2500, sizeObject.CalculatedSize);

			var batchMessage = "calculating size for batch of 1 StorageDocs";
			AssertEquals("There should be 2 batch messages with 1 doc in each", 2, Regex.Matches(serviceLogger.ToString(), batchMessage).Count);
		}

		public void TestExternalStorageSizeCalculation_BackFillFailedOverThreshold()
		{
			try
			{
				DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				EnvProxy.SetHostedLocationForTest("SYD");
				var serviceLogger = PrepareForExternalStorageSizeBackFill(true);
				var logs = serviceLogger.ToString();

				var expectedMessage = "The S3 storage billing will be calculated based on the actual bucket size";
				AssertContains(expectedMessage, logs);

				expectedMessage = $"More than {EDocsExternalStorageSizeCalculationTask.ZeroExternalStorageSizeEDocsCountThreshold} documents don't have external storage size filled, please check service log to see some of the document PKs.";
				AssertContains(expectedMessage, logs);

				// Load notification email by subject and verify body.
				var mailItem = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains("Documents with no external storage sizes"));
				AssertNotNull(mailItem);
				AssertContains(expectedMessage, mailItem.Body);

				var sizeData = DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
				var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
				AssertEquals("DoNotUseCalculatedSize should be true in saved JSON data", true, sizeObject.DoNotUseCalculatedSize);

				var size = new AWSPersister().GetBucketSizeInMb();
				AssertEquals("The size should be S3 bucket size which is 0 in test", 0, size);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest("");
			}
		}

		public void TestExternalStorageSizeCalculation_BackFillFailedUnderThreshold()
		{
			try
			{
				DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				EnvProxy.SetHostedLocationForTest("SYD");
				var serviceLogger = PrepareForExternalStorageSizeBackFill(false);
				var logs = serviceLogger.ToString();

				var expectedMessage = $"More than {EDocsExternalStorageSizeCalculationTask.ZeroExternalStorageSizeEDocsCountThreshold} documents have external storage size filled, please check service log to see the document PKs.";
				AssertNotContains(expectedMessage, logs);

				expectedMessage = "The S3 storage billing will be calculated based on the actual sizes recorded in SC_ExternalStorageSize column";
				AssertContains(expectedMessage, logs);

				// No notification email should be created
				var mailItem = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.FirstOrDefault(x => x.Subject.Contains("Documents with no external storage sizes"));
				AssertNull(mailItem);

				var sizeData = DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
				var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
				AssertEquals("DoNotUseCalculatedSize should be false in saved JSON data", false, sizeObject.DoNotUseCalculatedSize);

				var size = new AWSPersister().GetBucketSizeInMb();
				AssertEquals("The size should be calculated size 2000, which is 1 MB", 1, size);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest("");
			}
		}

		public void TestExternalStorageSizeCalculation_BackFillFailedUnderThresholdWhenNotUseCalculatedExternalStorageSize()
		{
			try
			{
				DocManagerRegistry.Instance.UseCalculatedExternalStorageSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				EnvProxy.SetHostedLocationForTest("SYD");
				var serviceLogger = PrepareForExternalStorageSizeBackFill(false);
				var logs = serviceLogger.ToString();

				var expectedMessage = "The S3 storage billing will be calculated based on the actual bucket size";
				AssertContains("Expect to see the log of using S3 bucket size because UseCalculatedExternalStorageSize is false", expectedMessage, logs);
			}
			finally
			{
				EnvProxy.SetHostedLocationForTest("");
			}
		}

		TestServiceLogger PrepareForExternalStorageSizeBackFill(bool overThreshold)
		{
			UnattendedUserNotification.Instance.ClearShownOnceADayErrorKeys();
			var sizes = new List<int>() { 2000 };
			var threshold = EDocsExternalStorageSizeCalculationTask.ZeroExternalStorageSizeEDocsCountThreshold;
			var count = overThreshold ? threshold + 1 : threshold;
			for (var i = 0; i < count; i++)
			{
				sizes.Add(0);
			}

			PrepareFilesWithExternalStorageSizes(sizes.ToArray());
			var processingTask = new EDocsExternalStorageSizeCalculationTask();

			var serviceLogger = InitialiseTaskSchedule(processingTask);
			RunTaskSchedule(processingTask);

			var sizeData = Business.DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
			AssertNotNullOrEmpty("External storage size data should not be null", sizeData);

			var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
			AssertEquals(2000, sizeObject.CalculatedSize);

			return serviceLogger;
		}

		public void TestExternalStorageSizeCalculation_NoFilesUploadedToS3()
		{
			PrepareFilesWithExternalStorageSizes([]);
			var processingTask = new EDocsExternalStorageSizeCalculationTask();

			var serviceLogger = InitialiseTaskSchedule(processingTask);
			RunTaskSchedule(processingTask);

			var sizeData = DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
			AssertNotNullOrEmpty("External storage size data should not be null", sizeData);

			var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
			AssertEquals("No files uploaded to S3, so calculated size is 0", 0, sizeObject.CalculatedSize);
			AssertEquals("Use S3 bucket size if no file uploaded to S3 bucket", true, sizeObject.DoNotUseCalculatedSize);

			var logs = serviceLogger.ToString();
			AssertContains("Information|Started calculating eDocs external storage size", logs);
			AssertContains("Warning|No eDocs are moved to S3 yet. Cannot get the start date for calculation. EDocsExternalStorageSize is set to 0.", logs);
			AssertContains("Information|Finished calculating eDocs external storage size", logs);
		}

		public void TestExternalStorageSizeCalculation_BackFillInProgress()
		{
			ExtProperty.Database.Update(Db.Connection, EDocsExternalStorageSizeCalculationTask.ExternalStorageSizeRetrievalStartAfter, "ABC");

			try
			{
				PrepareFilesWithExternalStorageSizes([100, 1000]);
				var processingTask = new EDocsExternalStorageSizeCalculationTask();
				var serviceLogger = InitialiseTaskSchedule(processingTask);
				RunTaskSchedule(processingTask);

				var sizeData = DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
				AssertNotNullOrEmpty("External storage size data should not be null", sizeData);

				var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
				AssertEquals("Backfill is in progress, so calculated size is 0", 0, sizeObject.CalculatedSize);
				AssertEquals("Use S3 bucket size if backfill is in progress", true, sizeObject.DoNotUseCalculatedSize);

				AssertContains("Information|Started calculating eDocs external storage size", serviceLogger.ToString());
				AssertContains("Warning|Process cannot start because external storage size retrieval transformation has not finished yet.", serviceLogger.ToString());
				AssertContains("Information|Finished calculating eDocs external storage size", serviceLogger.ToString());
			}
			finally
			{
				ExtProperty.Database.Delete(Db.Connection, EDocsExternalStorageSizeCalculationTask.ExternalStorageSizeRetrievalStartAfter);
			}
		}

		public void TestExternalStorageSizeCalculation_StopWhenNoDocDatabase()
		{
			int[] dbNumber = [1, 2, 3];
			var testDbHelper = new DocManagerDBHelperTestClass();
			foreach (var db in dbNumber)
			{
				if (testDbHelper.DatabaseExists(db))
				{
					testDbHelper.DropDatabase(testDbHelper.GetDatabaseName(db));
				}
			}

			SetupS3RegistrySettings();
			var processingTask = new EDocsExternalStorageSizeCalculationTask();
			var logger = InitialiseTaskSchedule(processingTask);
			RunTaskSchedule(processingTask);

			var sizeData = DocManagerRegistry.Instance.EDocsExternalStorageSize.Value;
			AssertNotNullOrEmpty("External storage size data should not be null", sizeData);

			var sizeObject = JsonConvert.DeserializeObject<EDocsExternalStorageSize>(sizeData);
			AssertEquals("No StorageDocs database created, so calculated size is 0", 0, sizeObject.CalculatedSize);
			AssertEquals("Use S3 bucket size if no StorageDocs database created", true, sizeObject.DoNotUseCalculatedSize);

			AssertContains("Information|Started calculating eDocs external storage size", logger.ToString());
			AssertContains("Warning|Process cannot start because there is no document database.", logger.ToString());
			AssertContains("Information|Finished calculating eDocs external storage size", logger.ToString());
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "DSC", hostedServiceAttribute.Code);
				AssertEquals("Description", EDocsExternalStorageSizeCalculationTask.ServiceTaskDescription,
					hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "10minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("DefaultScheduleRunEvery", "1day", hostedServiceAttribute.DefaultScheduleRunEvery);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleStartAtLocal", "1hour", hostedServiceAttribute.DefaultScheduleStartAtLocal);
			});
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);
			SetStorageAccess("KeyId=idfortest;Secret=secretfortest");
			SetStorageServiceUrl("https://a.b.c");
			SetStorageBucketName("testbucket");

			using (EnvProxy.Instance.TemporaryServiceTaskContext("DSC", canRunInAnyBranch: true))
			{
				var serviceLogger = new TestServiceLogger();
				var processingTask = new EDocsExternalStorageSizeCalculationTask { ServiceLogger = serviceLogger };
				processingTask.RunTask();
			}

			AssertEquals("Should not have any errors reported", string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestESCHostedServiceRequirement()
		{
			var checkResult = string.Empty;
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				checkResult = EDocsExternalStorageSizeCalculationTask.CheckEDocsStorageProvider();
				AssertEquals("The registry setting 'System -> DocManager -> eDocs Storage' requires a value other than 'DB'.", checkResult);
			}

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				checkResult = EDocsExternalStorageSizeCalculationTask.CheckEDocsStorageProvider();
				AssertEquals("", checkResult);
			}

			checkResult = EDocsExternalStorageSizeCalculationTask.CheckIsHostedWithCargeWise();
			AssertEquals("Service not required if not hosted by CW and not EDI client", EDocsExternalStorageSizeCalculationTask.ServiceNotRequiredMessage, checkResult);

			EnvProxy.SetHostedLocationForTest("SYD");
			checkResult = EDocsExternalStorageSizeCalculationTask.CheckIsHostedWithCargeWise();
			AssertEquals("No error if hosted by CW", "", checkResult);
			EnvProxy.SetHostedLocationForTest("");

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(CargoWise.Definitions.Clients.EDI))
			{
				checkResult = EDocsExternalStorageSizeCalculationTask.CheckIsHostedWithCargeWise();
				AssertEquals("No error if it's EDI client", "", checkResult);
			}
		}

		StorageDocs[] PrepareFilesWithExternalStorageSizes(int[] sizes)
		{
			TestCaseHelper.ClearTable(StorageMain.Schema.TableName);

			if (!testDbHelper.DatabaseExists(1))
			{
				testDbHelper.CreateDatabase(1);
			}

			var doc1DatabaseName = testDbHelper.GetDatabaseName(1);
			TestCaseHelper.ClearTable($"{doc1DatabaseName}..{StorageDocs.Schema.TableName}");
			SetupS3RegistrySettings();

			var masterFactory1 = new DbBackendDocumentFactory(Factory);
			var parentDB1 = masterFactory1.New<StorageMain>();
			parentDB1.SM_DB = 1;
			parentDB1.SM_ParentFK = ZGuid.NewZGuid();
			parentDB1.SM_Type = Core.Constants.DocManagerCodes.Shipment;

			var docs = new List<StorageDocs>(sizes.Length);
			foreach (var size in sizes)
			{
				var doc = parentDB1.Documents.AddNew();
				doc.SC_DocType = "AAA";
				doc.SC_DataType = "XLS";
				doc.SC_Desc = "AAA Doc 11";
				doc.SC_IsPublished = false;
				doc.SC_ImageData = ZBlob.Empty;
				doc.SC_ExternalStorageSize = size;
				doc.SC_UncompressedSize = size == 0 ? 100 : size;
				docs.Add(doc);
			}

			masterFactory1.Save();
			return docs.ToArray();
		}

		void SetupS3RegistrySettings()
		{
			SetStorageProvider(Core.Constants.EDocsStorageProviders.Code.S3);
			SetStorageAccess("KeyId=idfortest;Secret=secretfortest");
			SetStorageServiceUrl("https://a.b.c");
			SetStorageBucketName("testbucket");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();

		readonly DocManagerDBHelperTestClass testDbHelper = new ();

		static void SetStorageAccess(string credentials)
		{
			SystemDataRegistry.Instance.EDocsStorageAccess.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, credentials);
		}

		static void SetStorageServiceUrl(string serviceUrl)
		{
			SystemDataRegistry.Instance.EDocsStorageServiceUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, serviceUrl);
		}

		static void SetStorageProvider(string provider)
		{
			SystemDataRegistry.Instance.EDocsStorageProvider.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, provider);
		}

		static void SetStorageBucketName(string bucketName)
		{
			SystemDataRegistry.Instance.DocManagerStorageBucketName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bucketName);
		}
	}
}
