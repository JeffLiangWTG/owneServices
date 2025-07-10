using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	[TestedType(typeof(GUI.GoodsItemPackagesUserControl))]
	sealed class GoodsItemPackagesUserControlTest : TestCaseWithFactory
	{
		public void TestPackagesGrid_ColumnsVisibility()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			using (var form = new Phase5DepartureMovementForm(nctsHeader))
			{
				form.Show();

				var mainTabControl = form.FindSingle<ZTemplateTabControl>("MainTabControl");
				mainTabControl.SelectTab(form.HouseConsignmentsTabPage);

				var houseConsignmentTabControl = (TabControl)form.Controls.Find("HouseConsignmentTabControl", true).FirstOrDefault();
				var goodsItemsTabPage = (ZTabPage)houseConsignmentTabControl.Controls.Find("GoodsItemsTabPage", true).FirstOrDefault();
				houseConsignmentTabControl.SelectTab(goodsItemsTabPage);

				var phase5GoodsItemsTabUserControl = houseConsignmentTabControl.Controls.Find("Phase5GoodsItemsTabUserControl", true).FirstOrDefault();
				var goodsItemTabControl = (TabControl)phase5GoodsItemsTabUserControl.Controls.Find("GoodsItemTabControl", true).FirstOrDefault();
				var goodsItemPackagesAndContainersTabPage = (ZTabPage)phase5GoodsItemsTabUserControl.Controls.Find("GoodsItemPackagesAndContainersTabPage", true).FirstOrDefault();
				goodsItemTabControl.SelectTab(goodsItemPackagesAndContainersTabPage);

				var goodsItemPackagesAndContainersDynamicCreationUserControl = goodsItemTabControl.Controls.Find("GoodsItemPackagesAndContainersDynamicCreationUserControl", true).FirstOrDefault();
				var packagesUserControl = goodsItemPackagesAndContainersDynamicCreationUserControl.Controls.Find("DynamicPackagesUserControl", true).FirstOrDefault();
				var packagesGrid = packagesUserControl.Controls.Find("PackagesGrid", true).FirstOrDefault();

				CombineAssertions(() =>
				{
					goodsItem.IsVehicles = true;
					AssertColumnsVisibility((ZGrid)packagesGrid, true);

					goodsItem.IsVehicles = false;
					AssertColumnsVisibility((ZGrid)packagesGrid, false);
				});
			}

			void AssertColumnsVisibility(ZGrid packagesGrid, bool vehiclesColumnsShouldBeVisible)
			{
				AssertEquals($"B5_PackageID should be visible {vehiclesColumnsShouldBeVisible}", vehiclesColumnsShouldBeVisible, packagesGrid.Columns[NctsPackage.Schema.B5_PackageID].IsVisible);
				AssertEquals($"B5_Brand should be visible {vehiclesColumnsShouldBeVisible}", vehiclesColumnsShouldBeVisible, packagesGrid.Columns[NctsPackage.Schema.B5_Brand].IsVisible);
				AssertEquals($"B5_Model should be visible {vehiclesColumnsShouldBeVisible}", vehiclesColumnsShouldBeVisible, packagesGrid.Columns[NctsPackage.Schema.B5_Model].IsVisible);

				AssertEquals($"B5_UnitType should be visible {!vehiclesColumnsShouldBeVisible}", !vehiclesColumnsShouldBeVisible, packagesGrid.Columns[NctsPackage.Schema.B5_UnitType].IsVisible);
				AssertEquals($"B5_UnitCount should be visible {!vehiclesColumnsShouldBeVisible}", !vehiclesColumnsShouldBeVisible, packagesGrid.Columns[NctsPackage.Schema.B5_UnitCount].IsVisible);
				AssertEquals($"B5_MarksAndNumbers {!vehiclesColumnsShouldBeVisible}", !vehiclesColumnsShouldBeVisible, packagesGrid.Columns[NctsPackage.Schema.B5_MarksAndNumbers].IsVisible);
			}
		}
	}
}
