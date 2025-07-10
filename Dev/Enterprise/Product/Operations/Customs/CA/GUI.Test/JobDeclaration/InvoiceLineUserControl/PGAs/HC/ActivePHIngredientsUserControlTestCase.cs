using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ActivePHIngredientsUserControlTestCase : HCProgramsBasedUserUserControlTestCase
	{
		public void TestComponentGroupBoxVisibility()
		{
			using (var ctr = new ActivePHIngredientsUserControl(true))
			{
				AssertEquals(false, ctr.ComponentGroupBox.IsDisposed);
				AssertEquals(true, ctr.ComponentGroupBox.Visible);
			}

			using (var ctr = new ActivePHIngredientsUserControl(false))
			{
				AssertEquals(true, ctr.ComponentGroupBox.IsDisposed);
			}
		}

		public new void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new ActivePHIngredientsUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		protected override void AssertControlsVisibility(HCProgramsBasedUserUserControl control)
		{
			Assert(!control.ExpiryDateEdit.Visible);
			Assert(!control.ManufacturerUserControl.Visible);
			Assert(!control.TradeNameTextBox.Visible);
			Assert(!control.ModelNameTextBox.Visible);
			Assert(!control.ExceptProcessing1CheckBox.Visible);
		}

		protected override HCProgramsBasedUserUserControl GetUserControl() => new ActivePHIngredientsUserControl(true);
	}
}
