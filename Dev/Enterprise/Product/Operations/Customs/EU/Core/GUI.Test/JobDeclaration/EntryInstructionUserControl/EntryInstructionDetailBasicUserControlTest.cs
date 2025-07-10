using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class EntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
	{
		public void TestToWarehouseCodeTextBox()
		{
			AssertType<ZArchitecture.ZTextBox>(control.ToWarehouseCodeTextBox);
		}

		public void TestFromWarehouseCodeTextBox()
		{
			AssertType<ZArchitecture.ZTextBox>(control.FromWarehouseCodeTextBox);
		}

		public void TestStyleDropEdit()
		{
			AssertType<ZDropEditWithFixedWidth>(control.StyleDropEdit);
		}

		public void TestToWarehouseAddressControl()
		{
			AssertType<ZAddressControl>(control.ToWarehouseAddressControl);
		}

		public void TestFromWarehouseAddressControl()
		{
			AssertType<ZAddressControl>(control.FromWarehouseAddressControl);
		}

		EntryInstructionDetailBasicUserControl control;
		protected override void SetUp()
		{
			base.SetUp();
			control = new EntryInstructionDetailBasicUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
	}
}
