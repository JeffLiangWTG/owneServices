using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class TCVehicleBasedUserControlTest : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new TCVehicleBasedUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOsGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKAuthorizationCountry).IsUnavailable);
				Assert(!filterControl.LPCOsGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!filterControl.LPCOsGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOsGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOsGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}
	}
}
