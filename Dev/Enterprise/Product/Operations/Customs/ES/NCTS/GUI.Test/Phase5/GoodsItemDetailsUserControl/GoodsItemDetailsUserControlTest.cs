using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class GoodsItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestIsVehiclesCheckBox()
		{
			AssertType<ZCheckBox>(control.IsVehiclesCheckBox);
		}

		public void TestExciseCodeDropEdit()
		{
			AssertType<ZDropEdit>(control.ExciseCodeDropEdit);
		}

		public void TestPVPValueCalcDropEdit()
		{
			AssertType<ZCalcDropEdit>(control.PVPValueCalcDropEdit);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GoodsItemDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		GoodsItemDetailsUserControl control;
	}
}
