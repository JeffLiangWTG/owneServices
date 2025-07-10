using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicenseEntryInstructionDetailsUserControlTest : Customs.GUI.Testing.EntryInstructionDetailsUserControlTest
	{
		public void TestSplitMenuItems()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "DESCRIPTION";

			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx < BR.Business.CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "111111111";
			}

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportLicenseEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).FirstOrDefault();

				entryInstructionGrid.Select(0);
				var splitMenu = entryInstructionGrid.ContextMenu.MenuItems.FindByText("Auto Split");

				entryInstructionGrid.ContextMenu.OnPopup_ForTest();
				AssertEquals("Main Instruction selected, Auto Split Menu Item should be Enabled", true, splitMenu.Enabled);

				splitMenu.PerformClick();
				var lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Please save the job first.", lastMessage.Text);
				Factory.Save();

				splitMenu.PerformClick();

				lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Please generate entries first by clicking Brokerage > Generate Entries (Merge).", lastMessage.Text);

				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.CH_CEI_Instruction = instruction.PK;
				var entryLine = entry.MergedLines.AddNew();
				declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.JI_CL = entryLine.PK);

				Factory.Save();

				splitMenu.PerformClick();
				lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals(ImportLicenseEntryInstructionSplitter.NoNeedToSplitMessage, lastMessage.Text);

				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "111111111";
				invoiceLine.JI_CL = entryLine.PK;

				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				splitMenu.PerformClick();
				lastMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertEquals("Continue to Split?", lastMessage.Caption);
				AssertContains("This Entry Instruction is going to be split to", lastMessage.Text);
				AssertEquals(1, declaration.CustomsEntryInstructions.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				splitMenu.PerformClick();
				AssertEquals("Split succeeded!", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(2, declaration.CustomsEntryInstructions.Count);

				entryInstructionGrid.SelectAllElements();
				entryInstructionGrid.ContextMenu.OnPopup_ForTest();
				AssertEquals("Multiple Instructions selected, Auto Split Menu Item should be disabled", false, splitMenu.Enabled);
			}
		}

		public void TestChangeControlsVisibilityImportLicense()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = Factory.New<Business.CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "OUT DESC";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportLicenseEntryInstructionDetailsUserControl>().First();
				var brAdditionalInformationTextBox = instrUserControl.Controls.Find("BRAdditionalInformationTextBox", true)[0];
				AssertEquals(true, brAdditionalInformationTextBox.Visible);
			}
		}

		public void TestGridVisibleColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = Factory.New<Business.CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "OUT DESC";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction.PK;
			invoiceLine1.JI_BondedWhsQuantity = 50m;
			invoiceLine1.JI_CustomsUnitQty = "KG";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportLicenseEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).FirstOrDefault();
				Assert(entryInstructionGrid != null);
				Assert(entryInstructionGrid.Columns.Any(x => x.IsVisible));
				Assert("Should have CEI_Description column in EntryInstructionsGrid", entryInstructionGrid.Columns.Contains(BR.Business.CusEntryInstruction.Schema.CEI_Description));
			}
		}

		public void TestEntryInstructionsGridWithColumnsOrder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryInstruction = Factory.New<Business.CusEntryInstruction>();
			entryInstruction.CEI_JE = declaration.PK;
			entryInstruction.CEI_Description = "OUT DESC";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportLicenseEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).First();

				entryInstructionGrid.ResetColumns();

				Assert("EntryInstructionGrid grid count must have at least " + ExpectedColumnNamesListOnThisOrder.Count.ToString(),
					ExpectedColumnNamesListOnThisOrder.Count <= entryInstructionGrid.Columns.Count);

				for (int i = 0; i < ExpectedColumnNamesListOnThisOrder.Count; i++)
				{
					var column = entryInstructionGrid.Columns[i];
					AssertNotNull(column);
					var expectedColumnName = ExpectedColumnNamesListOnThisOrder[i];
					AssertEquals("Expected", expectedColumnName, column.ColumnStyle.MappingName);
					AssertEquals("IsVisible", true, column.IsVisible);
				}
			}
		}

		public void TestMessageMaximumCharactersExceed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Classification;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Description = "12345678901234567890123456789012345678901234567890";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var entryLine = entry.MergedLines.AddNew();

			var invoice = declaration.Invoices.AddNew();
			for (var idx = 0; idx <= BR.Business.CusEntryInstruction.MaximumInvoiceLinesAllowedForImportLicense; idx++)
			{
				var invoiceLine = invoice.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "111111111";
				invoiceLine.JI_CL = entryLine.PK;
			}

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var customsBrokerageUserControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				customsBrokerageUserControl.EntryInstructionDetailsTabPage.TabVisible = true;
				customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.EntryInstructionDetailsTabPage;
				var instrUserControl = customsBrokerageUserControl.EntryInstructionDetailsTabPage.Controls.OfType<ImportLicenseEntryInstructionDetailsUserControl>().First();
				var entryInstructionGrid = (ZGrid)instrUserControl.Controls.Find("EntryInstructionsGrid", true).FirstOrDefault();

				entryInstructionGrid.Select(0);
				var splitMenu = entryInstructionGrid.ContextMenu.MenuItems.FindByText("Auto Split");

				entryInstructionGrid.ContextMenu.OnPopup_ForTest();
				Assert("Main Instruction selected, Auto Split Menu Item should be Enabled", splitMenu.Enabled);

				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				splitMenu.PerformClick();
				var lastMessageList = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Split canceled!", lastMessageList[0].Text);
				AssertEquals("Continue to Split?", lastMessageList[1].Caption);
				AssertEquals(ImportLicenseEntryInstructionDetailsUserControl.MaximumCharactersNumberExceededMessage, lastMessageList[1].Text);
				AssertEquals(1, declaration.CustomsEntryInstructions.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				splitMenu.PerformClick();
				lastMessageList = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Split succeeded!", lastMessageList[0].Text);
				AssertEquals("Continue to Split?", lastMessageList[1].Caption);
				AssertContains("This Entry Instruction is going to be split to", lastMessageList[1].Text);
				AssertEquals("Continue to Split?", lastMessageList[2].Caption);
				AssertEquals(ImportLicenseEntryInstructionDetailsUserControl.MaximumCharactersNumberExceededMessage, lastMessageList[2].Text);
				AssertEquals(2, declaration.CustomsEntryInstructions.Count);
			}
		}

		List<ZString> ExpectedColumnNamesListOnThisOrder
		{
			get
			{
				if (expectedColumnNamesListOnThisOrder == null)
				{
					expectedColumnNamesListOnThisOrder = new List<ZString>();
					expectedColumnNamesListOnThisOrder.Add(BR.Business.CusEntryInstruction.Schema.CEI_Description);
				}
				return expectedColumnNamesListOnThisOrder;
			}
		}
		List<ZString> expectedColumnNamesListOnThisOrder;
	}
}
