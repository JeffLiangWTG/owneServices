using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.TimeEngineScheduler.ServiceTask.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.TimeEngineScheduler.Integration.Constants;

namespace Enterprise.BufferManagement.Business.Test
{
	internal class TransferRuleSchedulerActionTest : BMSTestCaseWithFactory
	{
		public void Test_ShouldProcessTransferRules()
		{
			var log = new SimpleLogger();
			var schedulerAction = new TransferRuleSchedulerAction();
			var workflowPK = Guid.NewGuid();
			var schematicServiceMock = new Mock<ISchematicService>();

			ObjectFactory.Substitute(schematicServiceMock.Object);

			schematicServiceMock.Setup(s => s.ProcessTransferRules(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ILogger>()));

			schedulerAction.Execute(new CancellationToken(), Factory, log, workflowPK, ProcessHeaderSchema.Constants.Prefix, string.Empty, ZDateTime.UtcNow);

			AssertNoExceptionThrown(() =>
			{
				schematicServiceMock.Verify(s =>
					s.ProcessTransferRules(It.Is<IReadOnlyCollection<Guid>>(guids => guids.Single() == workflowPK), It.Is<SimpleLogger>(l => l == log)));
			});
		}

		public void Test_ShouldProcessTransferRulesForPKsInParameters()
		{
			var log = new SimpleLogger();
			var schedulerAction = new TransferRuleSchedulerAction();
			var jobWorkflowPK = Guid.NewGuid();
			var workflowPKs = new[]
			{
				Guid.NewGuid(),
				Guid.NewGuid(),
				Guid.NewGuid(),
			};
			var schematicServiceMock = new Mock<ISchematicService>();

			ObjectFactory.Substitute(schematicServiceMock.Object);

			schematicServiceMock.Setup(s => s.ProcessTransferRules(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ILogger>()));

			schedulerAction.Execute(new CancellationToken(), Factory, log, jobWorkflowPK, ProcessHeaderSchema.Constants.Prefix, workflowPKs.JsonSerialize(), ZDateTime.UtcNow);

			AssertNoExceptionThrown(() =>
			{
				schematicServiceMock.Verify(s =>
					s.ProcessTransferRules(It.Is<IReadOnlyCollection<Guid>>(guids => AssertParameters(guids, workflowPKs)), It.Is<SimpleLogger>(l => l == log)));
			});
		}

		bool AssertParameters(IEnumerable<Guid> parameters, IEnumerable<Guid> expectedPKs)
		{
			AssertContainsExactElementsInAnyOrder(expectedPKs, parameters);
			return true;
		}

