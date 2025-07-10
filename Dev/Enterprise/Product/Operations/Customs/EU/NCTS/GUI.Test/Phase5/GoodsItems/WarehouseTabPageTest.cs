using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class WarehouseTabPageTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			AssertEquals("Warehouse", warehouseTabPage.Caption.Caption);
		}

		public void TestUserControlBindingMember()
		{
			AssertEquals(".", warehouseTabPage.UserControlBindingMember);
		}

		public void TestCreateUserControl()
		{
			using (var userControl = warehouseTabPage.CreateUserControl())
			{
				AssertType<Phase5GoodsItemWarehouseTabUserControl>(userControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			warehouseTabPage = new WarehouseTabPage();
		}
		WarehouseTabPage warehouseTabPage;
	}
}
