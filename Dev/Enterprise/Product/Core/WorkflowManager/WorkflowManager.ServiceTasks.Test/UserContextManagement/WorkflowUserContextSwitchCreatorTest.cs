using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using User = Enterprise.ZArchitecture.Environment.User;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class WorkflowUserContextSwitchCreatorTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetTemporaryUserContextAlsoSwitchesDepartment()
		{
			var job = Factory.New<DummyWithWorkflow>();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company.PK;
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var jobHeader = Factory.NewJobForTesting<JobHeader>();
			jobHeader.JH_GE = department.PK;
			jobHeader.JH_GB = branch1.PK;
			jobHeader.JH_GC = company.PK;
			jobHeader.JH_ParentID = job.PK;
			jobHeader.JH_JobNum = "Bork";
			Factory.Save();

			var trigger = job.WorkflowItems.Triggers.AddNew();
			trigger.P9_GC = company.PK;
			var sourceLog = job.Logs.AddNew(Events.CustomisableEvent00);
			WorkflowUserContext workflowUserContext;

			using (Env.SetTemporaryUserContext(Env.Instance.CurrentUser.PK, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				workflowUserContext = WorkflowUserContextDecider.GetTemporaryUserContext(trigger, job, new QueuedLogForTesting(Factory));
			}

			var triggeringEventData = new WorkflowTriggerEventData(
				new EventSource(sourceLog),
				new ZGuid(),
				GlbBranch.GetCurrentBranch(trigger.Factory).GB_Code,
				GlbDepartment.GetCurrentDepartment(trigger.Factory).GE_Code,
				workflowUserContext.StaffCode,
				workflowUserContext.BranchCode,
				workflowUserContext.DepartmentCode);

			var log = new QueuedLogForTesting(trigger, "WTE", triggeringEventData.ToReference());

			var userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, log, new DummyLogger());
			AssertEquals(branch1.PK, userContext.BranchPK);
			AssertEquals(department.PK, userContext.DepartmentPK);

			trigger.TriggerConditions.TriggerEventCode = "ATH";

			var user = Factory.New<GlbStaff>();
			user.GS_GB_HomeBranch = branch2.PK;
			user.GS_Code = "ACD";
			user.GS_LoginName = "aeceedee";
			user.GS_FullName = "Ae Cee Dee";
			jobHeader.MarkAsInactive();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.Instance.CurrentUser.PK, branch2.PK.ToGuid(), department.PK.ToGuid()))
			{
				workflowUserContext = WorkflowUserContextDecider.GetTemporaryUserContext(trigger, job, new QueuedLogForTesting(Factory) { SJ_GS_NKUser = "ACD" });
			}

			triggeringEventData = new WorkflowTriggerEventData(
				new EventSource(sourceLog),
				new ZGuid(),
				GlbBranch.GetCurrentBranch(trigger.Factory).GB_Code,
				GlbDepartment.GetCurrentDepartment(trigger.Factory).GE_Code,
				workflowUserContext.StaffCode,
				workflowUserContext.BranchCode,
				workflowUserContext.DepartmentCode);
			log = new QueuedLogForTesting(trigger, "WTE", triggeringEventData.ToReference());

			userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, log, new DummyLogger());
			AssertEquals(branch2.PK, userContext.BranchPK);

			user.GS_GB_HomeBranch = branch1.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.Instance.CurrentUser.PK, branch1.PK.ToGuid(), department.PK.ToGuid()))
			{
				workflowUserContext = WorkflowUserContextDecider.GetTemporaryUserContext(trigger, job, new QueuedLogForTesting(Factory) { SJ_GS_NKUser = "ACD" });
			}
			triggeringEventData = new WorkflowTriggerEventData(
				new EventSource(sourceLog),
				new ZGuid(),
				GlbBranch.GetCurrentBranch(trigger.Factory).GB_Code,
				GlbDepartment.GetCurrentDepartment(trigger.Factory).GE_Code,
				workflowUserContext.StaffCode,
				workflowUserContext.BranchCode,
				workflowUserContext.DepartmentCode);
			log = new QueuedLogForTesting(trigger, "WTE", triggeringEventData.ToReference());

			userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, job, log, new DummyLogger());
			AssertEquals(branch1.PK, userContext.BranchPK);
		}

		public void TestGetTemporaryUserContextFromAnotherBranch()
		{
			var company = Factory.New<GlbCompany>();

			var branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = company.PK;
			branch1.GB_Code = "GB1";

			var branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = company.PK;
			branch2.GB_Code = "GB2";

			Factory.Save();
			var parent = Factory.New<DummyJobHeaderParent>();
			parent.BranchExposed = branch2;

			var user = Factory.New<GlbStaff>();
			user.GS_GB_HomeBranch = branch1.PK;
			user.GS_Code = "ACD";
			user.GS_LoginName = "aeceedee";
			user.GS_FullName = "Ae Cee Dee";
			Factory.Save();

			var trigger = Factory.New<DummyProcessTask>();
			trigger.P9_ParentID = parent.PK;
			trigger.P9_GC = company.PK;

			var workflowUserContext = WorkflowUserContextDecider.GetTemporaryUserContext(trigger, parent, new QueuedLogForTesting(Factory));

			var sourceLog = trigger.Logs.AddNew(Events.CustomisableEvent00);

			var triggeringEventData = new WorkflowTriggerEventData(
					new EventSource(sourceLog),
					new ZGuid(),
					GlbBranch.GetCurrentBranch(trigger.Factory).GB_Code,
					GlbDepartment.GetCurrentDepartment(trigger.Factory).GE_Code,
					workflowUserContext.StaffCode,
					workflowUserContext.BranchCode,
					workflowUserContext.DepartmentCode);

			var log = new QueuedLogForTesting(trigger, "WTE", triggeringEventData.ToReference());

			var userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, parent, log, new DummyLogger());
			AssertEquals(branch2.PK, userContext.BranchPK);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestGetTemporaryUserContext_TriggerIsLineTrigger_ReturnLineContext()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var parentDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			var lineDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var parent = Factory.NewWithValidTestData<DummyJobHeaderParent>();
			var line = Factory.NewWithValidTestData<DummyJobHeaderParent>();

			var parentHeader = Factory.NewJobForTesting<JobHeader>();
			parentHeader.JH_GE = parentDepartment.PK;
			parentHeader.JH_GC = company.PK;
			parentHeader.JH_ParentID = parent.PK;
			parentHeader.JH_JobNum = "baba";

			var lineHeader = Factory.NewJobForTesting<JobHeader>();
			lineHeader.JH_GE = lineDepartment.PK;
			lineHeader.JH_GC = company.PK;
			lineHeader.JH_ParentID = line.PK;
			lineHeader.JH_JobNum = "executive";

			var trigger = Factory.NewWithValidTestData<ProcessTask>().With(
				p9_ParentID: parent.PK,
				p9_LineTriggerType: "DUM",
				p9_GC: company.PK);
			trigger.P9_ParentTableCode = DummyJobHeaderParent.Schema.TablePrefix;

			var sourceLog = lineHeader.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			WorkflowUserContext workflowUserContext;
			using (Env.SetTemporaryUserContext(Env.Instance.CurrentUser.PK, Env.Instance.CurrentBranch.PK, lineDepartment.PK.ToGuid()))
			{
				workflowUserContext = WorkflowUserContextDecider.GetTemporaryUserContext(trigger, line, new QueuedLogForTesting(Factory) { SJ_GS_NKUser = "ACD" });
			}

			var triggeringEventData = new WorkflowTriggerEventData(
					new EventSource(sourceLog),
					new ZGuid(),
					GlbBranch.GetCurrentBranch(trigger.Factory).GB_Code,
					GlbDepartment.GetCurrentDepartment(trigger.Factory).GE_Code,
					workflowUserContext.StaffCode,
					workflowUserContext.BranchCode,
					workflowUserContext.DepartmentCode);

			var log = new QueuedLogForTesting(trigger, "WTE", triggeringEventData.ToReference());

			AssertNotNull(DummyWorkflowDescriptor.Instance);
			var userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, line, log, new DummyLogger());
			AssertEquals("Department", lineDepartment.PK, userContext.DepartmentPK);
		}

		public void TestGetTemporaryUserContextHandlesInvalidTriggerContextCode()
		{
			var parent = Factory.New<DummyWithWorkflow>();
			var trigger = parent.WorkflowItems.Triggers.AddNew();
			((IBaseTrigger)trigger).TriggerContextCode = "XXX";
			var logger = new DummyLogger();
			var userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, parent, new QueuedLogForTesting(Factory) { SJ_GS_NKUser = "ACD" }, logger);
			AssertNotNull(userContext);
			AssertEquals(true, logger.Notes.HasWarnings());
		}

		public void TestUserContextIfSPKUserInactive()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var staffInvalid = Factory.NewWithValidTestData<GlbStaff>();
			staffInvalid.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staffInvalid.GS_IsActive = false;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			trigger.TriggerConditions.TriggerStaffCode = staffInvalid.GS_Code;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger.P9_Description = "Test";

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			triggerAction.PQ_FieldName = "<JobHeader.JH_Status>";
			triggerAction.PQ_FieldValue = "WHL";

			Factory.Save();
			using (Env.SetTemporaryUserContext(staffInvalid.GS_LoginName, staffInvalid.GS_GB_HomeBranch.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				shipmentWithWorkflow.Logs.AddNew(Events.CustomisableEvent00);
			}
			Factory.Save();

			ErrorReporter.Clear();

			var logwalker = MasterFilesTestHelper.RunLogWalker();
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertContains($"User {staffInvalid.GS_Code} is inactive. Default context will be used.", logwalker);

			var query = new ZQuery(StmJobQueueSchema.SJ_SE_NKEvent, "WTE");
			var workflowTriggerEvents = Factory.Load<IQueuedLog>(query);
			AssertEquals("WTE linked to Trigger", 1, workflowTriggerEvents.Length);

			var workflowTriggerEvent = new QueuedLogForTesting(workflowTriggerEvents[0]);
			var logger = new DummyLogger();
			var userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, (BusinessObject)shipment, workflowTriggerEvent, logger);
			AssertNotNull(userContext);
			AssertNotEquals("Context should not be set to an inactive user.", staffInvalid.PK, userContext.StaffPK);

			var staff = Factory.Load<GlbStaff>(userContext.StaffPK);
			AssertEquals("Context should be default user for the log (BP User).", User.ServiceUserCode, staff.GS_Code);
		}

		public void TestUserContextIfSPKUserInactiveChangedAfterWTE()
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();
			var shipmentWithWorkflow = (IWorkflowProvider)shipment;

			var staffValid = Factory.NewWithValidTestData<GlbStaff>();
			staffValid.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			var staffInvalid = Factory.NewWithValidTestData<GlbStaff>();
			staffInvalid.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			staffInvalid.GS_IsActive = false;

			var trigger = shipmentWithWorkflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			trigger.TriggerConditions.TriggerStaffCode = staffInvalid.GS_Code;
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger.P9_Description = "Test";

			Factory.Save();
			shipmentWithWorkflow.Logs.AddNew(Events.CustomisableEvent00);
			trigger.TriggerConditions.TriggerStaffCode = staffValid.GS_Code;
			Factory.Save();

			LogWalkerRunner.Master().Process(new LoggerForTest(), CancellationToken.None);

			var workflowTriggerEvents = Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_SE_NKEvent, "WTE"));
			AssertEquals("WTE linked to Trigger", 1, workflowTriggerEvents.Length);

			var workflowTriggerEvent = new QueuedLogForTesting(workflowTriggerEvents[0]);
			var userContext = WorkflowUserContextSwitchCreator.GetTemporaryUserContext(trigger, (BusinessObject)shipment, workflowTriggerEvent, new DummyLogger());
			AssertNotNull(userContext);
			AssertEquals("Context should be valid staff that it was changed to before saving", staffValid.PK, userContext.StaffPK);
		}

		sealed class DummyLogger : INotifications
		{
			public List<INotification> Notes = new List<INotification>();

			public void Add(INotification notification)
			{
				Notes.Add(notification);
			}
		}

		class DummyProcessTask : ProcessTask
		{
			public DummyProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			protected override Type ParentType
			{
				get
				{
					return typeof(DummyJobHeaderParent);
				}
			}
		}

		class DummyJobHeaderParent : DummyBusinessObject, IJobHeaderParent, IWorkflowProvider, IBranchProvider
		{
			public DummyJobHeaderParent(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public GlbBranch BranchExposed;
			GlbBranch IBranchProvider.Branch
			{
				get
				{
					return BranchExposed;
				}
			}

			public string JobNumber { get; set; }

			public bool AllowInvoiceDeletion { get; set; }

			public void OnJobCreated(JobHeader job) { }
			public void OnJobCreating(JobHeader job) { }
			public void OnJobDeleted(JobHeader job) { }
			public void OnJobDeleting(JobHeader job) { }
			public void SetJobNumberFieldOnSaving() { }

			public IProcessHeaderCollection Workflows { get { return null; } }
			public ProcessTaskCollection WorkflowItems { get { return null; } }
			public IWorkflowInformationProvider GetWorkflowInformationProvider() { return null; }
			public ZString WorkflowType { get; set; }

			public Logs Logs => null;

			public BusinessObjectFactory LogsFactory => Factory;

			public IColumnValueRanker GetTemplateSelectionCriteria() { return null; }
		}
	}
}
