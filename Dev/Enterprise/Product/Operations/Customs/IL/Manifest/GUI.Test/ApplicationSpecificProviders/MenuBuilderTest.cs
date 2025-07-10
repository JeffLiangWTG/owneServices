using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IL.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using AsycudaManifestHeader = Enterprise.Customs.IL.Manifest.Business.AsycudaManifestHeader;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestMenuCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using (var form = new ZForm(header))
			{
				AssertEquals("IL Manifest", new MenuBuilder(header, form).MenuCaption.EnglishText);
			}
		}

		public void TestMenuItemVisibility()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using (var form = new ZForm(header))
			{
				var menu = new AsycudaMenuForTest(header);
				form.Menu.MenuItems.Add(menu);
				form.Show();
				menu.OnPopup(EventArgs.Empty);

				var sendManifestMenuItem = menu.MenuItems.FindByText("Send to Customs");
				AssertNotNull("Send to Customs", sendManifestMenuItem);
				AssertEquals("Menu item 'Send to Customs' Visibility", true, sendManifestMenuItem.Visible);

				var sendSupportingDocumentsMenuItem = menu.MenuItems.FindByText("Send Supporting Documents");
				AssertNotNull("Send Supporting Documents", sendSupportingDocumentsMenuItem);
				AssertEquals("Menu item 'Send Supporting Documents' Visibility", true, sendSupportingDocumentsMenuItem.Visible);
			}
		}

		public void TestMenuItem_SendToCustoms()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			using (var form = new ManifestForm(header))
			{
				var manifestMenuItem = form.Menu.MenuItems.FindByText("Manifest", true);
				AssertNotNull("Manifest menu item", manifestMenuItem);
				AssertType<AsycudaMenu>("Manifest menu item should of type AsycudaMenu", manifestMenuItem);
				((AsycudaMenu)manifestMenuItem).BuildMenu();

				var sendMessageMenuItem = manifestMenuItem.MenuItems.FindByText("Send to Customs", true);
				AssertNotNull("Send to Customs", sendMessageMenuItem);

				sendMessageMenuItem.PerformClick();
				AssertType<MessageSendingFormWithValidationDetails>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestMessagesFromDatabaseAreDifferentFromMessagesInMemory()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var message = header.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			message = Factory.New<ILEDIMessage>();
			message.EM_MessageNum = "1235";
			message.EM_MessageType = ILMessageTypeList.Codes.MAN;
			message.EM_MessageSubType = ILEDIMessageSubTypeList.Codes.ForwarderManifestRequest;
			message.EM_LinkTable = header.TableName;
			message.EM_LinkUniqueID = header.PK;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageText = "ABC";

			using (var form = new ManifestForm(header))
			{
				var manifestMenuItem = form.Menu.MenuItems.FindByText("Manifest", true);
				AssertNotNull("Manifest menu item", manifestMenuItem);
				AssertType<AsycudaMenu>("Manifest menu item should of type AsycudaMenu", manifestMenuItem);
				((AsycudaMenu)manifestMenuItem).BuildMenu();

				var sendMessageMenuItem = manifestMenuItem.MenuItems.FindByText("Send to Customs", true);
				AssertNotNull("Send to Customs", sendMessageMenuItem);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				sendMessageMenuItem.PerformClick();
				AssertEquals("A new message has been attached to this manifest header, please reopen the form before sending a message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItem_SendSupportingDocuments_NoSupportingDocuments()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var bill = header.Bills.AddNew();
			var supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_Status = "REJ";

			using (var form = new ManifestForm(header))
			{
				var manifestMenuItem = form.Menu.MenuItems.FindByText("Manifest", true);
				AssertNotNull("Manifest menu item", manifestMenuItem);
				AssertType<AsycudaMenu>("Manifest menu item should of type AsycudaMenu", manifestMenuItem);
				((AsycudaMenu)manifestMenuItem).BuildMenu();

				var sendSupportingDocumentsMenuItem = manifestMenuItem.MenuItems.FindByText("Send Supporting Documents", true);
				AssertNotNull("Send Supporting Documents", sendSupportingDocumentsMenuItem);

				sendSupportingDocumentsMenuItem.PerformClick();
				AssertEquals("There are no any supporting documents ready to be sent to customs (Status ‘Requested’ or Empty).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItem_SendSupportingDocuments_WithSupportingDocuments()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;
			var bill = header.Bills.AddNew();
			var supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "111";
			supportingDocument.CSI_Status = "REQ";

			using (var form = new ManifestForm(header))
			{
				var manifestMenuItem = form.Menu.MenuItems.FindByText("Manifest", true);
				AssertNotNull("Manifest menu item", manifestMenuItem);
				AssertType<AsycudaMenu>("Manifest menu item should of type AsycudaMenu", manifestMenuItem);
				((AsycudaMenu)manifestMenuItem).BuildMenu();

				var sendSupportingDocumentsMenuItem = manifestMenuItem.MenuItems.FindByText("Send Supporting Documents", true);
				AssertNotNull("Send Supporting Documents", sendSupportingDocumentsMenuItem);

				sendSupportingDocumentsMenuItem.PerformClick();
				AssertType<SupportingDocumentsSelectionDialog>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestMenuItem_SendManifestQuery()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Israel;

			using (var form = new ManifestForm(header))
			{
				var manifestMenuItem = form.Menu.MenuItems.FindByText("Manifest", true);
				AssertNotNull("Manifest menu item", manifestMenuItem);
				AssertType<AsycudaMenu>("Manifest menu item should of type AsycudaMenu", manifestMenuItem);
				((AsycudaMenu)manifestMenuItem).BuildMenu();

				var sendSupportingDocumentsMenuItem = manifestMenuItem.MenuItems.FindByText("Send Manifest Query", true);
				AssertNotNull("Send Manifest Query Menu item found", sendSupportingDocumentsMenuItem);

				sendSupportingDocumentsMenuItem.PerformClick();
				AssertType<ManifestQuerySelectionDialog>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
