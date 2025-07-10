using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.GUI.Testing
{
	class SetAsFailedFromTransmissionMenuItemCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var exitHeader = Factory.New<CusExitHeader>();
			using (var provider = new EU.ExitControl.GUI.Testing.ReportsGridUserControlProviderForTesting())
			{
				var setAsFailedFromTransmissionMenuItem = new SetAsFailedFromTransmissionMenuItemCreator(provider);
				var menuItem = setAsFailedFromTransmissionMenuItem.Create();
				AssertEquals("Set Entry as Failed From Transmission", menuItem.Text);
			}
		}

		public void TestSetAsFailedFromTransmission_Click_OneReport()
		{
			var confirmationMessageText = "Are you sure you want to set this Report as Failed from Transmission?";

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = ZString.Empty;
			exitHeader.CXH_CustomsProfile = ZString.Empty;

			var consignment = exitHeader.CusExitConsignments.AddNew();
			consignment.CXC_MovementReference = "AAA";
			var report = exitHeader.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_MessageStatus = "AAA";

			Factory.Save();

			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var unitTestUserNotificationInstance = UnitTestUserNotification.Instance;

				var reportsTabUserControl = GetReportsTabUserControl(form);
				var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

				var setAsFailedFromTransmissionMenuItem = grid.ContextMenu.MenuItems.FindByText("Set Entry as Failed From Transmission");

				CombineAssertions(() =>
				{
					grid.Select();
					grid.Focus();

					setAsFailedFromTransmissionMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					setAsFailedFromTransmissionMenuItem.PerformClick();
					AssertEquals("Should not have message asking to confirm the action when report selected has message status not SNT", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));

					AssertEquals("report has message status different from SNT so nothing was done", "0 Reports were set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("report has the same message status as before because it was not SNT", "AAA", report.CER_MessageStatus);

					report.CER_MessageStatus = LogicalStatusList.Codes.Sent;
					Factory.Save();
					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					unitTestUserNotificationInstance.ClearMessagesAndAnswers();
					unitTestUserNotificationInstance.AddAnswer(ZDialogResult.Cancel);
					setAsFailedFromTransmissionMenuItem.PerformClick();
					AssertEquals("Should have message asking to confirm the action (when cancel)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));

					AssertEquals("report has message status SNT but nothing was done because the confirmation was cancelled", "0 Reports were set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("report has the same message status as before", "SNT", report.CER_MessageStatus);

					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					unitTestUserNotificationInstance.ClearMessagesAndAnswers();
					unitTestUserNotificationInstance.AddOKAnswer();
					setAsFailedFromTransmissionMenuItem.PerformClick();
					AssertEquals("Should have message asking to confirm the action (when ok)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));

					AssertEquals("report has message status SNT and the confirmation was accepted so the message status is changed", "1 Report was set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("report has new message status", "FAL", report.CER_MessageStatus);

					var mainFactoryChangeSet = Factory.GetChanges();
					var mainFactoryHasChanges = mainFactoryChangeSet.GetChangedObjects().Any() || mainFactoryChangeSet.GetAddedObjects().Any();
					AssertEquals("After pressing OK in the pop up the change will not be saved. Does Main Factory have changes?", true, mainFactoryHasChanges);
				});
			}
		}

		public void TestSetAsFailedFromTransmission_Click_MultipleReports()
		{
			var confirmationMessageText = "Are you sure you want to set these Reports as Failed from Transmission?";

			var exitHeader = Factory.NewWithValidTestData<CusExitHeader>();
			exitHeader.CXH_GS_NKCustomsAgent = ZString.Empty;
			exitHeader.CXH_CustomsProfile = ZString.Empty;

			var consignment1 = exitHeader.CusExitConsignments.AddNew();
			consignment1.CXC_MovementReference = "AAA";
			var report1 = exitHeader.CusExitReports.AddNew();
			report1.CER_CXC_Consignment = consignment1.PK;
			report1.CER_MessageStatus = LogicalStatusList.Codes.Sent;
			var consignment2 = exitHeader.CusExitConsignments.AddNew();
			consignment2.CXC_MovementReference = "BBB";
			var report2 = exitHeader.CusExitReports.AddNew();
			report2.CER_CXC_Consignment = consignment2.PK;
			report2.CER_MessageStatus = "AAA";
			var consignment3 = exitHeader.CusExitConsignments.AddNew();
			consignment3.CXC_MovementReference = "CCC";
			var report3 = exitHeader.CusExitReports.AddNew();
			report3.CER_CXC_Consignment = consignment3.PK;
			report3.CER_MessageStatus = LogicalStatusList.Codes.Sent;

			Factory.Save();

			using (var form = new ExitControlForm(exitHeader))
			{
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;
				var unitTestUserNotificationInstance = UnitTestUserNotification.Instance;

				var reportsTabUserControl = GetReportsTabUserControl(form);
				var grid = reportsTabUserControl.FindSingle<ZArchitecture.ZGrid>("ReportsGrid");

				var setAsFailedFromTransmissionMenuItem = grid.ContextMenu.MenuItems.FindByText("Set Entry as Failed From Transmission");

				CombineAssertions(() =>
				{
					grid.Select();
					grid.Focus();

					setAsFailedFromTransmissionMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a row first", UnitTestUserNotification.Instance.LastMessage.Text);

					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					unitTestUserNotificationInstance.ClearMessagesAndAnswers();
					unitTestUserNotificationInstance.AddAnswer(ZDialogResult.Cancel);
					unitTestUserNotificationInstance.AddAnswer(ZDialogResult.Cancel);
					setAsFailedFromTransmissionMenuItem.PerformClick();
					AssertEquals("When cancelling action we should get one confirmation for each report with message status SNT", 2, UnitTestUserNotification.Instance.PreviousMessages.Count(m => m.Contains(confirmationMessageText)));
					AssertEquals("Should have message asking to confirm the action (when cancel)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));

					AssertEquals("Nothing was done because the confirmation was cancelled", "0 Reports were set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("report1 has the same message status as before", "SNT", report1.CER_MessageStatus);
					AssertEquals("report2 has the same message status as before", "AAA", report2.CER_MessageStatus);
					AssertEquals("report3 has the same message status as before", "SNT", report3.CER_MessageStatus);

					reportsTabUserControl.Refresh();
					grid.SelectAllElements();
					unitTestUserNotificationInstance.ClearMessagesAndAnswers();
					unitTestUserNotificationInstance.AddOKAnswer();
					setAsFailedFromTransmissionMenuItem.PerformClick();
					AssertEquals("When accepting action we should get only one confirmation", 1, UnitTestUserNotification.Instance.PreviousMessages.Count(m => m.Contains(confirmationMessageText)));
					AssertEquals("Should have message asking to confirm the action (when ok)", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(confirmationMessageText));

					AssertEquals("Only 2 reports had message status SNT so they are the ones modified", "2 Reports were set to Failed from Transmission", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("report1 has new message status", "FAL", report1.CER_MessageStatus);
					AssertEquals("report2 has the same message status as before because it was not SNT", "AAA", report2.CER_MessageStatus);
					AssertEquals("report3 has new message status", "FAL", report3.CER_MessageStatus);

					var mainFactoryChangeSet = Factory.GetChanges();
					var mainFactoryHasChanges = mainFactoryChangeSet.GetChangedObjects().Any() || mainFactoryChangeSet.GetAddedObjects().Any();
					AssertEquals("After pressing OK in the pop up the change will not be saved. Does Main Factory have changes?", true, mainFactoryHasChanges);
				});
			}
		}

		ReportsTabUserControl GetReportsTabUserControl(ExitControlForm form)
		{
			var exitControlUserControl = form.FindSingle<ZUserControl>("ExitControlUserControl");
			var exitControlTabControl = exitControlUserControl.FindSingle<ZTabControl>("ExitControlTabControl");
			exitControlTabControl.SelectTab("ReportsTabPage");

			return (ReportsTabUserControl)exitControlUserControl.FindSingle<ZUserControl>("ReportsTabUserControl");
		}
	}
}
