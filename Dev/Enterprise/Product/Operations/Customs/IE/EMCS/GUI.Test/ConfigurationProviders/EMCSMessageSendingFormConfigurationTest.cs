using System.Windows.Forms;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.GUI.Testing;
using Enterprise.Customs.IE.GUI;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.GUI.Testing
{
	[TestedType(typeof(EMCSMessageSendingFormConfiguration))]
	sealed class EMCSMessageSendingFormConfigurationTest : EMCSMessageSendingFormConfigurationAbstractTest<EMCSMessageSendingFormConfiguration>
	{
		public override void TestIsOKToSend()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			var sendingParent = new MinimalSendingActionParent(declaration);
			AssertEquals("Default", true, Provider.IsOKToSend(sendingParent));

			declaration.JE_MessageStatus = EDIMessageStatusList.Codes.Sent;
			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
			{
				if (shownForm is ConfirmSendForm confirmSendForm)
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					var cancelButton = confirmSendForm.FindSingle<ZButton>("ZCancelButton");
					cancelButton.PerformClick();
				}
			});
			AssertEquals("Click Cancel", false, Provider.IsOKToSend(sendingParent));

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallOnFormShown(shownForm =>
			{
				if (shownForm is ConfirmSendForm confirmSendForm)
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					var reasonTextBox = confirmSendForm.FindSingle<ZTextBox>("ReasonTextBox");
					reasonTextBox.Text = "Reason for resending";
					var okButton = confirmSendForm.FindSingle<ZButton>("OKButton");
					okButton.PerformClick();
				}
			});
			CombineAssertions("Set reason and click OK", () =>
			{
				AssertEquals("result", true, Provider.IsOKToSend(sendingParent));
				AssertContains("log", "Reason for resending", declaration.Logs.MostRecentLogByPostedDate.SL_Reference);
			});
		}

		protected override string CountryOrGroupingCode => Core.Constants.CountryCodes.Ireland;
	}
}
