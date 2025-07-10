using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class GoodsItemPreviousDocumentsGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(Business.NctsPreviousDocumentCollection<Business.NctsPreviousDocument>), userControl.BindingSource.DataSourceType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new GoodsItemPreviousDocumentsGridUserControl();
		}

		GoodsItemPreviousDocumentsGridUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
