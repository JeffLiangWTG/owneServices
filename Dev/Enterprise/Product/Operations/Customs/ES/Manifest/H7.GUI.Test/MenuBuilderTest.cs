using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	public class MenuBuilderTest : TestCaseWithFactory
	{
		#region Create G3 Declaration

		[RequiresSTA]
		public void TestMenuItemAction_CreateG3Declaration_HeaderLevelMessageError()
		{
			AssertHeaderLevelMessageError(CreateG3DeclarationMenuLabel);
		}

		[RequiresSTA]
		public void TestMenuItemAction_CreateG3Declaration()
		{
			AssertMenuItem(CreateG3DeclarationMenuLabel);
		}

		[RequiresSTA]
		public void TestMenuItemAction_CreateG3Declaration_ShouldNotPopupValidationMessage()
		{
			AssertNotPopupValidationMessage(CreateG3DeclarationMenuLabel);
		}

		[RequiresSTA]
		public void TestMenuItemAction_CreateG3Declaration_ShouldPopupWarningMessageBeforeSavingChanges()
		{
			AssertPopupWarningMessageBeforeSavingChanges(CreateG3DeclarationMenuLabel);
		}

		#endregion

		#region Revoke G3 Declaration

		[RequiresSTA]
		public void TestMenuItemAction_RevokeG3Declaration_HeaderLevelMessageError()
		{
			AssertHeaderLevelMessageError(RevokeG3DeclarationMenuLabel);
		}

		[RequiresSTA]
		public void TestMenuItemAction_RevokeG3Declaration()
		{
			AssertMenuItem(RevokeG3DeclarationMenuLabel);
		}

		[RequiresSTA]
		public void TestMenuItemAction_RevokeG3Declaration_ShouldNotPopupValidationMessage()
		{
			AssertNotPopupValidationMessage(RevokeG3DeclarationMenuLabel);
		}

		[RequiresSTA]
		public void TestMenuItemAction_RevokeG3Declaration_ShouldPopupWarningMessageBeforeSavingChanges()
		{
			AssertPopupWarningMessageBeforeSavingChanges(RevokeG3DeclarationMenuLabel);
		}

		[RequiresSTA]
		public void TestMenuItemAction_RevokeG3Declaration_WhenNoBillWithAcceptedG3DMessage_ShouldShowNotification()
		{
			var manifestHeader = CreateAsycudaManifestHeader();
			var bill = manifestHeader.Bills.AddNew();
			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				menu.MenuItems.FindByText(RevokeG3DeclarationMenuLabel).PerformClick();
				AssertEquals(ExpectedMessageText_NoBillsWithAcceptedG3DMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		[RequiresSTA]
		public void TestCheckEmptyCertificateBeforeSend()
		{
			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;
			header.AMA_CustomsProfile = ZString.Empty;

			using var menu = new AsycudaMenuForTest(header);
			using var form = new ZForm(header);

			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			var sendMessageMenuItem = menu.MenuItems.FindByText("Send Message to Customs");
			sendMessageMenuItem.PerformClick();

			AssertEquals("Should check empty certificate", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[RequiresSTA]
		public void TestCheckEmptyBrokerBeforeSend()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = ZString.Empty;
			header.AMA_CustomsProfile = "test";

			using var menu = new AsycudaMenuForTest(header);
			using var form = new ZForm(header);

			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			var sendMessageMenuItem = menu.MenuItems.FindByText("Send Message to Customs");
			sendMessageMenuItem.PerformClick();

			AssertEquals("Should check empty broker", "Cannot send message without a broker and valid certificate; please enter the broker and a valid certificate.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void AssertHeaderLevelMessageError(string menuItemText)
		{
			var manifestHeader = CreateAsycudaManifestHeader();
			manifestHeader.AMA_OA_Declarant = ZGuid.Empty;
			manifestHeader.AMA_MasterBill = "MB1234";

			CreateAsycudaBill(manifestHeader, menuItemText == RevokeG3DeclarationMenuLabel);

			var sendingParent = new G3MessageSendingObjectParent(manifestHeader);
			using var form = new ZForm(sendingParent);
			using var menu = new AsycudaMenuForTest(manifestHeader);

			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			var g3MessageMenu = menu.MenuItems.FindByText(menuItemText);
			g3MessageMenu.PerformClick();
			var bindedEntity = (G3MessageSendingObjectParent)ZFormModaliser.LastIBusinessShownOnDialogForTest;
			var notifications = bindedEntity.SendingObjectsCollection[0].Notifications;
			var expectedError = @"Message Error - Message: 
Customs Office: You have not entered an office of type Office of Lodgement.
DSDT MRN/Flight No.: You have not entered a DSDT MRN/Flight No..
Declarant: You have not entered a Declarant.
Presenter: You have not entered a Presenter.
Transport Mode: You have not entered a Transport Mode.
Flight/Voyage: You have not entered a Flight/Voyage/Journey.";

			AssertEqualsIgnoreLineBreaks("Message error added", expectedError, notifications.FirstOrDefault().Message);
		}

		void AssertMenuItem(string menuItemText)
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();

			using var form = new ZForm(manifestHeader);
			using var menu = new AsycudaMenuForTest(manifestHeader);

			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			var menuItem = menu.MenuItems.FindByText(menuItemText);

			AssertNotNull(menuItem);
		}

		void AssertNotPopupValidationMessage(string menuItemText)
		{
			var manifestHeader = CreateAsycudaManifestHeader();
			CreateAsycudaBill(manifestHeader, menuItemText == RevokeG3DeclarationMenuLabel);
			Factory.Save();

			using var form = new ZForm(manifestHeader);
			using var menu = new AsycudaMenuForTest(manifestHeader);

			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);
			menu.MenuItems.FindByText(menuItemText).PerformClick();

			AssertNullOrEmpty("Should not popup validation message", UnitTestUserNotification.Instance.LastMessage?.Text);
		}

		void AssertPopupWarningMessageBeforeSavingChanges(string menuItemText)
		{
			var manifestHeader = CreateAsycudaManifestHeader();
			manifestHeader.AMA_CustomsProfile = "TESTCERT1";

			CreateAsycudaBill(manifestHeader, menuItemText == RevokeG3DeclarationMenuLabel);
			Factory.Save();

			using var form = new ZForm(manifestHeader);
			using var menu = new AsycudaMenuForTest(manifestHeader);

			form.Menu.MenuItems.Add(menu);
			form.Show();
			menu.OnPopup(EventArgs.Empty);

			var menuItem = menu.MenuItems.FindByText(menuItemText);
			menuItem.PerformClick();

			Assert("ManifestHeader should have no change.", !manifestHeader.HasChanges);
			AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage?.Text);

			manifestHeader.AMA_AgentType = "CHY";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			menuItem.PerformClick();

			CombineAssertions(() =>
			{
				Assert("ManifestHeader should have changes.", manifestHeader.HasChanges);
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage?.Text);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			menuItem.PerformClick();

			Assert(!manifestHeader.HasChanges);
		}

		[RequiresSTA]
		public void TestMenuItemAction_UploadDocuments_DisplaysUploadDocumentsForm()
		{
			var manifestHeader = CreateAsycudaManifestHeader();
			var bill = manifestHeader.Bills.AddNew();
			bill.DocumentationRequired = ESH7DocumentationRequiredList.Codes.Yes;
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

		[RequiresSTA]
		public void TestMenuItemAction_UploadDocuments_NoBillsWithDocumentsRequired_ShowsNotification()
		{
			var manifestHeader = CreateAsycudaManifestHeader();
			var bill = manifestHeader.Bills.AddNew();
			bill.DocumentationRequired = ESH7DocumentationRequiredList.Codes.No;
			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				menu.MenuItems.FindByText(UploadDocumentsMenuLabel).PerformClick();
				AssertEquals("No eligable bills", ExpectedMessageText_NoRequestedDocuments, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItem_DocumentRequest()
		{
			AssertMenuItem(DocumentRequestMenuLabel);
		}

		public void TestMenuItemAction_DocumentRequest_DisplaysDocumentRequestForm()
		{
			var manifestHeader = CreateAsycudaManifestHeader();
			var bill = manifestHeader.Bills.AddNew();
			var cusEntryNumber = bill.CustomsEntryNumbers.AddNew();
			cusEntryNumber.CE_EntryType = "CLR";
			cusEntryNumber.CE_EntryLineReference = "H7";
			cusEntryNumber.CE_EntryNum = "12345";

			Factory.Save();

			using (var form = new ZForm(manifestHeader))
			using (var menu = new AsycudaMenuForTest(manifestHeader))
			{
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				menu.MenuItems.FindByText(DocumentRequestMenuLabel).PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;

				AssertType<DocumentRequestForm>(lastFormShown);
			}
		}

		AsycudaManifestHeader CreateAsycudaManifestHeader()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AH";
			staff.GS_LoginName = "ahtest";

			var wrapper = ES.Business.GlbStaffWrapper.Get(staff);
			var cert = wrapper.ESBPasswordCollection.AddNew();
			cert.GP_Name = "TESTCERT1";
			cert.GP_MailBoxID = "Test1";
			cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

			var auth = cert.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = GlbStaff.CurrentUser.PK;

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_GS_NKCustomsAgent = "AH";

			return manifestHeader;
		}

		AsycudaBill CreateAsycudaBill(AsycudaManifestHeader manifestHeader, bool createAcceptedG3DMessage)
		{
			var bill = manifestHeader.Bills.AddNew();

			if (createAcceptedG3DMessage)
			{
				manifestHeader.G3MRNToRevoke = "MRN000123";
				var cusEntryNum = bill.CustomsEntryNumbers.AddNew();
				cusEntryNum.CE_EntryNum = "MRN000123";
				cusEntryNum.CE_EntryType = "MRN";
				cusEntryNum.CE_EntryLineReference = "G3";
			}

			return bill;
		}

		const string CreateG3DeclarationMenuLabel = "Create G3 Declaration";
		const string RevokeG3DeclarationMenuLabel = "Revoke G3 Declaration";
		const string UploadDocumentsMenuLabel = "Upload Documents";
		const string DocumentRequestMenuLabel = "Request Documents";

		const string ExpectedMessageText_NoRequestedDocuments = "No documents can be requested for the bills on this header.";
		const string ExpectedMessageText_NoBillsWithAcceptedG3DMessage = "No bills on this header have any 'G3D - G3D Declaration' message accepted with MRN = G3 MRN to Revoke.";
	}
}
