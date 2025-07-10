using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDutyCalculator : IDutyCalculator
	{
		public CMRDutyCalculator(ICMRDutyData dutyData)
		{
			this.dutyData = dutyData;
			this.randomLineDutyData = dutyData.RandomLineDutyData;
		}

		public DutyResult Duty
		{
			get
			{
				if (!dutyCalculated)
				{
					fDutyResult = CalculateDuty();
					dutyCalculated = true;
				}
				return fDutyResult;
			}
		}
		bool dutyCalculated;
		DutyResult fDutyResult;

		public ZDecimal FlatRateAmount
		{
			get
			{
				if (!dutyCalculated)
				{
					fDutyResult = CalculateDuty();
					dutyCalculated = true;
				}
				return fDutyResult.FlatRateAmount;
			}
		}

		public ZString FlatRateUQ
		{
			get
			{
				if (!dutyCalculated)
				{
					fDutyResult = CalculateDuty();
					dutyCalculated = true;
				}
				return fDutyResult.FlatRateUQ;
			}
		}

		public ZDecimal DutyRate
		{
			get
			{
				if (!dutyCalculated)
				{
					fDutyResult = CalculateDuty();
					dutyCalculated = true;
				}
				return fDutyResult.Percent;
			}
		}

		public ZDecimal GST
		{
			get
			{
				if (!gSTCalculated)
				{
					gSTCached = CalculateGST();
					gSTCalculated = true;
				}
				return gSTCached;
			}
		}
		bool gSTCalculated;
		ZDecimal gSTCached;

		public ZDecimal WET
		{
			get
			{
				if (!wETCalculated)
				{
					wETCached = CalculateWET();
					wETCalculated = true;
				}
				return wETCached;
			}
		}
		bool wETCalculated;
		ZDecimal wETCached;

		public ZDecimal LCT
		{
			get
			{
				if (!lCTCalculated)
				{
					lCTCached = CalculateLCT();
					lCTCalculated = true;
				}
				return lCTCached;
			}
		}
		bool lCTCalculated;
		ZDecimal lCTCached;

		public ZDecimal WoodLevy
		{
			get
			{
				if (!woodLevyCalculated)
				{
					woodLevyCached = CalculateWoodLevy();
					woodLevyCalculated = true;
				}
				return woodLevyCached;
			}
		}
		bool woodLevyCalculated;
		ZDecimal woodLevyCached;

		#region Implementation

		readonly ICMRDutyData dutyData;
		readonly DutyDataFromInvoiceLine randomLineDutyData;

		#region Duty Calculation
		internal DutyResult CalculateDuty(bool alwaysCalculate = false)
		{
			DutyResult result = new DutyResult();
			result.Amount = Money.Empty;

			Money manualDutyAmount = dutyData.ManualDutyAmount;
			if (manualDutyAmount != null && manualDutyAmount.Amount > 0)
			{
				result.Amount = manualDutyAmount;
			}
			else if (dutyData.IsSubjectToDutyAndTax || alwaysCalculate)
			{
				CMRTariffRatePeriodSnapshot tariffOne = CMRTariffRatePeriodSnapshot.Load(dutyData, randomLineDutyData.FirstTariffNumber);
				CMRTariffRatePeriodSnapshot tariffTwo = CMRTariffRatePeriodSnapshot.Load(dutyData, randomLineDutyData.SecondTariffNumber);
				CMRTreatmentRatePeriodSnapshot treatmentOne = CMRTreatmentRatePeriodSnapshot.Load(dutyData, randomLineDutyData.FirstTreatmentCode);
				CMRTreatmentRatePeriodSnapshot treatmentTwo = CMRTreatmentRatePeriodSnapshot.Load(dutyData, randomLineDutyData.SecondTreatmentCode);

				int selectionType = new DutySelectionTypeCalculator(treatmentOne, treatmentTwo, randomLineDutyData).GetSelectionType();

				switch (selectionType)
				{
					case 1:
					case 4:
						result = ComponentCalculator.CalculateDuty(dutyData, tariffOne, tariffOne == null ? null : CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffOne));
						break;
					case 2:
					case 3:
						result = ComponentCalculator.CalculateDuty(dutyData, treatmentOne, treatmentOne == null ? null : CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentOne));
						break;
					case 5:
					case 7:
						result = ComponentCalculator.CalculateDuty(dutyData, treatmentTwo, treatmentTwo == null ? null : CMRTreatmentRatePeriodAdditionalDutyCalculation.Load(treatmentTwo));
						break;
					case 6:
						result = ComponentCalculator.CalculateDuty(dutyData, tariffTwo, tariffTwo == null ? null : CMRTariffRatePeriodAdditionalDutyCalculation.Load(tariffTwo));
						break;
				}

				Money unRounded = result.Amount;
				result.Amount = unRounded.FuzzyRoundDown(2);
			}
			if (!dutyData.RandomLineDutyData.ICN.IsEmpty)
			{
				result.Amount = Money.Empty;
			}
			return result;
		}

		internal class DutySelectionTypeCalculator
		{
			public DutySelectionTypeCalculator(CMRTreatmentRatePeriodSnapshot treatmentOne, CMRTreatmentRatePeriodSnapshot treatmentTwo, DutyDataFromInvoiceLine dutyData)
			{
				this.treatmentOne = treatmentOne;
				this.treatmentTwo = treatmentTwo;
				this.dutyData = dutyData;
			}

			readonly CMRTreatmentRatePeriodSnapshot treatmentOne;
			readonly CMRTreatmentRatePeriodSnapshot treatmentTwo;
			readonly DutyDataFromInvoiceLine dutyData;

			public int GetSelectionType()
			{
				int result = 0;

				if (!dutyData.FirstTariffNumber.IsEmpty)
				{
					if (dutyData.FirstTreatmentCode.IsEmpty && dutyData.SecondTariffNumber.IsEmpty && dutyData.SecondTreatmentCode.IsEmpty)
					{
						result = 1;
					}
					else if (!dutyData.FirstTreatmentCode.IsEmpty && dutyData.SecondTariffNumber.IsEmpty && dutyData.SecondTreatmentCode.IsEmpty
						&& treatmentOne != null && !treatmentOne.IsInformationOnly)
					{
						result = 2;
					}
					else if (!dutyData.FirstTreatmentCode.IsEmpty && dutyData.SecondTariffNumber.IsEmpty && !dutyData.SecondTreatmentCode.IsEmpty
						&& treatmentOne != null && !treatmentOne.IsInformationOnly && treatmentTwo != null && treatmentTwo.IsInformationOnly)
					{
						result = 3;
					}
					else if (!dutyData.FirstTreatmentCode.IsEmpty && treatmentOne != null && treatmentOne.IsInformationOnly)
					{
						if (dutyData.SecondTariffNumber.IsEmpty && dutyData.SecondTreatmentCode.IsEmpty)
						{
							result = 4;
						}
						else if (dutyData.SecondTariffNumber.IsEmpty && !dutyData.SecondTreatmentCode.IsEmpty
							&& treatmentTwo != null && !treatmentTwo.IsInformationOnly)
						{
							result = 5;
						}
						else if (!dutyData.SecondTariffNumber.IsEmpty && dutyData.SecondTreatmentCode.IsEmpty)
						{
							result = 6;
						}
						else if (!dutyData.SecondTariffNumber.IsEmpty && !dutyData.SecondTreatmentCode.IsEmpty
							&& treatmentTwo != null && !treatmentTwo.IsInformationOnly)
						{
							result = 7;
						}
					}
				}
				return result;
			}
		}

		internal DutyComponentCalculator ComponentCalculator
		{
			get
			{
				if (fComponentCalculator == null)
				{
					fComponentCalculator = new DutyComponentCalculator();
				}
				return fComponentCalculator;
			}
		}
		DutyComponentCalculator fComponentCalculator;

		#endregion

		#region GST

		internal ZDecimal CalculateGST()
		{
			ZDecimal result = 0m;
			if (dutyData.IsSubjectToDutyAndTax && !dutyData.RandomLineDutyData.IsGSTExempt)
			{
				result = FuzzyRoundDown(VOTI * DutyCalculator.GSTRate);
			}
			return result;
		}

		ZDecimal FuzzyRoundDown(ZDecimal unRoundedAmount)
		{
			Money result = new Money(unRoundedAmount, JobDeclaration.GetLocalCurrency());
			return result.FuzzyRoundDown(2).Amount;
		}

		#endregion

		#region WET

		public const short WETCharacterCode = 16;
		public const decimal WETRate = .29m;

		internal ZDecimal CalculateWET()
		{
			ZDecimal result = 0m;
			if (dutyData.IsNotLowValueShipment && (!dutyData.IsNature20 || dutyData.IsDutyAndTaxEstimatedForWH) && !randomLineDutyData.IsWETExempt && SubjectToWET)
			{
				var dutyAmount = (dutyData.IsSubjectToDutyAndTax ? Duty : CalculateDuty(alwaysCalculate: true)).Amount.Amount;
				result = FuzzyRoundDown((dutyData.CustomsValue + dutyAmount + dutyData.TransportAndInsuranceInAUD) * WETRate);
			}
			return result;
		}

		internal bool SubjectToWET
		{
			get
			{
				CMRStatisticalClassificationPeriodCharacteristic[] statClassificationCharacters = this.StatClassificationCharacters;
				foreach (CMRStatisticalClassificationPeriodCharacteristic character in statClassificationCharacters)
				{
					if (character.SH_CharacteristicCode == WETCharacterCode)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region LCT

		const string FEVCode = "FEV";
		internal const short LCTCharacterCode = 7;

		protected RefCusTaxOrFee.Loader RefCusTaxOrFeeLoader => new RefCusTaxOrFee.Loader(dutyData.Factory);

		decimal LCTThreshold
		{
			get
			{
				var taxOrFee = RefCusTaxOrFeeLoader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, TaxOrFeeCodeLNT, randomLineDutyData.EffectiveDutyDate.Date);
				return taxOrFee != null ? taxOrFee.ZZF_Value : ZDecimal.Zero;
			}
		}

		decimal FEVThreshold
		{
			get
			{
				var taxOrFee = RefCusTaxOrFeeLoader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, TaxOrFeeCodeLFT, randomLineDutyData.EffectiveDutyDate.Date);
				return taxOrFee != null ? taxOrFee.ZZF_Value : ZDecimal.Zero;
			}
		}

		decimal LCTRate
		{
			get
			{
				var taxOrFee = RefCusTaxOrFeeLoader.LoadMostRecentEffectiveTaxOrFeeFromCodeDate(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, TaxOrFeeCodeLCT, randomLineDutyData.EffectiveDutyDate.Date);
				return taxOrFee != null ? taxOrFee.ZZF_Value : ZDecimal.Zero;
			}
		}

		const string TaxOrFeeCodeLCT = "LCT";
		const string TaxOrFeeCodeLFT = "LFT";
		const string TaxOrFeeCodeLNT = "LNT";

		internal ZDecimal CalculateLCT()
		{
			ZDecimal result = 0m;
			if (randomLineDutyData.IsLCTPayable)
			{
				ZDecimal threshold = randomLineDutyData.LCTE == FEVCode ? FEVThreshold : LCTThreshold;
				ZDecimal thresholdExcess = VOTI + GST - threshold;
				if (dutyData.IsSubjectToDutyAndTax && !randomLineDutyData.IsLCTExempt && SubjectToLCT && thresholdExcess > 0m)
				{
					result = FuzzyRoundDown(thresholdExcess / (1m + DutyCalculator.GSTRate) * LCTRate);
				}
			}
			return result;
		}

		internal bool SubjectToLCT
		{
			get
			{
				CMRStatisticalClassificationPeriodCharacteristic[] statClassificationCharacters = this.StatClassificationCharacters;
				foreach (CMRStatisticalClassificationPeriodCharacteristic character in statClassificationCharacters)
				{
					if (character.SH_CharacteristicCode == LCTCharacterCode)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Wood Levy

		internal ZDecimal CalculateWoodLevy()
		{
			ZDecimal result = 0m;
			if (dutyData.IsNotLowValueShipment)
			{
				string characterCode = GetWoodLevyCharacterCode();
				ZDecimal centsPerCustomsUQ = WoodLevyCharacterRates.GetRateWithCharacterCode(characterCode);
				result = FuzzyRoundDown(dutyData.FirstQty * centsPerCustomsUQ);
			}
			return result;
		}

		string GetWoodLevyCharacterCode()
		{
			string result = "";
			CMRStatisticalClassificationPeriodCharacteristic[] statClassificationCharacters = this.StatClassificationCharacters;
			foreach (CMRStatisticalClassificationPeriodCharacteristic character in statClassificationCharacters)
			{
				if (WoodLevyCharacterRates.HasCharacter(character.SH_CharacteristicCode.ToString()))
				{
					result = character.SH_CharacteristicCode.ToString();
					break;
				}
			}
			return result;
		}

		WoodLevyCharacterRates WoodLevyCharacterRates
		{
			get
			{
				if (fWoodLevyCharacterRates == null)
				{
					fWoodLevyCharacterRates = new WoodLevyCharacterRates();
				}
				return fWoodLevyCharacterRates;
			}
		}
		WoodLevyCharacterRates fWoodLevyCharacterRates;

		#endregion

		#endregion

		#region Related

		ZDecimal VOTI
		{
			get { return dutyData.CustomsValue + dutyData.TransportAndInsuranceInAUD + Duty.Amount.Amount + WET + dutyData.DumpingDuty + dutyData.CountervailingDuty; }
		}

		internal CMRStatisticalClassificationPeriodCharacteristic[] StatClassificationCharacters
		{
			get
			{
				if (fStatClassificationCharacters == null)
				{
					fStatClassificationCharacters = System.Array.Empty<CMRStatisticalClassificationPeriodCharacteristic>();
				}
				if (StatClassification != null)
				{
					ZString tariffNumber = randomLineDutyData.SecondTariffNumber.IsEmpty ? randomLineDutyData.FirstTariffNumber : randomLineDutyData.SecondTariffNumber;
					fStatClassificationCharacters = CMRStatisticalClassificationPeriodCharacteristic.Load(dutyData.Factory, tariffNumber, randomLineDutyData.StatCode, StatClassification.SC_PeriodIdentifier);
				}
				return fStatClassificationCharacters;
			}
		}
		CMRStatisticalClassificationPeriodCharacteristic[] fStatClassificationCharacters;

		internal CMRStatisticalClassificationPeriodSnapshot StatClassification
		{
			get
			{
				if (fStatClassification == null)
				{
					ZString tariffNumber = randomLineDutyData.SecondTariffNumber.IsEmpty ? randomLineDutyData.FirstTariffNumber : randomLineDutyData.SecondTariffNumber;
					fStatClassification = CMRStatisticalClassificationPeriodSnapshot.Load(dutyData.Factory, tariffNumber, randomLineDutyData.StatCode, randomLineDutyData.EffectiveDutyDate);
				}
				return fStatClassification;
			}
		}
		CMRStatisticalClassificationPeriodSnapshot fStatClassification;

		#endregion
	}
}
