using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class ToWarehouseAddressControlTest : TestCaseWithFactory
	{
		public void TestToWarehouseAddressControl()
		{
			AssertType<ZAddressControl>(control.ToWarehouseAddressControl);
		}

		public void TestToWarehouseTextBox()
		{
			AssertType<ZTextBox>(control.ToWarehouseCodeTextBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ToWarehouseUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ToWarehouseUserControl control;
	}
}
