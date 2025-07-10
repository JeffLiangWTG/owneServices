namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class PesticideUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		public void TestComponentGroupBoxVisibility()
		{
			using (var ctr = new PesticideUserControl(true))
			{
				AssertEquals(false, ctr.ComponentGroupBox.IsDisposed);
				AssertEquals(true, ctr.ComponentGroupBox.Visible);
			}

			using (var ctr = new PesticideUserControl(false))
			{
				AssertEquals(true, ctr.ComponentGroupBox.IsDisposed);
			}
		}

		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.GTINNumberTextBox.Visible);
			Assert(!control.ExpiryDateEdit.Visible);
			Assert(!control.ModelNameTextBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new PesticideUserControl(true);
	}
}
