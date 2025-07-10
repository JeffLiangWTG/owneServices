using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.LogWalker.Testing
{
	[TestedType(typeof(WorkflowEventsPublisher))]
	sealed class WorkflowEventsPublisherTest : LogSubscriberTest<WorkflowEventsPublisher>
	{
		public void TestProcessLogs()
		{
			var logParent1 = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();
			var logParent2 = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();
			var logParent3 = Factory.NewWithValidTestData<DummyEnterpriseBusinessObjectWithWorkflow>();

			var milestone1 = logParent1.WorkflowItems.Milestones.AddNew();
			var milestone2 = logParent2.WorkflowItems.Milestones.AddNew();
			var milestone3 = logParent3.WorkflowItems.Milestones.AddNew();
			var milestone4 = logParent3.WorkflowItems.Milestones.AddNew();

			milestone1.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
			milestone2.TriggerConditions.TriggerEventCode = Events.DepartureCode;
			milestone3.TriggerConditions.TriggerEventCode = Events.AttachedCode;
			milestone4.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			var subscription1 = CreateSubscription(DummyWorkflowDescriptor.Instance.Code);
			var subscription2 = CreateSubscription("SSS");
			var subscription3 = CreateSubscription(DummyWorkflowDescriptor.Instance.Code);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			var logs = new[]
			{
				new QueuedLogForTesting(newFactory) { PK = ZGuid.NewZGuid(), SJ_TargetID = subscription1.PK, SJ_Reference = "logs[0]", SJ_ParentID = logParent1.PK, SJ_SE_NKEvent = Events.ArrivalCode, SJ_EventTime = new ZDateTime(2017, 6, 1) },
				new QueuedLogForTesting(newFactory) { PK = ZGuid.NewZGuid(), SJ_TargetID = subscription2.PK, SJ_Reference = "logs[1]", SJ_ParentID = logParent2.PK, SJ_SE_NKEvent = Events.DepartureCode, SJ_EventTime = new ZDateTime(2017, 6, 2) },
				new QueuedLogForTesting(newFactory) { PK = ZGuid.NewZGuid(), SJ_TargetID = subscription3.PK, SJ_Reference = "logs[2]", SJ_ParentID = logParent3.PK, SJ_SE_NKEvent = Events.AttachedCode, SJ_EventTime = new ZDateTime(2017, 6, 3) },
				new QueuedLogForTesting(newFactory) { PK = ZGuid.NewZGuid(), SJ_TargetID = subscription3.PK, SJ_Reference = "logs[3]", SJ_ParentID = logParent3.PK, SJ_SE_NKEvent = Events.FlightManifestedCode, SJ_EventTime = new ZDateTime(2017, 6, 4) }
			};

			DummyWorkflowDescriptor.Instance.ExpectLogParentsToFireWorkflowForEvent(subscription1, WorkflowEventsPublisher.PublishedEvent.Create(logs[0]), new[] { logParent1 });
			DummyWorkflowDescriptor.Instance.ExpectLogParentsToFireWorkflowForEvent(subscription3, WorkflowEventsPublisher.PublishedEvent.Create(logs[2]), new[] { logParent3 });
			DummyWorkflowDescriptor.Instance.ExpectLogParentsToFireWorkflowForEvent(subscription3, WorkflowEventsPublisher.PublishedEvent.Create(logs[3]), new[] { logParent3 });

			newFactory.ResetDatabaseLoadCount();

			new WorkflowEventsPublisher().ProcessLogs(logs);

			var expectedHits = new Dictionary<string, int>
				{
					{ DummyBizoSchema.Constants.TableName, 1 },
					{ ProcessTaskNotificationSchema.Constants.TableName, 2 },
					{ ProcessTasksSchema.Constants.TableName, 3 },
					{ StmEventSubscriptionSchema.Constants.TableName, 1 },
					{ GlbBranchSchema.Constants.TableName, 1 },
					{ GlbCompanySchema.Constants.TableName, 1 },
					{ JobHeaderSchema.Constants.TableName, 2 }
				};
			AssertDbHits(expectedHits, newFactory);

			newFactory.Save();

			AssertEquals("milestone1 fired", new ZDateTime(2017, 6, 1), milestone1.P9_ActualDate);
			AssertEquals("Subscription has mismatching WorkflowDescriptor, milestone2 not fired", ZDateTime.Empty, milestone2.P9_ActualDate);
			AssertEquals("milestone3 fired", new ZDateTime(2017, 6, 3), milestone3.P9_ActualDate);
			AssertEquals("Different event on publisher, milestone4 not fired", ZDateTime.Empty, milestone4.P9_ActualDate);

			DummyWorkflowDescriptor.Instance.VerifyGetLogParentsToFireWorkflowForEventCalls();
		}

		public void TestPublishedEventCreate_DeferFiringWorkflow()
		{
			var log = new QueuedLogForTesting(Factory) { SJ_IsDelayFired = false };
			var logDelayFiringWorkflow = new QueuedLogForTesting(Factory) { SJ_IsDelayFired = true };
			Assert(!WorkflowEventsPublisher.PublishedEvent.Create(log).SL_FireWorkflow);
			Assert(WorkflowEventsPublisher.PublishedEvent.Create(logDelayFiringWorkflow).SL_FireWorkflow);
		}

		public override void TestStmJobQueueIsNotSubscribedForEvents()
		{
			Assert(true);
		}

		StmEventSubscription CreateSubscription(ZString agentDescriptor)
		{
			var subscripion = Factory.NewWithValidTestData<StmEventSubscription>();
			subscripion.SES_AgentDescriptor = agentDescriptor;

			return subscripion;
		}
	}
}
