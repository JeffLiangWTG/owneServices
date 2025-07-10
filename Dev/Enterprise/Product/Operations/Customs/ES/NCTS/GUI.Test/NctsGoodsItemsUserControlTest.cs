using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	public class NctsGoodsItemsUserControlTest : TestCaseWithFactory
	{
		public void TestPreviousDocumentsDynamicUserControlType()
		{
			using (var form = new ZForm())
			using (var control = new NctsGoodsItemsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.ItemPreviousDocumentsTabPage.Show();
				var previousDocumentsDynamicUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("NctsPreviousDocumentsDynamicUserControl");
				AssertEquals(typeof(NctsPreviousDocumentsUserControl), previousDocumentsDynamicUserControl.UserControlType);
			}
		}

		public void TestGoodsItemLineDetailDynamicUserControlType()
		{
			using (var form = new ZForm())
			using (var control = new NctsGoodsItemsUserControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				var itemDetail = control.FindSingle<ZDynamicControlCreationUserControl>("GoodsItemLineDetailDynamicUserControl");
				AssertEquals(typeof(ItemDetailsUserControl), itemDetail.UserControlType);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}

	class NctsGoodsItemsUserControlForTest : NctsGoodsItemsUserControl
	{
		public NctsGoodsItemsUserControlForTest() : base()
		{
		}

		public new TabPage ItemPreviousDocumentsTabPage => base.ItemPreviousDocumentsTabPage;
	}
}
