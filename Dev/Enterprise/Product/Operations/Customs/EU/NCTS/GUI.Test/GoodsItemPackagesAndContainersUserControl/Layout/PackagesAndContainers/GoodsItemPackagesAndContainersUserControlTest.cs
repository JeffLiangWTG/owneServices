using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class GoodsItemPackagesAndContainersUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestPackagesUserControl()
		{
			var packagesUserControl = control.PackagesUserControl;
			CombineAssertions(() =>
			{
				AssertType<GoodsItemPackagesUserControl>("Type", packagesUserControl);
				AssertEquals("BindingMember", "Packages", control.BindingSource.GetBindingMember(packagesUserControl));
			});
		}

		public void TestContainersUserControl()
		{
			var containersUserControl = control.ContainersUserControl;
			CombineAssertions(() =>
			{
				AssertType<GoodsItemContainersUserControl>("Type", containersUserControl);
				AssertEquals("BindingMember", ".", control.BindingSource.GetBindingMember(containersUserControl));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new GoodsItemPackagesAndContainersUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		GoodsItemPackagesAndContainersUserControl control;
	}
}
