using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class NctsGoodsItemsUserControlTest : TestCaseWithFactory
	{
		public void TestDynamicUserControlType()
		{
			using (var form = new ZForm())
			using (var control = new NctsGoodsItemsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.ItemPreviousDocumentsTabPage.Show();
				var previousDocumentsDynamicUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("NctsPreviousDocumentsDynamicUserControl");
				AssertEquals(typeof(NctsPreviousDocumentsUserControl), previousDocumentsDynamicUserControl.UserControlType);

				control.ItemContainersTabPage.Show();
				var goodsItemContainersUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("GoodsItemContainersDynamicUserControl");
				AssertEquals(typeof(NctsGoodsItemContainersUserControl), goodsItemContainersUserControl.UserControlType);
			}
		}
	}
	class NctsGoodsItemsUserControlForTest : NctsGoodsItemsUserControl
	{
		public NctsGoodsItemsUserControlForTest() : base()
		{
		}

		public new System.Windows.Forms.TabPage ItemPreviousDocumentsTabPage => base.ItemPreviousDocumentsTabPage;

		public new System.Windows.Forms.TabPage ItemContainersTabPage => base.ItemContainersTabPage;
	}
}
