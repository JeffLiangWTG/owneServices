using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.AU.Declaration.Business.CMRDutyCalculator;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DutySelectionTypeTest : TestCaseWithFactory
	{
		public void TestSelectionOne()
		{
			randomLineDutyData.FirstTreatmentCode = "";
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingTariff(randomLineDutyData.FirstTariffNumber);

			DutySelectionTypeCalculator calculator = new DutySelectionTypeCalculator(null, null, randomLineDutyData);
			AssertEquals("Selection type should be one", 1, calculator.GetSelectionType());

			tariffRate.TT_CustomsValueRate = 5m;
			dutyData.CustomsValueExposed = 2000m;
			CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 100m, dutyCalculator.Duty.Amount.Amount);
		}

		public void TestSelectionTwo()
		{
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingTariff(randomLineDutyData.FirstTariffNumber);
			CMRTreatmentRatePeriodSnapshot treatmentOne = SetTreatmentMatchingTreatment(false, randomLineDutyData.FirstTreatmentCode);

			DutySelectionTypeCalculator calculator = new DutySelectionTypeCalculator(treatmentOne, null, randomLineDutyData);
			AssertEquals("Selection type should be 2", 2, calculator.GetSelectionType());

			tariffRate.TT_CustomsValueRate = 5m;
			treatmentOne.TP_CustomsValueRate = 10m;
			dutyData.CustomsValueExposed = 2000m;
			CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 200m, dutyCalculator.Duty.Amount.Amount);
		}

		public void TestSelectionThree()
		{
			randomLineDutyData.SecondTariffNumber = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingTariff(randomLineDutyData.FirstTariffNumber);
			CMRTreatmentRatePeriodSnapshot treatmentOne = SetTreatmentMatchingTreatment(false, randomLineDutyData.FirstTreatmentCode);
			CMRTreatmentRatePeriodSnapshot treatmentTwo = SetTreatmentMatchingTreatment(true, randomLineDutyData.SecondTreatmentCode);

			DutySelectionTypeCalculator calculator = new DutySelectionTypeCalculator(treatmentOne, treatmentTwo, randomLineDutyData);
			AssertEquals("Selection type should be 3", 3, calculator.GetSelectionType());

			tariffRate.TT_CustomsValueRate = 5m;
			treatmentOne.TP_CustomsValueRate = 10m;
			treatmentTwo.TP_CustomsValueRate = 15m;

			dutyData.CustomsValueExposed = 2000m;
			CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 200m, dutyCalculator.Duty.Amount.Amount);
		}

		public void TestSelectionFour()
		{
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingTariff(randomLineDutyData.FirstTariffNumber);
			CMRTreatmentRatePeriodSnapshot treatmentOne = SetTreatmentMatchingTreatment(true, randomLineDutyData.FirstTreatmentCode);

			DutySelectionTypeCalculator calculator = new DutySelectionTypeCalculator(treatmentOne, null, randomLineDutyData);
			AssertEquals("Selection type should be 4", 4, calculator.GetSelectionType());

			tariffRate.TT_CustomsValueRate = 5m;
			treatmentOne.TP_CustomsValueRate = 10m;

			dutyData.CustomsValueExposed = 2000m;
			CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 100m, dutyCalculator.Duty.Amount.Amount);
		}

		public void TestSelectionFive()
		{
			randomLineDutyData.SecondTariffNumber = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;

			CMRTariffRatePeriodSnapshot tariffOne = SetTariffRateMatchingTariff(randomLineDutyData.FirstTariffNumber);
			CMRTreatmentRatePeriodSnapshot treatmentOne = SetTreatmentMatchingTreatment(true, randomLineDutyData.FirstTreatmentCode);
			CMRTreatmentRatePeriodSnapshot treatmentTwo = SetTreatmentMatchingTreatment(false, randomLineDutyData.SecondTreatmentCode);

			DutySelectionTypeCalculator calculator = new DutySelectionTypeCalculator(treatmentOne, treatmentTwo, randomLineDutyData);
			AssertEquals("Selection type should be 5", 5, calculator.GetSelectionType());

			tariffOne.TT_CustomsValueRate = 5m;
			treatmentOne.TP_CustomsValueRate = 10m;
			treatmentTwo.TP_CustomsValueRate = 15m;

			dutyData.CustomsValueExposed = 2000m;
			CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 300m, dutyCalculator.Duty.Amount.Amount);
		}

		public void TestSelectionSix()
		{
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;

			CMRTariffRatePeriodSnapshot tariffOne = SetTariffRateMatchingTariff(randomLineDutyData.FirstTariffNumber);
			CMRTariffRatePeriodSnapshot tariffTwo = SetTariffRateMatchingTariff(randomLineDutyData.SecondTariffNumber);
			CMRTreatmentRatePeriodSnapshot treatmentOne = SetTreatmentMatchingTreatment(true, randomLineDutyData.FirstTreatmentCode);

			DutySelectionTypeCalculator calculator = new DutySelectionTypeCalculator(treatmentOne, null, randomLineDutyData);
			AssertEquals("Selection type should be six", 6, calculator.GetSelectionType());

			tariffOne.TT_CustomsValueRate = 5m;
			tariffTwo.TT_CustomsValueRate = 7m;
			treatmentOne.TP_CustomsValueRate = 10m;

			dutyData.CustomsValueExposed = 2000m;
			CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 140m, dutyCalculator.Duty.Amount.Amount);
		}

		public void TestSelectionSeven()
		{
			CMRTariffRatePeriodSnapshot tariffOne = SetTariffRateMatchingTariff(randomLineDutyData.FirstTariffNumber);
			CMRTariffRatePeriodSnapshot tariffTwo = SetTariffRateMatchingTariff(randomLineDutyData.SecondTariffNumber);
			CMRTreatmentRatePeriodSnapshot treatmentOne = SetTreatmentMatchingTreatment(true, randomLineDutyData.FirstTreatmentCode);
			CMRTreatmentRatePeriodSnapshot treatmentTwo = SetTreatmentMatchingTreatment(false, randomLineDutyData.SecondTreatmentCode);

			DutySelectionTypeCalculator calculator = new DutySelectionTypeCalculator(treatmentOne, treatmentTwo, randomLineDutyData);
			AssertEquals("Selection type should be seven", 7, calculator.GetSelectionType());

			tariffOne.TT_CustomsValueRate = 5m;
			tariffTwo.TT_CustomsValueRate = 7m;
			treatmentOne.TP_CustomsValueRate = 10m;
			treatmentTwo.TP_CustomsValueRate = 15m;

			dutyData.CustomsValueExposed = 2000m;
			CMRDutyCalculator dutyCalculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 300m, dutyCalculator.Duty.Amount.Amount);
		}

		#region Implementation

		CMRTariffRatePeriodSnapshot SetTariffRateMatchingTariff(ZString tariffNumber)
		{
			CMRTariffRatePeriodSnapshot result = Factory.New<CMRTariffRatePeriodSnapshot>();
			result.TT_TariffClassificationNumber = tariffNumber;
			result.TT_StartDate = randomLineDutyData.EffectiveDutyDate.AddDays(-1);
			result.TT_EndDate = randomLineDutyData.EffectiveDutyDate;
			result.TT_RateNumber = randomLineDutyData.RateNumber;
			result.TT_PreferenceSchemeType = randomLineDutyData.Preference;
			return result;
		}

		CMRTreatmentRatePeriodSnapshot SetTreatmentMatchingTreatment(bool isInfo, ZString treatmentCodeNumber)
		{
			CMRTreatmentRatePeriodSnapshot result = Factory.New<CMRTreatmentRatePeriodSnapshot>();
			result.TP_Code = treatmentCodeNumber;
			result.TP_StartDate = randomLineDutyData.EffectiveDutyDate.AddDays(-1);
			result.TP_EndDate = randomLineDutyData.EffectiveDutyDate;
			result.TP_RateNumber = randomLineDutyData.RateNumber;
			result.TP_PreferenceSchemeType = randomLineDutyData.Preference;
			if (isInfo)
			{
				result.TP_CalculationType = Constants.DutyCalcTypes.Info;
			}
			return result;
		}

		DummyCMRDutyData dutyData;
		DutyDataFromInvoiceLine randomLineDutyData;

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency = "NZD";

			dutyData = new DummyCMRDutyData(Factory);

			randomLineDutyData = new DutyDataFromInvoiceLine();
			randomLineDutyData.FirstUQ = "LA";
			randomLineDutyData.SecondUQ = "L";
			randomLineDutyData.EffectiveDutyDate = new ZDateTime(2005, 1, 1);
			randomLineDutyData.FirstTariffNumber = "00000000";
			randomLineDutyData.FirstTreatmentCode = "000";
			randomLineDutyData.FirstUQ = "AA";
			randomLineDutyData.Preference = "XX";
			randomLineDutyData.RateNumber = "000";
			randomLineDutyData.SecondTariffNumber = "00000001";
			randomLineDutyData.SecondTreatmentCode = "001";
			randomLineDutyData.SecondUQ = "BB";
			randomLineDutyData.TreatmentRateNumber = "000";

			dutyData.CustomsValueExposed = 10000m;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.FirstQtyExposed = 200m;
			dutyData.SecondQtyExposed = 100m;
			dutyData.OtherDutyFactorExposed = 500m;
			dutyData.TransportAndInsuranceInAUDExposed = 300m;
		}

		#endregion
	}
}
