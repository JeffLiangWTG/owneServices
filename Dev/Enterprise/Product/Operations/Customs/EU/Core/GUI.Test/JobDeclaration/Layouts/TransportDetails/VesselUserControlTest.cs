using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	internal class VesselUserControlTest : TestCase
	{
		public void TestVesselName()
		{
			var vesselCodeFindBox = control.VesselCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), vesselCodeFindBox.Location);
				AssertEquals("Tab", 0, vesselCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_VesselName", vesselCodeFindBox.BindTo);
				var resString = vesselCodeFindBox.CaptionResourceString;
				AssertEquals("VesselCodeFindBox.Caption", "Vessel", resString.Caption);
				AssertEquals("VesselCodeFindBox.FullDescription", "Vessel Name", resString.FullDescription);
			});
		}

		public void TestLloydsIMO()
		{
			var lloydsIMOTextBox = control.LloydsIMOTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 0, true), lloydsIMOTextBox.Location);
				AssertEquals("Tab", 1, lloydsIMOTextBox.TabIndex);
				AssertEquals("Binding", "JE_LloydsIMO", lloydsIMOTextBox.BindTo);
				var resString = lloydsIMOTextBox.CaptionResourceString;
				AssertEquals("LloydsIMOTextBox.Caption", "Lloyds", resString.Caption);
				AssertEquals("LloydsIMOTextBox.FullDescription", "[19 08 017 000] Lloyds / IMO Number", resString.FullDescription);
			});
		}

		VesselUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new VesselUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
