namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class BloodComponentUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.ManufactureDateEdit.Visible);
			Assert(!control.BrandNameTextBox.Visible);
			Assert(!control.BatchLotNumberTextBox.Visible);
			Assert(!control.ComponentGroupBox.Visible);
			Assert(!control.ManufacturerUserControl.Visible);
			Assert(!control.TradeNameTextBox.Visible);
			Assert(!control.ModelNameTextBox.Visible);
			Assert(!control.ExceptProcessing1CheckBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new BloodComponentUserControl(true);
	}
}
