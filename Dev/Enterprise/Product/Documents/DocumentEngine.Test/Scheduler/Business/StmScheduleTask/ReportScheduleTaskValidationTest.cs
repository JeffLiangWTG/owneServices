using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class ReportScheduleTaskValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUnallowedReportCommand()
		{
			var command1 = Factory.Load<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Consol Summary"))[0];
			var command2 = Factory.Load<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Shipment Profile Report"))[0];
			var command3 = Factory.Load<ReportCommand>(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.Equal, "Consol Profile Report"))[0];

			var testGroup = Factory.New<GlbGroup>();
			testGroup.GG_Code = "BAM";
			testGroup.GG_IsActive = true;

			var testStaff = Factory.NewWithValidTestData<GlbStaff>();
			testStaff.GS_LoginName = "Bob.test2";

			var testLink = Factory.New<GlbGroupLink>();
			testLink.GK_GS = testStaff.PK;
			testLink.GK_GG = testGroup.PK;

			var reportSecurityItem = Factory.New<GlbSecurity>();
			reportSecurityItem.GU_GG = testGroup.PK;
			reportSecurityItem.GU_ItemGUID = command1.PK.ToGuid();
			reportSecurityItem.GU_SecurityRight = "Report";
			reportSecurityItem.GU_SecurityItemIsAllowed = false;

			var reportSecurityItem2 = Factory.New<GlbSecurity>();
			reportSecurityItem2.GU_GG = testGroup.PK;
			reportSecurityItem2.GU_ItemGUID = command2.PK.ToGuid();
			reportSecurityItem2.GU_SecurityRight = "Report";
			reportSecurityItem2.GU_SecurityItemIsAllowed = true;

			var reportSecurityItemR = Factory.New<GlbSecurity>();
			reportSecurityItemR.GU_GG = testGroup.PK;
			reportSecurityItemR.GU_SecurityRight = Env.Security.ForwardingReport.Code;
			reportSecurityItemR.GU_SecurityItemIsAllowed = true;

			var reportSecurityItemRR = Factory.New<GlbSecurity>();
			reportSecurityItemRR.GU_GG = testGroup.PK;
			reportSecurityItemRR.GU_SecurityRight = Env.Security.FindOrCreateReportRunCheckpoint(ModuleIDs.ForwardingReport, Env.Security.ForwardingReport).Code;
			reportSecurityItemRR.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			using (Env.SetTemporaryUserContext("Bob.test2", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				ScheduleTask.S5_ParentID = command1.PK;
				AssertHasError(ScheduleTask.S5_ParentIDInfo, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Reports -> Run Reports -> Consol Summary");

				ScheduleTask.S5_ParentID = command2.PK;
				AssertNoErrors(ScheduleTask.S5_ParentIDInfo);

				ScheduleTask.S5_ParentID = command3.PK;
				AssertHasError(ScheduleTask.S5_ParentIDInfo, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Reports -> Run Reports -> Consol Profile Report");
			}
		}

		public void TestCheckParentID()
		{
			var report = Factory.NewWithValidTestData<ReportCommand>();
			report.SU_BusinessContext = "RepBiManager";
			ScheduleTask.S5_ParentID = report.PK;
			AssertNoErrors(ScheduleTask.S5_ParentIDInfo);
		}

		[TestDate(2023, 5, 23)]
		public void TestValidateS5_NextScheduledPrintRunTime()
		{
			ScheduleTask.S5_IsPrivate = false;
			ScheduleTask.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			AssertNoErrors(ScheduleTask.S5_NextScheduledPrintRunTimeUtcInfo);

			ScheduleTask.S5_IsPrivate = true;
			ScheduleTask.Validation.ValidateS5_NextScheduledPrintRunTimeUtc();
			AssertHasError(ScheduleTask.S5_NextScheduledPrintRunTimeUtcInfo, "Please enter a " + ScheduleTask.S5_NextScheduledPrintRunTimeUtcInfo.Description + ".");

			var nextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddDays(2);

			var scheduleTask1 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask1.S5_NextScheduledPrintRunTimeUtc = nextScheduledPrintRunTimeUtc.AddMinutes(11);

			var scheduleTask2 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask2.S5_NextScheduledPrintRunTimeUtc = nextScheduledPrintRunTimeUtc.AddMinutes(10);

			var scheduleTask3 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask3.S5_NextScheduledPrintRunTimeUtc = nextScheduledPrintRunTimeUtc.AddMinutes(-10);
			Factory.Save();

			scheduleTask1.Reload();
			AssertEquals("Pre condition: schedule task1 next run time", nextScheduledPrintRunTimeUtc.AddMinutes(11), scheduleTask1.S5_NextScheduledPrintRunTimeUtc);

			scheduleTask2.Reload();
			AssertEquals("Pre condition: schedule task2 next run time", nextScheduledPrintRunTimeUtc.AddMinutes(10), scheduleTask2.S5_NextScheduledPrintRunTimeUtc);

			scheduleTask3.Reload();
			AssertEquals("Pre condition: schedule task3 next run time", nextScheduledPrintRunTimeUtc.AddMinutes(-10), scheduleTask3.S5_NextScheduledPrintRunTimeUtc);

			var scheduleTask4 = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask4.S5_NextScheduledPrintRunTimeUtc = nextScheduledPrintRunTimeUtc;

			AssertEquals("Pre-condition: Max Active Scheduled Reports Warning Threshold is 0.", 0, SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.Value);
			AssertNoWarnings(scheduleTask4.S5_NextScheduledPrintRunTimeUtcInfo);

			using (SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				AssertEquals("Pre-condition: Max Active Scheduled Reports Warning Threshold is 2.", 2, SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.Value);
				scheduleTask4.Validation.ValidateS5_NextScheduledPrintRunTimeUtc();
				AssertHasWarning(scheduleTask4.S5_NextScheduledPrintRunTimeUtcInfo, "There are currently 2 reports scheduled for processing at this time which potentially can lead to delays in the processing of this report.");
			}

			using (SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				AssertEquals("Pre-condition: Max Active Scheduled Reports Warning Threshold is 3.", 3, SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.Value);
				scheduleTask4.Validation.ValidateS5_NextScheduledPrintRunTimeUtc();
				AssertNoWarnings(scheduleTask4.S5_NextScheduledPrintRunTimeUtcInfo);
			}

			Factory.Save();
			scheduleTask4.Reload();

			using (SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 2))
			{
				AssertEquals("Pre-condition: Max Active Scheduled Reports Warning Threshold is 2.", 2, SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.Value);
				scheduleTask4.Validation.ValidateS5_NextScheduledPrintRunTimeUtc();
				AssertHasWarning(scheduleTask4.S5_NextScheduledPrintRunTimeUtcInfo, "There are currently 3 reports scheduled for processing at this time which potentially can lead to delays in the processing of this report.");
			}

			using (SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				AssertEquals("Pre-condition: Max Active Scheduled Reports Warning Threshold is 3.", 3, SystemDataRegistry.Instance.MaxActiveScheduledReportsWarningThreshold.Value);
				scheduleTask4.Validation.ValidateS5_NextScheduledPrintRunTimeUtc();
				AssertNoWarnings(scheduleTask4.S5_NextScheduledPrintRunTimeUtcInfo);
			}
		}

		public void TestValidateS5_ScheduleDescription()
		{
			ScheduleTask.Recipients.AddNew();
			ScheduleTask.S5_ScheduleDescription = "Hello";
			AssertNoErrors(ScheduleTask.S5_ScheduleDescriptionInfo);

			ScheduleTask.S5_ScheduleDescription = "";
			AssertHasError(ScheduleTask.S5_ScheduleDescriptionInfo, "Please enter a " + ScheduleTask.S5_ScheduleDescriptionInfo.Description + ".");

			ScheduleTask.S5_IsPrivate = true;
			ScheduleTask.Validation.ValidateS5_ScheduleDescription();
			AssertNoErrors(ScheduleTask.S5_ScheduleDescriptionInfo);

			ScheduleTask.Recipients.RemoveAndDeleteAll();
			ScheduleTask.Validation.ValidateS5_ScheduleDescription();
			AssertHasError(ScheduleTask.S5_ScheduleDescriptionInfo, "Please enter at least one recipient.");
		}

		public void TestValidateS5_ScheduleDescription_Duplicate()
		{
			var duplicate = new BusinessObjectFactory { RefreshEnabled = false }.NewWithValidTestData<ReportScheduleTask>();
			duplicate.S5_ScheduleDescription = "Test Dup";
			duplicate.S5_ParentID = ZGuid.NewZGuid();

			ScheduleTask.Recipients.AddNew();
			ScheduleTask.S5_ParentID = duplicate.S5_ParentID;
			ScheduleTask.S5_ScheduleDescription = duplicate.S5_ScheduleDescription;

			ScheduleTask.Validation.ValidateS5_ScheduleDescription();
			AssertNoNotifications(ScheduleTask.S5_ScheduleDescriptionInfo);

			duplicate.Factory.Save();

			ScheduleTask.Validation.ValidateS5_ScheduleDescription();
			AssertHasWarning(ScheduleTask.S5_ScheduleDescriptionInfo, "There is already a Scheduled Report with this Description using this template.");
		}

		public void TestS5_ScheduleStateValidationIsDisabledAndCanHavelargeValues()
		{
			var scheduleReport = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleReport.S5_ScheduleState = new ZBlob(new byte[10485777]);

			AssertNoNotifications(scheduleReport.S5_ScheduleStateInfo);
		}

		public void TestPrintUserIsValidAndActive()
		{
			ScheduleTask.UserFK = ZGuid.Empty;
			ScheduleTask.Validation.ValidateUserFK();
			AssertHasError(ScheduleTask.UserFKInfo, "Please enter a Print user.");

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsActive = false;
			ScheduleTask.UserFK = staff.PK;
			ScheduleTask.Validation.ValidateUserFK();
			AssertHasError(ScheduleTask.UserFKInfo, "Cannot assign to an inactive user.");

			AssertPrintUserHasNoErrorWhenScheduleTaskIsInactive(ScheduleTask, "Cannot assign to an inactive user.");

			staff.GS_IsActive = true;
			ScheduleTask.Validation.ValidateUserFK();
			AssertNoError(ScheduleTask.UserFKInfo, "Cannot assign to an inactive user.");

			staff.Delete();
			Factory.Save();
			ScheduleTask.Validation.ValidateUserFK();
			AssertHasError(ScheduleTask.UserFKInfo, "This user does not exist or has been deleted.");

			AssertPrintUserHasNoErrorWhenScheduleTaskIsInactive(ScheduleTask, "This user does not exist or has been deleted.");
		}

		public void TestSetPrintUserAsSystemUserThenHintValidationError()
		{
			ScheduleTask.UserFK = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "E")).PK;
			ScheduleTask.Validation.ValidateUserFK();
			AssertHasError(ScheduleTask.UserFKInfo, "Cannot assign to a system user.");

			ScheduleTask.UserFK = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "ZZ")).PK;
			ScheduleTask.Validation.ValidateUserFK();
			AssertNoError(ScheduleTask.UserFKInfo, "Cannot assign to a system user.");
		}

		void AssertPrintUserHasNoErrorWhenScheduleTaskIsInactive(ReportScheduleTask scheduleTask, string errorMessage)
		{
			scheduleTask.S5_IsActive = false;
			scheduleTask.Validation.ValidateUserFK();
			AssertNoError(scheduleTask.UserFKInfo, errorMessage);
			scheduleTask.S5_IsActive = true;
		}

		public void TestPrintUserHasValidEmailAddressIfUsingEPrint()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			ScheduleTask.UserFK = staff.PK;
			ScheduleTask.Validation.ValidateUserFK();
			AssertNoErrors(ScheduleTask.UserFKInfo);

			var recipient = ScheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			ScheduleTask.Validation.ValidateUserFK();
			AssertNoErrors(ScheduleTask.UserFKInfo);

			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.EPrint;
			Assert(staff.GS_EmailAddress.IsEmpty);
			ScheduleTask.Validation.ValidateUserFK();
			AssertHasError(ScheduleTask.UserFKInfo, "This user doesn't have an email address set up. It is required to use the ePrint delivery method.");

			AssertPrintUserHasNoErrorWhenScheduleTaskIsInactive(ScheduleTask, "This user doesn't have an email address set up. It is required to use the ePrint delivery method.");

			staff.GS_EmailAddress = "dexter@morgan.com";
			ScheduleTask.Validation.ValidateUserFK();
			AssertNoErrors(ScheduleTask.UserFKInfo);
		}

		#region Implementation

		ReportScheduleTask ScheduleTask
		{
			get
			{
				if (scheduletask == null)
				{
					scheduletask = Factory.NewWithValidTestData<ReportScheduleTask>();
				}
				return scheduletask;
			}
		}
		ReportScheduleTask scheduletask;

		#endregion
	}
}
