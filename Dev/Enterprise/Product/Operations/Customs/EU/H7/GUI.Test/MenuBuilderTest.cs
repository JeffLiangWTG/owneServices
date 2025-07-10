using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		const string ImportTestMessageDevOnlyMenuLabel = "Import EU H7 Incoming Message[DEV Only]";

		public void TestSendMessageToCustoms_HeaderLevelMessageError()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_OA_Declarant = ZGuid.Empty;
			manifestHeader.Bills.AddNew();
			var sendingParent = new MessageSendingObjectParent<MessageSendingObject>(manifestHeader);
			using (var form = new ZForm(sendingParent))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendMessageToCustomsMenu = menu.MenuItems.FindByText("Send Message to Customs");
				sendMessageToCustomsMenu.PerformClick();
				var bindedEntity = (MessageSendingObjectParent<MessageSendingObject>)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				var notifications = bindedEntity.SendingObjectsCollection[0].Notifications;
				var expectedError = @"Message Error - Message: 
Declarant: You have not entered a Declarant.
Representative: You have not entered a Representative.
Transport Mode: You have not entered a Transport Mode.
Flight/Voyage: You have not entered a Flight/Voyage/Journey.";

				AssertEqualsIgnoreLineBreaks("Message error added", expectedError, notifications.FirstOrDefault().Message);
			}
		}

		public void TestImportEUH7IncomingMessage_OnlyVisibleInTestEnvioronmentAndSupportUser()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Production;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertMenuItem(ImportTestMessageDevOnlyMenuLabel, isVisible: false);
			}

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertMenuItem(ImportTestMessageDevOnlyMenuLabel, isVisible: false);
			}

			ObjectFactory.Get<IProductRegistration>().KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				AssertMenuItem(ImportTestMessageDevOnlyMenuLabel, isVisible: true);
			}
		}

		const string SendMessageToCustomsMenuLabel = "Send Message to Customs";

		public void TestSendMessageToCustoms_MenuVisible()
		{
			AssertMenuItem(SendMessageToCustomsMenuLabel);
		}

		public void TestMenuItemAction_SendMessageToCustoms_ShouldNotPopupValidationMessage()
		{
			AssertMenuItemAction_ShouldNotPopupValidationMessage(SendMessageToCustomsMenuLabel);
		}

		public void TestMenuItemAction_SendMessageToCustoms_ShouldPopupWarningMessageBeforeSavingChanges()
		{
			AssertMenuItemAction_ShouldPopupWarningMessageBeforeSavingChanges(SendMessageToCustomsMenuLabel);
		}

		const string UploadDocumentsMenuLabel = "Upload Documents";

		public void TestMenuItem_UploadDocuments()
		{
			AssertMenuItem(UploadDocumentsMenuLabel);
		}

		public void TestMenuItemAction_UploadDocuments_DisplaysUploadDocumentsForm()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var document = bill.RequestedDocuments.AddNew();
			document.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				menu.MenuItems.FindByText(UploadDocumentsMenuLabel).PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;

				AssertType<UploadDocumentsForm>(lastFormShown);
			}
		}

		public void TestMenuItemAction_UploadDocuments_NoBillsWithOPEStatus_ShowsNotification()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var document = bill.RequestedDocuments.AddNew();
			document.CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				menu.MenuItems.FindByText(UploadDocumentsMenuLabel).PerformClick();
				AssertEquals("No entry", "No bills on this header have any Requested Documents with status of 'Request Opened by Customs'.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemAction_UploadDocuments_ShouldNotPopupValidationMessage()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var document = bill.RequestedDocuments.AddNew();
			document.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			AssertMenuItemAction_ShouldNotPopupValidationMessage(UploadDocumentsMenuLabel, manifestHeader);
		}

		public void TestMenuItemAction_UploadDocuments_ShouldPopupWarningMessageBeforeSavingChanges()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var document = bill.RequestedDocuments.AddNew();
			document.CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			AssertMenuItemAction_ShouldPopupWarningMessageBeforeSavingChanges(UploadDocumentsMenuLabel, manifestHeader);
		}

		const string DocumentRequestMenuLabel = "Request Documents";

		public void TestMenuItem_DocumentRequest_IsNotVisible()
		{
			AssertMenuItem(DocumentRequestMenuLabel, isVisible: false);
		}

		public void TestNoErrorsAreAppendedToMessageSendingObjectWhenNoHeaderErrors()
		{
			var declarantAddress = SetupValidCustomsOrgForManifestHeader("AA");
			var representativeAddress = SetupValidCustomsOrgForManifestHeader("BB");

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_OA_Declarant = declarantAddress.PK;
			manifestHeader.AMA_OA_Representative = representativeAddress.PK;
			manifestHeader.AMA_TransportMode = Core.Constants.TransportModes.Air;
			manifestHeader.AMA_Voyage = "AFR123";
			manifestHeader.Bills.AddNew();

			var sendingParent = new MessageSendingObjectParent<MessageSendingObject>(manifestHeader);
			using (var form = new ZForm(sendingParent))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendMessageToCustomsMenu = menu.MenuItems.FindByText("Send Message to Customs");
				sendMessageToCustomsMenu.PerformClick();
				var bindedEntity = (MessageSendingObjectParent<MessageSendingObject>)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				var messageSendingObject = bindedEntity.SendingObjectsCollection[0];

				AssertEquals("No Message error gets appended", false, messageSendingObject.HasRowMessageErrors);
			}
		}

		void AssertMenuItem(string menuText, AsycudaManifestHeader manifestHeader = null, bool isVisible = true)
		{
			if (manifestHeader == null)
			{
				manifestHeader = Factory.New<AsycudaManifestHeader>();
			}

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var menuItem = menu.MenuItems.FindByText(menuText);
				if (isVisible)
				{
					AssertNotNull(menuItem);
				}
				else
				{
					AssertNull(menuItem);
				}
			}
		}

		void AssertMenuItemAction_ShouldNotPopupValidationMessage(string menuText, AsycudaManifestHeader manifestHeader = null)
		{
			if (manifestHeader == null)
			{
				manifestHeader = Factory.New<AsycudaManifestHeader>();
			}

			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				menu.MenuItems.FindByText(menuText).PerformClick();
				AssertNullOrEmpty("Should not popup validation message", UnitTestUserNotification.Instance.LastMessage?.Text);
			}
		}

		void AssertMenuItemAction_ShouldPopupWarningMessageBeforeSavingChanges(string menuText, AsycudaManifestHeader manifestHeader = null)
		{
			if (manifestHeader == null)
			{
				manifestHeader = Factory.New<AsycudaManifestHeader>();
			}

			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				menu.MenuItems.FindByText(menuText).PerformClick();
				Assert(!manifestHeader.HasChanges);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage?.Text);

				manifestHeader.AMA_AgentType = "CHY";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menu.MenuItems.FindByText(menuText).PerformClick();
				Assert(manifestHeader.HasChanges);
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage?.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				menu.MenuItems.FindByText(menuText).PerformClick();
				Assert(!manifestHeader.HasChanges);
			}
		}

		OrgAddress SetupValidCustomsOrgForManifestHeader(ZString orgCode)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var contact = orgHeader.Contacts.AddNew();
			var personalAlloc = contact.Allocations.AddNew();
			orgHeader.OH_Code = orgCode;
			personalAlloc.PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact.OC_Phone = "1234567";
			contact.OC_Email = "test@test.com";

			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.Country.Code;
			eori.OK_CustomsRegNo = "TestReg";

			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_PostCode = "1111";
			orgAddress.OA_Address1 = "Address";

			return orgAddress;
		}
	}
}
