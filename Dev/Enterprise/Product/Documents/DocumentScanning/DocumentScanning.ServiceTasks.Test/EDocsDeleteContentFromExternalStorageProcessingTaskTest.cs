using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.DocumentScanning.ServiceTasks.Test
{
	[TestedType(typeof(EDocsDeleteContentFromExternalStorageProcessingTask))]
	class EDocsDeleteContentFromExternalStorageProcessingTaskTest : ServiceTaskTestCase<EDocsDeleteContentFromExternalStorageProcessingTask>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "DED", hostedServiceAttribute.Code);
				AssertEquals("Description", "Delete From External Storage Processing Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "DOC", hostedServiceAttribute.Category);
				AssertEquals("MinimumPeriod", "1day", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestServiceTaskCanRunInAnyBranch()
		{
			var insertStorageDocsToDeleteSql = $@"
INSERT {StorageDocsToDeleteSchema.Constants.TableName} (SCD_PK, SCD_StorageDocIdentifier)
VALUES ('{Guid.NewGuid()}', '{Guid.NewGuid()}');";
			TestConnection.ExecuteNonQuery(insertStorageDocsToDeleteSql);

			var externalPersisterProvider = new Mock<IExternalPersisterProvider>();
			var externalPersister = new Mock<IExternalPersister>();
			externalPersister.Setup(p => p.Delete(It.IsAny<ZGuid>())).Returns(true);
			externalPersisterProvider.Setup(p => p.GetExternalPersister(It.IsAny<string>())).Returns(externalPersister.Object);

			using (ObjectFactory.Substitute(externalPersisterProvider.Object))
			using (EnvProxy.Instance.TemporaryServiceTaskContext("DED", canRunInAnyBranch: true))
			{
				var serviceLogger = new TestServiceLogger();
				new EDocsDeleteContentFromExternalStorageProcessingTask { ServiceLogger = serviceLogger }.RunTask();
			}

			AssertEquals("Should not have any errors reported", string.Empty, ErrorReporter.LastMessageReported);
		}

		protected override void SetUpCore()
		{
			TestCaseHelper.ClearTable(StorageDocsToDeleteSchema.Constants.TableName);
		}

		public void TestDeletedSuccessfully()
		{
			var task = new EDocsDeleteContentFromExternalStorageProcessingTask();
			var externalPersisterProvider = new Mock<IExternalPersisterProvider>();
			var externalPersister = new Mock<IExternalPersister>();
			externalPersister.Setup(p => p.Delete(It.IsAny<ZGuid>())).Returns(true);
			externalPersister.Setup(p => p.AllowWrite).Returns(true);
			externalPersisterProvider.Setup(p => p.GetExternalPersister(It.IsAny<string>())).Returns(externalPersister.Object);

			using (ObjectFactory.Substitute(externalPersisterProvider.Object))
			{
				var docsToDelete1 = Factory.New<StorageDocsToDelete>();
				docsToDelete1.SCD_StorageDocIdentifier = Guid.NewGuid();

				var docsToDelete2 = Factory.New<StorageDocsToDelete>();
				docsToDelete2.SCD_StorageDocIdentifier = Guid.NewGuid();

				Factory.Save();
				AssertEquals(true, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.PK, docsToDelete1.PK)));
				AssertEquals(true, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.PK, docsToDelete2.PK)));

				var logger = InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals(false, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.PK, docsToDelete1.PK)));
				AssertEquals(false, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.PK, docsToDelete2.PK)));
				AssertContains("Information|Run Completed, successfully deleted 2 eDocs from external storage", logger.ToString());
			}
		}

		public void TestDeleteFailedSilently()
		{
			var docsToDelete1 = Factory.New<StorageDocsToDelete>();
			docsToDelete1.SCD_StorageDocIdentifier = Guid.NewGuid();

			var docsToDelete2 = Factory.New<StorageDocsToDelete>();
			docsToDelete2.SCD_StorageDocIdentifier = Guid.NewGuid();
			Factory.Save();

			var task = new EDocsDeleteContentFromExternalStorageProcessingTask();
			var externalPersisterProvider = new Mock<IExternalPersisterProvider>();
			var externalPersister = new Mock<IExternalPersister>();
			externalPersister.Setup(p => p.Delete(docsToDelete1.SCD_StorageDocIdentifier)).Returns(false);
			externalPersister.Setup(p => p.Delete(docsToDelete2.SCD_StorageDocIdentifier)).Returns(true);
			externalPersister.Setup(p => p.AllowWrite).Returns(true);
			externalPersisterProvider.Setup(p => p.GetExternalPersister(It.IsAny<string>())).Returns(externalPersister.Object);
			using (ObjectFactory.Substitute(externalPersisterProvider.Object))
			{
				var logger = InitialiseTaskSchedule(task);
				RunTaskSchedule(task);

				AssertEquals("doc1", true, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.PK, docsToDelete1.PK)));
				AssertEquals("doc2", false, new BusinessObjectFactory().Exists(typeof(StorageDocsToDelete), new ZQuery(StorageDocsToDeleteSchema.PK, docsToDelete2.PK)));
				AssertContains("Information|Run Completed, successfully deleted 1 eDocs from external storage", logger.ToString());
			}
		}

		public void TestDeleteFailedWithException()
		{
			var docsToDelete1 = Factory.New<StorageDocsToDelete>();
			docsToDelete1.SCD_StorageDocIdentifier = Guid.NewGuid();

			var docsToDelete2 = Factory.New<StorageDocsToDelete>();
			docsToDelete2.SCD_StorageDocIdentifier = Guid.NewGuid();
			Factory.Save();

			var task = new EDocsDeleteContentFromExternalStorageProcessingTask();
			var externalPersisterProvider = new Mock<IExternalPersisterProvider>();
			var externalPersister = new Mock<IExternalPersister>();
			externalPersister.Setup(p => p.Delete(docsToDelete1.SCD_StorageDocIdentifier)).Throws(new ExternalStorageException("CEPH fails", "S3", null));
			externalPersister.Setup(p => p.Delete(docsToDelete2.SCD_StorageDocIdentifier)).Returns(true);
			externalPersister.Setup(p => p.AllowWrite).Returns(true);
			externalPersisterProvider.Setup(p => p.GetExternalPersister(It.IsAny<string>())).Returns(externalPersister.Object);
			using (ObjectFactory.Substitute(externalPersisterProvider.Object))
			{
				var logger = InitialiseTaskSchedule(task);
				AssertExceptionThrown<HostedServiceException>(() => RunTaskSchedule(task));
				AssertNotContains("Information|Run Completed", logger.ToString());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						StorageDocsToDeleteSchema.Constants.TableName,
						null),
				};
			}
		}

		public void TestHostedServiceRequirement()
		{
			var checkResult = EDocsDeleteContentFromExternalStorageProcessingTask.CheckEDocsStorageProvider();
			AssertEquals("The registry setting 'System -> DocManager -> eDocs Storage' requires a value other than 'DB'.", checkResult);
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				checkResult = EDocsDeleteContentFromExternalStorageProcessingTask.CheckEDocsStorageProvider();
				AssertEquals("", checkResult);
			}

			var methodInfo = typeof(EDocsDeleteContentFromExternalStorageProcessingTask).GetMethod(nameof(EDocsDeleteContentFromExternalStorageProcessingTask.CheckIsNotHostedWithCargowise));
			Assert("HostedServiceRequirement for not-hosted-by-WiseCloud is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestRunTask_CW1Hosted_ShouldNotRun()
		{
			EnvProxy.SetHostedLocationForTest("hosted-cw1.test");
			Assert(EnvProxy.IsHostedWithCargowise);

			AssertEquals("This service task is not required to run as it's hosted by WiseCloud.", EDocsDeleteContentFromExternalStorageProcessingTask.CheckIsNotHostedWithCargowise());
		}

		public void TestRunTask_NotCW1Hosted()
		{
			EnvProxy.SetHostedLocationForTest("");
			Assert(!EnvProxy.IsHostedWithCargowise);

			AssertEquals("", EDocsDeleteContentFromExternalStorageProcessingTask.CheckIsNotHostedWithCargowise());
		}

		public void TestTakesSharedAppLock()
		{
			using (var otherConnection = Db.NewExtraConnectionToMainDb())
			{
				var gotLock = otherConnection.TryGetLock(BaseEDocsContentExternalStorageTask.S3ModificationLockName, out SqlApplicationLock appLock);
				using (appLock)
				{
					Assert("Precondition", gotLock);

					var task = new EDocsDeleteContentFromExternalStorageProcessingTask();
					var logger = InitialiseTaskSchedule(task);
					RunTaskSchedule(task);

					AssertContains("Could not acquire shared S3 lock, skipping run.", logger.ToString());
				}
			}
		}

		public void TestServiceStopsIfNoAccessToExternalStorage()
		{
			var processingTask = new EDocsDeleteContentFromExternalStorageProcessingTask();

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
	}
}
