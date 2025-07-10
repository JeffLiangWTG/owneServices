using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ReportManagementModule))]
	sealed class ReportManagementModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ReportManagement;
		}

		public void TestModuleID()
		{
			using (var module = new ReportManagementModule())
			{
				AssertEquals("ModuleID", ModuleIDs.ReportManagement, module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (var module = new ReportManagementModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ReportManagement, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestModuleDoesDotAllowNewEditAndDelete()
		{
			using (var module = new ReportManagementModule())
			{
				CombineAssertions(() =>
				{
					Assert(!module.AllowNew);
					Assert(!module.AllowEdit);
					Assert(!module.AllowDelete);
				});
			}
		}

		public void TestMenuItems()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var rightClickCancelMenuItem = module.ContextMenu.FindByText("Cancel");
				var rightClickCancelAndMarkInactiveMenuItem = module.ContextMenu.FindByText("Cancel && Mark As Inactive");

				AssertNotNull(rightClickCancelMenuItem);
				AssertNotNull(rightClickCancelAndMarkInactiveMenuItem);

				var formActionCancelMenuItem = module.FormActionMenu.FindByText("Cancel");
				var formActionCancelAndMarkInactiveMenuItem = module.FormActionMenu.FindByText("Cancel");

				AssertNull(formActionCancelMenuItem);
				AssertNull(formActionCancelAndMarkInactiveMenuItem);
			}
		}

		public void TestGetNewAdditionalContextMenuItems()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				Env.Security.ReportManagementCancelExecution.IsAllowed = false;
				Env.Security.ReportManagementCancelExecutionAndMarkAsInactive.IsAllowed = false;
				var menuItems = module.GetNewAdditionalContextMenuItems();
				AssertEquals(0, menuItems.Length);

				Env.Security.ReportManagementCancelExecution.IsAllowed = true;
				menuItems = module.GetNewAdditionalContextMenuItems();
				AssertEquals(1, menuItems.Length);
				AssertNotNull("Cancel", menuItems[0].Text);

				Env.Security.ReportManagementCancelExecutionAndMarkAsInactive.IsAllowed = true;
				menuItems = module.GetNewAdditionalContextMenuItems();
				AssertEquals(2, menuItems.Length);
				AssertNotNull("Cancel && Mark As Inactive", menuItems[1].Text);
				AssertNotNull("Cancel", module.DisplayGrid.ContextMenu.MenuItems.FindByName("Cancel"));
				AssertNotNull("CancelAndMarkInactive", module.DisplayGrid.ContextMenu.MenuItems.FindByName("CancelAndMarkInactive"));
			}
		}

		public void TestCancelRunningReport()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_EndAfterCount = 5;
				var stmReportRun = module.Factory.NewWithValidTestData<StmReportRun>();
				stmReportRun.RRI_Status = Constants.StmReportRunState.Running;
				stmReportRun.RRI_S5_Schedule = scheduleTask.PK;
				module.Factory.Save();

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, true);
				Assert(!scheduleTask.S5_IsActive);
				Assert("Status Scheduled report should be updated", scheduleTask.StmReportRuns[0].RRI_Status == Constants.StmReportRunState.Cancelled);
			}
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_EndAfterCount = 5;
				var stmReportRun = module.Factory.NewWithValidTestData<StmReportRun>();
				stmReportRun.RRI_Status = Constants.StmReportRunState.Running;
				stmReportRun.RRI_S5_Schedule = scheduleTask.PK;
				scheduleTask.S5_IsActive = true;
				module.Factory.Save();

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, false);
				Assert(scheduleTask.S5_IsActive);
				Assert("Status Scheduled report should be updated", scheduleTask.StmReportRuns[0].RRI_Status == Constants.StmReportRunState.Cancelled);
			}
		}

		public void TestCancelPrivateRunningReport()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_EndAfterCount = 5;
				scheduleTask.S5_IsPrivate = true;
				var stmReportRun = module.Factory.NewWithValidTestData<StmReportRun>();
				stmReportRun.RRI_Status = Constants.StmReportRunState.Running;
				stmReportRun.RRI_S5_Schedule = scheduleTask.PK;
				module.Factory.Save();

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, true);
				Assert(!scheduleTask.S5_IsActive);
				Assert("Status Scheduled report should be updated", scheduleTask.StmReportRuns[0].RRI_Status == Constants.StmReportRunState.Cancelled);
			}
		}

		public void TestCancelPrivateQueuedReport()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_EndAfterCount = 5;
				scheduleTask.S5_IsPrivate = true;
				module.Factory.Save();

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, true);
				AssertNull(module.Factory.Load(scheduleTask.TablePrefix, scheduleTask.PK));
			}
		}

		public void TestCancelQueuedReportNotOnLastOccurence()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_EndAfterCount = 3;
				scheduleTask.S5_ScheduleActualRunCount = 1;
				module.Factory.Save();
				var firstScheduleTime = scheduleTask.S5_NextScheduledPrintRunTimeUtc;

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, false);
				Assert("Schedule task should remain active since there is 1 more run remaining", scheduleTask.S5_IsActive);
				AssertEquals("Current run should be cancelled, so the next run time should be updated", scheduleTask.Recurrence.CalculateNextScheduleDate(firstScheduleTime), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			}
		}

		public void TestCancelQueuedReportOnLastOccurence()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_EndAfterCount = 3;
				scheduleTask.S5_ScheduleActualRunCount = 2;
				module.Factory.Save();
				var firstScheduleTime = scheduleTask.S5_NextScheduledPrintRunTimeUtc;

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, false);
				Assert("Schedule task should remain active and the last run should be rescheduled", scheduleTask.S5_IsActive);
				AssertEquals("Current run should be cancelled, so the next run time should be updated, even if the report is inactive.", scheduleTask.Recurrence.CalculateNextScheduleDate(firstScheduleTime), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			}
		}

		public void TestCancelQueuedReportLongBeforeEndDate()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				module.Factory.Save();
				scheduleTask.S5_EndDate = scheduleTask.S5_NextScheduledPrintRunTimeUtc.AddDays(30);
				module.Factory.Save();
				var firstScheduleTime = scheduleTask.S5_NextScheduledPrintRunTimeUtc;

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, false);
				Assert("Schedule task should remain active since there are many runs still remaining", scheduleTask.S5_IsActive);
				AssertEquals("Current run should be cancelled, so the next run time should be updated", scheduleTask.Recurrence.CalculateNextScheduleDate(firstScheduleTime), scheduleTask.S5_NextScheduledPrintRunTimeUtc);
			}
		}

		public void TestCancelQueuedReportJustBeforeEndDate()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_EndDate = scheduleTask.Recurrence.CalculateNextScheduleDate().AddHours(-1);
				module.Factory.Save();

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, false);
				Assert("Schedule task should not remain active", !scheduleTask.S5_IsActive);
			}
		}

		public void TestCancelMixedReports()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTaskWithQueuedReport = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTaskWithQueuedReport.S5_EndAfterCount = 1;
				var scheduleTaskWithCurrentlyRunningReport = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTaskWithCurrentlyRunningReport.S5_EndAfterCount = 5;
				var runningReport = module.Factory.NewWithValidTestData<StmReportRun>();
				runningReport.RRI_Status = Constants.StmReportRunState.Running;
				runningReport.RRI_S5_Schedule = scheduleTaskWithCurrentlyRunningReport.PK;
				module.Factory.Save();
				var firstScheduleTimeForQueued = scheduleTaskWithQueuedReport.S5_NextScheduledPrintRunTimeUtc;

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTaskWithQueuedReport, scheduleTaskWithCurrentlyRunningReport }, false);
				Assert("ScheduleTask's last run should be rescheduled, it should not be deactivated", scheduleTaskWithQueuedReport.S5_IsActive);
				AssertEquals("Queued run that was cancelled should be rescheduled", scheduleTaskWithQueuedReport.Recurrence.CalculateNextScheduleDate(firstScheduleTimeForQueued), scheduleTaskWithQueuedReport.S5_NextScheduledPrintRunTimeUtc);
				Assert("ScheduleTask still had several runs left over, show not be inactive", scheduleTaskWithCurrentlyRunningReport.S5_IsActive);
			}
		}

		public void TestCancelAndMarkInactiveMixedReports()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTaskWithQueuedReport = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTaskWithQueuedReport.S5_EndAfterCount = 5;
				var scheduleTaskWithCurrentlyRunningReport = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTaskWithCurrentlyRunningReport.S5_EndAfterCount = 5;
				var runningReport = module.Factory.NewWithValidTestData<StmReportRun>();
				runningReport.RRI_Status = Constants.StmReportRunState.Running;
				runningReport.RRI_S5_Schedule = scheduleTaskWithCurrentlyRunningReport.PK;
				module.Factory.Save();

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTaskWithQueuedReport, scheduleTaskWithCurrentlyRunningReport }, true);
				Assert("ScheduleTask was just force-marked as inactive, it should be inactive", !scheduleTaskWithQueuedReport.S5_IsActive);
				Assert("ScheduleTask was just force-marked as inactive, it should be inactive", !scheduleTaskWithCurrentlyRunningReport.S5_IsActive);
			}
		}

		public void TestCancelRunningReportHandlesSaveConcurrency()
		{
			var scheduleReport = Factory.NewWithValidTestData<ReportScheduleTask>();
			Factory.Save();
			using (var module = new ReportManagementModuleForTest())
			{
				var report = module.Factory.Load<ReportScheduleTask>(scheduleReport.PK);

				var secondFactory = new BusinessObjectFactory();
				secondFactory.RefreshEnabled = false;

				var reportBis = secondFactory.Load<ReportScheduleTask>(scheduleReport.PK);
				reportBis.Delete();
				secondFactory.Save();
				report.S5_ScheduleDescription = "this is not a string.";

				AssertNoExceptionThrown("Save concurrency is handled ", () => module.CancelScheduleReportCore(new BusinessObject[] { report }, false));
			}
		}

		[TestDate(2024, 12, 09)]
		public void TestCancelMultipleReports_IfSavingFaild_ShouldNotSendNotifications()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var lastEditStaff = Factory.NewWithValidTestData<GlbStaff>();
				var currentStaff = Factory.NewWithValidTestData<GlbStaff>();
				lastEditStaff.GS_Code = "GS1";
				lastEditStaff.GS_EmailAddress = "reportLastEditStaff@test.com";

				currentStaff.GS_Code = "GS2";
				currentStaff.GS_EmailAddress = "currentStaff@test.com";
				Factory.Save();

				var scheduleTask_Normal = module.Factory.NewWithValidTestData<ReportScheduleTask>();
				var scheduleTask_DeterminedToFail = module.Factory.NewWithValidTestData<ReportScheduleTaskForSavingFailureTest>();

				using (Env.SetTemporaryUserContext(lastEditStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					scheduleTask_Normal.S5_ScheduleDescription = "Testing Report 1";
					scheduleTask_DeterminedToFail.S5_ScheduleDescription = "Testing Report 2";
					module.Factory.Save();
					scheduleTask_DeterminedToFail.SavingShouldFail = true;
				}
				try
				{
					module.CancelScheduleReportCore([scheduleTask_Normal, scheduleTask_DeterminedToFail], true);
				}
				catch(InvalidOperationException)
				{
				}

				AssertEquals("EmailsCreated", 2, Env.OutgoingMailManager.EmailsCreated.Count);
				Assert("No emails should be saved because the cancelation of scheduled report failed", !Factory.ExistsInDatabase(MailDBItemsSchema.Constants.TableName, new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.StartsWith, "Report has been canceled")));
			}
		}

		class ReportScheduleTaskForSavingFailureTest : ReportScheduleTask
		{
			public ReportScheduleTaskForSavingFailureTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			internal bool SavingShouldFail { get; set; }

			public override void OnSaving()
			{
				if(SavingShouldFail)
				{
					throw new InvalidOperationException();
				}

				base.OnSaving();
			}
		}

		[TestDate(2024, 12, 09)]
		public void TestCancelReport_SendNotificationEmailForOneOffScheduledReport()
		{
			using var module = new ReportManagementModuleForTest();
			var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
			AssertReportCanceledNotificationEmail(new[] { "reportLastEditStaff@test.com", "reportCreateStaff@test.com" }, scheduleTask, module, isPrivate: true);
		}

		[TestDate(2024, 12, 09)]
		public void TestCancelReport_SendNotificationEmailToContactUser()
		{
			using var module = new ReportManagementModuleForTest();
			var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
			AssertReportCanceledNotificationEmail(Array.Empty<string>(), scheduleTask, module, true);
		}

		[TestDate(2024, 12, 09)]
		public void TestCancelReport_SendNotificationEmailToLastEditUser()
		{
			using var module = new ReportManagementModuleForTest();
			var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
			AssertReportCanceledNotificationEmail(new[] { "reportLastEditStaff@test.com", "reportCreateStaff@test.com" }, scheduleTask, module);
		}

		[TestDate(2024, 12, 09)]
		public void TestCancelReport_SendNotificationEmailToGroupStaff()
		{
			using var module = new ReportManagementModuleForTest();
			var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
			SetUpNotificationGroup(true);
			AssertReportCanceledNotificationEmail(new[] { "groupStaff@user.com", "reportCreateStaff@test.com" }, scheduleTask, module);
		}

		[TestDate(2024, 12, 09)]
		public void TestCancelReport_SendNotificationEmailToStaffRole()
		{
			using var module = new ReportManagementModuleForTest();
			var scheduleTask = module.Factory.NewWithValidTestData<ReportScheduleTask>();
			SetUpNotificationStaffRole("JTR", scheduleTask);
			AssertReportCanceledNotificationEmail(new[] { "roleStaff@user.com", "reportCreateStaff@test.com" }, scheduleTask, module);
		}

		void AssertReportCanceledNotificationEmail(string[] expectedRecipientEmails, StmScheduleTask scheduleTask, ReportManagementModuleForTest module, bool isContact = false, bool isPrivate = false)
		{
			var lastEditStaff = Factory.NewWithValidTestData<GlbStaff>();
			var currentStaff = Factory.NewWithValidTestData<GlbStaff>();
			var createStaff = Factory.NewWithValidTestData<GlbStaff>();

			createStaff.GS_Code = "GS0";
			createStaff.GS_EmailAddress = "reportCreateStaff@test.com";

			lastEditStaff.GS_Code = "GS1";
			lastEditStaff.GS_EmailAddress = "reportLastEditStaff@test.com";

			currentStaff.GS_Code = "GS2";
			currentStaff.GS_EmailAddress = "currentStaff@test.com";
			Factory.Save();

			using (Env.SetTemporaryUserContext(createStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				scheduleTask.S5_ScheduleDescription = "Testing Report";
				scheduleTask.S5_IsPrivate = true;
				module.Factory.Save();
			}

			using (Env.SetTemporaryUserContext(currentStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				if (isContact)
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();
					var contact = Factory.NewWithValidTestData<OrgContact>();
					org.Contacts.Add(contact);
					contact.OC_Email = "contactUser@test.com";
					Factory.Save();
					scheduleTask.S5_OC_ScheduledBy = contact.PK;
					scheduleTask.S5_SystemCreateUser = User.WebUserCode;
				}
				scheduleTask.S5_SystemLastEditUser = "GS1";

				module.CancelScheduleReportCore(new BusinessObject[] { scheduleTask }, true);
			}
			if (isContact)
			{
				AssertEquals("EmailsCreated", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				return;
			}
			AssertEquals("EmailsCreated", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email Subject", "Report has been canceled: Testing Report", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("Email Recipients Count", expectedRecipientEmails.Length, Env.OutgoingMailManager.EmailsCreated[0].Recipients.Count);
			AssertContainsExactElementsInAnyOrder(expectedRecipientEmails, Env.OutgoingMailManager.EmailsCreated[0].Recipients.ToStringCollection());
			AssertEquals("Email Body", $"Report Testing Report was canceled by GS2 on 09-Dec-24 00:00 +00:00.\r\n", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		void SetUpNotificationGroup(bool shouldAddUserToGroup)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();

			if (shouldAddUserToGroup)
			{
				var staff = group.Staff.AddNew();
				staff.GS_EmailAddress = "groupStaff@user.com";
				staff.GS_LoginName = "JerryTest";
				staff.GS_Code = "JYT";
			}
			Factory.Save();

			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.GRP);
			SystemDataRegistry.Instance.SRRErrorNotificationGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
		}

		void SetUpNotificationStaffRole(string roleCode, StmScheduleTask scheduleTask)
		{
			var staffRoles = new CodeDescriptionBoolDisallowNewCollection
			{
				new CodeDescriptionBoolDisallowNew { Bool = true, Code = "JTR" },
			};

			SystemDataRegistry.Instance.SRRErrorNotificationStaffRoles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, staffRoles);
			SystemDataRegistry.Instance.SRRErrorNotificationOptions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.ErrorNotificationOptions.Code.ROL);

			var newStaff = Factory.NewWithValidTestData<GlbStaff>();
			newStaff.GS_Code = "RSF";
			newStaff.GS_EmailAddress = "roleStaff@user.com";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();

			var staffAssignments = orgHeader.StaffAssignments.AddNew();
			staffAssignments.O8_Role = roleCode;
			staffAssignments.O8_GS_NKPersonResponsible = newStaff.GS_Code;

			scheduleTask.Recipients.RemoveAndDeleteAll();
			var recipient = scheduleTask.Recipients.AddNew();
			recipient.S6_OH = orgHeader.PK;
			recipient.S6_OC = contact.PK;
			Factory.Save();
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (var module = new ReportManagementModuleForTest())
			using (var filterControl = module.NewFilterControl)
			{
				Assert(filterControl is ReportManagementFilterControl);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var scheduleTasksCollection = module.NewGridCollection;
				Assert(scheduleTasksCollection is ReportScheduleTaskCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new ReportManagementModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert(filterBusinessObject is ReportManagementFilterBusinessObject);
			}
		}

		#endregion
	}
}
