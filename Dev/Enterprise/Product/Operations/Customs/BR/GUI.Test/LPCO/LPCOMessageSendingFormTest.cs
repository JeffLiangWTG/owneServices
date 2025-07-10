using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.BR.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	[TestedType(typeof(LPCOMessageSendingForm))]
	public class LPCOMessageSendingFormTest : ZFormBasherTest
	{
		public void TestProperties()
		{
			using (var form = (LPCOMessageSendingForm)GetFormToBashCore())
			{
				form.Show();
				var grid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");

				CombineAssertions(() =>
				{
					AssertNotNull("MessageSendingObjectsGrid should be null", grid);
					AssertType<ZGroupBox>("CredentialsGroupBox type should be", form.CredentialsGroupBox);
					AssertEquals("CredentialsGroupBox Caption should be", "Credentials", form.CredentialsGroupBox.CaptionResourceString.Caption);
					AssertType<ZCodeFindBox>("BrokerCodeFindBox type should be", form.BrokerCodeFindBox);
					AssertEquals("BrokerCodeFindBox Caption should be", "Broker", form.BrokerCodeFindBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestCheckIsOKToSendXtTest()
		{
			var header = Factory.New<CusLPCOHeader>();
			var lpcoObject = new LPCOMessageSendingObjectParent(header);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			using (var form = new LPCOMessageSendingForm(lpcoObject))
			{
				form.Show();

				var sendButton = form.FindSingle<ZButton>("SendButton");
				Assert("SendButton is disable", !sendButton.Enabled);

				form.MessageSendingObjectParent.SendingObjectsCollection.Cast<LPCOMessageSendingObject>().ForEach(x => x.ShouldSend = true);
				var sendWithWarning = form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox");
				sendWithWarning.Checked = true;

				var sendWithError = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWithError.Checked = true;

				Assert("SendButton is enable", sendButton.Enabled);

				sendButton.PerformClick();

				AssertEquals("The selected messages will be sent to a test environment!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckIsOKToSendXtProduction()
		{
			var header = Factory.New<CusLPCOHeader>();
			var lpcoObject = new LPCOMessageSendingObjectParent(header);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			using (var form = new LPCOMessageSendingForm(lpcoObject))
			{
				form.Show();

				var sendButton = form.FindSingle<ZButton>("SendButton");
				Assert("SendButton is disable", !sendButton.Enabled);

				form.MessageSendingObjectParent.SendingObjectsCollection.Cast<LPCOMessageSendingObject>().ForEach(x => x.ShouldSend = true);
				var sendWith = form.FindSingle<ZCheckBox>("SendWithAdditionalWarningCheckBox");
				sendWith.Checked = true;

				var sendWith2 = form.FindSingle<ZCheckBox>("SendWithValidationErrorsCheckBox");
				sendWith2.Checked = true;

				Assert("SendButton is enable", sendButton.Enabled);

				sendButton.PerformClick();

				Assert(!UnitTestUserNotification.Instance.PreviousMessages.Select(x => x.Text).Contains("The selected messages will be sent to a test environment!"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusLPCOHeader>();
			var sendingObjectParent = new LPCOMessageSendingObjectParent(header);
			return new LPCOMessageSendingForm(sendingObjectParent);
		}
	}
}
