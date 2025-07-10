using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class WorkflowFailureHandlerTest : TestCaseWithFactory
	{
		public void TestNotificationEmailFallback()
		{
			AssertWithFallback((companyPk, branchPK, departmentPK, groupPK) => WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPK));
		}

		public void TestNotificationEmailFallback_Company()
		{
			AssertWithFallback((companyPk, branchPK, departmentPK, groupPK) => WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(companyPk, Guid.Empty, Guid.Empty, groupPK));
		}

		public void TestNotificationEmailFallback_Branch()
		{
			AssertWithFallback((companyPk, branchPK, departmentPK, groupPK) => WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, branchPK, Guid.Empty, groupPK));
		}

		public void TestNotificationEmailFallback_Department()
		{
			AssertWithFallback((companyPk, branchPK, departmentPK, groupPK) => WorkflowDataRegistry.Instance.WorkflowManagerNotificationGroup.SetValue(Guid.Empty, Guid.Empty, departmentPK, groupPK));
		}

		void AssertWithFallback(Action<Guid, Guid, Guid, Guid> setFallback)
		{
			var defaultGroup = Factory.NewWithValidTestData<GlbGroup>();
			var defaultStaff = defaultGroup.Staff.AddNew();
			defaultStaff.FillWithValidTestData();
			defaultStaff.GS_EmailAddress = "workflowstuff@cargowise.com";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();

			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var departmentGroup = Factory.NewWithValidTestData<GlbGroup>();
			var departmentStaff = departmentGroup.Staff.AddNew();
			departmentStaff.FillWithValidTestData();
			departmentStaff.GS_EmailAddress = "departmentspecific@cargowise.com";

			var assignedGroup = Factory.NewWithValidTestData<GlbGroup>();
			var assignedStaffInGroup = assignedGroup.Staff.AddNew();
			assignedStaffInGroup.FillWithValidTestData();
			assignedStaffInGroup.GS_EmailAddress = "assignedingroup@cargowise.com";

			var assignedStaff = Factory.NewWithValidTestData<GlbStaff>();
			assignedStaff.GS_Code = "AAA";
			assignedStaff.GS_EmailAddress = "aaa@cargowise.com";

			var triggerOwnerStaff = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			setFallback(company.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid(), defaultGroup.PK.ToGuid());

			using (Env.SetTemporaryUserContext(company.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var processTask = Factory.NewWithValidTestData<ProcessTask>();
				WorkflowFailureHandler.HandleTriggerFailure(processTask, new WorkflowValidationException("fail"));
				AssertEquals("Failure notification email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("workflowstuff@cargowise.com", email.Recipients[0].Email);

				processTask.P9_GS_NKAssignedStaffMember = assignedStaff.GS_Code;
				WorkflowFailureHandler.HandleTriggerFailure(processTask, new WorkflowValidationException("fail"));
				AssertEquals("Failure notification email should be sent", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				email = Env.OutgoingMailManager.EmailsCreated[1];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("aaa@cargowise.com", email.Recipients[0].Email);

				processTask.P9_GS_NKAssignedStaffMember = null;
				processTask.P9_GG_AssignedGroup = assignedGroup.PK;
				WorkflowFailureHandler.HandleTriggerFailure(processTask, new WorkflowValidationException("fail"));
				AssertEquals("Failure notification email should be sent", 3, Env.OutgoingMailManager.EmailsCreated.Count);
				email = Env.OutgoingMailManager.EmailsCreated[2];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("assignedingroup@cargowise.com", email.Recipients[0].Email);
			}
		}
	}
}
