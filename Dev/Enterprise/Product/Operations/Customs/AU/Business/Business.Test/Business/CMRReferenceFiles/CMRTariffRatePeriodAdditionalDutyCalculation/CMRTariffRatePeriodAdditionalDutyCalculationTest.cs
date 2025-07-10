using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTariffRatePeriodAdditionalDutyCalculation))]
	sealed class CMRTariffRatePeriodAdditionalDutyCalculationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadWithTariffRate()
		{
			SetTariffRateLinked();
			AssertNull("No record found", CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffRate));

			SetTestBizO();
			AssertEquals("matching record", testBizO, CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffRate));

			testBizO.TA_TariffRatePeriodSnapshotTariffClassificationNumber = "";
			AssertNull("No record found", CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffRate));

			testBizO.TA_TariffRatePeriodSnapshotTariffClassificationNumber = tariffRate.TT_TariffClassificationNumber;
			testBizO.TA_TariffRatePeriodSnapshotRateNumber = "";
			AssertNull("No record found", CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffRate));

			testBizO.TA_TariffRatePeriodSnapshotRateNumber = tariffRate.TT_RateNumber;
			testBizO.TA_TariffRatePeriodSnapshotPeriodIdentifier = (short)100;
			AssertNull("No record found", CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffRate));

			testBizO.TA_TariffRatePeriodSnapshotPeriodIdentifier = tariffRate.TT_PeriodIdentifier;
			testBizO.TA_TariffRatePeriodSnapshotPreferenceSchemeType = "";
			AssertNull("No record found", CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffRate));
		}

		public void TestICMRDutyRate()
		{
			SetTestBizO();
			SetTariffRateLinked();
			tariffRate.TT_CalculationType = "UUU";

			testBizO.TA_CustomsValueRate = 0.5m;
			testBizO.TA_OtherDutyFactorRate = 0m;
			testBizO.TA_QuantityRate = 0m;
			testBizO.TA_SecondQuantityRate = 0m;

			AssertEquals("CalculationType", "UUU", ((ICMRDutyRate)testBizO).CalculationType);
			var rateInfo = ((ICMRDutyRate)testBizO).RatesApplicable[0];
			AssertEquals("RateApplicable", DutyRateField.CustomsValue, rateInfo.DutyRateField);
			AssertEquals("Rate", 0.5m, rateInfo.Rate);
			AssertEquals("Unit", "", rateInfo.Unit);

			testBizO.TA_CustomsValueRate = 0m;
			testBizO.TA_OtherDutyFactorRate = 0m;
			testBizO.TA_QuantityRate = 0m;
			testBizO.TA_SecondQuantityRate = 0.4m;
			tariffRate.TT_SecondQuantityUnit = "YY";

			rateInfo = ((ICMRDutyRate)testBizO).RatesApplicable[0];
			AssertEquals("RateApplicable", DutyRateField.SecondQty, rateInfo.DutyRateField);
			AssertEquals("Rate", 0.4m, rateInfo.Rate);
			AssertEquals("Unit", "YY", rateInfo.Unit);
		}

		public void TestIFourRates()
		{
			SetTestBizO();
			SetTariffRateLinked();

			tariffRate.TT_CalculationType = "YYY";
			tariffRate.TT_QuantityUnit = "AY";
			tariffRate.TT_SecondQuantityUnit = "BY";

			testBizO.TA_CustomsValueRate = 0.5m;
			testBizO.TA_OtherDutyFactorRate = 1m;
			testBizO.TA_QuantityRate = 2m;
			testBizO.TA_SecondQuantityRate = 3m;

			AssertEquals("Customs Rate", 0.5m, ((IFourRates)testBizO).CustomsRate);
			AssertEquals("FirstQtyRate", 2m, ((IFourRates)testBizO).FirstQtyRate);
			AssertEquals("FirstUQ", "AY", ((IFourRates)testBizO).FirstUQ);
			AssertEquals("SecondQtyRate", 3m, ((IFourRates)testBizO).SecondQtyRate);
			AssertEquals("SecondUQ", "BY", ((IFourRates)testBizO).SecondUQ);
			AssertEquals("OtherDutyFactorRate", 1m, ((IFourRates)testBizO).OtherDutyFactorRate);
		}

		public void TestTariffRate()
		{
			SetTestBizO();
			AssertNull("Tariff rate linked cannot be found", testBizO.TariffRate);

			SetTariffRateLinked();
			AssertEquals("Tariff rate linked found", tariffRate, testBizO.TariffRate);
		}

		public void TestRateInfoCalculator()
		{
			AssertNotNull(testBizO.RateInfoCalculator);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRTariffRatePeriodAdditionalDutyCalculation.New(Factory);

		CMRTariffRatePeriodAdditionalDutyCalculation testBizO;
		CMRTariffRatePeriodSnapshot tariffRate;

		protected override void SetUp()
		{
			base.SetUp();
			testBizO = Factory.New<CMRTariffRatePeriodAdditionalDutyCalculation>();
			tariffRate = Factory.New<CMRTariffRatePeriodSnapshot>();
		}

		void SetTestBizO()
		{
			testBizO.TA_TariffRatePeriodSnapshotTariffClassificationNumber = "00000000";
			testBizO.TA_TariffRatePeriodSnapshotRateNumber = "000";
			testBizO.TA_TariffRatePeriodSnapshotPreferenceSchemeType = "XXX";
			testBizO.TA_TariffRatePeriodSnapshotPeriodIdentifier = (short)0;
		}

		void SetTariffRateLinked()
		{
			tariffRate.TT_TariffClassificationNumber = "00000000";
			tariffRate.TT_RateNumber = "000";
			tariffRate.TT_PreferenceSchemeType = "XXX";
			tariffRate.TT_PeriodIdentifier = (short)0;
		}
	}
}
