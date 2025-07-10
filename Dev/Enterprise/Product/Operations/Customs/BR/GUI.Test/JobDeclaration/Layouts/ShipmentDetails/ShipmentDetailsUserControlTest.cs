using Enterprise.Customs.BR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ShipmentDetailsUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestObjectsTypesForComponentsOnUserControl()
		{
			AssertType<UcrAndBillTypeUserControl>("UcrAndBillTypeUserControl should be UcrAndBillTypeUserControl", control.UcrAndBillTypeUserControl);
			AssertType<CargoArrivalUserControl>("CargoArrivalUserControl should be CargoArrivalUserControl", control.CargoArrivalUserControl);
		}

		ShipmentDetailsUserControl control;

		protected override void SetUp()
		{
			base.SetUp();
			control = new ShipmentDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