		[TestDate(2023, 1, 16)]
		public void TestEndToEnd_ShouldProcessTransferRules_WhenChangeESD_OnWorkflowOrJobWorkflow()
		{
			BMSRegistry.Instance.TimeBeforeDeletingOldScheduledTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var system = BMSTestHelper.CreateSystem(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var bucket1 = BMSTestHelper.CreateBucket(system, "b1");
			var bucket2 = BMSTestHelper.CreateBucket(system, "b2");
			var link = BMSTestHelper.LinkComponents(bucket1, bucket2);

			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflowWithDoNotStartBeforeDate = BMSTestHelper.CreateWorkflow(jobHeader, "workflowWithDoNotStartBeforeDate", bucket1);
			workflowWithDoNotStartBeforeDate.FH_DoNotStartBeforeDate = ZDateTime.UtcNow;
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow1", bucket1);
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow2", bucket1);
			var workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow3", bucket1);

			var workflowWithChildren = BMSTestHelper.CreateWorkflow(jobHeader, "workflowWithChildren", bucket1);
			var workflowChild = BMSTestHelper.CreateWorkflow(jobHeader, "workflowChild", bucket1);
			var workflowGrandChild = BMSTestHelper.CreateWorkflow(jobHeader, "workflowGrandChild", bucket1);

			var workflowPKs = new[]
			{
				workflowWithDoNotStartBeforeDate.PK.ToGuid(),
				workflow1.PK.ToGuid(),
				workflow2.PK.ToGuid(),
				workflow3.PK.ToGuid(),
				workflowWithChildren.PK.ToGuid(),
				workflowChild.PK.ToGuid(),
				workflowGrandChild.PK.ToGuid(),
			};

			BMSTestHelper.MakeChildOf(workflowWithChildren, workflowChild);
			BMSTestHelper.MakeChildOf(workflowChild, workflowGrandChild);

			Factory.Save();

			var logger = new BufferManagementLogger();
			var tas = new SchedulerTask_ForTest(logger);
			var scheduledActions = Factory.Load<TimeActionSchedule>(new ZQuery());

			tas.RunTask(CancellationToken.None);

			AssertEquals("No actions processed", @"Debug - Started executing scheduled actions
Debug - Nothing to process
Debug - Finished executing scheduled actions
", logger.ToString());
			Assert("Should not create schedules, workflowWithDoNotStartBeforeDate has ESD but it is now, so PVE will process it", scheduledActions.Length == 0);

			CombineAssertions("Should not transfer, no actions to process", () =>
			{
				AssertEquals(bucket1.PK, workflowWithDoNotStartBeforeDate.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow3.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflowWithChildren.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflowChild.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflowGrandChild.FH_FC_CurrentComponent);
			});

			var oneHourInTheFuture = ZDateTime.UtcNow.AddHours(1);
			var twoHourInTheFuture = ZDateTime.UtcNow.AddHours(2);

			jobHeader.FH_DoNotStartBeforeDate = oneHourInTheFuture;
			workflowWithDoNotStartBeforeDate.FH_DoNotStartBeforeDate = oneHourInTheFuture;

			workflow1.FH_DoNotStartBeforeDate = twoHourInTheFuture;
			workflow2.FH_DoNotStartBeforeDate = twoHourInTheFuture;
			workflow3.FH_DoNotStartBeforeDate = twoHourInTheFuture;

			Factory.Save();
			scheduledActions = Factory.Load<TimeActionSchedule>(new ZQuery());

			Assert("Should create schedules", scheduledActions.Length == 5);
			AssertContainsExactElementsInAnyOrder("Should create only correct schedulers", new[]
			{
				jobHeader.PK,
				workflowWithDoNotStartBeforeDate.PK,
				workflow1.PK,
				workflow2.PK,
				workflow3.PK,
			}, scheduledActions.Select(s => s.TAS_TargetPK));

			var scheduledActionsSplited = scheduledActions.Split(s => s.TAS_TargetPK == jobHeader.PK);

			var headerActionParameters = scheduledActionsSplited.MatchingSet.Single().TAS_JsonParameter.ToString().JsonDeserialize<Guid[]>();
			AssertContainsExactElementsInAnyOrder("Header parameters should only contains workflowPKs that do not have ESD", new[] {
				workflowWithChildren.PK,
				workflowChild.PK,
				workflowGrandChild.PK,
			}, headerActionParameters);
			Assert("Normal workflows parameter should be empty", scheduledActionsSplited.NonMatchingSet.All(s => s.TAS_JsonParameter == ZString.Empty));

			logger.Clear();
			tas.RunTask(CancellationToken.None);

			AssertEquals("No actions processed, all were scheduled for the future", @"Debug - Started executing scheduled actions
Debug - Nothing to process
Debug - Finished executing scheduled actions
", logger.ToString());
			CombineAssertions("Should not transfer, all actions are still in the future", () =>
			{
				AssertEquals(bucket1.PK, workflowWithDoNotStartBeforeDate.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow3.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflowWithChildren.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflowChild.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflowGrandChild.FH_FC_CurrentComponent);
			});

			TestDateAttribute.AddMinutes(90);// Going to the future +1h:30m

			logger.Clear();
			tas.RunTask(CancellationToken.None);

			CombineAssertions("Should transfer only workflows with ESD +1h, now is in the future", () =>
			{
				AssertEquals(bucket2.PK, workflowWithDoNotStartBeforeDate.FH_FC_CurrentComponent);
				AssertEquals(bucket2.PK, workflowWithChildren.FH_FC_CurrentComponent);
				AssertEquals(bucket2.PK, workflowChild.FH_FC_CurrentComponent);
				AssertEquals(bucket2.PK, workflowGrandChild.FH_FC_CurrentComponent);
			});

			var closedScheduledActions = new BusinessObjectFactory().Load<TimeActionSchedule>(new ZQuery(TimeActionScheduleSchema.TAS_ExecutionStatus, TimeActionScheduleStatus.Closed));
			AssertContainsExactElementsInAnyOrder("Should close correct actions", new[]
			{
				jobHeader.PK,
				workflowWithDoNotStartBeforeDate.PK
			}, closedScheduledActions.Select(s => s.TAS_TargetPK));

			AssertContains("Action processed transfer using ResponsiveTransferRuleProcessor", "processing transfer rules using ResponsiveTransferRuleProcessor", logger.ToString());
			AssertContains("ResponsiveTransferRuleProcessor runs transfer rules", "workflows transferred", logger.ToString());
			CombineAssertions("Should not transfer workflow with ESD +2h, still in the past", () =>
			{
				AssertEquals(bucket1.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals(bucket1.PK, workflow3.FH_FC_CurrentComponent);
			});

			TestDateAttribute.AddHours(1);// Going to the future +1h

			logger.Clear();
			tas.RunTask(CancellationToken.None);

			AssertContains("Action processed transfer using ResponsiveTransferRuleProcessor", "processing transfer rules using ResponsiveTransferRuleProcessor", logger.ToString());
			AssertContains("ResponsiveTransferRuleProcessor runs transfer rules", "workflow transferred", logger.ToString());
			CombineAssertions("Should transfer workflows with ESD +2h, now is in the future", () =>
			{
				AssertEquals(bucket2.PK, workflow1.FH_FC_CurrentComponent);
				AssertEquals(bucket2.PK, workflow2.FH_FC_CurrentComponent);
				AssertEquals(bucket2.PK, workflow3.FH_FC_CurrentComponent);
			});

			closedScheduledActions = new BusinessObjectFactory().Load<TimeActionSchedule>(new ZQuery());
			Assert("All schedules should be closed", closedScheduledActions.All(s => s.TAS_ExecutionStatus == TimeActionScheduleStatus.Closed));
		}
	}
}
