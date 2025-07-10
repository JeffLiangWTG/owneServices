using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(CustomsAndExciseReportsForm))]
	public class CustomsAndExciseReportsFormTest : ZTemplateFormTest
	{
		public void TestFormCaption()
		{
			using (var form = GetFormToBash() as CustomsAndExciseReportsForm)
			{
				AssertEquals("Customs and Excise Reports", form.FormCaption);
			}
		}

		public void TestInitializeComponentAndBinding()
		{
			using (var form = GetFormToBash() as CustomsAndExciseReportsForm)
			{
				var reportTypeZDropEdit = form.GetControl<ZDropEdit>("ReportTypeZDropEdit");
				AssertNotNull(reportTypeZDropEdit);
				AssertEquals(reportTypeZDropEdit.BindTo, "EM_MessageType");
				var messageNumberZTextBox = form.GetControl<ZTextBox>("MessageNumberZTextBox");
				AssertNotNull(messageNumberZTextBox);
				AssertEquals(messageNumberZTextBox.BindTo, "EM_MessageNum");
				var dateZDateEdit = form.GetControl<ZDateEdit>("DateZDateEdit");
				AssertNotNull(dateZDateEdit);
				AssertEquals(dateZDateEdit.BindTo, "MessageDate");
				var messageStatusTextBox = form.GetControl<ZTextBox>("MessageStatusTextBox");
				AssertNotNull(messageStatusTextBox);
				AssertEquals(messageStatusTextBox.BindTo, "EM_Status");
			}
		}

		public void TestMessagesTabPage()
		{
			using (var form = GetFormToBash() as CustomsAndExciseReportsForm)
			{
				var mainTabControl = form.Controls.Find("MainTabControl", true).OfType<ZTemplateTabControl>().Single();
				var messagesTabPage = mainTabControl.FindSingle<ZTabPage>("MessagesTabPage");
				var messagesUserControl = messagesTabPage.FindSingle<ZDynamicControlCreationUserControl>("MessagesUserControl");
				CombineAssertions("MessagesTabPage properties", () =>
				{
					AssertEquals("MessagesTabPage should be visible.", true, messagesTabPage.TabVisible);
					AssertEquals("MessagesTabPage caption should be 'Message's.", "Messages", messagesTabPage.CaptionResourceString.Caption);
					AssertNotNull("MessagesUserControl should contain an user control of type MessagesUserControl.", messagesUserControl);
					AssertEquals("MessagesUserControl is correct type", typeof(EU.GUI.MessagesTabUserControl), messagesUserControl.UserControlType);
				});
			}
		}

		public void TestSaveButton()
		{
			var reportMessage = Factory.New<CustomsAndExciseReportOutboundMessage>();
			InterchangeProcessorTestHelper.CreateValidCredential(reportMessage.Company);
			Factory.Save();

			using (var form = new CustomsAndExciseReportsForm(reportMessage))
			{
				form.Show();
				reportMessage.EM_MessageType = CustomsAndExciseReportTypeList.Codes.PSR;
				reportMessage.MessageDate = new CargoWise.Types.ZDateTime(2025, 1, 1);
				form.FireSaveButton();
				AssertEquals("Message queued for sending.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var reportMessage = Factory.New<CustomsAndExciseReportOutboundMessage>();
			var message = reportMessage.Messages.AddNew();
			message.FillWithValidTestData();
			Factory.Save();
			var form = new CustomsAndExciseReportsForm(reportMessage);
			return form;
		}
	}
}
