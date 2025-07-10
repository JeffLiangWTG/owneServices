using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class LiabilityAmountTotalValueCalculationMethodUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType() => AssertEquals(typeof(CalculateLiabilityBizObj), control.BindingSource.DataSourceType);

		public void TestUseDutiesAndTaxesCheckBox()
		{
			var useDutesAndTaxesCheckBox = control.UseDutiesAndTaxesCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", useDutesAndTaxesCheckBox);
				AssertEquals("BindTo", nameof(CalculateLiabilityBizObj.UseDutiesAndTaxes), useDutesAndTaxesCheckBox.BindTo);
			});
		}

		public void TestUseMonetaryValueCheckBox()
		{
			var useMonetaryValueCheckBox = control.UseMonetaryValueCheckBox;
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>("Type", useMonetaryValueCheckBox);
				AssertEquals("BindTo", nameof(CalculateLiabilityBizObj.UseMonetaryValue), useMonetaryValueCheckBox.BindTo);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new LiabilityAmountTotalValueCalculationMethodUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		LiabilityAmountTotalValueCalculationMethodUserControl control;
	}
}
