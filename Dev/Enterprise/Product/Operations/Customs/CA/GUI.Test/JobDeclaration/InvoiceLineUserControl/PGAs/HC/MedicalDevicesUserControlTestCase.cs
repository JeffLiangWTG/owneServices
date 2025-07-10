namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class MedicalDevicesUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.ExpiryDateEdit.Visible);
			Assert(!control.ManufacturerUserControl.Visible);
			Assert(!control.TradeNameTextBox.Visible);
			Assert(!control.ComponentGroupBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new MedicalDevicesUserControl(true);
	}
}
