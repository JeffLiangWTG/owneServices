using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentRatePeriodAdditionalDutyCalculation))]
	sealed class CMRTreatmentRatePeriodAdditionalDutyCalculationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadWithTreatmentRate()
		{
			SetTreatmentRateLinked();
			AssertNull("No record found", CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentRate));

			SetTestBizO();
			AssertEquals("matching record", testBizO, CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentRate));

			testBizO.TD_TreatmentRatePeriodSnapshotCode = "";
			AssertNull("No record found", CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentRate));

			testBizO.TD_TreatmentRatePeriodSnapshotCode = treatmentRate.TP_Code;
			testBizO.TD_TreatmentRatePeriodSnapshotRateNumber = "";
			AssertNull("No record found", CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentRate));

			testBizO.TD_TreatmentRatePeriodSnapshotRateNumber = treatmentRate.TP_RateNumber;
			testBizO.TD_TreatmentRatePeriodSnapshotPeriodIdentifier = (short)100;
			AssertNull("No record found", CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentRate));

			testBizO.TD_TreatmentRatePeriodSnapshotPeriodIdentifier = treatmentRate.TP_PeriodIdentifier;
			testBizO.TD_TreatmentRatePeriodSnapshotPreferenceSchemeType = "";
			AssertNull("No record found", CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentRate));
		}

		public void TestICMRDutyRate()
		{
			SetTestBizO();
			SetTreatmentRateLinked();
			treatmentRate.TP_CalculationType = "UUU";

			testBizO.TD_CustomsValueRate = 0.5m;
			testBizO.TD_OtherDutyFactorRate = 0m;
			testBizO.TD_QuantityRate = 0m;
			testBizO.TD_SecondQuantityRate = 0m;

			AssertEquals("CalculationType", "UUU", ((ICMRDutyRate)testBizO).CalculationType);
			var rateInfo = ((ICMRDutyRate)testBizO).RatesApplicable[0];
			AssertEquals("RateApplicable", DutyRateField.CustomsValue, rateInfo.DutyRateField);
			AssertEquals("Rate", 0.5m, rateInfo.Rate);
			AssertEquals("Unit", "", rateInfo.Unit);

			testBizO.TD_CustomsValueRate = 0m;
			testBizO.TD_OtherDutyFactorRate = 0m;
			testBizO.TD_QuantityRate = 0m;
			testBizO.TD_SecondQuantityRate = 0.4m;
			treatmentRate.TP_SecondQuantityUnit = "YY";

			rateInfo = ((ICMRDutyRate)testBizO).RatesApplicable[0];
			AssertEquals("RateApplicable", DutyRateField.SecondQty, rateInfo.DutyRateField);
			AssertEquals("Rate", 0.4m, rateInfo.Rate);
			AssertEquals("Unit", "YY", rateInfo.Unit);
		}

		public void TestIFourRates()
		{
			SetTestBizO();
			SetTreatmentRateLinked();

			treatmentRate.TP_CalculationType = "YYY";
			treatmentRate.TP_QuantityUnit = "AY";
			treatmentRate.TP_SecondQuantityUnit = "BY";

			testBizO.TD_CustomsValueRate = 0.5m;
			testBizO.TD_OtherDutyFactorRate = 1m;
			testBizO.TD_QuantityRate = 2m;
			testBizO.TD_SecondQuantityRate = 3m;

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
			AssertNull("Tariff rate linked cannot be found", testBizO.TreatmentRate);

			SetTreatmentRateLinked();
			AssertEquals("Treatment rate linked found", treatmentRate, testBizO.TreatmentRate);
		}

		public void TestRateInfoCalculator()
		{
			AssertNotNull(testBizO.RateInfoCalculator);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRTreatmentRatePeriodAdditionalDutyCalculation.New(Factory);

		CMRTreatmentRatePeriodAdditionalDutyCalculation testBizO;
		CMRTreatmentRatePeriodSnapshot treatmentRate;

		protected override void SetUp()
		{
			base.SetUp();
			testBizO = Factory.New<CMRTreatmentRatePeriodAdditionalDutyCalculation>();
			treatmentRate = Factory.New<CMRTreatmentRatePeriodSnapshot>();
		}

		void SetTestBizO()
		{
			testBizO.TD_TreatmentRatePeriodSnapshotCode = "001";
			testBizO.TD_TreatmentRatePeriodSnapshotRateNumber = "000";
			testBizO.TD_TreatmentRatePeriodSnapshotPreferenceSchemeType = "XXX";
			testBizO.TD_TreatmentRatePeriodSnapshotPeriodIdentifier = (short)0;
		}

		void SetTreatmentRateLinked()
		{
			treatmentRate.TP_Code = "001";
			treatmentRate.TP_RateNumber = "000";
			treatmentRate.TP_PreferenceSchemeType = "XXX";
			treatmentRate.TP_PeriodIdentifier = (short)0;
		}
	}
}
