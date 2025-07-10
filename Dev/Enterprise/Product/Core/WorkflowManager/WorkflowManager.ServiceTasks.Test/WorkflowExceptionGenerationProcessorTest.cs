using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class WorkflowExceptionGenerationProcessorTest : TestCaseWithFactory
	{
		[TestDate(2016, 7, 3)]
		public void TestDontBotherApplyingTemplatesDuringThisServiceTask()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var overdueMilestone = CreateOverdueMilestone(Dummy);
			SaveFactoryWithoutRaisingMilestoneException();

			// Create workflow template in new factory to avoid applying template
			var template = Factory.CreateNewFactory().NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var t1 = template.WorkflowItems.Tasks.AddNew();
			t1.P9_Description = "Japan";
			template.Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(3);
			DummyWithWorkflow.ShouldApplyWorkflowTemplateOnSave.Value = true;
			Processor.Process(Notifications);

			job.WorkflowItems.Reload(true);
			AssertEquals(0, job.WorkflowItems.Tasks.Count);
		}

		[TestDate]
		public void TestExceptionsRaisedInDifferentTimeZones()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "COM";

			var auBranch = Factory.NewWithValidTestData<GlbBranch>();
			auBranch.GB_GC = company.PK;
			auBranch.GB_Code = "AUB";
			auBranch.GB_RL_NKHomePort = "AUSYD";

			var usBranch = Factory.NewWithValidTestData<GlbBranch>();
			usBranch.GB_GC = company.PK;
			usBranch.GB_Code = "USB";
			usBranch.GB_RL_NKHomePort = "USCHI";

			var ukBranch = Factory.NewWithValidTestData<GlbBranch>();
			ukBranch.GB_GC = company.PK;
			ukBranch.GB_Code = "UKB";
			ukBranch.GB_RL_NKHomePort = "GBLON";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, ukBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var milestone1 = CreateMilestone(Dummy);
				milestone1.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
				milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
				milestone1.P9_ScheduledDate = ZDateTime.Now.AddHours(1);

				SaveFactoryWithoutRaisingMilestoneException();

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, auBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					Processor.Process(Notifications);
				}
				Dummy.WorkflowItems.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
				AssertEquals("Shifting around time zones doesn't raise the exception", 0, Dummy.WorkflowItems.Exceptions.Count);

				using (Env.SetTemporaryUserContext(Env.CurrentUserPK, usBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					Processor.Process(Notifications);
				}
				Dummy.WorkflowItems.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
				AssertEquals("Shifting around time zones doesn't raise the exception", 0, Dummy.WorkflowItems.Exceptions.Count);
			}
		}

		[TestDate(2016, 7, 3)]
		public void TestMilestoneWithEmptyExceptionTypeDoesNotCreateException()
		{
			var milestone1 = CreateMilestone(Dummy);
			milestone1.P9_SE_NKExceptionEvent = ZString.Empty;
			milestone1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			milestone1.P9_ScheduledDate = ZDateTime.UtcNow.AddDays(1);

			var milestone2 = CreateMilestone(Dummy);
			milestone2.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			milestone2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01.Code;
			milestone2.P9_ScheduledDate = ZDateTime.UtcNow.AddDays(1);

			SaveFactoryWithoutRaisingMilestoneException();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(3);

			Processor.Process(Notifications);

			Dummy.WorkflowItems.Reload(reLoadExistingRows: true, assumeRowsMissingFromQueryResultsAreDeleted: true);
			AssertEquals("1 exception", 1, Dummy.WorkflowItems.Exceptions.Count);
		}

		[TestDate(2000, 1, 2, 11, 30, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestGenerateExceptionFromOverdueMilestone()
		{
			ProcessTask scheduledMilestone = CreateScheduledMilestone(Dummy);
			ProcessTask unscheduledMilestone = CreateUnscheduledMilestone(Dummy);
			ProcessTask completedMilestone = CreateCompletedMilestone(Dummy);
			ProcessTask overdueMilestone = CreateOverdueMilestone(Dummy);
			ProcessTask expiredMilestone = CreateExpiredMilestone(Dummy);
			SaveFactoryWithoutRaisingMilestoneException();

			Processor.Process(Notifications);
			Dummy.WorkflowItems.Load();
			AssertEquals("2 exception generated for overdue milestones", 2, Dummy.WorkflowItems.Exceptions.Count);

			Processor.Process(Notifications);
			Dummy.WorkflowItems.Load();
			AssertEquals("No additional exception generated", 2, Dummy.WorkflowItems.Exceptions.Count);

			ProcessTask overdueMilestone2 = CreateOverdueMilestone(Dummy);
			overdueMilestone2.TriggerConditions.TriggerEventCode = Events.BookingConfirmed.Code;
			SaveFactoryWithoutRaisingMilestoneException();
			Processor.Process(Notifications);
			Dummy.WorkflowItems.Load();
			AssertEquals("1 new exception generated for a subsequent overdue milestone", 3, Dummy.WorkflowItems.Exceptions.Count);
		}

		[TestDate(2000, 1, 2, 11, 30, 0)]
		public void TestGenerateExceptionFromOverdueMilestone_ExceptionActualDate()
		{
			var expiredMilestone = CreateExpiredMilestone(Dummy);
			expiredMilestone.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			expiredMilestone.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			expiredMilestone.TriggerConditions.TriggerConditionValue = "REFERENCE";
			//This log does not fire the milestone and should not be used to set the actual date of the exception generated
			Dummy.Logs.AddNew(Events.CustomisableEvent00, ZDateTimeOffset.Now.AddDays(10));
			SaveFactoryWithoutRaisingMilestoneException();

			AssertEquals(ZDateTime.Empty, expiredMilestone.P9_ActualDate);

			Processor.Process(Notifications);
			Dummy.WorkflowItems.Load();
			AssertEquals("1 exception generated for overdue milestones", 1, Dummy.WorkflowItems.Exceptions.Count);

			var exception = Dummy.WorkflowItems.Exceptions[0];
			exception.P9_ReferencedID = Guid.NewGuid();
			AssertEquals("P9_ReferencedID setter should not update the actual date of an exception", ZDateTime.Now, exception.P9_ActualDate);
		}

		[TestDate(2000, 1, 2, 11, 30, 0)]
		public void TestDontGenerateExceptionOnCancelledJob()
		{
			var repository = new RowWrapperRepository();
			var row = repository.CreateShipment().Row;
			repository.Save();

			var shipment = Factory.Load<Integration.Forwarding.IForwardingShipment>(new Guid(row[JobShipmentSchema.PK].ToString()));
			((BusinessObject)shipment).FillWithValidTestData();

			var workflowProvider = (IWorkflowProvider)shipment;
			var milestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			milestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			milestone.P9_ScheduledDate = ZDateTime.Now.AddDays(1);

			var milestone2 = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent01Code;
			milestone2.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			milestone2.P9_ScheduledDate = ZDateTime.Now.AddDays(3);

			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(2).ToDateTime();

			Processor.Process(Notifications);
			workflowProvider.WorkflowItems.Load();
			AssertEquals("Precondition: 1 exception from missed milestone", 1, workflowProvider.WorkflowItems.Exceptions.Count);

			//Hack to cancel shipment without deleting all open milestones
			row[JobShipmentSchema.JS_IsCancelled] = true;
			repository.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(4).ToDateTime();

			Processor.Process(Notifications);
			workflowProvider.WorkflowItems.Load();
			AssertEquals("No more exceptions created as job is cancelled", 1, workflowProvider.WorkflowItems.Exceptions.Count);
		}

		[TestDate(2000, 1, 2, 11, 30, 0)]
		public void TestCancelledJobsThatDontDeleteTasksDontGlitchOut()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(CancellableDummy);
			DummyProcessTaskLoadStrategy.OverriddenTypeForLoad.Value = typeof(DummyTaskThatNoCancel);

			var workflowProvider = Factory.New<CancellableDummy>();
			var milestone = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			milestone.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			milestone.P9_ScheduledDate = ZDateTime.Now.AddDays(1);

			var milestone2 = workflowProvider.WorkflowItems.Milestones.AddNew();
			milestone2.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent01Code;
			milestone2.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			milestone2.P9_ScheduledDate = ZDateTime.Now.AddDays(3);

			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddDays(2).ToDateTime();

			Processor.Process(Notifications);
			workflowProvider.WorkflowItems.Load();
			AssertEquals("Precondition: 1 exception from missed milestone", 1, workflowProvider.WorkflowItems.Exceptions.Count);

			var newFactory = new BusinessObjectFactory();
			newFactory.Load<CancellableDummy>(workflowProvider.PK).IsCancelled = true;
			newFactory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddDays(4).ToDateTime();

			Processor.Process(Notifications);
			workflowProvider.WorkflowItems.Load();
			AssertEquals("No more exceptions created as job is cancelled", 1, workflowProvider.WorkflowItems.Exceptions.Count);

			var otherFactory = new BusinessObjectFactory();
			milestone2 = otherFactory.Load<ProcessTask>(milestone2.PK);
			AssertNotEquals("But still expect Exception date to have a value because otherwise the service task will load it again. Oops.", ZDateTime.Empty, milestone2.P9_MilestoneExceptionAdded);
		}

		class CancellableDummy : DummyWithWorkflow, ICancellable
		{
			public CancellableDummy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override DummyProcessTaskCollection GetNewWorkflowItems()
			{
				return this.GetOrCreateProcessTaskCollection(() => new DummyCancellableCollection(this));
			}

			[EventDateProperty(Events.CustomisableEvent00Code, EstimateActual.Estimate)]
			public override ZDateTime Z0_Date
			{
				get => base.Z0_Date;
				set
				{
					if (Z0_Date != value)
					{
						base.Z0_Date = value;
						Logs.CreateRecreateOrUpdateEventLog(Events.CustomisableEvent00, EstimateActual.Estimate, value.ToOffset());
					}
				}
			}

			public override bool IsCancelled
			{
				get => Z0_Bool;
				set => Z0_Bool = value;
			}
		}

		class DummyCancellableCollection : DummyProcessTaskCollection
		{
			public DummyCancellableCollection(BusinessObject master)
				: base(master)
			{
			}

			public new DummyTaskThatNoCancel this[int index]
			{
				get { return (DummyTaskThatNoCancel)Elements[index]; }
			}

			public new DummyTaskThatNoCancel AddNew()
			{
				return (DummyTaskThatNoCancel)base.AddNew();
			}
		}

		class DummyTaskThatNoCancel : DummyProcessTask
		{
			public DummyTaskThatNoCancel(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override void CancelIfParentIsCancelled()
			{
			}

			protected override Type ParentType => typeof(CancellableDummy);

			protected override string WorkflowTypeCore => "DUM";
		}

		[TestDate(2000, 1, 2)]
		public void TestGenerateManyExceptions()
		{
			List<DummyWithWorkflow> parents = new List<DummyWithWorkflow>();
			for (int i = 0; i < 150; i++)
			{
				DummyWithWorkflow parent = Factory.New<DummyWithWorkflow>();
				ProcessTask scheduledMilestone = CreateScheduledMilestone(parent);
				ProcessTask unscheduledMilestone = CreateUnscheduledMilestone(parent);
				ProcessTask completedMilestone = CreateCompletedMilestone(parent);
				ProcessTask overdueMilestone = CreateOverdueMilestone(parent);
				parents.Add(parent);
			}
			SaveFactoryWithoutRaisingMilestoneException();

			Processor.Process(Notifications);
			foreach (DummyWithWorkflow parent in parents)
			{
				parent.WorkflowItems.Load();
				AssertEquals("Exceptions generated for all overdue milestones", 1, parent.WorkflowItems.Exceptions.Count);
			}
			Processor.Process(Notifications);
			foreach (DummyWithWorkflow parent in parents)
			{
				parent.WorkflowItems.Load();
				AssertEquals("No additional exceptions generated", 1, parent.WorkflowItems.Exceptions.Count);
			}
		}

		[TestDate(2000, 1, 1, 23, 0, 0)]
		public void TestExceptionGeneratedOnDateBasisNotTimeBasis()
		{
			ProcessTask milestone = CreateMilestone(Dummy);
			milestone.P9_ScheduledDate = new ZDateTime(2000, 1, 1, 13, 0, 0);
			SaveFactoryWithoutRaisingMilestoneException();

			Processor.Process(Notifications);
			AssertEquals("No exception is raised until the day after the estimated date", 0, Dummy.WorkflowItems.Exceptions.Count);

			TestDateAttribute.Date = new DateTime(2000, 1, 2, 1, 0, 0);
			Processor.Process(Notifications);
			Dummy.WorkflowItems.Load();
			AssertEquals("Exception raised the day after the estimated date. This behaviour is to work around timezone related issues.", 1, Dummy.WorkflowItems.Exceptions.Count);
		}

		[TestDate(2011, 12, 21, 10, 00, 00)]
		public void TestNoDoubleTriggerringOnExceptionGeneration()
		{
			// Prepare template with a trigger
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "CON";
			template.GlobalTemplate = true;

			ProcessTask trigger = AddNewTrigger(template.WorkflowItems, Events.ExceptionRaisedCode, ExceptionActionConditionList.Codes.EventType, Events.InStoreCode);
			ProcessTaskNotification notification = trigger.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = "XML";
			notification.PQ_TriggerParty = "EML";
			notification.PQ_EmailAddr = "xxx@yyy.zzz";

			Factory.Save();

			// Create consol with 2 oudated milestones
			Integration.Forwarding.IForwardingConsol consol = Factory.New<Integration.Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_UniqueConsignRef = "TSC00001001";

			IWorkflowProvider workflowProvider = (IWorkflowProvider)consol;
			AddNewMilestone(workflowProvider.WorkflowItems, Events.DepartureCode, ProcessWorkflowExceptionType.ExceptionScheduledActionMissedCheckedDaily, new ZDateTime(2011, 12, 21, 15, 00, 00));
			AddNewMilestone(workflowProvider.WorkflowItems, Events.InStoreCode, ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired, new ZDateTime(2011, 12, 21, 15, 00, 00));

			Factory.Save();

			AssertEquals(1, workflowProvider.WorkflowItems.Triggers.Count);
			AssertEquals(Events.ExceptionRaisedCode, workflowProvider.WorkflowItems.Triggers[0].P9_SE_NKMilestoneEvent);

			// Service task runs in other company
			GlbBranch newBranch = Factory.New<GlbCompany>().Branches.AddNew();
			Factory.Save();
			using (new TemporaryUserContext { BranchPK = newBranch.PK.ToGuid() }.Set())
			{
				// Precondition check when nothing is outdated yet
				ZQuery wteQuery = new ZQuery(StmALogSchema.SL_Parent, workflowProvider.WorkflowItems.Triggers[0].PK);
				wteQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);

				Processor.Process(Notifications);
				workflowProvider.WorkflowItems.Load();
				AssertEquals("No exceptions are raised yet", 0, workflowProvider.WorkflowItems.Exceptions.Count);
				AssertEquals("No triggerring yet", 0, Factory.GetDatabaseCount(typeof(StmALog), wteQuery));

				// First run - immediate outdated exception generation
				TestDateAttribute.Date = new DateTime(2011, 12, 21, 15, 01, 29, 937);
				Processor.Process(Notifications);
				workflowProvider.WorkflowItems.Load();
				AssertEquals("1 exception generated", 1, workflowProvider.WorkflowItems.Exceptions.Count);
				AssertEquals("1 triggerring", 1, Factory.GetDatabaseCount(typeof(StmALog), wteQuery));

				// Second run - generate outdated exception for second milestone
				TestDateAttribute.Date = new DateTime(2011, 12, 22, 00, 00, 29, 900);
				Processor.Process(Notifications);
				workflowProvider.WorkflowItems.Load();
				AssertEquals("1 more exception generated", 2, workflowProvider.WorkflowItems.Exceptions.Count);
				AssertEquals("No more triggerring", 1, Factory.GetDatabaseCount(typeof(StmALog), wteQuery));
			}
		}

		[TestDate(2016, 7, 3)]
		public void TestDeletedParentsDoNotReportErrors()
		{
			var job = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var overdueMilestone = CreateOverdueMilestone(job);
			job.Delete();
			AssertEquals(false, overdueMilestone.IsDeleted);
			SaveFactoryWithoutRaisingMilestoneException();

			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(3);
			DummyWithWorkflow.ShouldApplyWorkflowTemplateOnSave.Value = true;
			Processor.Process(Notifications);

			overdueMilestone.Reload();
			AssertNotEquals(ZDateTime.Empty, overdueMilestone.P9_MilestoneExceptionAdded.ToZDateTime());
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		#region Implementation

		ProcessTask AddNewMilestone(ProcessTaskCollection workflowItems, ZString eventCode, ZString exceptionCode, ZDateTime scheduledDate)
		{
			ProcessTask milestone = AddNewProcessTask(workflowItems, eventCode);
			milestone.IsMilestone = true;
			milestone.P9_SE_NKExceptionEvent = exceptionCode;
			milestone.P9_ScheduledDateUtc = scheduledDate;
			return milestone;
		}

		ProcessTask AddNewTrigger(ProcessTaskCollection workflowItems, ZString eventCode, ZString condition, ZString reference)
		{
			ProcessTask trigger = AddNewProcessTask(workflowItems, eventCode);
			trigger.IsWorkflowTrigger = true;
			trigger.TriggerConditions.TriggerCondition = condition;
			trigger.TriggerConditions.TriggerConditionValue = reference;
			return trigger;
		}

		ProcessTask AddNewProcessTask(ProcessTaskCollection workflowItems, ZString eventCode)
		{
			ProcessTask task = workflowItems.AddNew();
			task.TriggerConditions.TriggerEventCode = eventCode;
			return task;
		}

		NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;

		WorkflowExceptionGenerationProcessor Processor
		{
			get
			{
				if (processor == null)
				{
					processor = new WorkflowExceptionGenerationProcessor();
				}
				return processor;
			}
		}
		WorkflowExceptionGenerationProcessor processor;

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;

		ProcessTask CreateScheduledMilestone(DummyWithWorkflow parent)
		{
			ProcessTask result = CreateMilestone(parent);
			result.P9_ScheduledDate = ZDateTime.Today;
			result.P9_SE_NKExceptionEvent = Events.Arrival.Code;
			return result;
		}

		ProcessTask CreateUnscheduledMilestone(DummyWithWorkflow parent)
		{
			ProcessTask result = CreateMilestone(parent);
			result.P9_SE_NKExceptionEvent = Events.Attached.Code;
			return result;
		}

		ProcessTask CreateCompletedMilestone(DummyWithWorkflow parent)
		{
			ProcessTask result = CreateMilestone(parent);
			result.P9_ScheduledDate = ZDateTime.Today.AddDays(-1);
			result.SetMilestoneActualDateForTest(ZDateTime.Today.AddDays(-1));
			result.P9_SE_NKExceptionEvent = Events.CallBackClient.Code;
			return result;
		}

		internal static ProcessTask CreateOverdueMilestone(DummyWithWorkflow parent)
		{
			ProcessTask result = CreateMilestone(parent);
			result.P9_ScheduledDate = ZDateTime.Today.AddMinutes(-1);
			return result;
		}

		static ProcessTask CreateExpiredMilestone(DummyWithWorkflow parent)
		{
			ProcessTask result = CreateMilestone(parent);
			result.TriggerConditions.TriggerEventCode = AutoEvents.ArrivalCode;
			result.P9_SE_NKExceptionEvent = ProcessWorkflowExceptionType.ExceptionWorkflowTimeExpired;
			result.P9_ScheduledDate = ZDateTime.Now.AddMinutes(-1);
			return result;
		}

		static ProcessTask CreateMilestone(DummyWithWorkflow parent)
		{
			ProcessTask result = parent.Factory.New<DummyProcessTask>();
			result.P9_Type = Core.Constants.Workflow.MilestoneType;
			result.P9_ParentTableCode = DummyBizoSchema.Constants.Prefix;
			result.P9_ParentID = parent.PK;
			return result;
		}

		void SaveFactoryWithoutRaisingMilestoneException()
		{
			bool wasTestDateActive = TestDateAttribute.IsActive;
			TestDateAttribute.Date = ZDateTime.Now.AddDays(-2).ToDateTime();
			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddDays(2).ToDateTime();
		}

		#endregion
	}
}
