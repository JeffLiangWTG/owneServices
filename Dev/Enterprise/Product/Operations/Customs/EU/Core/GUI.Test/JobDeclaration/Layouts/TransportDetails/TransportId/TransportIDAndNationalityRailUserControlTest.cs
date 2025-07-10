using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class TransportIDAndNationalityRailControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTransportIDTextBox()
		{
			var transportIDTextBox = control.TransportIDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", ControlDpiScalingHelper.NewScaledPoint(0, 0, true), transportIDTextBox.Location);
				AssertEquals("Tab", 0, transportIDTextBox.TabIndex);
				AssertEquals("Caption", "Transport ID", transportIDTextBox.CaptionResourceString.Caption);
				AssertEquals("Short Caption", "Trans. ID", transportIDTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Full Description", "[UCC 7/9] Transport ID", transportIDTextBox.CaptionResourceString.FullDescription);
				AssertEquals("Binding", "JE_VesselName", transportIDTextBox.BindTo);
			});
		}

		public void TestTransportNationalityFindBox()
		{
			var transportNationalityFindBox = control.TransportNationalityFindBox;
			CombineAssertions("Checks for TransportNationalityFindBox", () =>
			{
				AssertEquals("Location", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), transportNationalityFindBox.Location);
				AssertEquals("Tab", 1, transportNationalityFindBox.TabIndex);
				AssertEquals("Caption", "Nationality", transportNationalityFindBox.CaptionResourceString.Caption);
				AssertEquals("Short Caption", "Nat.", transportNationalityFindBox.CaptionResourceString.ShortCaption);
				AssertEquals("Full Description", "[UCC 7/15] Nationality", transportNationalityFindBox.CaptionResourceString.FullDescription);
				AssertEquals("Binding", "JE_RN_NKTransportNationality", transportNationalityFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportIDAndNationalityRailUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportIDAndNationalityRailUserControl control;
	}
}
