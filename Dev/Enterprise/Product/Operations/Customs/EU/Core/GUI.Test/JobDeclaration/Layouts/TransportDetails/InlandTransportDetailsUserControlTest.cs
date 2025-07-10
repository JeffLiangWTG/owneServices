using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class InlandTransportDetailsUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestTransportIDTextBox()
		{
			var textBox = control.TransportIDTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), textBox.Location);
				AssertEquals("First", 0, textBox.TabIndex);
			});
		}

		public void TestTransportNationalityCodeFindBox()
		{
			var findBox = control.TransportNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), findBox.Location);
				AssertEquals("Second", 1, findBox.TabIndex);
				AssertEquals("Module ID", ZArchitecture.Modules.ModuleIDs.RefCountry, findBox.ModuleID);
				AssertEquals("Pre Bound Max Length", 2, findBox.PreBoundMaxLength);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InlandTransportDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		InlandTransportDetailsUserControl control;
	}
}
