using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class TransportInlandRailUserControlTest : TestCaseWithFactory
	{
		public void TestTrainNumberTextBox()
		{
			var trainNumberTextBox = control.TrainNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), trainNumberTextBox.Location);
				AssertEquals("Tab", 0, trainNumberTextBox.TabIndex);
				AssertEquals("Caption", "Train No.", trainNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("Binding", "ZG_Box18TransportID", trainNumberTextBox.BindTo);
			});
		}

		public void TestTrainNationalityCodeFindBox()
		{
			var trainNationalityCodeFindBox = control.TrainNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 0, true), trainNationalityCodeFindBox.Location);
				AssertEquals("Tab", 1, trainNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "ZG_Box18TransportNationality", trainNationalityCodeFindBox.BindTo);
			});
		}

		public void TestWagonNumberTextBox()
		{
			var wagonNumberTextBox = control.WagonNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 0, true), wagonNumberTextBox.Location);
				AssertEquals("Tab", 2, wagonNumberTextBox.TabIndex);
				AssertEquals("Caption", "Wagon No.", wagonNumberTextBox.CaptionResourceString.Caption);
				AssertEquals("Binding", "JE_Trailer1RegNo", wagonNumberTextBox.BindTo);
			});
		}

		public void TestWagonNationalityCodeFindBox()
		{
			var wagonNationalityCodeFindBox = control.WagonNationalityCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true), wagonNationalityCodeFindBox.Location);
				AssertEquals("Tab", 3, wagonNationalityCodeFindBox.TabIndex);
				AssertEquals("Binding", "JE_RN_NKTrailer1Nationality", wagonNationalityCodeFindBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportInlandRailUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportInlandRailUserControl control;
	}
}
