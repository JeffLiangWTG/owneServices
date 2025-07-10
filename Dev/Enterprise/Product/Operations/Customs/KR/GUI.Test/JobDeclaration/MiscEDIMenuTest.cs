using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using IJobDeclarationMessageSendingObjectParent = Enterprise.Customs.KR.Business.IJobDeclarationMessageSendingObjectParent;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class MiscEDIMenuTest : TestCaseWithFactory
	{
		const string menuName008 = "Send 008 - Personal Items Declaration";
		const string menuNameD87 = "Send D87 - Carnet Temporary Import Certificate";
		const string menuName5SM = "Send 5SM - Valuation Declaration Template";

		public void TestSendMenuVisibilityForImportMessages()
		{
			declaration.InvoiceLines.AddNew();
			using (var form = new MiscDeclarationForm(declaration))
			{
				form.Show();

				var ediMenu = (MiscEDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var send008menu = ediMenu.MenuItems.FindByText(menuName008, true);
				var sendD87menu = ediMenu.MenuItems.FindByText(menuNameD87, true);

				CombineAssertions(() =>
				{
					AssertNotNull("Send 008 menu should exist.", send008menu);
					AssertNotNull("Send D87 menu should exist.", sendD87menu);
				});

				declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertEquals("If the JobDeclaration Type is 008, the visibility of the Send 008 menu is true.", true, send008menu.Visible);
					AssertEquals("If the JobDeclaration Type is 008, the visibility of the Send D87 menu is false.", false, sendD87menu.Visible);
				});

				declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._D87;
				ediMenu.RefreshMenu();
				CombineAssertions(() =>
				{
					AssertNotNull("Send 008 menu should exist.", send008menu);
					AssertEquals("If the JobDeclaration Type is D87, the visibility of the Send 008 menu is false.", false, send008menu.Visible);
					AssertEquals("If the JobDeclaration Type is D87, the visibility of the Send D87 menu is true.", true, sendD87menu.Visible);
				});
			}

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			using (var form = new MiscDeclarationForm(declaration))
			{
				form.Show();

				var ediMenu = (MiscEDIMenu)form.Menu.MenuItems.FindByText("&Brokerage");
				var send5SMmenu = ediMenu.MenuItems.FindByText(menuName5SM, true);

				CombineAssertions(() =>
				{
					AssertNotNull("Send 5SM menu should exist.", send5SMmenu);
					AssertEquals("The visibility of Send 5SM menu is always true and not changed", true, send5SMmenu.Visible);
				});
			}
		}

		public void TestSendPromptSaveJobBeforeSendingMessage()
		{
			var ediMenu = new MiscEDIMenu();
			ediMenu.Declaration = declaration;
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			declaration.InvoiceLines.AddNew();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Assert("Pre-condition", declaration.HasChanges);
				var send008menu = ediMenu.MenuItems.FindByText(menuName008, true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				send008menu.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			declaration.Invoices[1].InvoiceLines.AddNew();
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Assert("Pre-condition", declaration.HasChanges);
				var send5SMmenu = ediMenu.MenuItems.FindByText(menuName5SM, true);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				send5SMmenu.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendMissingExpectedEntries008()
		{
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._008;
			declaration.InvoiceLines.AddNew();
			var ediMenu = new MiscEDIMenu();
			ediMenu.Declaration = declaration;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var send008menu = ediMenu.MenuItems.FindByText(menuName008, true);
				send008menu.PerformClick();
				AssertEquals("Entries for this job have not been generated. Do you want to generate entries and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send008menu.PerformClick();
				AssertEquals("1 entry has been generated", 1, declaration.ActiveEntryHeaders.Count);
			}
		}

		public void TestSendMissingExpectedEntries5SM()
		{
			var ediMenu = new MiscEDIMenu();
			ediMenu.Declaration = declaration;
			declaration.JE_MessageType = ElectronicDocumentTypeList.Codes._5SM;
			declaration.InvoiceLines.AddNew();
			ediMenu = new MiscEDIMenu();
			ediMenu.Declaration = declaration;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				Factory.Save();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var send5SMmenu = ediMenu.MenuItems.FindByText(menuName5SM, true);
				send5SMmenu.PerformClick();
				AssertEquals("Entries for this job have not been generated. Do you want to generate entries and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				send5SMmenu.PerformClick();
				AssertEquals("1 entry has been generated", 1, declaration.ActiveEntryHeaders.Count);
			}
		}

		public void TestCheckNullReferenceException()
		{
			AssertNoExceptionThrown(() => new MiscEDIMenu().RefreshMenu());
		}

		void AssertSendCheckHasValidationMessageErrorsIsAllowedFalseDialogResultYes(string messageType, string menuName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = messageType;
			declaration.InvoiceLines.AddNew();
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				var ediMenu = new MiscEDIMenuForTest();
				ediMenu.Declaration = declaration;
				var sendMenu = ediMenu.MenuItems.FindByText(menuName, true);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, messageType, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				var sendingObject = sendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				AssertNotEquals(ZString.Empty, sendingObject.BizObjValidationMessageErrors);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				sendMenu.PerformClick();
				AssertEquals("0 message has been generated", 0, declaration.ActiveEntryHeaders[0].Messages.Count);
				var expectedMessage = "There are following errors. Please check and rectify the problems before attempting to print this document again.\r\n" + sendingObject.GetErrors().First().Message;
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultYes(string messageType, string menuName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = messageType;
			declaration.InvoiceLines.AddNew();
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				var ediMenu = new MiscEDIMenuForTest();
				ediMenu.Declaration = declaration;
				var sendMenu = ediMenu.MenuItems.FindByText(menuName, true);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, messageType, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				var sendingObject = sendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				AssertNotEquals(ZString.Empty, sendingObject.BizObjValidationMessageErrors);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMenu.PerformClick();
				AssertEquals("1 message has been generated", 1, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(ZString.Format(Business.MessageSender.MessageSendSuccessful, 1), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultNo(string messageType, string menuName)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_MessageType = messageType;
			declaration.InvoiceLines.AddNew();
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
			using (KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, "12345"))
			{
				declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
				Factory.Save();
				var ediMenu = new MiscEDIMenuForTest();
				ediMenu.Declaration = declaration;
				var send008menu = ediMenu.MenuItems.FindByText(menuName, true);
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration, ElectronicDocumentTypeList.Codes._008, MessageFunctions.MessageFunctionCode.Original) as JobDeclarationMessageSendingObjectParent;
				AssertEquals(1, sendingObjectParent.SendingObjectsCollection.Count);
				var sendingObject = sendingObjectParent.SendingObjectsCollection[0];
				sendingObject.ShouldSend = true;
				AssertNotEquals(ZString.Empty, sendingObject.BizObjValidationMessageErrors);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				send008menu.PerformClick();
				AssertEquals("0 message has been generated", 0, declaration.ActiveEntryHeaders[0].Messages.Count);
				AssertEquals(sendingObject.BizObjValidationMessageErrors, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend008()
		{
			AssertSendCheckHasValidationMessageErrorsIsAllowedFalseDialogResultYes(ElectronicDocumentTypeList.Codes._008, menuName008);
			AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultYes(ElectronicDocumentTypeList.Codes._008, menuName008);
			AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultNo(ElectronicDocumentTypeList.Codes._008, menuName008);
		}

		public void TestSend5SM()
		{
			AssertSendCheckHasValidationMessageErrorsIsAllowedFalseDialogResultYes(ElectronicDocumentTypeList.Codes._5SM, menuName5SM);
			AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultYes(ElectronicDocumentTypeList.Codes._5SM, menuName5SM);
			AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultNo(ElectronicDocumentTypeList.Codes._5SM, menuName5SM);
		}

		public void TestSendD87()
		{
			//AssertSendCheckHasValidationMessageErrorsIsAllowedFalseDialogResultYes(ElectronicDocumentTypeList.Codes._D87, menuNameD87); //currently no validations - will be done in WI00676567.
			AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultYes(ElectronicDocumentTypeList.Codes._D87, menuNameD87);
			AssertSendCheckHasValidationMessageErrorsIsAllowedTrueDialogResultNo(ElectronicDocumentTypeList.Codes._D87, menuNameD87);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
		}
		JobDeclaration declaration;

		public class MiscEDIMenuForTest : MiscEDIMenu
		{
			public IJobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParentExposed(JobDeclaration declaration, ZString messageType, MessageFunctions.MessageFunctionCode messageFunctionCode) => GetJobDeclarationMessageSendingObjectParent(declaration, messageType, messageFunctionCode);
			protected override IJobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent(JobDeclaration declaration, ZString messageType, MessageFunctions.MessageFunctionCode messageFunctionCode) => GetSendingObjectParent(declaration, messageType, messageFunctionCode);

			IJobDeclarationMessageSendingObjectParent GetSendingObjectParent(JobDeclaration declaration, ZString messageType, MessageFunctions.MessageFunctionCode messageFunctionCode)
			{
				if (parent == null)
				{
					parent = base.GetJobDeclarationMessageSendingObjectParent(declaration, messageType, messageFunctionCode);
				}
				return parent;
			}
			IJobDeclarationMessageSendingObjectParent parent;
		}
	}
}
