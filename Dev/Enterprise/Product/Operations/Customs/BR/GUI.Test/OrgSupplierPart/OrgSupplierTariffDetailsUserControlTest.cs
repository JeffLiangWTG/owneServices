using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class OrgSupplierTariffDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			using (var control = new OrgSupplierTariffDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<NveUserControl>("NveGridLayout must be", control.NveGridLayout);
					AssertType<TariffDetachUserControl>("TariffDetachLayout must be", control.TariffDetachLayout);
					AssertEquals("DataSourceType must be", typeof(CusClassPartPivot), control.DataSourceType);
				});
			}
		}
	}
}
