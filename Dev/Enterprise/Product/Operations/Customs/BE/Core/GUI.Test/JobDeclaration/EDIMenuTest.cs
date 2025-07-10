using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class EDIMenuTest : TestCaseForAttachGUI
{
	public void TestPreSave()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		var invoiceHeader = dec.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		dec.JE_AgentsReference = "agent";
		using (var form = new JobDeclarationFormTestClass(dec))
		{
			var menu = form.EDIMenu;
			menu.Declaration = dec;

			menu.RefreshMenu();
			menu.MenuItems.FindByText("Send to Customs").PerformClick();
			AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestAutoSaveAfterMerge()
	{
		var dec = Factory.New<JobDeclaration>();
		var entryInstruction = dec.CustomsEntryInstructions.AddNew();
		var invoiceHeader = dec.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;
		dec.JE_ApplicationCode = "BLT";
		Factory.Save();
		using (var form = new JobDeclarationFormTestClass(dec))
		{
			var menu = form.EDIMenu;
			menu.Declaration = dec;

			menu.RefreshMenu();
			menu.MenuItems.FindByText("Send to Customs").PerformClick();
			AssertEquals(false, dec.HasChanges);
		}
	}

	public void TestGenerateEntriesExecutedBeforeSend()
	{
		var dec = Factory.New<JobDeclaration>();
		using (var form = new JobDeclarationFormTestClass(dec))
		{
			var menu = form.EDIMenu;
			menu.Declaration = dec;
			dec.JE_ApplicationCode = "BLT";

			menu.RefreshMenu();
			menu.MenuItems.FindByText("Send to Customs").PerformClick();
			AssertEquals(true, ((EdiMenuForTest)menu).PerformMergeCalled);
		}
	}

	public void TestDisplayGenerateEntriesMenuOption()
	{
		CombineAssertions(() =>
		{
			var nullBaseMenu = new EDIMenu();
			AssertNoExceptionThrown(() => nullBaseMenu.RefreshMenu());
			AssertEquals("[No Associated Declaration] Generate Entries menu visible?", false, nullBaseMenu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);

			var dec = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationFormTestClass(dec))
			{
				var menu = form.EDIMenu;
				menu.Declaration = dec;

				menu.Declaration.JE_ApplicationCode = string.Empty;
				AssertNoExceptionThrown(() => menu.RefreshMenu());
				AssertEquals("[Default App Code Declaration] Generate Entries menu visible?", false, menu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);

				menu.Declaration.JE_ApplicationCode = "ITF";
				AssertNoExceptionThrown(() => menu.RefreshMenu());
				AssertEquals("[ITF Declaration] Generate Entries menu visible?", false, menu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);

				menu.Declaration.JE_ApplicationCode = "BLT";
				AssertNoExceptionThrown(() => menu.RefreshMenu());
				AssertEquals("[BLT Declaration] Generate Entries menu visible?", true, menu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);
			}
		});
	}

	public void TestDisplaySendMessageMenuOption()
	{
		CombineAssertions(() =>
		{
			var nullBaseMenu = new EDIMenu();
			AssertNoExceptionThrown(() => nullBaseMenu.RefreshMenu());
			AssertEquals("[No Associated Declaration] Send to Customs menu visible?", false, nullBaseMenu.MenuItems.FindByText("Send to Customs").Visible);

			var dec = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationFormTestClass(dec))
			{
				var menu = form.EDIMenu;
				menu.Declaration = dec;

				menu.Declaration.JE_ApplicationCode = string.Empty;
				AssertNoExceptionThrown(() => menu.RefreshMenu());
				AssertEquals("[Default App Code Declaration] Send to Customs menu visible?", false, menu.MenuItems.FindByText("Send to Customs").Visible);

				menu.Declaration.JE_ApplicationCode = "ITF";
				AssertNoExceptionThrown(() => menu.RefreshMenu());
				AssertEquals("[ITF Declaration] Send to Customs menu visible?", false, menu.MenuItems.FindByText("Send to Customs").Visible);

				menu.Declaration.JE_ApplicationCode = "BLT";
				AssertNoExceptionThrown(() => menu.RefreshMenu());
				AssertEquals("[BLT Declaration] Send to Customs menu visible?", true, menu.MenuItems.FindByText("Send to Customs").Visible);
			}
		});
	}

	public void TestSendAmendment()
	{
		TestSendMessage(BEExportEntryTypeList.Codes.ExportAmendment);
	}

	public void TestSendExportDeclaration()
	{
		TestSendMessage(BEExportEntryTypeList.Codes.ExportDeclaration);
	}

	public void TestSendPresentationNotification()
	{
		TestSendMessage(BEExportEntryTypeList.Codes.PresentationNotification);
	}

	public void TestSendInvalidation()
	{
		TestSendMessage(BEExportEntryTypeList.Codes.CancellationRequest);
	}

	public void TestSendMessageEmptyDeclarationNoException()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		using (var form = new JobDeclarationFormTestClass(declaration))
		{
			var menu = form.EDIMenu;
			menu.Declaration = declaration;

			form.Show();
			var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (ExportMessageSendingForm)obj;
				var action = (ExportEntryMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				action.ShouldSend = true;
			});

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertNoExceptionThrown(() => sendToCustomsMenu.PerformClick());
		}
	}

	void TestSendMessage(ZString entryType)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		var header = declaration.CustomsEntryHeaders.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Style = Constants.EntryInstructionStyle._A;
		header.CH_CEI_Instruction = entryInstruction.PK;
		using (var form = new JobDeclarationFormTestClass(declaration))
		{
			var menu = form.EDIMenu;
			menu.Declaration = declaration;

			form.Show();
			var sendToCustomsMenu = menu.MenuItems.Cast<MenuItem>().LastOrDefault(x => x.Text == "Send to Customs");

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
			{
				var dialog = (ExportMessageSendingForm)obj;
				var action = (ExportEntryMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
				action.TypeOfEntry = entryType;
				action.ShouldSend = true;
			});

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendToCustomsMenu.PerformClick();
			AssertEquals("The message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(1, header.Messages.Count);
		}
	}

	class JobDeclarationFormTestClass : BaseJobDeclarationFormTestClass
	{
		public JobDeclarationFormTestClass(JobDeclaration dec) : base(dec) { }

		protected override Customs.GUI.IEDIMenu GetNewTopLevelMenuCore() => new EdiMenuForTest();
	}

	class EdiMenuForTest : EDIMenu
	{
		public bool PerformMergeCalled;
		protected override bool PerformMerge()
		{
			base.PerformMerge();
			PerformMergeCalled = true;
			return PerformMergeCalled;
		}
	}
}
