using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ECCCWasteUserControlTestCase : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new ECCCWasteUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}
	}
}
