using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class PriceGroupBoxUserControlTest : TestCaseWithFactory
	{
		public void TestBasisForCalculation()
		{
			using (var control = new PriceGroupBoxUserControl())
			{
				var groupBox = control.FindSingle<ZGroupBox>("BasisForCalculationGroupBox");
				AssertEquals(true, groupBox.Visible);
				var panel = groupBox.FindSingle<DynamicLayoutPanel>("BasisForCalculationPanel");
				AssertNotNull(panel);

				AssertEquals(true, panel.FindSingle<ConvertToLocalCurrencyControl>("PaymentAmountConvertToLocalCurrencyControl").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("ExchangeRateCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("IndirectAmountCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("PaymentAmountCalcEdit").Visible);
			}
		}

		public void TestAdditionalCosts()
		{
			using (var control = new PriceGroupBoxUserControl())
			{
				var groupBox = control.FindSingle<ZGroupBox>("AdditionalCostsGroupBox");
				AssertEquals(true, groupBox.Visible);
				var panel = groupBox.FindSingle<DynamicLayoutPanel>("AdditionalCostsPanel");
				AssertNotNull(panel);

				AssertEquals(true, panel.FindSingle<ZCalcEdit>("PurchaseCostCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("BrokerageFeeCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("ContainerPackagingCostCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("GoodsCostCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("ProductToolCostsCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("CommodityUsageCostsCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("ProductDevCostsCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("RoyaltyCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("ProfitAmountCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("ExcludingTransportationCostsCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("FreightCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("UnloadCostCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("InsuranceCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("TransportationCostCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("TotalAdditionalAmountCalcEdit").Visible);
			}
		}

		public void TestDeductionCosts()
		{
			using (var control = new PriceGroupBoxUserControl())
			{
				var groupBox = control.FindSingle<ZGroupBox>("DeductionCostsGroupBox");
				AssertEquals(true, groupBox.Visible);
				var panel = groupBox.FindSingle<DynamicLayoutPanel>("DeductionCostsPanel");
				AssertNotNull(panel);

				AssertEquals(true, panel.FindSingle<ZCalcEdit>("LocalTransportationCostCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("TechnicalCostCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("OtherCostsCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("DiscountAmountCalcEdit").Visible);
				AssertEquals(true, panel.FindSingle<ZCalcEdit>("TotalDeductionAmountCalcEdit").Visible);
			}
		}
	}
}
