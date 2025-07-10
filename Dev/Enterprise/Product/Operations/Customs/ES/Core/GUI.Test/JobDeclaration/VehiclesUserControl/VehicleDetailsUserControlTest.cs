using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class VehicleDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusVehicle), control.BindingSource.DataSourceType);
		}

		public void TestControls()
		{
			using (var control = new VehicleDetailsUserControl())
			{
				AssertEquals("VinTextBox.BindTo", "CVH_VehicleIdentificationNumber", control.VinTextBox.BindTo);
				AssertEquals("BrandTextBox.BindTo", "CVH_BrandName", control.BrandTextBox.BindTo);
				AssertEquals("ModelTextBox.BindTo", "CVH_ModelName", control.ModelTextBox.BindTo);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new VehicleDetailsUserControl();
		}
		VehicleDetailsUserControl control;

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
