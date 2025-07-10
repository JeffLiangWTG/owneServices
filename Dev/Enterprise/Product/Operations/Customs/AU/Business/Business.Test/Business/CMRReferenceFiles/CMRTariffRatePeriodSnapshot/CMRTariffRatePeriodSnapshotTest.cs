using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTariffRatePeriodSnapshot))]
	sealed class CMRTariffRatePeriodSnapshotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICompositeDutyRate()
		{
			var testTariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			testTariffRate.TT_CalculationType = "free";
			testTariffRate.TT_TariffClassificationNumber = "00000000";
			testTariffRate.TT_PreferenceSchemeType = "GEN";
			testTariffRate.TT_RateNumber = "000";
			testTariffRate.TT_PeriodIdentifier = 10;

			var additionalRateNotMatching = CMRTariffRatePeriodAdditionalDutyCalculation.New(Factory);
			additionalRateNotMatching.TA_TariffRatePeriodSnapshotRateNumber = "000";
			additionalRateNotMatching.TA_TariffRatePeriodSnapshotPeriodIdentifier = 10;
			additionalRateNotMatching.TA_TariffRatePeriodSnapshotPreferenceSchemeType = "GEN";
			additionalRateNotMatching.TA_TariffRatePeriodSnapshotTariffClassificationNumber = "00000001";

			AssertEquals("Calculation type", "FREE", ((ICompositeDutyRate)testTariffRate).CalculationType);
			AssertEquals("Additional duty Rate", null, ((ICompositeDutyRate)testTariffRate).AdditionalDutyRate);

			var additionalRateMatching = CMRTariffRatePeriodAdditionalDutyCalculation.New(Factory);
			additionalRateMatching.TA_TariffRatePeriodSnapshotRateNumber = "000";
			additionalRateMatching.TA_TariffRatePeriodSnapshotPeriodIdentifier = 10;
			additionalRateMatching.TA_TariffRatePeriodSnapshotPreferenceSchemeType = "GEN";
			additionalRateMatching.TA_TariffRatePeriodSnapshotTariffClassificationNumber = "00000000";
			AssertEquals("Additional duty Rate", additionalRateMatching, ((ICompositeDutyRate)testTariffRate).AdditionalDutyRate);
		}

		public void TestGetDutyRateDescription()
		{
			var testTariffRate = CMRTariffRatePeriodSnapshot.New(Factory);
			testTariffRate.TT_CalculationType = "FREE";
			AssertEquals("description including duty rate", "Duty: FREE", testTariffRate.GetDutyRateDescription());

			testTariffRate.TT_TariffClassificationNumber = "00000000";
			testTariffRate.TT_PreferenceSchemeType = "GEN";
			testTariffRate.TT_CalculationType = "CALC";
			testTariffRate.TT_CustomsValueRate = 5m;
			testTariffRate.TT_QuantityRate = 1.25m;
			testTariffRate.TT_QuantityUnit = "KG";

			AssertEquals("description including duty rate", "Duty: 5% + $1.25000/KG", testTariffRate.GetDutyRateDescription());
		}

		public void TestWithDutyData()
		{
			var invoiceLineDutyData = new DutyDataFromInvoiceLine();
			invoiceLineDutyData.EffectiveDutyDate = new ZDateTime(2005, 1, 1);
			invoiceLineDutyData.FirstTariffNumber = "00000002";
			invoiceLineDutyData.SecondTariffNumber = "00000001";
			invoiceLineDutyData.Preference = "AA";
			invoiceLineDutyData.RateNumber = "000";
			dummyDutyData.RandomLineDutyDataExposed = invoiceLineDutyData;

			var result = CMRTariffRatePeriodSnapshot.Load(dummyDutyData, "00000002");
			AssertEquals("No matching record", null, result);

			SetTestBizOData();
			result = CMRTariffRatePeriodSnapshot.Load(dummyDutyData, "00000001");
			AssertEquals("No matching record", null, result);

			invoiceLineDutyData.SecondTariffNumber = "00000000";
			dummyDutyData.RandomLineDutyDataExposed = invoiceLineDutyData;
			result = CMRTariffRatePeriodSnapshot.Load(dummyDutyData, "00000000");
			AssertEquals("matching record", testBizO, result);
		}

		public void TestWithDutyDataFallsBackToGeneralRate()
		{
			var invoiceLineDutyData = new DutyDataFromInvoiceLine();
			invoiceLineDutyData.EffectiveDutyDate = new ZDateTime(2017, 1, 1);
			invoiceLineDutyData.FirstTariffNumber = "00000002";
			invoiceLineDutyData.SecondTariffNumber = "00000001";
			invoiceLineDutyData.Preference = "";
			invoiceLineDutyData.RateNumber = "000";
			dummyDutyData.RandomLineDutyDataExposed = invoiceLineDutyData;

			var result = CMRTariffRatePeriodSnapshot.Load(dummyDutyData, "00000002");
			AssertEquals("No matching record", null, result);

			var testBizOGeneralRate = Factory.New<CMRTariffRatePeriodSnapshot>();
			testBizOGeneralRate.TT_RateNumber = "000";
			testBizOGeneralRate.TT_TariffClassificationNumber = "00000000";
			testBizOGeneralRate.TT_PreferenceSchemeType = "GEN";
			testBizOGeneralRate.TT_PeriodIdentifier = (short)0;
			testBizOGeneralRate.TT_StartDate = new ZDateTime(2016, 12, 30);
			testBizOGeneralRate.TT_CalculationType = "XXX";
			dummyDutyData = new DummyCMRDutyData(Factory);

			result = CMRTariffRatePeriodSnapshot.Load(dummyDutyData, "00000001");
			AssertEquals("No matching record", null, result);

			invoiceLineDutyData.SecondTariffNumber = "00000000";
			dummyDutyData.RandomLineDutyDataExposed = invoiceLineDutyData;
			result = CMRTariffRatePeriodSnapshot.Load(dummyDutyData, "00000000");
			AssertEquals("matching record finds period snapshot for General rate", testBizOGeneralRate, result);
		}

		public void TestLoadWithFourfields()
		{
			AssertNull("No matching record", CMRTariffRatePeriodSnapshot.Load(Factory, "00000000", "000", "AA", (short)0));

			SetTestBizOData();
			AssertEquals(testBizO, CMRTariffRatePeriodSnapshot.Load(Factory, "00000000", "000", "AA", (short)0));

			testBizO.TT_RateNumber = "";
			AssertNull("No matching record", CMRTariffRatePeriodSnapshot.Load(Factory, "00000000", "000", "AA", (short)0));

			testBizO.TT_RateNumber = "000";
			testBizO.TT_TariffClassificationNumber = "";
			AssertNull("No matching record", CMRTariffRatePeriodSnapshot.Load(Factory, "00000000", "000", "AA", (short)0));

			testBizO.TT_TariffClassificationNumber = "00000000";
			testBizO.TT_PreferenceSchemeType = "";
			AssertNull("No matching record", CMRTariffRatePeriodSnapshot.Load(Factory, "00000000", "000", "AA", (short)0));

			testBizO.TT_PreferenceSchemeType = "AA";
			testBizO.TT_PeriodIdentifier = (short)100;
			AssertNull("No matching record", CMRTariffRatePeriodSnapshot.Load(Factory, "00000000", "000", "AA", (short)0));
		}

		public void TestICMRDutyRate()
		{
			SetTestBizOData();
			AssertEquals("Calculation type", testBizO.TT_CalculationType, ((ICMRDutyRate)testBizO).CalculationType);

			testBizO.TT_CustomsValueRate = 0.5m;
			var rateInfo = ((ICMRDutyRate)testBizO).RatesApplicable[0];
			AssertEquals("DutyRateField", DutyRateField.CustomsValue, rateInfo.DutyRateField);
			AssertEquals("CustomsRate", 0.5m, rateInfo.Rate);
			AssertEquals("Unit", "", rateInfo.Unit);

			testBizO.TT_CustomsValueRate = 0m;
			testBizO.TT_QuantityRate = 9m;
			testBizO.TT_QuantityUnit = "~~";
			rateInfo = ((ICMRDutyRate)testBizO).RatesApplicable[0];
			AssertEquals("DutyRateField", DutyRateField.FirstQty, rateInfo.DutyRateField);
			AssertEquals("Rate", 9m, rateInfo.Rate);
			AssertEquals("Unit", "~~", rateInfo.Unit);
		}

		public void TestRateInfoCalculator()
		{
			AssertNotNull("RateInfoCalculator", testBizO.RateInfoCalculator);
		}

		public void TestIFourRates()
		{
			SetTestBizOData();
			testBizO.TT_CustomsValueRate = 0.5m;
			testBizO.TT_QuantityRate = 1m;
			testBizO.TT_QuantityUnit = "AS";
			testBizO.TT_SecondQuantityRate = 2.5m;
			testBizO.TT_SecondQuantityUnit = "CV";
			testBizO.TT_OtherDutyFactorRate = 9.0m;

			AssertEquals("CustomsRate", 0.5m, ((IFourRates)testBizO).CustomsRate);
			AssertEquals("FirstQtyRate", 1m, ((IFourRates)testBizO).FirstQtyRate);
			AssertEquals("FirstUQ", "AS", ((IFourRates)testBizO).FirstUQ);
			AssertEquals("SecondQtyRate", 2.5m, ((IFourRates)testBizO).SecondQtyRate);
			AssertEquals("SecondUQ", "CV", ((IFourRates)testBizO).SecondUQ);
			AssertEquals("OtherDutyFactorRate", 9.0m, ((IFourRates)testBizO).OtherDutyFactorRate);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRTariffRatePeriodSnapshot.New(Factory);

		CMRTariffRatePeriodSnapshot testBizO;
		DummyCMRDutyData dummyDutyData;

		protected override void SetUp()
		{
			base.SetUp();
			testBizO = Factory.New<CMRTariffRatePeriodSnapshot>();
			dummyDutyData = new DummyCMRDutyData(Factory);
		}

		void SetTestBizOData()
		{
			testBizO.TT_RateNumber = "000";
			testBizO.TT_TariffClassificationNumber = "00000000";
			testBizO.TT_PreferenceSchemeType = "AA";
			testBizO.TT_PeriodIdentifier = (short)0;

			testBizO.TT_StartDate = new ZDateTime(2005, 1, 1);
			testBizO.TT_CalculationType = "XXX";
		}
	}
}
