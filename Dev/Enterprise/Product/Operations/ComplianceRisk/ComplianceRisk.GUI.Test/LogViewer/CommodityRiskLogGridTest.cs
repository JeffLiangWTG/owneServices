using System;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class CommodityRiskLogGridTest : ComplianceRiskHelperTest
	{
		public void TestCommodityRiskGridColumnNames()
		{
			var expectedColumns = new[]
			{
				"RiskStatusDescription", "NomenclatureCondition", "SpecificCondition", "Code", "GoodsDescription", "OriginOfGoods", "Source", "CommoditySource", "DateAddedUtc"
			};

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var userControl = new ComplianceRiskLogViewerUserControl())
			{
				var commodityRiskGrid = userControl.Controls.Find("SnapshotCommoditiesGrid", searchAllChildren: true).Single() as ZGrid;
				form.Controls.Add(commodityRiskGrid);
				form.Show();

				AssertContainsExactElementsInAnyOrder(expectedColumns, commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(a => a.ColumnName));
				AssertEquals(false, commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(a => a.ColumnName == "NomenclatureCondition").IsVisible);
				AssertEquals(false, commodityRiskGrid.ColumnStyles.OfType<ZGridColumnInfo>().First(a => a.ColumnName == "SpecificCondition").IsVisible);
			}
		}
	}
}
