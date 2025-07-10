using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DutyCalculator : IDutyCalculator
	{
		public const decimal GSTRate = .1M;
		public const string ManualDutyCalculationCode = "99";

		public DutyCalculator(IDutyData dutyData)
		{
			if (dutyData == null)
			{
				throw new ApplicationException("DutyCalculator requires an object that implements IDutyData to be passed to its constructor");
			}
			this.DutyData = dutyData;
		}

		public DutyResult Duty
		{
			get
			{
				var result = new DutyResult();
				result.Amount = new Money(Money.Empty, false);
				return result;
			}
		}

		public Money WineEqualisationTax => Money.Empty;

		public Money LuxuryCarTax
		{
			get
			{
				Money result;
				if (DutyData.AddInfo.ZA_LCTQ.IsEmpty && DutyData.AddInfo.ZA_LCTE.IsEmpty && !DutyData.AddInfo.ZA_LCT.IsEmpty)
				{
					result = new Money(DutyData.AddInfo.ZA_LCT, JobDeclaration.GetLocalCurrency());
				}
				else
				{
					result = Money.Empty;
				}
				return result;
			}
		}

		public Money GSTAmount
		{
			get
			{
				Money result;
				if (!DutyData.IsNature20 && DutyData.AddInfo.ZA_GSTE.IsEmpty)
				{
					//					ZDecimal CustomsValue = Math.Round(DutyData.Price.Amount * DutyData.CustomsFactor, 2);
					ZDecimal tAndI = DutyData.CurrencyConverter.ConvertExact(DutyData.TransportAndInsurance, JobDeclaration.GetLocalCurrency()).Amount;
					result = new Money((CustomsValue.Amount + tAndI + Duty.Amount.Amount + WineEqualisationTax.Amount) * GSTRate, JobDeclaration.GetLocalCurrency()).FuzzyRoundDown(2);
				}
				else
				{
					result = Money.Empty;
				}
				return result;
			}
		}

		public Money WoodLevy => Money.Empty;

		public Money PriceInAUD
		{
			get
			{
				return new Money(DutyData.Price.Amount, JobDeclaration.GetLocalCurrency()).FuzzyRoundDown(2);
			}
		}

		public Money CustomsValue
		{
			get
			{
				ZString vALB = DutyData.AddInfo.AggregatedZA_ValuationBasis_Hidden;
				if (vALB == "IG" || vALB == "SG" || vALB == "DV")
				{
					return PriceInAUD;
				}
				else
				{
					return RoundedAUDCustomsValue;
				}
			}
		}

		//		public bool HasGST
		//		{
		//			get
		//			{
		//				bool Result = true;
		//				Result = TariffStatisticalCode != null ? TariffStatisticalCode.HasGST : Result;
		//				Result = TreatmentCode != null ? TreatmentCode.HasGST : Result;
		//				return Result;
		//			}
		//		}

		#region Implementation

		internal IDutyData DutyData;

		protected decimal WET_RATE
		{
			get
			{
				// As WET changes, keep the original values here for tests to keep passing
				if (DutyData.EffectiveDutyDate < new ZDateTime(2003, 12, 12))
				{
					return .29m;
				}
				else
				{
					return .29m;
				}
			}
		}

		protected Money RoundedAUDCustomsValue
		{
			get
			{
				Money unRoundedAUD = DutyData.CustomsValue;
				return unRoundedAUD.Round(2);
			}
		}

		//		protected Money RoundedAUDCustomsValue
		//		{
		//			get
		//			{
		//				if (DutyData.AddInfo.AdjustmentAmount_Hidden != 0)
		//				{
		//					if (DutyData.AddInfo.AdjustmentDollarPercentage_Hidden == "$")
		//					{
		//						Money AdjustmentAmount = new Money(DutyData.AddInfo.AdjustmentAmount_Hidden, DutyData.AddInfo.AdjustmentCurrency);
		//						return DutyData.CurrencyConverter.Add(RoundedLinePriceTimesFactor, AdjustmentAmount).Round(2);
		//					}
		//					else
		//					{
		//						return (RoundedLinePriceTimesFactor * (1 + DutyData.AddInfo.AdjustmentAmount_Hidden / 100)).Round(2);
		//					}
		//				}
		//				else
		//				{
		//					return RoundedLinePriceTimesFactor;
		//				}
		//			}
		//		}

		#endregion

		#region IDutyCalculator Members

		ZDecimal IDutyCalculator.GST
		{
			get
			{
				return GSTAmount.Amount;
			}
		}

		ZDecimal IDutyCalculator.WET
		{
			get
			{
				return WineEqualisationTax.Amount;
			}
		}

		ZDecimal IDutyCalculator.LCT
		{
			get
			{
				return LuxuryCarTax.Amount;
			}
		}

		ZDecimal IDutyCalculator.WoodLevy
		{
			get
			{
				return WoodLevy.Amount;
			}
		}

		ZDecimal IDutyCalculator.DutyRate
		{
			get
			{
				return Duty.Percent;
			}
		}

		#endregion
	}
}
