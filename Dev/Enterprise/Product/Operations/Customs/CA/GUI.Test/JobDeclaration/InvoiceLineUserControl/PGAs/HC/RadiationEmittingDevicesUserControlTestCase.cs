namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class RadiationEmittingDevicesUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.ExpiryDateEdit.Visible);
			Assert(!control.ManufacturerUserControl.Visible);
			Assert(!control.TradeNameTextBox.Visible);
			Assert(!control.IntendedUseCodeDropEdit.Visible);
			Assert(!control.GTINNumberTextBox.Visible);
			Assert(!control.ManufactureDateEdit.Visible);
			Assert(!control.BrandNameTextBox.Visible);
			Assert(!control.BatchLotNumberTextBox.Visible);
			Assert(!control.ComponentGroupBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new RadiationEmittingDevicesUserControl(true);
	}
}
