using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class FlightAndNationalityUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTransportNationalityFindBox()
		{
			var transportNationalityFindBox = control.TransportNationalityFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), transportNationalityFindBox.Location);
				AssertEquals("Tab", 1, transportNationalityFindBox.TabIndex);
				AssertEquals("Caption", "Nationality", transportNationalityFindBox.CaptionResourceString.Caption);
				AssertEquals("Short Caption", "Nat.", transportNationalityFindBox.CaptionResourceString.ShortCaption);
				AssertEquals("Full Description", "[UCC 7/15] Nationality", transportNationalityFindBox.CaptionResourceString.FullDescription);
				AssertEquals("Binding", "JE_RN_NKTransportNationality", transportNationalityFindBox.BindTo);
			});
		}

		public void TestFlightNoTextBox()
		{
			var flightNumberTextBox = control.FlightNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), flightNumberTextBox.Location);
				AssertEquals("Tab", 0, flightNumberTextBox.TabIndex);
				AssertEquals("Caption", "[UCC 7/9] Flight No.", flightNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("Short Caption", "Flight", flightNumberTextBox.CaptionResourceString.ShortCaption);
				AssertEquals("Binding", "JE_VoyageFlightNo", flightNumberTextBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new FlightAndNationalityUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		FlightAndNationalityUserControl control;
	}
}
