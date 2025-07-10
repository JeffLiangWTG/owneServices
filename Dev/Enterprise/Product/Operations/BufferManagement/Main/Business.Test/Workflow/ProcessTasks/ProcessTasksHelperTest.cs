using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessTasksHelperTest : BMSTestCaseWithFactory
	{
		public void TestCanDelete_ShouldReturnFalse_WhenServiceTaskCanNotDeleteTasks()
		{
			var helper = new ProcessTaskHelper();
			var task = Factory.New<ProcessTask>();
			var serviceContainer = Factory.ServiceContainer;

			AssertEquals("task is not in DB, so task can be deleted", helper.CanDelete(task), true);

			Factory.Save();

			AssertEquals("ServiceTaskCodeService was not added, so task can be deleted", helper.CanDelete(task), true);

			void AssertCanBeDeleteByServiceTask(string serviceTaskCode)
			{
				serviceContainer.RemoveService<ServiceTaskCodeService>();
				serviceContainer.AddService(new ServiceTaskCodeService(serviceTaskCode));

				AssertEquals($"{serviceTaskCode} is running, so task can NOT be deleted", helper.CanDelete(task), false);
			}

			CombineAssertions(() =>
			{
				AssertCanBeDeleteByServiceTask(CapabilityTaskAutoAssignmentServiceTask.Code);
				AssertCanBeDeleteByServiceTask(ReleaseGateRunnerServiceTask.Code);
				AssertCanBeDeleteByServiceTask(SchematicTransferLoopMonitorServiceTask.Code);
				AssertCanBeDeleteByServiceTask(StaggeredReleaseDelayCalculatorTask.Code);
				AssertCanBeDeleteByServiceTask(TagMonitorServiceTask.Code);
				AssertCanBeDeleteByServiceTask(TagServiceTask.Code);
				AssertCanBeDeleteByServiceTask(TransferRuleRunnerServiceTask.Code);
				AssertCanBeDeleteByServiceTask("PVE");
				AssertCanBeDeleteByServiceTask("TAS");
			});
		}
	}
}
