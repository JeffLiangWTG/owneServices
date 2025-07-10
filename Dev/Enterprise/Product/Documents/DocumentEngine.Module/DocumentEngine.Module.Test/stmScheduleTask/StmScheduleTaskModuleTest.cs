using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ScheduledReportsModule))]
	sealed class StmScheduleTaskModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ScheduledReports;
		}

		public void TestModuleID()
		{
			using (ScheduledReportsModule module = new ScheduledReportsModule())
			{
				AssertEquals("ModuleID", ModuleIDs.ScheduledReports, module.ID);
			}
		}

		public void TestCheckpoints()
		{
			using (ScheduledReportsModule module = new ScheduledReportsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ScheduledTask, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		public void TestGetNewFilterControl()
		{
			using (ScheduledReportsModuleForTest module = new ScheduledReportsModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is StmScheduleTaskFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (ScheduledReportsModuleForTest module = new ScheduledReportsModuleForTest())
			{
				IBusinessObjectCollection scheduleTasksCollection = module.NewGridCollection;
				Assert("Invalid type", scheduleTasksCollection is ReportScheduleTaskCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (ScheduledReportsModuleForTest module = new ScheduledReportsModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is StmScheduleTaskFilterBusinessObject);
			}
		}

		#endregion

		SecurityCore GetTempSecurityInstance()
		{
			return new SecurityCore(null, Env.CurrentUserPK, Env.CurrentBranchPK, Env.CurrentDepartmentPK, Env.CurrentCompanyPK);
		}

		StmScheduleTask CreateOwnedScheduleTask(IUser taskOwner)
		{
			var stmScheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			stmScheduleTask.S5_ScheduleDescription = "stmScheduleTask";
			stmScheduleTask.S5_IsPrivate = false;
			Factory.Save();
			stmScheduleTask.S5_SystemCreateUser = taskOwner?.Initials ?? string.Empty;
			Factory.Save();
			return stmScheduleTask;
		}

		IDisposable SetTemporaryScheduledTaskEditOtherReportIsAllowed(bool isAllowed)
		{
			var securityInstance = GetTempSecurityInstance();
			securityInstance.ScheduledTaskEditOtherReport.IsAllowed = isAllowed;
			return Env.SetTemporarySecurityInstanceForTest(securityInstance);
		}

		void AssertPermissionDenied(bool expectedPermissionDenied)
		{
			if (expectedPermissionDenied)
			{
				AssertEquals("Should show permission denied message", "Access Denied: Edit Other People's reports", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
			else
			{
				AssertNotEquals("Should not show permission denied message", "Access Denied: Edit Other People's reports", UnitTestUserNotification.Instance.LastMessage.Caption);
			}
		}

		public void TestScheduleNowMenu_Click_WhenUserCannotEditOtherReportsAndReportIsNotOwned()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var stmScheduleTask = CreateOwnedScheduleTask(null);

			using (SetTemporaryScheduledTaskEditOtherReportIsAllowed(false))
			{
				using var module = new ScheduledReportsModuleForTest();
				var selectedElements = new BusinessObject[] { stmScheduleTask };
				module.SetSelectedGridElements(selectedElements);
				var menuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				menuItem.PerformClick();
				AssertPermissionDenied(true);
			}
		}

		public void TestScheduleNowMenu_Click_WhenUserCannotEditOtherReportsAndReportIsOwnedByUser()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var stmScheduleTask = CreateOwnedScheduleTask(Env.CurrentUser);

			using (SetTemporaryScheduledTaskEditOtherReportIsAllowed(false))
			{
				using var module = new ScheduledReportsModuleForTest();
				var selectedElements = new BusinessObject[] { stmScheduleTask };
				module.SetSelectedGridElements(selectedElements);
				var menuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				menuItem.PerformClick();
				AssertPermissionDenied(false);
			}
		}

		public void TestScheduleNowMenu_Click_WhenUserCannotEditOtherReportsAndOwnsSomeReports()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var stmScheduleTaskOwned1 = CreateOwnedScheduleTask(Env.CurrentUser);
			var stmScheduleTaskOwned2 = CreateOwnedScheduleTask(Env.CurrentUser);
			var stmScheduleTaskNotOwned1 = CreateOwnedScheduleTask(null);
			var stmScheduleTaskNotOwned2 = CreateOwnedScheduleTask(null);

			using (SetTemporaryScheduledTaskEditOtherReportIsAllowed(false))
			{
				using var module = new ScheduledReportsModuleForTest();
				var selectedElements = new BusinessObject[] { stmScheduleTaskOwned1, stmScheduleTaskOwned2, stmScheduleTaskNotOwned1, stmScheduleTaskNotOwned2 };
				module.SetSelectedGridElements(selectedElements);
				var menuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				menuItem.PerformClick();
				AssertPermissionDenied(true);
			}
		}

		public void TestScheduleNowMenu_Click_WhenUserCannotEditOtherReportsAndOwnsAllReports()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var stmScheduleTaskOwned1 = CreateOwnedScheduleTask(Env.CurrentUser);
			var stmScheduleTaskOwned2 = CreateOwnedScheduleTask(Env.CurrentUser);

			using (SetTemporaryScheduledTaskEditOtherReportIsAllowed(false))
			{
				using var module = new ScheduledReportsModuleForTest();
				var selectedElements = new BusinessObject[] { stmScheduleTaskOwned1, stmScheduleTaskOwned2 };
				module.SetSelectedGridElements(selectedElements);
				var menuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				menuItem.PerformClick();
				AssertPermissionDenied(false);
			}
		}

		public void TestScheduleNowMenu_Click_WhenUserCannotEditOtherReportsAndOwnsCurrentBusinessObjectInGrid()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var stmScheduleTask = CreateOwnedScheduleTask(Env.CurrentUser);

			using (SetTemporaryScheduledTaskEditOtherReportIsAllowed(false))
			{
				using var module = new ScheduledReportsModuleForTest();
				module.SetSelectedGridElements(Array.Empty<BusinessObject>());
				module.CurrentBusinessObjectInGrid_Exposed = stmScheduleTask;
				var menuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				menuItem.PerformClick();
				AssertPermissionDenied(false);
			}
		}

		public void TestScheduleNowMenu_Click_WhenUserCannotEditOtherReportsAndDoesNotOwnCurrentBusinessObjectInGrid()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var stmScheduleTask = CreateOwnedScheduleTask(null);

			using (SetTemporaryScheduledTaskEditOtherReportIsAllowed(false))
			{
				using var module = new ScheduledReportsModuleForTest();
				module.SetSelectedGridElements(Array.Empty<BusinessObject>());
				module.CurrentBusinessObjectInGrid_Exposed = stmScheduleTask;
				var menuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				menuItem.PerformClick();
				AssertPermissionDenied(true);
			}
		}

		public void TestScheduleNowMenu_Click_WhenUserCanEditOtherReports()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var stmScheduleTask = CreateOwnedScheduleTask(null);

			using (SetTemporaryScheduledTaskEditOtherReportIsAllowed(true))
			{
				using var module = new ScheduledReportsModuleForTest();
				var selectedElements = new BusinessObject[] { stmScheduleTask };
				module.SetSelectedGridElements(selectedElements);
				var menuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				menuItem.PerformClick();
				AssertPermissionDenied(false);
			}
		}

		public void TestScheduleNowMenu_Click()
		{
			var stmScheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			stmScheduleTask.S5_ScheduleDescription = "stmScheduleTask";
			stmScheduleTask.S5_IsPrivate = false;
			stmScheduleTask.CalcNextRunTimeLocal = ZDateTime.Now.AddDays(2);

			var stmScheduleTask2 = Factory.NewWithValidTestData<StmScheduleTask>();
			stmScheduleTask2.S5_ScheduleDescription = "stmScheduleTask2";
			stmScheduleTask2.S5_IsPrivate = false;
			stmScheduleTask2.CalcNextRunTimeLocal = ZDateTime.Now.AddDays(2);

			Factory.Save();

			using (var module = new ScheduledReportsModuleForTest())
			{
				var scheduleNowMenuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
				AssertNotNull("Schedule Now menu item should exist", scheduleNowMenuItem);

				var scheduleNowButton = Array.Find(module.ToolBarButtons, (button) => button.Text == "Schedule Now");
				AssertNotNull("Schedule Now button should exist", scheduleNowButton);
				AssertEquals(Icons.GetImageIndex(IconTypes.Events), scheduleNowButton.ImageIndex);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				scheduleNowMenuItem.PerformClick();
				AssertEquals("Do you really want to schedule the selected schedule report(s) to run now?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				scheduleNowMenuItem.PerformClick();
				AssertEquals("Please select a record in the grid.", ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);

				module.GridCollection.Clear();
				module.SetSelectedGridElements(new[] { stmScheduleTask });
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				scheduleNowMenuItem.PerformClick();

				var query = new ZQuery(StmScheduleTaskSchema.PK, SQLComparisonOperator.NotEqual, stmScheduleTask.PK);
				query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleDescription, "stmScheduleTask");
				var stmScheduleTaskClone = Factory.LoadTop1<StmScheduleTask>(query);
				AssertNotNull("Schedule Task Clone should exist", stmScheduleTaskClone);
				AssertEquals(true, stmScheduleTaskClone.S5_IsPrivate);
				AssertGreaterThanOrEqualTo(ZDateTime.Now, stmScheduleTaskClone.CalcNextRunTimeLocal);

				module.GridCollection.Clear();
				module.SetSelectedGridElements(Array.Empty<StmScheduleTask>());
				module.CurrentBusinessObjectInGrid_Exposed = stmScheduleTask2;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				scheduleNowMenuItem.PerformClick();

				query = new ZQuery(StmScheduleTaskSchema.PK, SQLComparisonOperator.NotEqual, stmScheduleTask2.PK);
				query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleDescription, "stmScheduleTask2");
				var stmScheduleTask2Clone = Factory.LoadTop1<StmScheduleTask>(query);
				AssertNotNull("Schedule Task Clone should exist", stmScheduleTask2Clone);
				AssertEquals(true, stmScheduleTask2Clone.S5_IsPrivate);
				AssertGreaterThanOrEqualTo(ZDateTime.Now, stmScheduleTask2Clone.CalcNextRunTimeLocal);
			}
		}

		public void TestActivateDeactivate()
		{
			var scheduleTask1 = Factory.NewWithValidTestData<StmScheduleTask>();
			var scheduleTask2 = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask1.S5_IsActive = true;
			scheduleTask2.S5_IsActive = false;

			var activator = new BusinessObjectActivator();
			activator.Activate(new BusinessObject[] { scheduleTask1, scheduleTask2 }, EnvProxy.Instance.Security.None);
			CombineAssertions("Activate should work properly", () =>
			{
				Assert(scheduleTask1.S5_IsActive);
				Assert(scheduleTask2.S5_IsActive);
			});

			activator.Deactivate(new BusinessObject[] { scheduleTask1, scheduleTask2 }, EnvProxy.Instance.Security.None);
			CombineAssertions("Deactivate should work properly", () =>
			{
				Assert(!scheduleTask1.S5_IsActive);
				Assert(!scheduleTask2.S5_IsActive);
			});
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScheduleNow_FiltersWereUpdated()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DateRangeFilterWithRequired.xls", TestFilesSubFolder.ReportTestFiles);
			var template = Factory.NewWithValidTestData<StmTemplateBase>();
			template.SO_Name = "NewLine Template 1";
			template.SO_Template = excelTemplate.GetAsByteArray();
			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(reportCommand))
			using (var report = new Report(documentPack, excelTemplate))
			{
				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.Recipients.RemoveAndDeleteAll();

				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = ContactNotifyModes.Email;
				recipient.AttachmentType = AttachmentTypeList.Codes.Xls;
				recipient.Email = @"unit.test@cargowise.com";

				var scheduledReport = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.PrepareForRender();
				report.SetScheduleTask(scheduledReport);
				((DateRangeField)report.FilterCollection["Some date"]).LowSchedule.PeriodScope = PeriodScopeList.Codes.Previous;

				scheduledReport.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
				scheduledReport.S5_ScheduleDescription = "stmScheduleTask";
				scheduledReport.S5_ParentID = reportCommand.PK;
				scheduledReport.S5_ScheduleState = scheduledReport.SerializeForTesting(deliveryInstructions, report);
				scheduledReport.S5_EndDate = scheduledReport.S5_StartDate.AddDays(10);
				scheduledReport.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(-5);

				var scheduledReportRecipient = scheduledReport.Recipients[0];
				scheduledReportRecipient.S6_EmptyReportDeliveryOptions = EmptyReportContingencyList.Codes.SendEmailNotification;
				Factory.Save();

				using (var module = new ScheduledReportsModuleForTest())
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					var scheduleNowMenuItem = module.GetNewAdditionalMenuItems().FindByText("Schedule Now");
					module.GridCollection.Clear();
					module.SetSelectedGridElements(new[] { scheduledReport });
					scheduleNowMenuItem.PerformClick();

					var query = new ZQuery(StmScheduleTaskSchema.PK, SQLComparisonOperator.NotEqual, scheduledReport.PK);
					query.AddToFilter(StmScheduleTaskSchema.S5_ScheduleDescription, "stmScheduleTask");
					var scheduledReportClone = Factory.LoadTop1<ReportScheduleTask>(query);
					var reportInfo = scheduledReportClone.CreateReportFromTask();

					AssertEquals(true, reportInfo.Report.ShouldUpdateSchedulableFilters);
					AssertEquals(PeriodScopeList.Codes.Previous, ((DateRangeField)reportInfo.Report.FilterCollection["Some date"]).LowSchedule.PeriodScope);
					AssertNoExceptionThrown(() => scheduledReportClone.Run());
				}
			}
		}

		public void TestAllowUniversalCopy()
		{
			using var module = new ScheduledReportsModuleForTest();
			Assert("Scheduled Report module does not support universal copy at current stage.", !module.AllowUniversalCopy);
		}
	}
}
