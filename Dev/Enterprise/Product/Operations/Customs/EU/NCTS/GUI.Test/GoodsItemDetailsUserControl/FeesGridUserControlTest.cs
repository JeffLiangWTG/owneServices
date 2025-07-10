using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class FeesGridUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestFeesGrid()
		{
			CombineAssertions(() =>
			{
				AssertType<ZGrid>("Type", control.FeesGrid);
				AssertEquals("BindTo", nameof(NctsDepartureCargoDesc.Fees), control.FeesGrid.BindTo);
			});
		}

		public void TestFeesGridDecimals()
		{
			var columnBaseValue = control.FeesGrid.GetColumnStyle("BFE_BaseValue") as ZCalcEditColumnStyleInfo;
			var columnChargeAmount = control.FeesGrid.GetColumnStyle("BFE_ChargeAmount") as ZCalcEditColumnStyleInfo;
			CombineAssertions(() =>
			{
				AssertNotNull("Column BFE_BaseValue", columnBaseValue);
				AssertNotNull("Column BFE_ChargeAmount", columnChargeAmount);
			});
			CombineAssertions(() =>
			{
				AssertEquals("BFE_BaseValue.Decimals", 2, columnBaseValue.Decimals);
				AssertEquals("BFE_ChargeAmount.Decimals", 2, columnChargeAmount.Decimals);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new FeesGridUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		FeesGridUserControl control;
	}
}
