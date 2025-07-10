using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	[TestedType(typeof(JPAFRHeaderLevelMessageForm))]
	class JPAFRHeaderLevelMessageFormTest : ZFormBasherTest
	{
		public void TestATDFormChange()
		{
			var testHeader = Factory.NewWithValidTestData<JPAFRHeader>();
			var testMessageAction = new MessageSendingAction(testHeader, ActionCode.RegisterDepartureTime);
			using (var testForm = new JPAFRHeaderLevelMessageForm(testMessageAction))
			{
				AssertEquals("Do you want to register the Departure Time?", testForm.CaptionResourceString.Caption);
				AssertEquals(MessageLabelCaptionForATD, testForm.Controls.Find("MessageLabel", true).Cast<ZLabel>().FirstOrDefault().CaptionResourceString.Caption);
				AssertEquals("Send ATD Update message", testForm.Controls.Find("HasATDBeenSentCheckBox", true).Cast<ZCheckBox>().FirstOrDefault().CaptionResourceString.Caption);
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(600), testForm.Width);
				AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(400), testForm.Height);

				testMessageAction.HasATDBeenSent = true;
				AssertEquals(ActionCode.ChangeDepartureTimeAfterATD, testMessageAction.ActionCode);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<JPAFRHeader>();
			var sendingAction = new MessageSendingAction(header, ActionCode.RegisterCompletionByRegistration);
			return new JPAFRHeaderLevelMessageForm(sendingAction);
		}

		const string MessageLabelCaptionForATD = @"Do you want to register the Departure Time (ATD) for the manifest?

Once the Departure Time is successfully registered the departure time can be amended.

1. If you do need to change the Vessel Information* after a successful lodgement of Departure Time Registration, you will need to submit an amendment (‘Amend Manifest’ menu item), after which a new Departure Time Registration will need to be sent.

2. Note that once Departure Time is registered, bill details can only be amended on receipt of a customs assessment notice or by re-manifesting the bills onto a new manifest (vessel information* should change).

*(Carrier Code, Vessel Call Sign, Voyage Number, Port of Loading and Suffix)

Please ensure that all data is correct before proceeding.


If you have already registered the Departure Time (ATD) but accidentally canceled the 'Departure Time Registration' event, please tick the check box 'Send ATD update message' and send again.";
	}
}
