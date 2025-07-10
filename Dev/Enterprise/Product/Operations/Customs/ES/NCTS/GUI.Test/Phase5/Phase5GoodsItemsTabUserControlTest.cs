using CargoWise.Application;
using CargoWise.Application.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	public class Phase5GoodsItemsTabUserControlTest : TestCaseWithFactory
	{
		public void TestGoodsItemPreviousDocumentsTabPageVisibility()
		{
			nctsHeader.ESNctsHeader.CEN_TNNArrival = false;

			userControl.SetDataBinding(bill, "GoodsItems");
			form.Controls.Add(userControl);
			form.Show();

			var goodsItemPreviousDocumentsTabPage = userControl.GoodsItemTabControl.GetTabPage("GoodsItemPreviousDocumentsTabPage");

			CombineAssertions(() =>
			{
				AssertEquals("When is not TNN tab is visible", true, goodsItemPreviousDocumentsTabPage.TabVisible);

				nctsHeader.ESNctsHeader.CEN_TNNArrival = true;
				AssertEquals("When is TNN tab is not visible", false, goodsItemPreviousDocumentsTabPage.TabVisible);

				nctsHeader.ESNctsHeader.CEN_TNNArrival = false;
				AssertEquals("Visibility changes dynamically when we set not TNN Phase again", true, goodsItemPreviousDocumentsTabPage.TabVisible);
			});
		}

		[RequiresSTA]
		public void TestGoodsItemPackagesAndContainersTabPageVisibility()
		{
			using (ObjectFactory.Substitute("NctsPhase5LayoutProviders", GetNCTSPhase5LayoutProviders()))
			{
				userControl.SetDataBinding(bill, "GoodsItems");
				form.Controls.Add(userControl);
				form.Show();

				var goodsItemPackagesAndContainersTabPage = userControl.GoodsItemTabControl.GetTabPage("GoodsItemPackagesAndContainersTabPage");

				CombineAssertions(() =>
				{
					AssertEquals("Before changing goodsItem.IsVehicles is false by default, tab is visible", true, goodsItemPackagesAndContainersTabPage.TabVisible);

					goodsItem.IsVehicles = ZBool.True;
					AssertEquals("After changing goodsItem.IsVehicles to true tab is not visible", true, goodsItemPackagesAndContainersTabPage.TabVisible);

					goodsItem.IsVehicles = ZBool.False;
					AssertEquals("After changing goodsItem.IsVehicles to false tab is visible", true, goodsItemPackagesAndContainersTabPage.TabVisible);
				});
			}
		}

		KeyObjectHandleDictionaryObject GetNCTSPhase5LayoutProviders() => new KeyObjectHandleDictionaryObject { { "Default", new TestObjectHandle(new NctsPhase5LayoutProvider()) } };

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			bill = nctsHeader.Bills.AddNew();
			goodsItem = bill.GoodsItems.AddNew();

			form = new ZForm(nctsHeader);
			userControl = new Phase5GoodsItemsTabUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
			userControl.Dispose();
		}
		NctsHeader nctsHeader;
		NctsBill bill;
		NctsDepartureCargoDesc goodsItem;

		ZForm form;
		Phase5GoodsItemsTabUserControl userControl;
	}
}
