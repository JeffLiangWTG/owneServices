namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class OfficeOfControlledSubstancesUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		public void TestComponentGroupBoxVisibility()
		{
			using (var ctr = new OfficeOfControlledSubstancesUserControl(true))
			{
				AssertEquals(false, ctr.ComponentGroupBox.IsDisposed);
				AssertEquals(true, ctr.ComponentGroupBox.Visible);
			}

			using (var ctr = new OfficeOfControlledSubstancesUserControl(false))
			{
				AssertEquals(true, ctr.ComponentGroupBox.IsDisposed);
			}
		}

		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.GTINNumberTextBox.Visible);
			Assert(!control.ManufactureDateEdit.Visible);
			Assert(!control.ExpiryDateEdit.Visible);
			Assert(!control.TradeNameTextBox.Visible);
			Assert(!control.ModelNameTextBox.Visible);
			Assert(!control.ExceptProcessing1CheckBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new OfficeOfControlledSubstancesUserControl(true);
	}
}
