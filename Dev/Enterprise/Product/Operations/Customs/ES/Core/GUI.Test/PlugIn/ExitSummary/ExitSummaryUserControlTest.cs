using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class ExitSummaryUserControlTest : ExitSummaryUserControlForVirtualPropertiesTest<ExitSummaryUserControl>
	{
		protected override ZBool DefaultDynamicLayoutApplied => true;

		public void TestMovementsGrid_ExtraColumns()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");
				foreach (var columnName in MovementColumnDetails.Select(x => x.ColumnName))
				{
					AssertNotNull(columnName, movementsGrid.GetColumnStyle(columnName));
				}
			}
		}

		public void TestMovementsGrid_ExtraColumns_Width()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");
				foreach (var (columnName, _, columnWidth) in MovementColumnDetails)
				{
					AssertEquals(columnName, columnWidth, movementsGrid.GetColumnStyle(columnName).Width);
				}
			}
		}

		public void TestMovementsGrid_ExtraColumns_Caption()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");
				foreach (var (columnName, columnCaption, _) in MovementColumnDetails)
				{
					AssertEquals(columnName, columnCaption, movementsGrid.GetColumnStyle(columnName).CaptionResourceString.Caption);
				}
			}
		}

		public void TestMovementsGrid_CED_ArrivalNotificationPlace_DropEdit()
		{
			using (var control = new ExitSummaryUserControl())
			{
				var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");
				AssertType<ZDropEditColumnStyleInfo>(movementsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == CusExitDetail.Schema.CED_ArrivalNotificationPlace));
			}
		}

		public void TestArrivalNotificationPlaceTextBox()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertExceptionThrown<InvalidOperationException>("ArrivalNotificationPlaceTextBox should not exist", () => control.FindSingle<ZTextBox>("ArrivalNotificationPlaceTextBox"));
			}
		}

		public void TestArrivalNotificationPlaceDropEdit()
		{
			using (var control = new ExitSummaryUserControl())
			{
				AssertNotNull(control.FindSingle<ZDropEdit>("ArrivalNotificationPlaceDropEdit"));
			}
		}

		public void TestMessagesTabUserControlType()
		{
			using (var exitSummaryUserControl = new ExitSummaryUserControl())
			{
				var messagesUserControl = exitSummaryUserControl.FindSingle<ZDynamicControlCreationUserControl>("MessagesUserControl");
				AssertEquals("MessagesTab is correct type", typeof(MessagesTabUserControl), messagesUserControl.UserControlType);
			}
		}

		public void TestUpdateCSVClearanceMenuItem()
		{
			var exitHeader = Factory.New<CusExitControlHeader>();
			var exitDetail = exitHeader.CusExitDetails.AddNew();

			using (var form = new ZForm(exitHeader))
			using (var control = new ExitSummaryUserControl())
			{
				CombineAssertions(() =>
				{
					form.Controls.Add(control);
					form.Show();

					var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");
					AssertNotNull("Context menu to update CSV Clearance exists", movementsGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance"));
					AssertEquals("Menu always visible", true, movementsGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance").Visible);
				});
			}
		}

		public void TestUpdateCSVClearanceMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = "ExitHeaderRef";
			var exitDetail1 = exitHeader.CusExitDetails.AddNew();
			exitDetail1.CED_MovementReferenceNumber = "MRNCode1";
			var exitDetail2 = exitHeader.CusExitDetails.AddNew();
			exitDetail2.CED_Status = "CDA";
			exitDetail2.CED_MovementReferenceNumber = "MRNCode2";

			using (var form = new ZForm(exitHeader))
			using (var control = new ExitSummaryUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;

				var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");
				var updateCSVClearanceMenuItem = movementsGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				CombineAssertions(() =>
				{
					movementsGrid.Select();
					movementsGrid.Focus();

					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("Needs to select a row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
					movementsGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("Needs to select only one row", "Please select a single row first", UnitTestUserNotification.Instance.LastMessage.Text);

					movementsGrid.SelectSingleElementByPK(exitDetail1.PK);
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("Can only update csv on movements with status CDA", "Please select only a movement with status CDA", UnitTestUserNotification.Instance.LastMessage.Text);

					movementsGrid.SelectSingleElementByPK(exitDetail2.PK);
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("Should have message asking to save the declaration before updating", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The Job has not yet been saved. Do you want to save and proceed?"));

					Factory.Save();
					movementsGrid.SelectSingleElementByPK(exitDetail2.PK);
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("Should have pop up asking to intoduce csv code", true, UnitTestUserNotification.Instance.LastMessage.Text.Contains(@"You are about to change the clearance number (CSV Clearance) of movement MRNCode2.
Please make sure that the number that you are entering is the right clearance number.
New Clearance Number:"));
				});
			}
		}

		public void TestUpdateCSVClearanceActions()
		{
			var declaration = Factory.New<JobDeclaration>();
			var exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_Parent = declaration;
			exitHeader.CEH_ReferenceNumber = "ExitHeaderRef";
			var exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_Status = "CDA";
			exitDetail.CED_MovementReferenceNumber = "MRNCode";

			using (var form = new ZForm(exitHeader))
			using (var control = new ExitSummaryUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				ZFormModaliser.ShowDialogsInTest = false;

				var movementsGrid = control.FindSingle<ZGrid>("MovementsGrid");
				var updateCSVClearanceMenuItem = movementsGrid.ContextMenu.MenuItems.FindByText("Update CSV Clearance");

				Factory.Save();
				CombineAssertions(() =>
				{
					movementsGrid.Select();
					movementsGrid.Focus();

					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("Nothing has been done when the pop up was cancelled", ZString.Empty, exitDetail.ZG_CSVClearance);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddUserResponse("+--**__aa/#€&@");
					movementsGrid.SelectAllElements();
					updateCSVClearanceMenuItem.PerformClick();
					AssertEquals("CSV clearance has not been changed when the pop up was accepted but code is not valid", ZString.Empty, exitDetail.ZG_CSVClearance);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddUserResponse("CLEARANCE1234567");
					movementsGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before updating CSV", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After updating CSV", declaration.Factory);
					AssertEquals("CSV clearance has been changed when the pop up was accepted", "CLEARANCE1234567", exitDetail.ZG_CSVClearance);
					AssertEquals("New event in logs", "|NEW=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Exit Movement MRNCode|TYP=CSV", exitDetail.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					UnitTestUserNotification.Instance.AddUserResponse("AAAAAAAAAAAAAAAA");
					movementsGrid.SelectAllElements();
					TestHelper.CheckFactoryHasNoPendingChanges("Before updating CSV", declaration.Factory);
					updateCSVClearanceMenuItem.PerformClick();
					TestHelper.CheckFactoryHasNoPendingChanges("After updating CSV", declaration.Factory);
					AssertEquals("CSV clearance has been changed when the pop up was accepted a second time", "AAAAAAAAAAAAAAAA", exitDetail.ZG_CSVClearance);
					AssertEquals("New event in logs for second change", "|NEW=AAAAAAAAAAAAAAAA|OLD=CLEARANCE1234567|RES=Manually Added CSV Clearance Code to Exit Movement MRNCode|TYP=CSV", exitDetail.Logs.MostRecentLogByEventTime(Events.ChangeOfIdentifier).SL_Reference);
				});
			}
		}

		static (string ColumnName, string ColumnCaption, int ColumnWidth)[] MovementColumnDetails => new[]
		{
			(CusExitDetail.Schema.ZG_AcceptanceDate, "Acceptance Date", 100),
			(CusExitDetail.Schema.FormattedCircuit, "Circuit", 60),
			(CusExitDetail.Schema.ZG_CSVClearance, "CSV Clearance", 80),
			(CusExitDetail.Schema.CED_ArrivalNotificationPlace, "Arrival Notification Place", 140)
		};
	}
}
