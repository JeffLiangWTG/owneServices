using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseWithFactory
	{
		public void TestDisplayGenerateEntriesMenuOption()
		{
			var ediMenu = new EDIMenuTestHelper();
			var displayGenerateEntriesMenuOption = ediMenu.GetDisplayGenerateEntriesMenuOption();
			Assert("DisplayGenerateEntriesMenuOption flag is true", displayGenerateEntriesMenuOption);
		}

		public void TestGenerateEntriesMenuVisible()
		{
			var ediMenu = new EDIMenu();
			var dec = Factory.New<JobDeclaration>();

			ediMenu.OnPopup(EventArgs.Empty);

			var generateEntriesMenu = ediMenu.MenuItems.FindByText("Generate Entries (Merge)");

			AssertEquals("Generate Entries menu is invisible when declaration is null", false, generateEntriesMenu.Visible);

			ediMenu.Declaration = dec;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			ediMenu.RefreshMenu();

			AssertEquals("Generate Entries menu is invisible when declaration is ITF", false, generateEntriesMenu.Visible);

			dec.JE_ApplicationCode = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			ediMenu.RefreshMenu();

			AssertEquals("Generate Entries menu is visible when declaration is BLT and Import", true, generateEntriesMenu.Visible);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			ediMenu.RefreshMenu();

			AssertEquals("Generate Entries menu is visible when declaration is BLT and Export", true, generateEntriesMenu.Visible);

			ediMenu.Dispose();
		}

		public void TestSendMessageMenuItemVisible()
		{
			var ediMenu = new EDIMenu();
			var dec = Factory.New<JobDeclaration>();

			ediMenu.OnPopup(EventArgs.Empty);

			var sendMessageMenuItem = ediMenu.MenuItems.FindByText("Send to Customs");

			AssertEquals("Send to Customs menu is invisible when declaration is null", false, sendMessageMenuItem.Visible);

			ediMenu.Declaration = dec;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			ediMenu.RefreshMenu();

			AssertEquals("Send to Customs menu is invisible when declaration is ITF", false, sendMessageMenuItem.Visible);

			dec.JE_ApplicationCode = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			ediMenu.RefreshMenu();

			AssertEquals("Send to Customs menu is visible when declaration is BLT and Import", true, sendMessageMenuItem.Visible);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			ediMenu.RefreshMenu();

			AssertEquals("Send to Customs menu is visible when declaration is BLT and Export", true, sendMessageMenuItem.Visible);

			ediMenu.Dispose();
		}

		public void TestSendMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_InvoiceAmount = 100;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			Factory.Save();

			var messageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			declaration.MessageInitiator = messageInitiator;
			declaration.DoMerge();

			using (var ediMenu = new EDIMenu { Declaration = declaration })
			{
				var sendMessageMenu = GetSendMessageMenu(ediMenu);

				CombineAssertions(() =>
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					sendMessageMenu.PerformClick();
					AssertType<MessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);
				});
			}
		}

		MenuItem GetSendMessageMenu(EDIMenu ediMenu)
		{
			ediMenu.OnPopup(EventArgs.Empty);

			var sendMessageMenu = ediMenu.MenuItems.FindByText("Send to Customs");
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			return sendMessageMenu;
		}

		class EDIMenuTestHelper : EDIMenu
		{
			public bool GetDisplayGenerateEntriesMenuOption() => DisplayGenerateEntriesMenuOption;
		}
	}
}
