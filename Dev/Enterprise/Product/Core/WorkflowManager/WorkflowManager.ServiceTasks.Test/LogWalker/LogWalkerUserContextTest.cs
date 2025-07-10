using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	public abstract class LogWalkerUserContextTest : TestCaseWithFactory
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Template = MakeTemplate();
			TemplateTrigger = MakeTrigger(Template, Events.CustomisableEvent00Code);
			ViewModel = new TriggerConditionsViewModel(TemplateTrigger);
			_ = MakeNotification(TemplateTrigger);
			DummyWithWorkflow.AutoLogState.Value = EnterpriseBusinessObject.AutologState.AutoLogged;
			WorkflowDataRegistry.Instance.EnableUserContextChangeLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		IBaseTrigger TemplateTrigger { get; set; }

		TriggerConditionsViewModel ViewModel { get; set; }

		ProcessTaskTemplate Template { get; set; }

		protected abstract IBaseTrigger MakeTrigger(ProcessTaskTemplate provider, ZString code);

		protected abstract ProcessTaskTemplate MakeTemplate();

		protected DummyWithWorkflow MakeDummy()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			dummy.ApplyWorkflowTemplates();
			return dummy;
		}

		protected ProcessTaskNotification MakeNotification(IBaseTrigger trigger)
		{
			var action = (ProcessTaskNotification)trigger.TriggerActions.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			action.PQ_FieldName = "<Z0_Code>";
			action.PQ_FieldValue = "NOOB";
			return action;
		}

		protected (GlbCompany company, GlbBranch branch) MakeBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;

			return (company, branch);
		}

		protected GlbDepartment MakeDepartment()
		{
			return Factory.NewWithValidTestData<GlbDepartment>();
		}

		protected GlbStaff MakeStaff()
		{
			return Factory.NewWithValidTestData<GlbStaff>();
		}

		#endregion

		public void TestLWKLogs()
		{
			Factory.Save();
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			var log = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			AssertContains("Switched to user context", log);
		}

		public void TestNonDefaultContext_DisabledByRegistry()
		{
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Event;
			WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.Save();
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			var log = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			AssertContains(FormattableString.Invariant($"Ignoring Trigger Context configuration because {WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration.HumanReadableRegistryPath()} is disabled and parent has a non-default Trigger Context"), log);
		}

		public virtual void TestDefaultContext()
		{
			var (company, branch) = MakeBranch();
			var department = MakeDepartment();
			var staff = MakeStaff();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Default;
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			// We are switching to another context for template application, and event creation because this how to choose a user context by default.
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				dummy.ApplyWorkflowTemplates();
				dummy.Logs.AddNew(Events.CustomisableEvent00);
			}
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals(staff.GS_Code, @event.SL_GS_NKUser);
			AssertEquals(branch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals(department.GE_Code, @event.SL_GE_NKDepartment);
		}

		public void TestCurrentUserContext()
		{
			var (company, branch) = MakeBranch();
			var department = MakeDepartment();
			var staff = MakeStaff();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Event;
			var dummy = MakeDummy();
			// The event is being created in a different context, and we expect this context to be chosen
			// This is regardless of every other thing happening in the default context.
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				dummy.Logs.AddNew(Events.CustomisableEvent00);
			}
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals(staff.GS_Code, @event.SL_GS_NKUser);
			AssertEquals(branch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals(department.GE_Code, @event.SL_GE_NKDepartment);
		}

		public virtual void TestBespokeContext_Company()
		{
			var (company, branch) = MakeBranch();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			ViewModel.TriggerCompany = company.PK;
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals("As staff is not overriden we use the DEF staff", GlbStaff.CurrentUser.GS_Code, @event.SL_GS_NKUser);
			AssertEquals("As the company is overriden on the trigger, using the DEF logic, it chooses an appropriate branch for that company", branch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals("Not overriden, so we use default.", GlbDepartment.CurrentDepartment.GE_Code, @event.SL_GE_NKDepartment);
		}

		public virtual void TestSpecificContext_MultipleStaff()
		{
			var staff1 = MakeStaff();
			var staff2 = MakeStaff();
			staff1.GS_EmailAddress = "staff1@test.com";
			staff2.GS_EmailAddress = "staff2@test.com";
			Factory.Save();

			var trigger1 = MakeTrigger(Template, Events.CustomisableEvent01Code);
			trigger1.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			trigger1.TriggerStaffCode = staff1.GS_Code;
			var action1 = (ProcessTaskNotification)trigger1.TriggerActions.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action1.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action1.PQ_EmailAddr = "abc@email.com";
			action1.PQ_EmailText = "Test email";

			var trigger2 = MakeTrigger(Template, Events.CustomisableEvent02Code);
			trigger2.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			trigger2.TriggerStaffCode = staff2.GS_Code;
			var action2 = (ProcessTaskNotification)trigger2.TriggerActions.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			action2.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			action2.PQ_EmailAddr = "abc@email.com";
			action2.PQ_EmailText = "Test email";
			Factory.Save();

			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent01);
			dummy.Logs.AddNew(Events.CustomisableEvent02);
			Factory.Save();
			var logs = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

			CombineAssertions("Expecting 1 email from each staff", () =>
			{
				var eMails = Env.Instance.OutgoingMailManager.EmailsCreated;
				var fromAddresses = eMails.Select(m => m.FromAddress);
				AssertEquals(2, fromAddresses.Count());
				AssertContainsExactElementsInAnyOrder(new string[] { staff1.GS_EmailAddress, staff2.GS_EmailAddress }, fromAddresses);
			});
		}

		public virtual void TestBespokeContext_Branch()
		{
			var (company, branch) = MakeBranch();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			ViewModel.TriggerBranch = branch.PK;
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals("The branch is explicitly set on the trigger", branch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals("Everything else is defaulted", GlbStaff.CurrentUser.GS_Code, @event.SL_GS_NKUser);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, @event.SL_GE_NKDepartment);
		}

		public virtual void TestBespokeContext_Department()
		{
			var department = MakeDepartment();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			ViewModel.TriggerDepartment = department.PK;
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals("The department is explicitly set on the trigger", department.GE_Code, @event.SL_GE_NKDepartment);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, @event.SL_GS_NKUser);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, @event.SL_GB_NKBranch);
		}

		public virtual void TestBespokeContext_User()
		{
			var (company, branch) = MakeBranch();
			var department = MakeDepartment();
			var staff = MakeStaff();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			ViewModel.TriggerStaffCode = staff.GS_Code;
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals("The staff is explicitly set on the trigger", staff.GS_Code, @event.SL_GS_NKUser);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, @event.SL_GE_NKDepartment);
		}

		public virtual void TestBespokeContext_UserBranchDepartment()
		{
			var (company, branch) = MakeBranch();
			var department = MakeDepartment();
			var staff = MakeStaff();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			ViewModel.TriggerStaffCode = staff.GS_Code;
			ViewModel.TriggerBranch = branch.PK;
			ViewModel.TriggerDepartment = department.PK;
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals("Everything got overriden on the trigger", staff.GS_Code, @event.SL_GS_NKUser);
			AssertEquals(branch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals(department.GE_Code, @event.SL_GE_NKDepartment);
		}

		public virtual void TestBespokeContext_UserCompany()
		{
			var (company, branch) = MakeBranch();
			var staff = MakeStaff();
			Factory.Save();
			ViewModel.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			ViewModel.TriggerCompany = company.PK;
			ViewModel.TriggerStaffCode = staff.GS_Code;
			var dummy = MakeDummy();
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();
			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals("Staff was overriden", staff.GS_Code, @event.SL_GS_NKUser);
			AssertEquals("Find the best branch for the company", branch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals("Deparment is as default, falls back to current.", GlbDepartment.CurrentDepartment.GE_Code, @event.SL_GE_NKDepartment);
		}
	}

	public abstract class TemplateLogWalkerUserContextTest : LogWalkerUserContextTest
	{
		protected override ProcessTaskTemplate MakeTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			return template;
		}
	}

	public class MilestonesLogWalkerUserContextTest : TemplateLogWalkerUserContextTest
	{
		[TestDate(2019, 11, 8, 14, 0, 0)]
		public void TestTriggerField()
		{
			var hoursToKeepProcessedLogs = SystemDataRegistry.Instance.WorkflowFieldChangeClearProcessedLogsAfterHours.Value;
			SystemDataRegistry.Instance.WorkflowFieldChangeTriggerHWM.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, SqlDateTime.MinValue.Value.AddHours(hoursToKeepProcessedLogs));

			var (company, branch) = MakeBranch();
			var department = MakeDepartment();
			var staff = MakeStaff();
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(DummyBizoSchema.Z0_VarCharMax);
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;
			var fieldTrigger = dummy.RelatedWorkflowProvider.WorkflowItems.Triggers.AddNew();
			fieldTrigger.TriggerConditions.TriggerFieldName = DummyBizoSchema.Z0_VarCharMax.Name;
			fieldTrigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			fieldTrigger.TriggerConditions.TriggerStaffCode = staff.GS_Code;
			fieldTrigger.TriggerConditions.TriggerBranch = branch.PK;
			fieldTrigger.TriggerConditions.TriggerDepartment = department.PK;
			MakeNotification(fieldTrigger);
			Factory.Save();
			TestDateAttribute.Date = ZDateTime.UtcNow.AddMinutes(1).ToDateTime();
			new WorkflowServiceTaskTester().RunChain();

			var @event = Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Parent, dummy.PK)).MaxBy(m => m.SL_PostedTimeUtc);
			AssertEquals("Field change trigger ignores the SPE trigger context, because we can't figure out the department for EVT type.", GlbStaff.CurrentUser.GS_Code, @event.SL_GS_NKUser);
			AssertEquals(GlbBranch.CurrentBranch.GB_Code, @event.SL_GB_NKBranch);
			AssertEquals(GlbDepartment.CurrentDepartment.GE_Code, @event.SL_GE_NKDepartment);
		}

		protected override IBaseTrigger MakeTrigger(ProcessTaskTemplate template, ZString code)
		{
			var trigger = template.WorkflowItems.Milestones.AddNew();
			trigger.P9_Description = "Ima Milestone Aylamayo";
			trigger.TriggerConditions.TriggerEventCode = code;
			return trigger;
		}
	}

	public class TriggersLogWalkerUserContextTest : TemplateLogWalkerUserContextTest
	{
		protected override IBaseTrigger MakeTrigger(ProcessTaskTemplate template, ZString code)
		{
			var trigger = template.WorkflowItems.Milestones.AddNew();
			trigger.P9_Description = "The BIG TRIG";
			trigger.TriggerConditions.TriggerEventCode = code;
			return trigger;
		}
	}

	public class UniversalTriggersLogWalkerUserContextTest : TemplateLogWalkerUserContextTest
	{
		protected override ProcessTaskTemplate MakeTemplate()
		{
			var template = base.MakeTemplate();
			template.P0_IsUniversal = true;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			return template;
		}

		protected override IBaseTrigger MakeTrigger(ProcessTaskTemplate template, ZString code)
		{
			var trigger = (IBaseTrigger)template.TemplateTriggers.AddNew();
			trigger.Description = "The Trig";
			trigger.TriggerEventCode = code;
			return trigger;
		}

		public override void TestDefaultContext()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}

		public override void TestBespokeContext_UserCompany()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}

		public override void TestBespokeContext_UserBranchDepartment()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}

		public override void TestBespokeContext_User()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}

		public override void TestBespokeContext_Department()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}

		public override void TestBespokeContext_Company()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}

		public override void TestBespokeContext_Branch()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}

		public override void TestSpecificContext_MultipleStaff()
		{
			Assert("Only Event user context is supported for universal triggers", true);
		}
	}
}
