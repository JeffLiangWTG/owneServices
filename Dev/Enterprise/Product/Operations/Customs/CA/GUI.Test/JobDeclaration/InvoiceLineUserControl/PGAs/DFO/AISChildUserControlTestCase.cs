using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class AISChildUserControlTestCase : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new AISProgramUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		public void TestAutoScroll()
		{
			using (var filterControl = new AISProgramUserControl(false))
			{
				filterControl.Show();
				Assert(filterControl.AutoScroll);
				AssertEquals(260, filterControl.AutoScrollMinSize.Height);
				AssertEquals(1080, filterControl.AutoScrollMinSize.Width);
			}
		}
	}
}
