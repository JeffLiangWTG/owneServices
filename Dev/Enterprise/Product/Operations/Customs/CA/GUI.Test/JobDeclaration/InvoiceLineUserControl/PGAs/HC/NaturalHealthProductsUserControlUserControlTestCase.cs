namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class NaturalHealthProductsUserControlUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.ExpiryDateEdit.Visible);
			Assert(!control.ManufacturerUserControl.Visible);
			Assert(!control.TradeNameTextBox.Visible);
			Assert(!control.ModelNameTextBox.Visible);
			Assert(!control.ExceptProcessing1CheckBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new NaturalHealthProductsUserControl(true);
	}
}
