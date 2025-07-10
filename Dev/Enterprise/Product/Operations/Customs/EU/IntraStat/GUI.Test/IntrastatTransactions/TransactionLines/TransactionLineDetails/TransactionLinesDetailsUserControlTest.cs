using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.GUI.Testing
{
	sealed class TransactionLinesDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(CusIntrastatLine), control.BindingSource.DataSourceType);
		}

		public void TestMassDropEdit()
		{
			var editControl = control.MassDropEdit;

			AssertType<ZCalcDropEdit>("Type", editControl);
			AssertEquals("BindToAmount", nameof(CusIntrastatLine.CIL_MassInKilograms), editControl.BindToAmount);
			AssertEquals("BindToUnit", nameof(CusIntrastatLine.CIL_MassInKilogramsUnit), editControl.BindToUnit);
		}

		public void TestInvoiceValueDropEdit()
		{
			var editControl = control.InvoiceValueDropEdit;

			AssertType<ZCalcDropEdit>("Type", editControl);
			AssertEquals("BindToAmount", nameof(CusIntrastatLine.CIL_InvoiceValue), editControl.BindToAmount);
			AssertEquals("BindToUnit", nameof(CusIntrastatLine.CIL_RX_NKCurrency), editControl.BindToUnit);
		}

		public void TestStatisticalValueDropEdit()
		{
			var editControl = control.StatisticalValueDropEdit;

			AssertType<ZCalcDropEdit>("Type", editControl);
			AssertEquals("BindToAmount", nameof(CusIntrastatLine.CIL_StatisticalValue), editControl.BindToAmount);
			AssertEquals("BindToUnit", nameof(CusIntrastatLine.CIL_RX_NKCurrency), editControl.BindToUnit);
		}

		public void TestSupplementaryUnitsCalcDropEdit()
		{
			var editControl = control.SupplementaryUnitsCalcDropEdit;

			AssertType<ZCalcDropEdit>("Type", editControl);
			AssertEquals("BindToAmount", nameof(CusIntrastatLine.CIL_SupplementaryQuantity), editControl.BindToAmount);
			AssertEquals("BindToUnit", nameof(CusIntrastatLine.CIL_SupplementaryQuantityUnit), editControl.BindToUnit);
		}

		public void TestDescriptionOfGoodsTextBox()
		{
			var editControl = control.DescriptionOfGoodsTextBox;

			AssertType<ZTextBox>("Type", editControl);
			AssertEquals("BindTo", nameof(CusIntrastatLine.CIL_DescriptionOfGoods), editControl.BindTo);
		}

		public void TestCountryOfOriginDropEdit()
		{
			var editControl = control.CountryOfOriginDropEdit;

			AssertType<ZDropEdit>("Type", editControl);
			AssertEquals("BindTo", nameof(CusIntrastatLine.CIL_RN_NKCountryOfOrigin), editControl.BindTo);
		}

		public void TestRegionDropEdit()
		{
			var editControl = control.RegionDropEdit;

			AssertType<ZDropEdit>("Type", editControl);
			AssertEquals("BindTo", nameof(CusIntrastatLine.CIL_Region), editControl.BindTo);
		}

		public void TestTariffFindBox()
		{
			var editControl = control.TariffFindBox;

			AssertType<TariffFindBox>("Type", editControl);
			AssertEquals("BindTo", nameof(CusIntrastatLine.CIL_Tariff), editControl.BindTo);
			AssertEquals("Type", "EXP", editControl.TariffType);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransactionLineDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransactionLineDetailsUserControl control;
	}
}
