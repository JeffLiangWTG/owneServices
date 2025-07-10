using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.EMCS.GUI.Testing
{
	sealed class InvoiceLineGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(EMCSInvoiceLineViewCollection), userControl.BindingSource.DataSourceType);
		}

		public void TestAvailableColumns()
		{
			AssertSequencesEqual("Columns",
				new[] { "JI_LineNo", "JI_PartNo", "JI_Tariff", "TariffDescription", "ZG_ExciseProductCode",
					"JI_NDescription", "JI_Weight", "JI_WeightUQ", "JI_CustomsQuantity", "CustomsUnitQtyDescription",
					"JI_NetWeight", "JI_NetWeightUQ", "JI_BrandName", "ZG_Origin" },
				customsInvoiceLinesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
		}

		public void TestColumnsWidth()
		{
			CombineAssertions(() =>
			{
				AssertEquals("JI_LineNo", 30, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_LineNo").Width);
				AssertEquals("JI_PartNo", 80, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_PartNo").Width);
				AssertEquals("JI_Tariff", 80, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_Tariff").Width);
				AssertEquals("TariffDescription", 80, customsInvoiceLinesBoundGrid.GetColumnStyle("TariffDescription").Width);
				AssertEquals("ZG_ExciseProductCode", 88, customsInvoiceLinesBoundGrid.GetColumnStyle("ZG_ExciseProductCode").Width);
				AssertEquals("JI_Weight", 60, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_Weight").Width);
				AssertEquals("JI_WeightUQ", 30, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_WeightUQ").Width);
				AssertEquals("JI_CustomsQuantity", 110, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_CustomsQuantity").Width);
				AssertEquals("CustomsUnitQtyDescription", 137, customsInvoiceLinesBoundGrid.GetColumnStyle("CustomsUnitQtyDescription").Width);
				AssertEquals("JI_NetWeight", 80, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_NetWeight").Width);
				AssertEquals("JI_NetWeightUQ", 80, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_NetWeightUQ").Width);
				AssertEquals("JI_BrandName", 80, customsInvoiceLinesBoundGrid.GetColumnStyle("JI_BrandName").Width);
				AssertEquals("ZG_Origin", 100, customsInvoiceLinesBoundGrid.GetColumnStyle("ZG_Origin").Width);
			});
		}

		public void TestJI_NDescription()
		{
			CombineAssertions(() =>
			{
				var columnStyle = customsInvoiceLinesBoundGrid.GetColumnStyle("JI_NDescription");
				AssertEquals("Width", 200, columnStyle.Width);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, columnStyle.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new InvoiceLineGridUserControl();
			customsInvoiceLinesBoundGrid = userControl.CustomsInvoiceLinesBoundGrid;
		}
		InvoiceLineGridUserControl userControl;
		ZGrid customsInvoiceLinesBoundGrid;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
