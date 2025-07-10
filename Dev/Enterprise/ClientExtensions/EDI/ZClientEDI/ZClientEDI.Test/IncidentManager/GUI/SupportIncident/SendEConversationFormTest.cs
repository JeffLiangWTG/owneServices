using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(SendEConversationForm))]
	public class SendEConversationFormTest : ZFormBasherTest
	{
		public void TestAction()
		{
			using (var form = new SendEConversationFormForTest("blah blah"))
			{
				AssertEquals("Default action", SendEConversationForm.PerformAction.Discard, form.Action);
				form.Show();
				form.ClickButton1();
				AssertEquals(SendEConversationForm.PerformAction.Send, form.Action);
			}

			using (var form = new SendEConversationFormForTest("blah blah"))
			{
				form.Show();
				form.ClickButton2();
				AssertEquals(SendEConversationForm.PerformAction.AwaitingResponse, form.Action);
			}

			using (var form = new SendEConversationFormForTest("blah blah"))
			{
				form.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ClickButton3();
				AssertEquals(SendEConversationForm.PerformAction.Discard, form.Action);
			}

			using (var form = new SendEConversationFormForTest("blah blah"))
			{
				form.Show();
				form.ClickCancel();
				AssertEquals(SendEConversationForm.PerformAction.Cancel, form.Action);
			}
		}
		public void TestShouldShowAwaitingResponse()
		{
			using (var form = new SendEConversationFormForTest("blah blah", shouldShowAwaitingResponse: true))
			{
				form.Show();
				var awaitingResponseButton = form.Find(x => x.Name == "AwaitingResponseButton").Cast<ZButton>().Single();
				AssertEquals(true, awaitingResponseButton.Visible);
				var label = form.Find(x => x.Name == "Label2").Cast<ZLabel>().Single();
				AssertEquals("Please click \"Send\" or \"Awaiting Response\" to send pending message. If you choose \"Discard\", the message will be lost.", label.Text);
			}

			using (var form = new SendEConversationFormForTest("blah blah", shouldShowAwaitingResponse: false))
			{
				form.Show();
				var awaitingResponseButton = form.Find(x => x.Name == "AwaitingResponseButton").Cast<ZButton>().Single();
				AssertEquals(false, awaitingResponseButton.Visible);
				var label = form.Find(x => x.Name == "Label2").Cast<ZLabel>().Single();
				AssertEquals("Please click \"Send\" to send pending message. If you choose \"Discard\", the message will be lost.", label.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new SendEConversationForm("Hello world!", false);
		}

		class SendEConversationFormForTest : SendEConversationForm
		{
			internal SendEConversationFormForTest(string message, bool shouldShowAwaitingResponse = true) : base(message, shouldShowAwaitingResponse)
			{
			}

			public void ClickButton1()
			{
				SendButton.PerformClick();
			}

			public void ClickButton2()
			{
				AwaitingResponseButton.PerformClick();
			}

			public void ClickButton3()
			{
				DiscardButton.PerformClick();
			}

			public void ClickCancel()
			{
				CancelButton.PerformClick();
			}
		}
	}
}
