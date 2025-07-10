using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class NctsGoodsItemsUserControlTest : TestCaseWithFactory
	{
		public void TestInitTabsVisibility()
		{
			CombineAssertions(() =>
			{
				AssertEquals("SupportingDocumentsTabPage", true, control.FindSingle<ZTabPage>("SupportingDocumentsTabPage").TabVisible);
				AssertEquals("ItemAdditionalInfosTabPage", true, control.FindSingle<ZTabPage>("ItemAdditionalInfosTabPage").TabVisible);
				AssertEquals("ItemPreviousDocumentsTabPage", true, control.FindSingle<ZTabPage>("ItemPreviousDocumentsTabPage").TabVisible);
			});
		}

		public void TestSupportingDocumentsUserControl()
		{
			AssertDynamicControlType<SupportingDocumentsUserControl>("SupportingDocumentsDynamicUserControl");
		}

		public void TestPreviousDocumentsUserControl()
		{
			AssertDynamicControlType<PreviousDocumentsUserControl>("NctsPreviousDocumentsDynamicUserControl");
		}

		public void TestPackagesUserControl()
		{
			AssertDynamicControlType<ItemPackagesTabUserControl>("NctsPackagesDynamicUserControl");
		}

		public void TestItemSecurityTabUserControl()
		{
			AssertDynamicControlType<ItemSecurityTabUserControl>("ItemSecurityDynamicUserControl");
		}

		public void TestAdditionalInfosUserControl()
		{
			AssertDynamicControlType<AdditionalInfosUserControl>("AdditionalInfosDynamicUserControl");
		}

		public void TestItemDetailsUserControl()
		{
			AssertDynamicControlType<ItemDetailsUserControl>("GoodsItemLineDetailDynamicUserControl");
		}

		public void TestGoodsItemsGridUserControl()
		{
			AssertDynamicControlType<GoodsItemsGridUserControl>("GoodsItemsGridDynamicUserControl");
		}

		public void TestGoodsItemContainersUserControl()
		{
			AssertDynamicControlType<GoodsItemContainersUserControl>("GoodsItemContainersDynamicUserControl");
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new NctsGoodsItemsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		void AssertDynamicControlType<T>(string controlName) where T : class
		{
			var dynamicUserControl = control.FindSingle<ZDynamicControlCreationUserControl>(controlName);
			AssertEquals(typeof(T), dynamicUserControl.UserControlType);
		}

		NctsGoodsItemsUserControl control;
	}
}
