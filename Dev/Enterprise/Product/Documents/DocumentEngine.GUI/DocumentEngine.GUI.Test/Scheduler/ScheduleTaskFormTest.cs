using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Scheduler.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Common;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	[TestedType(typeof(ScheduleTaskForm))]
	sealed class ScheduleTaskFormTest : ZFormBasherTest
	{
		public void TestReportFilterIsNotShownOnSaveIfIsPrivate()
		{
			using (ScheduleTaskForm form = (ScheduleTaskForm)GetFormToBash())
			{
				form.BusinessEntity.S5_IsActive = true;
				form.BusinessEntity.S5_IsPrivate = true;
				form.BusinessEntity.S5_ParentID = ReportScheduleTaskTest.TestReportPK;
				form.BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				form.BusinessEntity.UserFK = staff.PK;
				AddRecipient(form.BusinessEntity);

				form.FireSaveButton();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("No form should be shown.", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("BusinessEntity.IsInDatabase", true, form.BusinessEntity.IsInDatabase);
			}
		}

		public void TestReportFilterIsNotShownOnSaveIfInactive()
		{
			using (ScheduleTaskForm form = (ScheduleTaskForm)GetFormToBash())
			{
				form.BusinessEntity.S5_IsActive = false;
				form.BusinessEntity.S5_ParentID = ReportScheduleTaskTest.TestReportPK;
				form.BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				form.BusinessEntity.S5_ScheduleDescription = "!@#";
				AddRecipient(form.BusinessEntity);

				form.FireSaveButton();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("No form should be shown.", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("BusinessEntity.IsInDatabase", true, form.BusinessEntity.IsInDatabase);
			}
		}

		public void TestShowReportFilter_IfReportIsNull()
		{
			using (var form = (ScheduleTaskForm)GetFormToBash())
			{
				var reportCommand = Factory.New<ReportCommand>();

				form.BusinessEntity.S5_IsActive = true;
				form.BusinessEntity.S5_IsPrivate = false;
				form.BusinessEntity.S5_ScheduleDescription = "Test of the year";
				form.BusinessEntity.S5_ParentID = reportCommand.PK;
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				form.BusinessEntity.UserFK = staff.PK;
				AddRecipient(form.BusinessEntity);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertEquals("The report does not contain any template. Please set the report again.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestReportFilterIsShownOnSaveIfIsNotPrivate()
		{
			using (ScheduleTaskForm form = (ScheduleTaskForm)GetFormToBash())
			{
				form.BusinessEntity.S5_IsActive = true;
				form.BusinessEntity.S5_IsPrivate = false;
				form.BusinessEntity.S5_ScheduleDescription = "!@#";
				form.BusinessEntity.S5_ParentID = ReportScheduleTaskTest.TestReportPK;
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				form.BusinessEntity.UserFK = staff.PK;
				AddRecipient(form.BusinessEntity);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				form.FireSaveButton();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				using (RuntimeOptionsForm lastShownForm = (RuntimeOptionsForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("ZFormModaliser.LastFormShownDialogForTest.ReportExposedForTesting.ScheduleTask", form.BusinessEntity, lastShownForm.ReportExposedForTesting.ScheduleTask);
				}
				AssertEquals("BusinessEntity.IsInDatabase", false, form.BusinessEntity.IsInDatabase);

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();
				AssertNull("There should not be any error messages.", UnitTestUserNotification.Instance.LastMessage.Text);
				using (RuntimeOptionsForm lastShownForm = (RuntimeOptionsForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals("ZFormModaliser.LastFormShownDialogForTest.ReportExposedForTesting.ScheduleTask", form.BusinessEntity, lastShownForm.ReportExposedForTesting.ScheduleTask);
				}
				AssertEquals("BusinessEntity.IsInDatabase", true, form.BusinessEntity.IsInDatabase);
			}
		}

		public void TestReportFilterCannotBeShowWithoutEditSecurity()
		{
			using (ScheduleTaskForm form = (ScheduleTaskForm)GetFormToBash())
			{
				form.BusinessEntity.S5_IsPrivate = true;
				form.BusinessEntity.S5_ParentID = ReportScheduleTaskTest.TestReportPK;
				form.BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				AddRecipient(form.BusinessEntity);

				form.FireSaveButton();
				form.BusinessEntity.S5_SystemCreateUser = Env.CurrentUser.Initials;

				Env.Security.ScheduledTaskEdit.IsAllowed = false;
				Env.Security.ScheduledTaskEditOtherReport.IsAllowed = true;
				form.ChangeReportFilters_Click(null, null);

				AssertEquals("There should be error messages.", Env.Security.ScheduledTaskEdit.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("No form should be shown.", ZFormModaliser.LastFormShownDialogForTest);

				form.BusinessEntity.S5_SystemCreateUser = "ABC";

				Env.Security.ScheduledTaskEdit.IsAllowed = true;
				Env.Security.ScheduledTaskEditOtherReport.IsAllowed = false;
				form.ChangeReportFilters_Click(null, null);

				AssertEquals("There should be error messages.", Env.Security.ScheduledTaskEditOtherReport.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("No form should be shown.", ZFormModaliser.LastFormShownDialogForTest);

				form.BusinessEntity.S5_SystemCreateUser = "";

				Env.Security.ScheduledTaskEdit.IsAllowed = false;
				Env.Security.ScheduledTaskEditOtherReport.IsAllowed = true;
				form.ChangeReportFilters_Click(null, null);

				AssertEquals("There should be error messages.", Env.Security.ScheduledTaskEdit.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("No form should be shown.", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestScheduleOtherStaffAsPrintUserIsAllowed()
		{
			Env.Security.ScheduleOtherStaffAsPrintUser.IsAllowed = false;
			using (var formUserNotAllowed = (ScheduleTaskForm)GetFormToBash())
			{
				AssertEquals("UserFindBox should be readonly.", true, formUserNotAllowed.GetUserFindBoxForTest().ReadOnly);
			}

			Env.Security.ScheduleOtherStaffAsPrintUser.IsAllowed = true;
			using (var formUserAllowed = (ScheduleTaskForm)GetFormToBash())
			{
				AssertEquals("UserFindBox should not be readonly.", false, formUserAllowed.GetUserFindBoxForTest().ReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ScheduleTaskForm(Factory.NewWithValidTestData<ReportScheduleTask>());
		}

		void AddRecipient(ReportScheduleTask scheduleTask)
		{
			OrgHeader organisation = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact = organisation.Contacts.AddNew();
			contact.OC_ContactName = "Bob";
			contact.OC_Email = "bob@bob.com";

			ReportScheduleTaskRecipient recipient = scheduleTask.Recipients.AddNew();
			recipient.S6_DeliveryToType = ScheduledReportDeliveryRecipientConstants.RecipientType.Contact;
			recipient.S6_OH = organisation.PK;
			recipient.S6_DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.S6_AttachmentType = OrgConstants.AttachmentType.PDF;
			recipient.S6_OC = contact.PK;
			recipient.EmailToRecipients.Value = contact.OC_Email;
			recipient.S6_EmptyReportDeliveryOptions = "REP";
		}

		public void TestPrintUserAccessibility()
		{
			Env.Security.ScheduleOtherStaffAsPrintUser.IsAllowed = false;

			using var form = (ScheduleTaskForm)GetFormToBash();
			form.BusinessEntity.S5_IsPrivate = true;
			form.BusinessEntity.S5_ParentID = ReportScheduleTaskTest.TestReportPK;
			form.BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			form.BusinessEntity.S5_IsActive = true;
			form.BusinessEntity.S5_ScheduleDescription = "!@#";
			form.Show();

			var userFindBox = form.GetUserFindBoxForTest();

			AssertEquals("Print user should be readonly.", expected: true, userFindBox.ReadOnly);
			AssertEquals("The print user should be same as the create user.", form.BusinessEntity.S5_GS_NKPrintUser, userFindBox.SearchCode);
		}

		public void TestReportFilterButtonContent()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			using (var form = new ScheduleTaskForm(scheduleTask))
			{
				var reportFilterButton = form.ChangeReportFilters;
				AssertEquals("Change Report Filters", reportFilterButton.CaptionResourceString.Caption);
			}
			scheduleTask.S5_IsPrivate = true;
			using (var form = new ScheduleTaskForm(scheduleTask))
			{
				var reportFilterButton = form.ChangeReportFilters;
				AssertEquals("View Report Filters", reportFilterButton.CaptionResourceString.Caption);
			}
		}

		[RequiresSTA]
		public void TestMessageIsShownAfterClickReportFilterIfIsPrivate()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_IsPrivate = true;
			using (var form = new ScheduleTaskForm(scheduleTask))
			{
				form.BusinessEntity.S5_IsActive = true;
				form.BusinessEntity.S5_ParentID = ReportScheduleTaskTest.TestReportPK;
				form.BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				AddRecipient(form.BusinessEntity);

				form.ChangeReportFilters_Click(null,null);

				AssertEquals("Report Filters for One Off Reports cannot be modified and will be displayed in a read only view.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestReportFilterIsReadOnlyIfIsPrivate()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_IsPrivate = true;
			using (var form = new ScheduleTaskForm(scheduleTask))
			{
				form.BusinessEntity.S5_IsActive = true;
				form.BusinessEntity.S5_ParentID = ReportScheduleTaskTest.TestReportPK;
				form.BusinessEntity.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
				AddRecipient(form.BusinessEntity);

				form.ChangeReportFilters_Click(null, null);

				using (var lastShownForm = (RuntimeOptionsForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertEquals(ODisplayMode.ReadOnly, lastShownForm.DisplayMode);
				}
			}
		}

		public void TestReportStatisticsForm_ShouldBeOpenedInEditMode()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var stmReportRun = Factory.NewWithValidTestData<StmReportRun>();
			stmReportRun.RRI_S5_Schedule = reportScheduleTask.PK;

			Factory.Save();

			using var scheduleTaskForm = new ScheduleTaskForm(reportScheduleTask);
			scheduleTaskForm.Show();
			Application.DoEvents();

			var mainTabControl = scheduleTaskForm.FindSingle<ZTemplateTabControl>("MainTabControl");
			mainTabControl.SelectedIndex = 1;
			Application.DoEvents();

			var reportStatisticsModuleButtonGrid = scheduleTaskForm.FindSingle<ReportStatisticsModuleButtonGrid>("ReportStatisticsModuleButtonGrid");
			var showEditFormMethod = reportStatisticsModuleButtonGrid.GetType().GetMethod("ShowEditForm", BindingFlags.Instance | BindingFlags.NonPublic);
			showEditFormMethod.Invoke(reportStatisticsModuleButtonGrid, new object[] { stmReportRun });

			using var reportStatisticsForm = ZApplication.GetOpenForms().OfType<ReportStatisticsForm>().First();
			AssertNotNull(reportStatisticsForm);

			var saveButtonUserControl = reportStatisticsForm.FindSingle<Enterprise.Core.Forms.ZPostingButtonsUserControl>("SaveButtonUserControl");
			CombineAssertions(() =>
			{
				AssertEquals("Save button should be visible", true, saveButtonUserControl.SaveButton.Visible);
				AssertEquals("SaveAndCloseButton button should be visible", true, saveButtonUserControl.SaveAndCloseButton.Visible);
			});
		}

		[RequiresSTA]
		public void TestAuditTabShouldBeInScheduleTaskForm()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			using (var form = new ScheduleTaskForm(scheduleTask))
			{
				AssertEquals("Audit plugIn should be added", true, form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}
	}
}
