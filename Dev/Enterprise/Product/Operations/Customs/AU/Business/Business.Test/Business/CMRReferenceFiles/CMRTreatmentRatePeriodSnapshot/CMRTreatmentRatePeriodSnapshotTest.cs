using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRTreatmentRatePeriodSnapshot))]
	sealed class CMRTreatmentRatePeriodSnapshotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRateForTreatment462()
		{
			var testRate = CMRTreatmentRatePeriodSnapshot.New(Factory);
			testRate.TP_CustomsValueRate = 1m;
			AssertEquals("Actual rate returned", 1m, testRate.TP_CustomsValueRate);
			testRate.TP_Code = "462";
			AssertEquals("Actual rate returned", 1m, testRate.TP_CustomsValueRate);
			testRate.TP_RateNumber = "61A";
			AssertEquals("Negative rate returned", -1m, testRate.TP_CustomsValueRate);
			testRate.TP_CustomsValueRate = -1m;
			AssertEquals("Actual rate returned", -1m, testRate.TP_CustomsValueRate);
		}

		public void TestIsCalculable()
		{
			var testRate = CMRTreatmentRatePeriodSnapshot.New(Factory);

			testRate.TP_CalculationType = Constants.DutyCalcTypes.Higher;
			AssertEquals("IsCalculable", true, testRate.IsCalculable);

			testRate.TP_CalculationType = Constants.DutyCalcTypes.Free;
			AssertEquals("IsCalculable", false, testRate.IsCalculable);

			testRate.TP_CalculationType = Constants.DutyCalcTypes.Calc;
			AssertEquals("IsCalculable", true, testRate.IsCalculable);

			testRate.TP_CalculationType = Constants.DutyCalcTypes.Info;
			AssertEquals("IsCalculable", false, testRate.IsCalculable);

			testRate.TP_CalculationType = Constants.DutyCalcTypes.Lower;
			AssertEquals("IsCalculable", true, testRate.IsCalculable);
		}

		public void TestICompositeDutyRate()
		{
			var testRate = CMRTreatmentRatePeriodSnapshot.New(Factory);
			testRate.TP_CalculationType = "free";
			testRate.TP_Code = "000";
			testRate.TP_PreferenceSchemeType = "GEN";
			testRate.TP_RateNumber = "00";
			testRate.TP_PeriodIdentifier = 10;

			var additionalRateNotMatching = CMRTreatmentRatePeriodAdditionalDutyCalculation.New(Factory);
			additionalRateNotMatching.TD_TreatmentRatePeriodSnapshotCode = "000";
			additionalRateNotMatching.TD_TreatmentRatePeriodSnapshotPeriodIdentifier = 10;
			additionalRateNotMatching.TD_TreatmentRatePeriodSnapshotPreferenceSchemeType = "GEN";
			additionalRateNotMatching.TD_TreatmentRatePeriodSnapshotRateNumber = "01";

			AssertEquals("Calculation type", "FREE", ((ICompositeDutyRate)testRate).CalculationType);
			AssertEquals("Additional duty Rate", null, ((ICompositeDutyRate)testRate).AdditionalDutyRate);

			var additionalRateMatching = CMRTreatmentRatePeriodAdditionalDutyCalculation.New(Factory);
			additionalRateMatching.TD_TreatmentRatePeriodSnapshotCode = "000";
			additionalRateMatching.TD_TreatmentRatePeriodSnapshotPeriodIdentifier = 10;
			additionalRateMatching.TD_TreatmentRatePeriodSnapshotPreferenceSchemeType = "GEN";
			additionalRateMatching.TD_TreatmentRatePeriodSnapshotRateNumber = "00";
			AssertEquals("Additional duty Rate", additionalRateMatching, ((ICompositeDutyRate)testRate).AdditionalDutyRate);
		}

		public void TestWithDutyData()
		{
			var invoiceLineDutyData = new DutyDataFromInvoiceLine();
			invoiceLineDutyData.EffectiveDutyDate = new ZDateTime(2005, 1, 1);
			invoiceLineDutyData.FirstTreatmentCode = "002";
			invoiceLineDutyData.SecondTreatmentCode = "001";
			invoiceLineDutyData.Preference = "AA";
			invoiceLineDutyData.TreatmentRateNumber = "000";
			dummyDutyData.RandomLineDutyDataExposed = invoiceLineDutyData;

			var result = CMRTreatmentRatePeriodSnapshot.Load(dummyDutyData, dummyDutyData.RandomLineDutyData.FirstTreatmentCode);
			AssertEquals("No matching record", null, result);

			SetTestBizOData();
			result = CMRTreatmentRatePeriodSnapshot.Load(dummyDutyData, dummyDutyData.RandomLineDutyData.SecondTreatmentCode);
			AssertEquals("No matching record", null, result);

			invoiceLineDutyData.SecondTreatmentCode = "000";
			dummyDutyData.RandomLineDutyDataExposed = invoiceLineDutyData;
			result = CMRTreatmentRatePeriodSnapshot.Load(dummyDutyData, dummyDutyData.RandomLineDutyData.SecondTreatmentCode);
			AssertEquals("matching record", testBizO, result);

			testBizO.TP_RateNumber = "001";
			invoiceLineDutyData.TreatmentRateNumber = "01";
			dummyDutyData.RandomLineDutyDataExposed = invoiceLineDutyData;
			result = CMRTreatmentRatePeriodSnapshot.Load(dummyDutyData, dummyDutyData.RandomLineDutyData.SecondTreatmentCode);
			AssertEquals("matching record", testBizO, result);

			testBizO.TP_PreferenceSchemeType = "GEN";
			result = CMRTreatmentRatePeriodSnapshot.Load(dummyDutyData, dummyDutyData.RandomLineDutyData.SecondTreatmentCode);
			AssertNull("No matching record", result);

			testBizO.TP_CalculationType = "INFO";
			result = CMRTreatmentRatePeriodSnapshot.Load(dummyDutyData, dummyDutyData.RandomLineDutyData.SecondTreatmentCode);
			AssertEquals("matching record", testBizO, result);
		}

		public void TestLoadWithFourfields()
		{
			AssertNull("No matching record", CMRTreatmentRatePeriodSnapshot.Load(Factory, "000", "000", "AA", (short)0));

			SetTestBizOData();
			AssertEquals(testBizO, CMRTreatmentRatePeriodSnapshot.Load(Factory, "000", "000", "AA", (short)0));

			testBizO.TP_RateNumber = "";
			AssertNull("No matching record", CMRTreatmentRatePeriodSnapshot.Load(Factory, "000", "000", "AA", (short)0));

			testBizO.TP_RateNumber = "000";
			testBizO.TP_Code = "";
			AssertNull("No matching record", CMRTreatmentRatePeriodSnapshot.Load(Factory, "000", "000", "AA", (short)0));

			testBizO.TP_Code = "000";
			testBizO.TP_PreferenceSchemeType = "";
			AssertNull("No matching record", CMRTreatmentRatePeriodSnapshot.Load(Factory, "000", "000", "AA", (short)0));

			testBizO.TP_PreferenceSchemeType = "AA";
			testBizO.TP_PeriodIdentifier = (short)100;
			AssertNull("No matching record", CMRTreatmentRatePeriodSnapshot.Load(Factory, "000", "000", "AA", (short)0));
		}

		public void TestICMRDutyRate()
		{
			SetTestBizOData();
			AssertEquals("Calculation type", testBizO.TP_CalculationType, ((ICMRDutyRate)testBizO).CalculationType);

			testBizO.TP_CustomsValueRate = 0.5m;
			var rateInfo = ((ICMRDutyRate)testBizO).RatesApplicable[0];
			AssertEquals("DutyRateField", DutyRateField.CustomsValue, rateInfo.DutyRateField);
			AssertEquals("CustomsRate", 0.5m, rateInfo.Rate);
			AssertEquals("Unit", "", rateInfo.Unit);

			testBizO.TP_CustomsValueRate = 0m;
			testBizO.TP_QuantityRate = 9m;
			testBizO.TP_QuantityUnit = "~~";
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
			testBizO.TP_CustomsValueRate = 0.5m;
			testBizO.TP_QuantityRate = 1m;
			testBizO.TP_QuantityUnit = "AS";
			testBizO.TP_SecondQuantityRate = 2.5m;
			testBizO.TP_SecondQuantityUnit = "CV";
			testBizO.TP_OtherDutyFactorRate = 9.0m;

			AssertEquals("CustomsRate", 0.5m, ((IFourRates)testBizO).CustomsRate);
			AssertEquals("FirstQtyRate", 1m, ((IFourRates)testBizO).FirstQtyRate);
			AssertEquals("FirstUQ", "AS", ((IFourRates)testBizO).FirstUQ);
			AssertEquals("SecondQtyRate", 2.5m, ((IFourRates)testBizO).SecondQtyRate);
			AssertEquals("SecondUQ", "CV", ((IFourRates)testBizO).SecondUQ);
			AssertEquals("OtherDutyFactorRate", 9.0m, ((IFourRates)testBizO).OtherDutyFactorRate);
		}

		protected override BusinessObject GetNewBusinessObject() => CMRTreatmentRatePeriodSnapshot.New(Factory);

		CMRTreatmentRatePeriodSnapshot testBizO;
		DummyCMRDutyData dummyDutyData;

		protected override void SetUp()
		{
			base.SetUp();
			testBizO = Factory.New<CMRTreatmentRatePeriodSnapshot>();
			dummyDutyData = new DummyCMRDutyData(Factory);
		}

		void SetTestBizOData()
		{
			testBizO.TP_RateNumber = "000";
			testBizO.TP_Code = "000";
			testBizO.TP_PreferenceSchemeType = "AA";
			testBizO.TP_PeriodIdentifier = (short)0;

			testBizO.TP_StartDate = new ZDateTime(2005, 1, 1);
			testBizO.TP_CalculationType = "XXX";
		}
	}
}
