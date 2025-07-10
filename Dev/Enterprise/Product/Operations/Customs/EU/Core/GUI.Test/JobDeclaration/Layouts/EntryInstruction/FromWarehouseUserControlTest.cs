using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class FromWarehouseUserControlTest : TestCaseWithFactory
	{
		public void TestFromWarehouseAddressControl()
		{
			AssertType<ZAddressControl>(control.FromWarehouseAddressControl);
		}

		public void TestFromWarehouseTextBox()
		{
			AssertType<ZTextBox>(control.FromWarehouseCodeTextBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new FromWarehouseUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		FromWarehouseUserControl control;
	}
}
