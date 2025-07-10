using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class TTPChildUserControlTestCase : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new TTPProgramUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		public void TestAutoScroll()
		{
			using (var filterControl = new TTPProgramUserControl(false))
			{
				filterControl.Show();
				Assert(filterControl.AutoScroll);
				AssertEquals(295, filterControl.AutoScrollMinSize.Height);
				AssertEquals(1080, filterControl.AutoScrollMinSize.Width);
			}
		}
	}
}
