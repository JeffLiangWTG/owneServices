using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.ExitControl.Business;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class ConsignmentItemUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem>), userControl.BindingSource.DataSourceType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ConsignmentItemUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		ConsignmentItemUserControl userControl;
	}
}
