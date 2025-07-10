using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class VehiclesUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ICusVehicleCollection<CusVehicle, JobComInvoiceLine>), control.BindingSource.DataSourceType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new VehiclesUserControl();
		}
		VehiclesUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
