namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CellsTissuesAndOrgansUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.ManufactureDateEdit.Visible);
			Assert(!control.TradeNameTextBox.Visible);
			Assert(!control.ManufacturerUserControl.Visible);
			Assert(!control.BrandNameTextBox.Visible);
			Assert(!control.BatchLotNumberTextBox.Visible);
			Assert(!control.ModelNameTextBox.Visible);
			Assert(!control.ComponentGroupBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new CellsTissuesAndOrgansUserControl(true);
	}
}
