using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class SecurityAtDepartureUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new SecurityAtDepartureUserControl())
			{
				AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new SecurityAtDepartureUserControl())
			{
				AssertEquals("PlaceOfLoadingCodeFindBox.BindTo", "MovementHeader.BM_PlaceOfLoading", control.PlaceOfLoadingCodeFindBox.BindTo);
				AssertEquals("PlaceOfUnloadingCodeFindBox.BindTo", "MovementHeader.BM_PlaceOfUnloading", control.PlaceOfUnloadingCodeFindBox.BindTo);
			}
		}
	}
}
