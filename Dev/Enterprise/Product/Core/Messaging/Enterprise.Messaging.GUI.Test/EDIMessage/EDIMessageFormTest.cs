using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.HttpXmlMessaging;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Messaging.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.GUI.Testing
{
	[TestedType(typeof(EDIMessageForm))]
	public class EDIMessageFormTest : ZFormBasherTest
	{
		public void TestMessagaModificationMenu()
		{
			SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			EDIMessage message = EDIMessageTestFactory.New(Factory);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			GlbStaff staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_IsDeveloper = false;
			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff.Add(staff);
			Factory.Save();
			SystemDataRegistry.Instance.MessageModificationBeforeSendingAuthorisationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				AssertNull("Is Not Transmit", form.Menu.MenuItems.FindByText("Modify Message"));
			}

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				AssertNotNull(form.Menu.MenuItems.FindByText("Modify Message"));
			}

			group.Staff.Remove(staff);
			Factory.Save();
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				AssertNull(form.Menu.MenuItems.FindByText("Modify Message"));
			}

			message.EM_Status = EDIMessage.Status.Sent;
			Factory.Save();
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				AssertNull("Is Not Queued", form.Menu.MenuItems.FindByText("Modify Message"));
			}

			message.EM_Status = EDIMessage.Status.Queued;
			Factory.Save();

			SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				AssertNull(form.Menu.MenuItems.FindByText("Modify Message"));
			}

			staff.GS_IsDeveloper = true;
			Factory.Save();
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				AssertNotNull("Is a developer", form.Menu.MenuItems.FindByText("Modify Message"));
			}

			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				AssertNotNull("Is a developer and inbound", form.Menu.MenuItems.FindByText("Modify Message"));
			}
		}

		public void TestMessageModification_Click()
		{
			SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			EDIMessage message = EDIMessageTestFactory.New(Factory);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			GlbStaff staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			GlbGroup group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff.Add(staff);
			Factory.Save();
			SystemDataRegistry.Instance.MessageModificationBeforeSendingAuthorisationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();
			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				MenuItem item = form.Menu.MenuItems.FindByText("Modify Message");
				AssertNotNull(item);
				item.PerformClick();
				AssertEquals(typeof(EDIMessageModificationForm), ZFormModaliser.LastFormShownForTest.GetType());
				ZFormModaliser.LastFormShownForTest.Dispose();
			}
		}

		public void TestMessageLargeUI()
		{
			string intputFilePath = LargeMessageTestHelper.CreateTestFile(LargeMessageTestHelper.FileType.Text, 100 * 1024 * 1024);
			try
			{
				LargeFileHolder tester = new LargeFileHolder(intputFilePath);
				EDIMessage message = EDIMessageTestFactory.New(Factory);
				message.SetEM_MessageTextSource(tester);

				using (EDIMessageForm form = new EDIMessageForm(message))
				{
					form.Show();
					CheckControlExistsAndVisible(form, "zLabelTruncateNotification");
					CheckControlExistsAndVisible(form, "zButtonSaveFormatedMessage");
					CheckControlExistsAndVisible(form, "MessageTextTextBox");
					string sizeText = LargeMessageHelper.SizeInKb(LargeMessageHelper.DetailTextSizeLimit).ToString() + "KB";
					AssertEquals("Interchange content size should be " + sizeText, LargeMessageHelper.DetailTextSizeLimit, ((TextBox)form.Controls.Find("MessageTextTextBox", true)[0]).Text.Length);
				}
			}
			finally
			{
				TempFile.Delete(intputFilePath);
			}
		}

		public void TestValuesOfControls()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_SessionGUID = Guid.NewGuid();

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_EI = interchange.PK;
			message.EM_SystemCreateTimeUtc = DateTime.UtcNow;
			message.EM_ExternalReferenceNumber = "8a64619e-9a9b-40b8-aa52-330511287dc6";

			var party = Factory.NewWithValidTestData<EDICommunicationParty>();
			party.ECP_Name = "Party";
			message.EM_ECC_CommunicationPartyConfig = party.OutboundConfig.PK;

			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();

				var createTimeControl = CheckControlExistsAndVisible(form, "systemCreateTimeUtc");
				AssertType(typeof(ZDateEdit), createTimeControl);
				AssertEquals(message.EM_SystemCreateTimeUtc, ((ZDateEdit)createTimeControl).DateTimeValue);
				AssertEquals(161, createTimeControl.Width);

				var eHubIdControl = CheckControlExistsAndVisible(form, "interchangeEHubIDTextBox");
				AssertType(typeof(ZTextBox), eHubIdControl);
				AssertEquals(message.InterchangeEHubID, ((ZTextBox)eHubIdControl).Text);

				var messageTimeDateEdit = CheckControlExistsAndVisible(form, "messageTimeDateEdit");
				AssertType(typeof(ZDateEdit), messageTimeDateEdit);
				AssertEquals(message.EM_MessageDateTime, ((ZDateEdit)messageTimeDateEdit).DateTimeValue);
				AssertEquals(161, messageTimeDateEdit.Width);

				var interchangeTimeDateEdit = CheckControlExistsAndVisible(form, "interchangeTimeDateEdit");
				AssertType(typeof(ZDateEdit), interchangeTimeDateEdit);
				AssertEquals(message.EM_DateTimeInterchangeSent, ((ZDateEdit)interchangeTimeDateEdit).DateTimeValue);
				AssertEquals(161, interchangeTimeDateEdit.Width);

				var zTextBoxEdiClient = CheckControlExistsAndVisible(form, "zTextBoxEdiClient");
				AssertType(typeof(ZTextBox), zTextBoxEdiClient);
				AssertEquals(message.CommunicationPartyConfig.Party.Name, ((ZTextBox)zTextBoxEdiClient).Text);
				AssertEquals(161, zTextBoxEdiClient.Width);

				var zTextBoxExternalReferenceNumber = CheckControlExistsAndVisible(form, "externalReferenceNumberTextBox");
				AssertType(typeof(ZTextBox), zTextBoxExternalReferenceNumber);
				AssertEquals(message.EM_ExternalReferenceNumber, ((ZTextBox)zTextBoxExternalReferenceNumber).Text);
			}
		}

		public void TestShowInterchangeButtonWithValidInterchange()
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_SessionGUID = Guid.NewGuid();

			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_EI = interchange.PK;
			Factory.Save();

			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();

				var showInterchangeButton = CheckControlExistsAndVisible(form, "showInterchangeButton");
				AssertType(typeof(ZButton), showInterchangeButton);
				AssertEquals("View", ((ZButton)showInterchangeButton).Text);
				AssertEquals(60, showInterchangeButton.Width);

				AssertNoExceptionThrown(() => ((ZButton)showInterchangeButton).PerformClick());
				using (var ediInterchangeForm = ZApplication.GetOpenForms().FirstOrDefault(f => f.Name == "EDIInterchangeForm"))
				{
					AssertNotNull(ediInterchangeForm);
				}
			}
		}

		public void TestShowInterchangeButtonWithoutInterchange()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();

			using (EDIMessageForm form = new EDIMessageForm(message))
			{
				form.Show();
				var showInterchangeButton = CheckControlExistsAndVisible(form, "showInterchangeButton");
				AssertEquals(false, showInterchangeButton.Enabled);
			}
		}
		public void TestShouldDisplayLinkedMessageControlsForRequestMessageWithLinkedMessage()
		{
			CreateTestMessages(true, true, out var requestMessage, out var responseMessage);

			using (var form = new EDIMessageForm(Factory.Load<EDIMessage>(requestMessage)))
			{
				AssertLinkedMessageControlsAreCorrect(form, true, Factory.Load<EDIMessage>(responseMessage).EM_MessageNum);
			}
		}

		public void TestShouldDisplayLinkedMessageControlsForResponseMessageWithLinkedMessage()
		{
			CreateTestMessages(true, true, out var requestMessage, out var responseMessage);

			using (var form = new EDIMessageForm(Factory.Load<EDIMessage>(responseMessage)))
			{
				AssertLinkedMessageControlsAreCorrect(form, true, Factory.Load<EDIMessage>(requestMessage).EM_MessageNum);
			}
		}

		public void TestShouldNotDisplayLinkedMessageControlsForRequestMessageWithoutLinkedMessage()
		{
			CreateTestMessages(true, false, out var requestMessage, out var _);

			using (var form = new EDIMessageForm(Factory.Load<EDIMessage>(requestMessage)))
			{
				AssertLinkedMessageControlsAreCorrect(form, false);
			}
		}

		public void TestShouldNotDisplayLinkedMessageControlsForResponseMessageWithoutLinkedMessage()
		{
			CreateTestMessages(false, true, out var _, out var responseMessage);

			using (var form = new EDIMessageForm(Factory.Load<EDIMessage>(responseMessage)))
			{
				AssertLinkedMessageControlsAreCorrect(form, false);
			}
		}

		public void TestInterpretationTabVisible()
		{
			using (var form = new EDIMessageForm(EDIMessageTestFactory.New(Factory)))
			{
				form.Show();
				var tab = (ZTabPage)form.Controls.Find("InterpretationTabPage", true)[0];
				AssertEquals("InterpretationTabPage", true, tab.TabVisible);
				tab.Show();
				var htmlBox = tab.Controls.Find("EDIMessageInterpretationUserControl", true)[0];
				AssertEquals("EDIMessageInterpretationUserControl", true, htmlBox.Visible);
			}
		}

		static Control CheckControlExistsAndVisible(Form form, string controlName)
		{
			var controls = CheckControlExists(form, controlName);

			Assert(controlName + " control should be visible", controls[0].Visible);

			return controls[0];
		}

		static Control[] CheckControlExists(Form form, string controlName)
		{
			Control[] controls = form.Controls.Find(controlName, true);

			if (!(controls != null && controls.Length == 1))
			{
				Assert(controlName + " control should exist", false);
			}

			return controls;
		}

		protected override Form GetFormToBashCore()
		{
			var bizO = EDIMessageTestFactory.New(Factory);
			bizO.EM_ReceiveTransmit = EDIMessage.Status.Received;
			bizO.ClearHasChanges();
			return new EDIMessageForm(bizO);
		}

		void CreateTestMessages(bool shouldCreateRequest, bool shouldCreateResponse, out ZGuid requestMessagePk, out ZGuid responseMessagePk)
		{
			var interchange = Factory.NewWithValidTestData<EDIInterchange>();
			interchange.EI_SessionGUID = Guid.NewGuid();

			requestMessagePk = ZGuid.Empty;
			responseMessagePk = ZGuid.Empty;
			if (shouldCreateRequest)
			{
				var requestMessage = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
				requestMessage.EM_EI = interchange.PK;
				requestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				requestMessagePk = requestMessage.PK;
			}

			if (shouldCreateResponse)
			{
				var responseMessage = Factory.NewWithValidTestData<HttpXmlEDIMessage>();
				responseMessage.EM_EI = interchange.PK;
				responseMessage.EM_EM_RequestMessage = requestMessagePk;
				responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				responseMessagePk = responseMessage.PK;
			}

			Factory.Save();
		}

		void AssertLinkedMessageControlsAreCorrect(EDIMessageForm form, bool isLinkedMessagePresent, string linkedMessageNumber = "")
		{
			form.Show();

			if (isLinkedMessagePresent)
			{
				var linkedMessageNumberTextBox = CheckControlExistsAndVisible(form, "linkedEDIMessageNumberTextBox");
				var showLinkedMessageButton = CheckControlExistsAndVisible(form, "showLinkedEDIMessageButton");
				var originalMessageNumber = form.Controls.Find("zTextBoxMessageNumber", true)[0].Text;

				AssertType(typeof(ZTextBox), linkedMessageNumberTextBox);
				AssertEquals(linkedMessageNumber, ((ZTextBox)linkedMessageNumberTextBox).Text);

				AssertNoExceptionThrown(() => ((ZButton)showLinkedMessageButton).PerformClick());

				using (var ediMessageForms = new DisposableList(ZApplication.GetOpenForms().Where(f => f.Name == "EDIMessageForm")))
				{
					AssertEquals(2, ediMessageForms.Count);
					Assert(ediMessageForms.Any(f => ((EDIMessageForm)f).Controls.Find("zTextBoxMessageNumber", true)[0].Text.Equals(linkedMessageNumber)
					&& ((EDIMessageForm)f).Controls.Find("linkedEDIMessageNumberTextBox", true)[0].Text.Equals(originalMessageNumber)));
				}
			}
			else
			{
				var linkedMessageNumberTextBox = CheckControlExists(form, "linkedEDIMessageNumberTextBox")[0];
				var showLinkedMessageButton = CheckControlExists(form, "showLinkedEDIMessageButton")[0];

				Assert("Linked Message Text Box should not be visible", !linkedMessageNumberTextBox.Visible);
				Assert("Show Linked Message Button should not be visible", !showLinkedMessageButton.Visible);

				using (var ediMessageForms = new DisposableList(ZApplication.GetOpenForms().Where(f => f.Name == "EDIMessageForm")))
				{
					AssertEquals(1, ediMessageForms.Count);
				}
			}
		}
	}
}
