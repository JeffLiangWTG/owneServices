using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class ShipmentDetailsUserControlTest : TestCase
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestObjectsTypesForComponentsOnUserControl()
		{
			AssertType<ZDropEdit>("GoodsOriginDropEdit should be ZDropEdit", control.GoodsOriginDropEdit);
			AssertType<ZDropEdit>("GoodsDestinationDropEdit should be ZDropEdit", control.GoodsDestinationDropEdit);
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
