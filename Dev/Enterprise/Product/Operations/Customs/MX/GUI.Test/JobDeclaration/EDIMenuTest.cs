using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class EDIMenuTest : TestCaseWithFactory
	{
		public void TestDisplayGenerateEntriesMenuOption()
		{
			using (var ediMenu = new EDIMenu())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				ediMenu.Declaration = declaration;

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				ediMenu.RefreshMenu();
				AssertEquals(false, ediMenu.GenerateEntriesMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				ediMenu.RefreshMenu();
				AssertEquals(true, ediMenu.GenerateEntriesMenuItem.Visible);
			}
		}

		public void TestSendToCustomsMenuOption()
		{
			using (var ediMenu = new EDIMenu())
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				ediMenu.Declaration = declaration;

				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
				ediMenu.RefreshMenu();
				AssertEquals(false, ediMenu.SendToCustomsMenuItem.Visible);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				ediMenu.RefreshMenu();
				AssertEquals(true, ediMenu.SendToCustomsMenuItem.Visible);

				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				ediMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				declaration.Factory.Save();
				ediMenu.SendToCustomsMenuItem.PerformClick();
				AssertEquals("Declaration B00001000 has no entry.", UnitTestUserNotification.Instance.LastMessage.Text);
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = "11";
				var testInvHeader = declaration.Invoices.AddNew();
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + "00";
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				declaration.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ediMenu.SendToCustomsMenuItem.PerformClick();
				AssertType<JobDeclarationMessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
