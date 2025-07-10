using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class WarehouseAdjustmentInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestAdditionalInfoPanelLayout()
		{
			using (var control = new WarehouseAdjustmentInvoiceLineUserControlForTest())
			{
				var layout = control.GetAdditionalInfoPanelLayout_Exposed().Layout;
				var expectedLayout = ((IPanelLayoutProvider)new WarehouseAdjustmentAdditionalInfoLayout()).Layout;
				AssertContainsExactElementsInAnyOrder("IncludedControls", layout.IncludedControls, expectedLayout.IncludedControls);
			}
		}

		public void TestInvoiceLineDetailsPanelLayout()
		{
			using (var control = new WarehouseAdjustmentInvoiceLineUserControlForTest())
			{
				var layout = control.GetNewInvoiceLineDetailsPanelLayout_Exposed().Layout;
				var warehouseAdjustmentLayout = ((IPanelLayoutProvider)new WarehouseAdjustmentInvoiceLineDetailsLayout()).Layout;
				AssertContainsExactElementsInAnyOrder("IncludedControls", layout.IncludedControls, warehouseAdjustmentLayout.IncludedControls);
			}
		}

		public void TestGridColumnsVisibility()
		{
			using (var control = new WarehouseAdjustmentInvoiceLineUserControl())
			{
				control.JobDeclaration = Factory.New<JobDeclaration>();
				control.InitializeGridLayout();

				var customsInvoiceLinesBoundGrid = control.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");
				var columsStyles = customsInvoiceLinesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
				CombineAssertions(() =>
				{
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_PartNo);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_FormattedTariff);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_Description);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_InvoiceQuantity);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_InvoiceUQ);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_CountryOfOrigin);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_PrimaryPreference);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_CustomsQuantity);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_LinePrice);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_SupplementaryCode1);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_SupplementaryCode2);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_CustomsSecondQuantity);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_BondedWhsQuantity);
					AssertColumnNotVisible(columsStyles, JobComInvoiceLine.Schema.JI_BondedWhsUnitQty);
					AssertColumnNotVisible(columsStyles, nameof(JobComInvoiceLine.JI_NetPrice));
					AssertColumnNotVisible(columsStyles, nameof(JobComInvoiceLine.JI_RX_NKNetPriceCurr));
				});
			}

			void AssertColumnNotVisible(ZGridColumnInfo[] columnStyles, ZString columnName)
			{
				var columnStyle = columnStyles.FirstOrDefault(x => x.ColumnName == columnName);
				AssertNull(columnName, columnStyle);
			}
		}
	}

	class WarehouseAdjustmentInvoiceLineUserControlForTest : WarehouseAdjustmentInvoiceLineUserControl
	{
		public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout_Exposed() => base.GetNewInvoiceLineDetailsPanelLayout();

		public IPanelLayoutProvider GetAdditionalInfoPanelLayout_Exposed() => base.GetAdditionalInfoPanelLayout();
	}
}
