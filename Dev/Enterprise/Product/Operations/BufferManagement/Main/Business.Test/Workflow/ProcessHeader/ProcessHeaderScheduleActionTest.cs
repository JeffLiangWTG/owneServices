using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ProcessHeaderScheduleActionTest : TestCaseWithFactory
	{
		[TestDate(2023, 1, 15)]
		public void Test_ShouldScheduleNewWorkflow_IfJobHeaderHasDoNotStartBeforeDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			Factory.Save();

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddSeconds(1);

			AssertEquals(false, workflow1.FH_DoNotStartBeforeDate.IsValid);

			Factory.Save();

			AssertNoExceptionThrown("First workflow should be scheduled as json under job header", () =>
			{
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
					Times.Once());
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(
					   TransferRuleSchedulerAction.Code,
					   jobHeader.FH_DoNotStartBeforeDate,
					   jobHeader.PK,
					   ProcessHeaderSchema.Constants.Prefix,
					   It.Is<string>(parameter => AssertJsonParameterParameter(parameter, workflow1.PK)),
					   null,
					   null,
					   null,
					   false,
					   null)
				   );
			});

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			Factory.Save();

			AssertNoExceptionThrown("Second workflow should get its own schedule using job header's start date", () =>
			{
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
					Times.Exactly(2));
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(
					   TransferRuleSchedulerAction.Code,
					   jobHeader.FH_DoNotStartBeforeDate,
					   workflow2.PK,
					   ProcessHeaderSchema.Constants.Prefix,
					   null,
					   null,
					   null,
					   null,
					   false,
					   null)
				   );
			});
		}

		[TestDate(2023, 1, 15)]
		public void Test_ShouldScheduleNewWorkflow_AndOverrideJobHeaderDoNotStartBeforeDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = jobHeader.ProcessHeaders.AddNew();
			Factory.Save();

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddSeconds(1);

			AssertEquals(false, workflow1.FH_DoNotStartBeforeDate.IsValid);

			Factory.Save();

			AssertNoExceptionThrown("First workflow should be scheduled as json under job header", () =>
			{
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
					Times.Once());
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(
					   TransferRuleSchedulerAction.Code,
					   jobHeader.FH_DoNotStartBeforeDate,
					   jobHeader.PK,
					   ProcessHeaderSchema.Constants.Prefix,
					   It.Is<string>(parameter => AssertJsonParameterParameter(parameter, workflow1.PK)),
					   null,
					   null,
					   null,
					   false,
					   null)
				   );
			});

			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			workflow2.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddSeconds(1337);
			Factory.Save();

			AssertNoExceptionThrown("Second workflow should get its own schedule using its own start date", () =>
			{
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
					Times.Exactly(2));
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(
					   TransferRuleSchedulerAction.Code,
					   workflow2.FH_DoNotStartBeforeDate,
					   workflow2.PK,
					   ProcessHeaderSchema.Constants.Prefix,
					   null,
					   null,
					   null,
					   null,
					   false,
					   null)
				   );
			});
		}

		[TestDate(2023, 1, 15)]
		public void Test_ShouldScheduleActionWithParamenters_WhenChangeJobHeaderDoNotStartBeforeDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowWithDoNotStartBeforeDate = BMSTestHelper.CreateWorkflow(jobHeader, "workflowWithDoNotStartBeforeDate");
			workflowWithDoNotStartBeforeDate.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			var workflowWithChildren = BMSTestHelper.CreateWorkflow(jobHeader, "workflowWithChildren");
			var workflowChild = BMSTestHelper.CreateWorkflow(jobHeader, "workflowChild");
			var workflowGrandChild = BMSTestHelper.CreateWorkflow(jobHeader, "workflowGrandChild");

			BMSTestHelper.MakeChildOf(workflowWithChildren, workflowChild);
			BMSTestHelper.MakeChildOf(workflowChild, workflowGrandChild);

			Factory.Save();

			jobHeader.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddSeconds(1);

			AssertEquals(false, workflow.FH_DoNotStartBeforeDate.IsValid);

			Factory.Save();

			AssertNoExceptionThrown("Should schedule action with right parameters", () =>
			{
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
					Times.Once());
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(
					   TransferRuleSchedulerAction.Code,
					   jobHeader.FH_DoNotStartBeforeDate,
					   jobHeader.PK,
					   ProcessHeaderSchema.Constants.Prefix,
					   It.Is<string>(parameter => AssertJsonParameterParameter(parameter, workflow.PK, workflowWithChildren.PK, workflowChild.PK, workflowGrandChild.PK)),
					   null,
					   null,
					   null,
					   false,
					   null)
				   );
			});
		}

		static bool AssertJsonParameterParameter(string jsonParameter, params ZGuid[] expectedPKs)
		{
			var parameterPKs = jsonParameter.JsonDeserialize<ZGuid[]>();

			AssertContainsExactElementsInAnyOrder(expectedPKs, parameterPKs);
			return true;
		}

		[TestDate(2023, 1, 15)]
		public void Test_ShouldScheduleActionForTransfer_WhenCreateWithDoNotStartBeforeDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			workflow.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddSeconds(1);

			Factory.Save();

			AssertScheduleWasScheduled(workflow);
		}

		[TestDate(2023, 1, 15)]
		public void Test_ShouldScheduleActionForTransfer_WhenChangeDoNotStartBeforeDate()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");

			Factory.Save();

			workflow.FH_DoNotStartBeforeDate = ZDateTime.UtcNow.AddSeconds(1);

			Factory.Save();

			AssertScheduleWasScheduled(workflow);
		}

		[TestDate(2023, 1, 15)]
		public void Test_ShouldOnlyScheduleActionForTransfer_WhenChangeDoNotStartBeforeDateToFuture()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow");
			Factory.Save();

			actionScheduleProviderMock.Verify(p =>
				p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
				Times.Never());

			var now = ZDateTime.UtcNow;
			workflow.FH_DoNotStartBeforeDate = now;
			Factory.Save();

			actionScheduleProviderMock.Verify(p =>
				p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
				 Times.Never());

			workflow.FH_DoNotStartBeforeDate = now.AddSeconds(-1);
			Factory.Save();

			actionScheduleProviderMock.Verify(p =>
				p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
				Times.Never());

			workflow.FH_DoNotStartBeforeDate = now.AddSeconds(2);
			Factory.Save();

			AssertScheduleWasScheduled(workflow);

			workflow.FH_DoNotStartBeforeDate = ZDateTime.Empty;
			Assert(!workflow.FH_DoNotStartBeforeDate.IsValid);
			actionScheduleProviderMock.Reset();
			Factory.Save();

			actionScheduleProviderMock.Verify(p =>
				p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
				Times.Never());
		}

		void AssertScheduleWasScheduled(ProcessHeader workflow)
		{
			AssertNoExceptionThrown("Should schedule action with right parameters", () =>
			{
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(It.IsAny<string>(), It.IsAny<ZDateTime>(), It.IsAny<ZGuid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ZGuid?>(), It.IsAny<ZGuid?>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<BusinessObjectFactory>()),
					Times.Once());
				actionScheduleProviderMock.Verify(p =>
					p.ScheduleAction(
						TransferRuleSchedulerAction.Code,
						workflow.FH_DoNotStartBeforeDate,
						workflow.PK,
						ProcessHeaderSchema.Constants.Prefix,
						null,
						null,
						null,
						null,
						false,
						null),
				   Times.Once);
			});
		}

		readonly Mock<IActionScheduleProvider> actionScheduleProviderMock = new Mock<IActionScheduleProvider>();

		protected override void SetUp()
		{
			BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			ObjectFactory.Substitute(actionScheduleProviderMock.Object);

			base.SetUp();
		}
	}
}
