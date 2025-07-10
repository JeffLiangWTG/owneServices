using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using Enterprise.Registry.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.DbMaintenance.Testing
{
	[TestedType(typeof(DocManagerDatabaseCreatorTask))]
	sealed class DocManagerDatabaseCreatorTaskTest : ServiceTaskTestCase<DocManagerDatabaseCreatorTask>
	{
		DocManagerDatabaseCreatorTask ServiceTask;
		TestServiceLogger Logger;

		public void TestServiceTaskCanRunInAnyBranch()
		{
			Assert(GetHostedServiceAttributes().All(x => x.CanRunInAnyBranch));
		}

		public void TestFinishedProcessingLogs()
		{
			var helper = ObjectFactory.Get<DocumentScanning.Integration.IDocManagerDBHelper>();
			helper.LastWritableDatabaseWithFreeSpace(0, forceNewDatabase: true);
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				RunTaskSchedule(ServiceTask);

				AssertContains("Finished processing with no database created.", Logger.ToString());
			}
		}

		public void TestEDocsStorageProviderSetting()
		{
			var helper = ObjectFactory.Get<DocumentScanning.Integration.IDocManagerDBHelper>();
			var initialNumber = helper.LastWritableDatabaseWithFreeSpace(0, forceNewDatabase: true);

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				AssertEquals("The registry setting 'System -> DocManager -> eDocs Storage' requires a value equal to 'DB'.", DocManagerDatabaseCreatorTask.CheckShouldRun());
			}

			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				AssertEquals("", DocManagerDatabaseCreatorTask.CheckShouldRun());
				RunTaskSchedule(ServiceTask);
			}

			var diff = helper.LastWritableDatabaseWithFreeSpace(0, forceNewDatabase: true) - initialNumber;

			Assert("0-1 databases created", diff == 0 || diff == 1);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			ServiceTask = new DocManagerDatabaseCreatorTask();
			Logger = InitialiseAndRunTaskSchedule(ServiceTask);
		}
	}
}
