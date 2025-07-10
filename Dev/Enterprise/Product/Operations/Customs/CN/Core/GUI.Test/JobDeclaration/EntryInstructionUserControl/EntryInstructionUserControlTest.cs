using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(EntryInstructionUserControl))]
	class EntryInstructionUserControlTest : TestCaseWithFactory
	{
		public void TestControlsExistence()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var entryInstructionUserControl = form.FindEntryInstructionUserControl();
			CombineAssertions(() =>
			{
				TestUtility.AssertControlExistance(entryInstructionUserControl, "EntryInstructionsGrid", "CustomsEntryInstructions");
				AssertEquals(typeof(LayoutEntryInstructionDetailUserControl), entryInstructionUserControl.FindSingle<ZDynamicControlCreationUserControl>("DetailsUserControl").UserControlType);
			});
		}

		public void TestSplitMenuItems()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var childInstruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "0110";
			var invoiceHeader = declaration.Invoices.AddNew();
			for (var line = 0; line < 51; line++)
			{
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				invoiceLine.JI_Tariff = "10010" + line.ToString().PadLeft(2, '0');
			}

			declaration.DoMerge();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var entryInstructionsGrid = form.FindEntryInstructionUserControl().FindSingle<ZGrid>("EntryInstructionsGrid");
			entryInstructionsGrid.Select(0);
			var splitMenu = entryInstructionsGrid.ContextMenu.MenuItems.FindByText("Auto Split");

			entryInstructionsGrid.ContextMenu.OnPopup_ForTest();
			AssertEquals("Main Instruction selected, Auto Split Menu Item should be Enabled", true, splitMenu.Enabled);
			AssertSplitSubMenuItem("Main Instruction selected", splitMenu, "Up to 20 Entry Lines", true, "This Entry Instruction is going to be split to 3 Entry Instructions, do you want to save the job and continue?");
			AssertSplitSubMenuItem("Main Instruction selected", splitMenu, "Up to 50 Entry Lines", true, "This Entry Instruction is going to be split to 2 Entry Instructions, do you want to save the job and continue?");
			AssertSplitSubMenuItem("Main Instruction selected", splitMenu, "Based on Legal Inspection required or not", true, "All entry lines do not require Legal Inspection.");

			entryInstructionsGrid.Select(1);
			entryInstructionsGrid.ContextMenu.OnPopup_ForTest();
			AssertEquals("Child Instruction selected, Auto Split Menu Item should be disabled", false, splitMenu.Enabled);
			AssertSplitSubMenuItem("Child Instruction selected", splitMenu, "Up to 20 Entry Lines", false, "");
			AssertSplitSubMenuItem("Child Instruction selected", splitMenu, "Up to 50 Entry Lines", false, "");
			AssertSplitSubMenuItem("Child Instruction selected", splitMenu, "Based on Legal Inspection required or not", false, "");

			entryInstructionsGrid.SelectAllElements();
			entryInstructionsGrid.ContextMenu.OnPopup_ForTest();
			AssertEquals("Multiple Instructions selected, Auto Split Menu Item should be disabled", false, splitMenu.Enabled);
			AssertSplitSubMenuItem("Multiple Instructions selected", splitMenu, "Up to 20 Entry Lines", false, "");
			AssertSplitSubMenuItem("Multiple Instructions selected", splitMenu, "Up to 50 Entry Lines", false, "");
			AssertSplitSubMenuItem("Multiple Instructions selected", splitMenu, "Based on Legal Inspection required or not", false, "");
		}

		static void AssertSplitSubMenuItem(string reason, MenuItem splitMenu, string menuText, bool shouldBeEnabled, string expectedMsg)
		{
			var splitSubMenu = splitMenu.MenuItems.FindByText(menuText);
			var message = reason + ", Menu Item \"" + menuText + "\" should be " + (shouldBeEnabled ? "enabled." : "disabled.");
			AssertEquals(message, shouldBeEnabled, splitMenu.Enabled);

			if (shouldBeEnabled)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				splitSubMenu.PerformClick();
				AssertEquals(menuText, expectedMsg, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCusEntryInstructionGridColumnsVisibility()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = DecTypeList.Codes.CustomsEntry;
			declaration.CustomsEntryInstructions.AddNew();
			using var form = new JobDeclarationForm(declaration);
			form.Show();
			var entryInstructionUserControl = form.FindEntryInstructionUserControl();
			var entryInstructionGrid = entryInstructionUserControl.FindSingle<ZGrid>("EntryInstructionsGrid");
			AssertNotNull(entryInstructionGrid.GetColumnStyle("EntryTypeDescription"));
			AssertNotNull(entryInstructionGrid.GetColumnStyle("CEI_Style"));
			AssertNotNull(entryInstructionGrid.GetColumnStyle("CEI_StyleDescription"));
			AssertNotNull(entryInstructionGrid.GetColumnStyle("CEI_Description"));
			AssertNotNull(entryInstructionGrid.GetColumnStyle("LinkedFormalEntryHeader+ReferenceNumber"));
			AssertNotNull(entryInstructionGrid.GetColumnStyle("LinkedFormalEntryHeader+DeclarationUnifiedNumber"));
			AssertNotNull(entryInstructionGrid.GetColumnStyle("LinkedFormalEntryHeader+EntryNumber"));
			var parentColumn = entryInstructionGrid.GetColumnStyle("CEI_CEI_Parent");
			var parentDescColumn = entryInstructionGrid.GetColumnStyle("ParentInstruction+CEI_Description");
			Assert("Parent Column", parentColumn.IsUnavailable);
			Assert("Parent Description Column", parentDescColumn.IsUnavailable);
			declaration.JE_MessageSubType = DecTypeList.Codes.Both;
			Assert("Parent Column", !parentColumn.IsUnavailable);
			Assert("Parent Description Column", !parentDescColumn.IsUnavailable);
			declaration.JE_MessageSubType = DecTypeList.Codes.RecordListing;
			Assert("Parent Column", parentColumn.IsUnavailable);
			Assert("Parent Description Column", parentDescColumn.IsUnavailable);
		}
	}
}
