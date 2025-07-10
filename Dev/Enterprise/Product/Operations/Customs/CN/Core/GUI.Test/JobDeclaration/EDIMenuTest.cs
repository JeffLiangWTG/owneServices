using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using CusEntryInstruction = Enterprise.Customs.Business.CusEntryInstruction;

namespace Enterprise.Customs.CN.GUI.Testing
{
	sealed class EDIMenuTest : TestCaseForAttachGUI
	{
		public void TestDisplayGenerateEntriesMenuOption()
		{
			CombineAssertions(() =>
				{
					var nullBaseMenu = new EDIMenu();
					AssertNoExceptionThrown(() => nullBaseMenu.RefreshMenu());
					AssertEquals("[No Associated Declaration] Generate Entries menu visible?", false, nullBaseMenu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);
					var dec = Factory.New<JobDeclaration>();
					dec.JE_TransportMode = "AIR";
					dec.JE_MessageType = JobMessageTypeList.Codes.Import;
					dec.JE_ApplicationCode = string.Empty;
					using (var form = new JobDeclarationFormTestClass(dec))
					{
						var menu = form.EDIMenu;
						menu.Declaration = dec;
						AssertNoExceptionThrown(() => menu.RefreshMenu());
						AssertEquals("[Default App Code Declaration] Generate Entries menu visible?", false, menu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);
						dec.JE_ApplicationCode = "ITF";
						menu = form.EDIMenu;
						menu.Declaration = dec;
						AssertNoExceptionThrown(() => menu.RefreshMenu());
						AssertEquals("[ITF Declaration] Generate Entries menu visible?", false, menu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);
						dec.JE_ApplicationCode = "BLT";
						menu = form.EDIMenu;
						menu.Declaration = dec;
						AssertNoExceptionThrown(() => menu.RefreshMenu());
						AssertEquals("[BLT Declaration] Generate Entries menu visible?", true, menu.MenuItems.FindByText("Generate Entries (&Merge)").Visible);
					}
				}

			);
		}

		public void TestSendUniversalCustomsMessagingMenuItem()
		{
			var declaration = JobDeclaration(out CNSWClientSetting clientSetting, out CusEntryInstruction instruction);
			using (CNCustomsDataRegistry.Instance.CNSWClientSetting.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, clientSetting))
			using (var form = new JobDeclarationFormTestClass(declaration))
			{
				var menu = form.EDIMenu;
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var sendUniversalXmlCustomsMessageMenuItem = menu.MenuItems.FindByText("Send to Customs");
				Assert(sendUniversalXmlCustomsMessageMenuItem.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendUniversalXmlCustomsMessageMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Declaration Has Changes", true, declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendUniversalXmlCustomsMessageMenuItem.PerformClick();
				AssertEquals("Last dialog form type = MessageSendingFormWithValidationDetails", true, ZFormModaliser.LastFormShownDialogForTest is MessageSendingFormWithValidationDetails);
			}

			using (var form = new JobDeclarationFormTestClass(declaration))
			{
				var menu = form.EDIMenu;
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var sendUniversalXmlCustomsMessageMenuItem = menu.MenuItems.FindByText("Send to Customs");
				sendUniversalXmlCustomsMessageMenuItem.PerformClick();
				AssertContains("Error when no setting", CNSWClientSettingChecker.EHubClientNotRegisteredMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldSendUniversalMessage()
		{
			ShouldSendUniversalMessageWithArgs(true, EDIMessageTypeList.Codes.DEC);
			ShouldSendUniversalMessageWithArgs(false, "XUS");
		}

		void ShouldSendUniversalMessageWithArgs(bool shouldSendUniversalMessage, String exceptedType)
		{
			var declaration = JobDeclaration(out CNSWClientSetting clientSetting, out CusEntryInstruction instruction);
			var cusHeader = declaration.CustomsEntryHeaders.AddNew();
			cusHeader.CH_CEI_Instruction = instruction.PK;
			cusHeader.EntryNumber = "NO1";
			cusHeader.CH_MessageType = "CUS";
			cusHeader.CH_BGMReference = "REF";

			using (CNCustomsDataRegistry.Instance.CNSWClientSetting.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, clientSetting))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CNBuildMessageInCW1, Core.Constants.CountryCodes.China, ZDateTime.Today, shouldSendUniversalMessage))
			using (var form = new JobDeclarationFormTestClass(declaration))
			{
				var menu = form.EDIMenu;
				menu.Declaration = declaration;
				menu.RefreshMenu();
				var sendUniversalXmlCustomsMessageMenuItem = menu.MenuItems.FindByText("Send to Customs");
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingFormWithValidationDetails)obj;
					var action = (CNJobDeclarationMessageSendingObject)dialog.MessageSendingObjectParent.SendingObjectsCollection[0];
					action.ShouldSend = true;
				});

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				sendUniversalXmlCustomsMessageMenuItem.PerformClick();

				AssertEquals("Message is created", exceptedType, cusHeader.Messages[0].EM_MessageType);
			}
		}

		JobDeclaration JobDeclaration(out CNSWClientSetting clientSetting, out CusEntryInstruction instruction)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_JE = declaration.PK;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_CEI = instruction.PK;
			clientSetting = CNSWClientSettingCheckerTest.CreateCNSWClientSetting(Factory, declaration.CompanyPK.ToGuid(), Guid.Empty);
			return declaration;
		}

		class JobDeclarationFormTestClass : BaseJobDeclarationFormTestClass
		{
			public JobDeclarationFormTestClass(JobDeclaration dec) : base(dec)
			{
			}

			protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();
		}
	}
}
