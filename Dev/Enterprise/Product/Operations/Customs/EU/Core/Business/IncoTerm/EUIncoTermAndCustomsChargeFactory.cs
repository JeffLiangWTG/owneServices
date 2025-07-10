using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business
{
	public class EUIncoTermAndCustomsChargeFactory : CommonIncoTermAndCustomsChargeFactory
	{
		public EUIncoTermAndCustomsChargeFactory()
		{ }

		protected override ICustomsChargeCode[] GetCharges()
		{
			var result = new List<ICustomsChargeCode>(base.GetCharges());
			var oftCharge = result.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.OverseasFreight);
			if (oftCharge != null)
			{
				result.Remove(oftCharge);
			}
			var onsCharge = result.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.OverseasInsurance);
			if (onsCharge != null)
			{
				result.Remove(onsCharge);
			}
			result.Add(CreateFreightToEUBorder());
			result.Add(CreateInternationalInsurance());
			result.Add(ChargeCodeProvider.StatisticalValue);
			return SetCustomsChargeCode(result.ToArray());
		}

		CustomsChargeCode CreateInternationalInsurance() => new CustomsChargeCode(CustomsChargeTypeList.Codes.OverseasInsurance, CustomsChargeTypeList.Descriptions.OverseasInsurance)
		{
			IsDutiable = true,  // Bingo!
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicable = true,
			IsStatisticalValueApplicableDeemed = true
		};

		protected CustomsChargeCode CreateFreightToEUBorder() => new CustomsChargeCode(FreightToEUBorderCodeCore, FreightToEUBorderDesc)
		{
			IsDutiable = true,
			IsVATible = true,
			IsStatisticalValueApplicable = true,
			IsDutiableDeemedForThisCharge = false,
			IsVATibleDeemedForThisCharge = true,
			IsStatisticalValueApplicableDeemed = false,
		};

		public bool ShouldCreatedFreightChargeBeIncludedInITOT { get; set; }

		protected virtual string FreightToEUBorderCodeCore => ChargeTypeList.Codes.InternationalFreight;
		public string FreightToEUBorderCode => FreightToEUBorderCodeCore;
		protected virtual MultilingualString FreightToEUBorderDesc => ChargeTypeList.Descriptions.InternationalFreight;
		public string FreightAfterEUBorderCode => FreightAfterEUBorderCodeCore;
		protected virtual string FreightAfterEUBorderCodeCore => ChargeTypeList.Codes.InternationalFreight;
		protected virtual MultilingualString FreightAfterEUBorderDesc => ChargeTypeList.Descriptions.InternationalFreight;
		public virtual string FreightDomesticCode => ChargeTypeList.Codes.InternationalFreight;

		public string InsuranceChargeCode => InsuranceChargeCodeCore;

		protected virtual string InsuranceChargeCodeCore => ChargeTypeList.Codes.InternationalInsurance;

		public override bool MakeFlagsReadOnlyWhenDeemed => true;

		protected virtual ICustomsChargeCode[] SetCustomsChargeCode(ICustomsChargeCode[] result)
		{
			var dedCharge = result.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.DeductionCharge);
			if (dedCharge is CustomsChargeCode dedCode)
			{
				dedCode.IsDutiableDeemedForThisCharge = false;
				dedCode.IsStatisticalValueApplicableDeemed = false;
			}
			var addCharge = result.FirstOrDefault(x => x.Code == CustomsChargeTypeList.Codes.AdditionCharge);
			if (addCharge is CustomsChargeCode addCode)
			{
				addCode.IsDutiableDeemedForThisCharge = false;
				addCode.IsStatisticalValueApplicableDeemed = false;
			}
			return result;
		}

		public virtual void SetupToEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FreightToEUBorderCodeCore;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
			charge.J7_IsDutiable = true;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsStatisticalValueApplicable = true;
			charge.J7_IsIncludedInITOT = ShouldCreatedFreightChargeBeIncludedInITOT;
		}

		public virtual void SetupAfterEUBorderCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FreightAfterEUBorderCode;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsStatisticalValueApplicable = true;
			charge.J7_IsIncludedInITOT = ShouldCreatedFreightChargeBeIncludedInITOT;
		}

		public virtual void SetupDomesticCharge(JobComInvCharge charge, ZDecimal amount, ZString currency)
		{
			charge.J7_ChargeType = FreightDomesticCode;
			charge.J7_Amount = amount;
			charge.J7_RX_NKCurrency = currency;
			charge.J7_IsDutiable = false;
			charge.J7_IsGSTApplicable = true;
			charge.J7_IsStatisticalValueApplicable = false;
			charge.J7_IsIncludedInITOT = ShouldCreatedFreightChargeBeIncludedInITOT;
		}

		public virtual void AddOrUpdateInsuranceCharge(IJobComInvChargeCollection<JobComInvCharge> charges, ZDecimal amount, ZString currency, ZDecimal dutiablePercent, bool includeInITOT)
		{
			var dutiableCharge = charges.FirstOrDefault(x => x.J7_ChargeType == InsuranceChargeCode && x.J7_IsDutiable == true);
			var nondutiableCharge = charges.FirstOrDefault(x => x.J7_ChargeType == InsuranceChargeCode && x.J7_IsDutiable == false);

			if (dutiablePercent > 0m)
			{
				if (dutiableCharge == null)
				{
					dutiableCharge = charges.AddNew();

					dutiableCharge.J7_ChargeType = InsuranceChargeCode;
					dutiableCharge.J7_IsDutiable = true;
					dutiableCharge.J7_IsGSTApplicable = true;
					dutiableCharge.J7_IsStatisticalValueApplicable = true;
				}

				dutiableCharge.J7_IsIncludedInITOT = includeInITOT;
				dutiableCharge.J7_Amount = amount * dutiablePercent / 100m;
				dutiableCharge.J7_RX_NKCurrency = currency;
			}

			if (dutiablePercent < 100m)
			{
				if (nondutiableCharge == null)
				{
					nondutiableCharge = charges.AddNew();

					nondutiableCharge.J7_ChargeType = InsuranceChargeCode;
					nondutiableCharge.J7_IsDutiable = false;
					nondutiableCharge.J7_IsGSTApplicable = true;
					nondutiableCharge.J7_IsStatisticalValueApplicable = false;
				}

				nondutiableCharge.J7_IsIncludedInITOT = includeInITOT;
				nondutiableCharge.J7_Amount = amount * (100m - dutiablePercent) / 100m;
				nondutiableCharge.J7_RX_NKCurrency = currency;
			}

			if (dutiablePercent == 0m || dutiableCharge?.J7_Amount.Round(2) == 0.00m)
			{
				dutiableCharge?.Delete();
			}

			if (dutiablePercent == 100m || nondutiableCharge?.J7_Amount.Round(2) == 0.00m)
			{
				nondutiableCharge?.Delete();
			}
		}
	}
}

