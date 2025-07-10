using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.ServiceTasks.Testing
{
	public class QualityIterationHelperTest : TestCaseWithFactory
	{
		public void TestCreateQualityIteration_WhenContainmentBarrierTaskNotMarkedAsContainmentBarrier_ShouldNotAttemptToCreateQualityIteration()
		{
			BMSTestHelper.EnableBMSInRegistry();
			var staffForSomeReason = Factory.NewWithValidTestData<GlbStaff>();
			staffForSomeReason.GS_Code = QualityIterationHelper.FailedQualityIterationUserCode;
			staffForSomeReason.GS_LoginName = "NyanCat";
			MasterFilesTestHelper.AddTaskTypesToRegistry("WKI", ";)", ":S");
			MasterFilesTestHelper.AddIterationReasonToRegistry("WKI", "OOH", "Oooh wee!");
			var registryItemValue = EDIDataRegistry.Instance.QualityIterationAssignments.Value;
			registryItemValue.IsDefaultOptionSelected = true;
			EDIDataRegistry.Instance.QualityIterationAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemValue);
			var workflow = BMSTestHelper.CreateWorkflowAndParents<NewWorkItem>(Factory, "Don't forget to vote yes!");
			var task = BMSTestHelper.CreateTask(workflow, taskType: ";)");
			var nonQcbTask = (WorkItemProcessTask)BMSTestHelper.CreateTask(workflow, taskType: ":S");
			Factory.Save();
			var logger = new SimpleLogger();
			AssertEquals("Task proposed as the QCB task isn't actually marked as a containment barrier in the registry, so we can't create a QI. It certainly shouldn't throw an exception. That would bring down DAT's ability to patch changesets.", false, TryCreateQualityIteration(nonQcbTask, logger));
			AssertMultilineASCIIEquals("Logs are useful, good, happy, convenient, good", "Warning: Task Type [:S] is not configured as a Containment Barrier in the Task Types registry item.", logger.ToString());
			MasterFilesTestHelper.SetAsQualityContainmentBarrierTaskType(":S", "WKI");
			logger = new SimpleLogger();
			AssertEquals("Now that we've enabled the task type as a containment barrier, a quality iteration can be created.", true, TryCreateQualityIteration(nonQcbTask, logger));
			AssertMultilineASCIIEquals("Nothing to log because everything is fine.", "", logger.ToString());
		}

		static bool TryCreateQualityIteration(WorkItemProcessTask task, ILogger logger)
		{
			var info = new QualityIterationInfo(task, "Stahp", "OOH", task.P9_GS_NKAssignedStaffMember);
			var helper = new QualityIterationHelper(info, task.Factory, s =>
			{
			});
			return helper.CreateQualityIteration(shouldStartFromCurrentQcbTask: true, logger: logger);
		}
	}
}
