using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CMRDutyCalculatorTest : TestCaseWithFactory
	{
		public void TestManualDutyAmountSetInsteadOfCalculated()
		{
			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);

			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingFirstTariff();
			tariffRate.TT_CustomsValueRate = 1m;//100 Duty
			tariffRate.TT_CalculationType = Constants.DutyCalcTypes.Calc;

			CMRTreatmentRatePeriodSnapshot treatment = SetTreatmentMatchingFirstTreatment(false);
			treatment.TP_CustomsValueRate = 0.5m;//50 Duty
			treatment.TP_CalculationType = Constants.DutyCalcTypes.Calc;
			AssertEquals("Duty calculated", 50m, calculator.CalculateDuty().Amount.Amount);

			dutyData.ManualDutyAmountExposed = new Money(300, JobDeclaration.GetLocalCurrency());
			AssertEquals("Duty set from manual duty amount", 300m, calculator.CalculateDuty().Amount.Amount);
		}

		public void TestICNSetZeroInsteadOfCalculated()
		{
			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);

			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingFirstTariff();
			tariffRate.TT_CustomsValueRate = 1m;//100 Duty
			tariffRate.TT_CalculationType = Constants.DutyCalcTypes.Calc;

			CMRTreatmentRatePeriodSnapshot treatment = SetTreatmentMatchingFirstTreatment(false);
			treatment.TP_CustomsValueRate = 0.5m;//50 Duty
			treatment.TP_CalculationType = Constants.DutyCalcTypes.Calc;
			AssertEquals("Duty calculated", 50m, calculator.CalculateDuty().Amount.Amount);

			randomLineDutyData.ICN = "XXXXX";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			DutyResult result = calculator.CalculateDuty();
			AssertEquals("Duty should be zero", 0m, result.Amount.Amount);
			AssertEquals("Duty rate sould not be zero", 0.5m, result.Percent);
		}

		public void TestStatClassification()
		{
			CMRStatisticalClassificationPeriodSnapshot statClassification = SetUpStatClassification("00000000", "00", new ZDateTime(2005, 1, 1));
			CMRStatisticalClassificationPeriodSnapshot statClassification2 = SetUpStatClassification("00000001", "00", new ZDateTime(2005, 1, 1));

			SetDummyDutyData();
			randomLineDutyData.FirstTariffNumber = "0000.00.00";
			randomLineDutyData.SecondTariffNumber = "0000.00.01";
			randomLineDutyData.StatCode = "00";
			randomLineDutyData.EffectiveDutyDate = new ZDateTime(2005, 1, 1);

			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("StatClassification", statClassification2, calculator.StatClassification);

			randomLineDutyData.SecondTariffNumber = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("StatClassification", statClassification, calculator.StatClassification);
		}

		public void TestStatClassificationCharacters()
		{
			CMRStatisticalClassificationPeriodSnapshot statClassification0 = SetUpStatClassification("00000000", "00", new ZDateTime(2005, 1, 1));
			statClassification0.SC_PeriodIdentifier = (short)1;

			CMRStatisticalClassificationPeriodSnapshot statClassification1 = SetUpStatClassification("00000001", "00", new ZDateTime(2005, 1, 1));
			statClassification1.SC_PeriodIdentifier = (short)1;

			CMRStatisticalClassificationPeriodSnapshot statClassification2_1 = SetUpStatClassification("00000001", "00", new ZDateTime(2005, 1, 1));
			statClassification2_1.SC_EndDate = new ZDateTime(2005, 1, 1);
			statClassification2_1.SC_PeriodIdentifier = (short)1;

			CMRStatisticalClassificationPeriodSnapshot statClassification2_2 = SetUpStatClassification("00000001", "00", new ZDateTime(2005, 1, 2));
			statClassification2_2.SC_PeriodIdentifier = (short)2;

			CMRStatisticalClassificationPeriodCharacteristic character0 = SetUpCharacter("00000000", "00", statClassification0.SC_PeriodIdentifier, (short)99);
			CMRStatisticalClassificationPeriodCharacteristic character1 = SetUpCharacter("00000001", "00", statClassification1.SC_PeriodIdentifier, (short)100);
			CMRStatisticalClassificationPeriodCharacteristic character2 = SetUpCharacter("00000001", "00", statClassification2_1.SC_PeriodIdentifier, (short)101);
			CMRStatisticalClassificationPeriodCharacteristic character3 = SetUpCharacter("00000001", "00", statClassification2_2.SC_PeriodIdentifier, (short)102);

			SetDummyDutyData();
			randomLineDutyData.FirstTariffNumber = "00000000";
			randomLineDutyData.SecondTariffNumber = "00000001";
			randomLineDutyData.StatCode = "00";
			randomLineDutyData.EffectiveDutyDate = new ZDateTime(2005, 1, 1);

			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);

			CMRStatisticalClassificationPeriodCharacteristic[] characters = calculator.StatClassificationCharacters;
			AssertEquals("Two characters", 2, characters.Length);

			bool hasSeenCharacter2 = false;
			bool hasSeenCharacter3 = false;

			foreach (CMRStatisticalClassificationPeriodCharacteristic character in characters)
			{
				if (character == character2)
				{
					hasSeenCharacter2 = true;
				}
				else
				{
					hasSeenCharacter3 = true;
				}
			}

			AssertEquals("HasSeenCharacter2", true, hasSeenCharacter2);
			AssertEquals("HasSeenCharacter3", true, hasSeenCharacter3);
		}

		public void TestSubjectToLCT()
		{
			CMRStatisticalClassificationPeriodSnapshot statClassification = SetUpStatClassification("00000000", "00", new ZDateTime(2005, 1, 1));
			statClassification.SC_PeriodIdentifier = (short)1;

			CMRStatisticalClassificationPeriodCharacteristic character = SetUpCharacter("00000000", "00", (short)1, CMRDutyCalculator.LCTCharacterCode);

			SetDummyDutyData();
			randomLineDutyData.FirstTariffNumber = "0000.00.00";
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.StatCode = "00";
			randomLineDutyData.EffectiveDutyDate = new ZDateTime(2005, 1, 1);

			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Subject to LCT", true, calculator.SubjectToLCT);
		}

		public void TestWoodLevy()
		{
			SetDummyDutyData();

			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;

			CMRStatisticalClassificationPeriodSnapshot statClassfification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassfification.SC_TariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			statClassfification.SC_StatisticalClassificationCode = randomLineDutyData.StatCode;
			statClassfification.SC_PeriodIdentifier = (short)0;
			statClassfification.SC_StartDate = randomLineDutyData.EffectiveDutyDate;

			CMRStatisticalClassificationPeriodCharacteristic character = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			character.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			character.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = randomLineDutyData.StatCode;
			character.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = statClassfification.SC_PeriodIdentifier;
			character.SH_CharacteristicCode = (short)23;

			calculator = new CMRDutyCalculator(dutyData);
			ZDecimal result = 200m * 0.58m;
			AssertEquals("Wood levy is calculated", 116m, calculator.CalculateWoodLevy());

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Wood levy calculated", 116m, calculator.CalculateWoodLevy());

			dutyData.IsNotLowValueShipmentExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Wood levy calculated", 0m, calculator.CalculateWoodLevy());
		}

		ZDateTime EffectiveDateForLCT
		{
			get { return new ZDateTime(2013, 7, 1); }
		}

		ZDecimal PreviousValueForLCT
		{
			get { return decimal.Truncate((59133m - 330m) / 1.1m); } // alow for T&I and GST
		}

		ZDecimal NewValueForLCT
		{
			get { return decimal.Truncate((60316m - 330m) / 1.1m); } // alow for T&I and GST
		}

		public void TestCalculateLCTOldRate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LNT", new ZDecimal(60316), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2013, 7, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("LNT", new ZDecimal(59133), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), new ZDateTime(2013, 6, 30, 23, 59, 59));
			helper.CreateTaxOrFee("LCT", new ZDecimal(0.33), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			randomLineDutyData.LCTE = "OTHER";
			randomLineDutyData.EffectiveDutyDate = EffectiveDateForLCT.AddDays(-1);
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = 100000m;

			CMRStatisticalClassificationPeriodSnapshot statClassfification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassfification.SC_TariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			statClassfification.SC_StatisticalClassificationCode = randomLineDutyData.StatCode;
			statClassfification.SC_PeriodIdentifier = (short)0;
			statClassfification.SC_StartDate = randomLineDutyData.EffectiveDutyDate;

			CMRStatisticalClassificationPeriodCharacteristic character = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			character.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			character.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = randomLineDutyData.StatCode;
			character.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = statClassfification.SC_PeriodIdentifier;
			character.SH_CharacteristicCode = CMRDutyCalculator.LCTCharacterCode;

			dutyData.IsNature20Exposed = true;
			AssertEquals("Nature 20 does not attract LCT", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTPayable = true;

			dutyData.IsNature20Exposed = false;
			randomLineDutyData.IsLCTExempt = true;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT Exempt", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTExempt = false;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = PreviousValueForLCT - 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Customs Value less than threshold", 0m, calculator.CalculateLCT());

			dutyData.CustomsValueExposed = PreviousValueForLCT + 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 0.24m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.CustomsValueExposed = PreviousValueForLCT + 50000m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 16499.91m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT calculated", 0m, calculator.CalculateLCT());

			dutyData.IsSubjectToDutyAndTaxExposed = true;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 16499.91m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			randomLineDutyData.IsLCTPayable = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT calculated", 0m, calculator.CalculateLCT());
		}

		public void TestCalculateLCTNewRate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LCT", new ZDecimal(0.33), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("LNT", new ZDecimal(64132), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), new ZDateTime(2017, 6, 30, 23, 59, 59));
			helper.CreateTaxOrFee("LNT", new ZDecimal(65094), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2017, 7, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			randomLineDutyData.LCTE = "OTHER";
			randomLineDutyData.EffectiveDutyDate = EffectiveDateForLCT;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = 100000m;

			CMRStatisticalClassificationPeriodSnapshot statClassfification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassfification.SC_TariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			statClassfification.SC_StatisticalClassificationCode = randomLineDutyData.StatCode;
			statClassfification.SC_PeriodIdentifier = (short)0;
			statClassfification.SC_StartDate = randomLineDutyData.EffectiveDutyDate;

			CMRStatisticalClassificationPeriodCharacteristic character = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			character.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			character.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = randomLineDutyData.StatCode;
			character.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = statClassfification.SC_PeriodIdentifier;
			character.SH_CharacteristicCode = CMRDutyCalculator.LCTCharacterCode;

			dutyData.IsNature20Exposed = true;
			AssertEquals("Nature 20 does not attract LCT", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTPayable = true;

			dutyData.IsNature20Exposed = false;
			randomLineDutyData.IsLCTExempt = true;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT Exempt", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTExempt = false;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = NewValueForLCT - 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Customs Value less than threshold", 0m, calculator.CalculateLCT());

			dutyData.CustomsValueExposed = NewValueForLCT + 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 0m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.CustomsValueExposed = NewValueForLCT + 50000m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 15354.96m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT calculated", 0m, calculator.CalculateLCT());

			dutyData.IsSubjectToDutyAndTaxExposed = true;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 15354.96m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			randomLineDutyData.IsLCTPayable = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT calculated", 0m, calculator.CalculateLCT());
		}

		ZDateTime EffectiveDateForFEVLCT
		{
			get { return new ZDateTime(2010, 7, 1); }
		}

		ZDecimal PreviousValueForFEVLCT
		{
			get { return decimal.Truncate((75000m - 330m) / 1.1m); } // alow for T&I and GST
		}

		ZDecimal NewValueForFEVLCT
		{
			get { return decimal.Truncate((75375m - 330m) / 1.1m); } // alow for T&I and GST
		}

		public void TestCalculateLCTforFEVOldRate()
		{
			// when threshold is updated in registry change these tests and update the above
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LCT", new ZDecimal(0.33), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("LFT", new ZDecimal(75526), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			randomLineDutyData.LCTE = "FEV";
			randomLineDutyData.EffectiveDutyDate = EffectiveDateForFEVLCT.AddDays(-1);
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = 100000m;

			CMRStatisticalClassificationPeriodSnapshot statClassfification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassfification.SC_TariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			statClassfification.SC_StatisticalClassificationCode = randomLineDutyData.StatCode;
			statClassfification.SC_PeriodIdentifier = (short)0;
			statClassfification.SC_StartDate = randomLineDutyData.EffectiveDutyDate;

			CMRStatisticalClassificationPeriodCharacteristic character = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			character.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			character.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = randomLineDutyData.StatCode;
			character.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = statClassfification.SC_PeriodIdentifier;
			character.SH_CharacteristicCode = CMRDutyCalculator.LCTCharacterCode;

			dutyData.IsNature20Exposed = true;
			AssertEquals("Nature 20 does not attract LCT", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTPayable = true;

			dutyData.IsNature20Exposed = false;
			randomLineDutyData.IsLCTExempt = true;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT Exempt", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTExempt = false;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = PreviousValueForFEVLCT - 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Customs Value less than threshold", 0m, calculator.CalculateLCT());

			dutyData.CustomsValueExposed = PreviousValueForFEVLCT + 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 0m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.CustomsValueExposed = PreviousValueForFEVLCT + 50000m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 16341.93m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT calculated", 0m, calculator.CalculateLCT());
		}

		public void TestCalculateLCTforFEVNewRate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("LCT", new ZDecimal(0.33), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(2010, 1, 1), new ZDateTime(2079, 6, 6));
			helper.CreateTaxOrFee("LFT", new ZDecimal(75526), GlbCompany.CurrentCompany.GC_RN_NKCountryCode, new ZDateTime(1970, 7, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			randomLineDutyData.LCTE = "FEV";
			randomLineDutyData.EffectiveDutyDate = EffectiveDateForFEVLCT;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = 100000m;

			CMRStatisticalClassificationPeriodSnapshot statClassfification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassfification.SC_TariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			statClassfification.SC_StatisticalClassificationCode = randomLineDutyData.StatCode;
			statClassfification.SC_PeriodIdentifier = (short)0;
			statClassfification.SC_StartDate = randomLineDutyData.EffectiveDutyDate;

			CMRStatisticalClassificationPeriodCharacteristic character = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			character.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			character.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = randomLineDutyData.StatCode;
			character.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = statClassfification.SC_PeriodIdentifier;
			character.SH_CharacteristicCode = CMRDutyCalculator.LCTCharacterCode;

			dutyData.IsNature20Exposed = true;
			AssertEquals("Nature 20 does not attract LCT", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTPayable = true;

			dutyData.IsNature20Exposed = false;
			randomLineDutyData.IsLCTExempt = true;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT Exempt", 0m, calculator.CalculateLCT());

			randomLineDutyData.IsLCTExempt = false;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CustomsValueExposed = NewValueForFEVLCT - 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Customs Value less than threshold", 0m, calculator.CalculateLCT());

			dutyData.CustomsValueExposed = NewValueForFEVLCT + 1m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 0m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.CustomsValueExposed = NewValueForFEVLCT + 50000m;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Calculated", 16454.46m, calculator.CalculateLCT());// may vary slightly with a new threshold due to rounding

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("LCT calculated", 0m, calculator.CalculateLCT());
		}

		public void TestCalculateWET()
		{
			SetDummyDutyData();
			randomLineDutyData.StatCode = "00";

			randomLineDutyData.FirstTreatmentCode = "";
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";

			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingFirstTariff();
			tariffRate.TT_CustomsValueRate = 1m;//100 Duty
			tariffRate.TT_CalculationType = Constants.DutyCalcTypes.Calc;

			dutyData.RandomLineDutyDataExposed = randomLineDutyData;

			CMRStatisticalClassificationPeriodSnapshot statClassification = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			statClassification.SC_TariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			statClassification.SC_StatisticalClassificationCode = randomLineDutyData.StatCode;
			statClassification.SC_PeriodIdentifier = (short)0;
			statClassification.SC_StartDate = randomLineDutyData.EffectiveDutyDate;

			dutyData.IsNature20Exposed = true;
			calculator = new CMRDutyCalculator(dutyData);

			AssertNotNull("StatClassification ", calculator.StatClassification);
			AssertEquals("Nature 20 does not attract WET", 0m, calculator.CalculateWET());

			dutyData.IsNature20Exposed = false;
			CMRStatisticalClassificationPeriodCharacteristic statTariffCharacter = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			statTariffCharacter.SH_CharacteristicCode = CMRDutyCalculator.WETCharacterCode;
			statTariffCharacter.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = statClassification.SC_PeriodIdentifier;
			statTariffCharacter.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = randomLineDutyData.FirstTariffNumber;
			statTariffCharacter.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = randomLineDutyData.StatCode;

			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Subject to WET", true, calculator.SubjectToWET);
			AssertEquals("WET calculated", 3016m, calculator.CalculateWET());
			AssertEquals("WET calculated", 3016m, calculator.WET);

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("WET calculated", 3016m, calculator.CalculateWET());
			dutyData.IsNotLowValueShipmentExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("WET calculated", 0m, calculator.CalculateWET());
		}

		public void TestWETWithTemporaryImportEndToEnd()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tax1 = helper.CreateTaxOrFee("DAN", 50M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax1.ZZF_Threshold = 10000m;
			var tax2 = helper.CreateTaxOrFee("DAH", 152M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			tax2.ZZF_Threshold = 10000m;
			helper.CreateTaxOrFee("Q1A", 33M, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, 0, 0, "FLA", new ZDateTime(1970, 1, 1), new ZDateTime(2079, 6, 6));
			Factory.Save();

			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 2000;
			invoice.JZ_RX_NKInvoice_Currency = "AUD";
			invoice.AddInfo.ZA_PST = "GEN";
			var line = invoice.JobComInvoiceLines.AddNew();
			line.JI_LinePrice = 2000m;
			line.JI_Tariff = "2204.21.20 71";
			line.AddInfo.ZA_TreatmentCode_Hidden = "352";
			line.JI_CustomsUnitQty = "L";
			line.JI_CustomsQuantity = 1m;
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
			AssertEquals("Duty should be zero", 0m, entryHeader.DutyAmount);
			AssertEquals("GST should be zero", 0m, entryHeader.GSTAmount);
			AssertEquals("WET should apply", 609m, entryHeader.WETAmount);
			AssertEquals("Charge should apply", 50m, entryHeader.DeclarationProcessingCharge);

			line.AddInfo.ZA_TreatmentCode_Hidden = "";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
			AssertEquals("Duty should apply", 100m, entryHeader.DutyAmount);
			AssertEquals("GST should apply", 270.9m, entryHeader.GSTAmount);
			AssertEquals("WET should apply", 609m, entryHeader.WETAmount);
			AssertEquals("Charge should apply", 50m, entryHeader.DeclarationProcessingCharge);

			line.AddInfo.ZA_TreatmentCode_Hidden = "354";
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders[0];
			AssertEquals("Duty should be zero", 0m, entryHeader.DutyAmount);
			AssertEquals("GST should be zero", 0m, entryHeader.GSTAmount);
			AssertEquals("WET should apply", 609m, entryHeader.WETAmount);
			AssertEquals("Charge should not apply", 0m, entryHeader.DeclarationProcessingCharge);
		}

		public void TestCalculateDuty()
		{
			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;

			calculator = new CMRDutyCalculator(dutyData);

			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingFirstTariff();
			tariffRate.TT_CustomsValueRate = 1m;//100 Duty
			tariffRate.TT_CalculationType = Constants.DutyCalcTypes.Calc;

			CMRTreatmentRatePeriodSnapshot treatment = SetTreatmentMatchingFirstTreatment(false);
			treatment.TP_CustomsValueRate = 0.5m;//50 Duty
			treatment.TP_CalculationType = Constants.DutyCalcTypes.Calc;

			AssertEquals("Duty calculated", 50m, calculator.CalculateDuty().Amount.Amount);

			treatment.TP_CalculationType = Constants.DutyCalcTypes.Info;
			AssertEquals("Duty calculated", 100m, calculator.CalculateDuty().Amount.Amount);
			AssertEquals("Duty", 100m, calculator.Duty.Amount.Amount);
		}

		public void TestFlatRate()
		{
			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.FirstTreatmentCode = "";
			randomLineDutyData.SecondTreatmentCode = "";
			randomLineDutyData.FirstUQ = "LA";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);

			CMRTariffRatePeriodSnapshot tariffRate = SetTariffRateMatchingFirstTariff();
			tariffRate.TT_QuantityRate = 10m;//10 dollars
			tariffRate.TT_QuantityUnit = "LA";
			tariffRate.TT_RateNumber = randomLineDutyData.RateNumber;
			tariffRate.TT_CalculationType = Constants.DutyCalcTypes.Calc;

			AssertEquals("Duty calculated", 2000m, calculator.CalculateDuty().Amount.Amount);
			AssertEquals("Duty FlatRate", 10m, calculator.CalculateDuty().FlatRateAmount);
			AssertEquals("Duty FlatRateUQ", "LA", calculator.CalculateDuty().FlatRateUQ);

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 0m, calculator.CalculateDuty().Amount.Amount);
		}

		public void TestCalculateGST()
		{
			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			CMRTreatmentRatePeriodSnapshot treatment1 = SetTreatmentMatchingFirstTreatment(false);
			treatment1.TP_CustomsValueRate = 0.5m;//50 Duty
			treatment1.TP_CalculationType = Constants.DutyCalcTypes.Calc;
			calculator = new CMRDutyCalculator(dutyData);

			AssertEquals("GST calculated", 1035m, calculator.CalculateGST());
			AssertEquals("GST calculated", 1035m, calculator.GST);

			randomLineDutyData.IsGSTExempt = true;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("GST exempt", 0m, calculator.CalculateGST());

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 0m, calculator.CalculateGST());
		}

		public void TestCalculateGSTWithOtherDuties()
		{
			SetDummyDutyData();
			randomLineDutyData.SecondTariffNumber = "";
			randomLineDutyData.SecondTreatmentCode = "";
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			dutyData.CountervailingDutyExposed = 1000m;
			dutyData.DumpingDutyExposed = 2000m;

			CMRTreatmentRatePeriodSnapshot treatment1 = SetTreatmentMatchingFirstTreatment(false);
			treatment1.TP_CustomsValueRate = 0.5m;//50 Duty
			treatment1.TP_CalculationType = Constants.DutyCalcTypes.Calc;
			calculator = new CMRDutyCalculator(dutyData);

			AssertEquals("GST calculated", 1335m, calculator.CalculateGST());
			AssertEquals("GST calculated", 1335m, calculator.GST);

			randomLineDutyData.IsGSTExempt = true;
			dutyData.RandomLineDutyDataExposed = randomLineDutyData;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("GST exempt", 0m, calculator.CalculateGST());

			dutyData.IsSubjectToDutyAndTaxExposed = false;
			calculator = new CMRDutyCalculator(dutyData);
			AssertEquals("Duty calculated", 0m, calculator.CalculateGST());
		}

		public void TestComponentCalculator()
		{
			AssertNotNull("Component Calculator", calculator.ComponentCalculator);
		}

		#region Implementation

		CMRStatisticalClassificationPeriodCharacteristic SetUpCharacter(ZString tariff, ZString statCode, ZShort periodID, ZShort characterCode)
		{
			CMRStatisticalClassificationPeriodCharacteristic result = CMRStatisticalClassificationPeriodCharacteristic.New(Factory);
			result.SH_StatisticalClassificationPeriodSnapshotTariffClassificationNumber = tariff;
			result.SH_StatisticalClassificationPeriodSnapshotStatisticalClassificationCode = statCode;
			result.SH_StatisticalClassificationPeriodSnapshotPeriodIdentifier = periodID;
			result.SH_CharacteristicCode = characterCode;
			return result;
		}

		CMRStatisticalClassificationPeriodSnapshot SetUpStatClassification(ZString tariffNumber, ZString statCode, ZDateTime dutyDate)
		{
			CMRStatisticalClassificationPeriodSnapshot result = CMRStatisticalClassificationPeriodSnapshot.New(Factory);
			result.SC_TariffClassificationNumber = tariffNumber;
			result.SC_StatisticalClassificationCode = statCode;
			result.SC_StartDate = dutyDate;
			return result;
		}

		CMRTreatmentRatePeriodSnapshot SetTreatmentMatchingFirstTreatment(bool isInfo)
		{
			CMRTreatmentRatePeriodSnapshot result = Factory.New<CMRTreatmentRatePeriodSnapshot>();
			result.TP_Code = randomLineDutyData.FirstTreatmentCode;
			result.TP_PreferenceSchemeType = randomLineDutyData.Preference;
			result.TP_StartDate = randomLineDutyData.EffectiveDutyDate.AddDays(-1);
			result.TP_EndDate = randomLineDutyData.EffectiveDutyDate;
			result.TP_RateNumber = randomLineDutyData.RateNumber;
			if (isInfo)
			{
				result.TP_CalculationType = Constants.DutyCalcTypes.Info;
			}
			return result;
		}

		CMRTariffRatePeriodSnapshot SetTariffRateMatchingFirstTariff()
		{
			CMRTariffRatePeriodSnapshot result = Factory.New<CMRTariffRatePeriodSnapshot>();
			result.TT_TariffClassificationNumber = "00000000";
			result.TT_PreferenceSchemeType = randomLineDutyData.Preference;
			result.TT_StartDate = randomLineDutyData.EffectiveDutyDate.AddDays(-1);
			result.TT_EndDate = randomLineDutyData.EffectiveDutyDate;
			result.TT_RateNumber = randomLineDutyData.RateNumber;
			return result;
		}

		DummyCMRDutyData dutyData;
		CMRDutyCalculator calculator;
		DutyDataFromInvoiceLine randomLineDutyData;

		protected override void SetUp()
		{
			base.SetUp();
			dutyData = new DummyCMRDutyData(Factory);
			calculator = new CMRDutyCalculator(dutyData);

			randomLineDutyData = new DutyDataFromInvoiceLine();
			randomLineDutyData.FirstUQ = "LA";
			randomLineDutyData.SecondUQ = "L";
			randomLineDutyData.EffectiveDutyDate = new ZDateTime(2005, 1, 1);
			randomLineDutyData.FirstTariffNumber = "00000000";
			randomLineDutyData.FirstTreatmentCode = "000";
			randomLineDutyData.FirstUQ = "AA";
			randomLineDutyData.Preference = "XX";
			randomLineDutyData.RateNumber = "000";
			randomLineDutyData.TreatmentRateNumber = "000";
			randomLineDutyData.SecondTariffNumber = "00000001";
			randomLineDutyData.SecondTreatmentCode = "001";
			randomLineDutyData.SecondUQ = "BB";
		}

		void SetDummyDutyData()
		{
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
