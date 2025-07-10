using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class RetailerFeesCalculator : IExtraFeeCalculator
	{
		public RetailerFeesCalculator(CusEntryLine entryLine)
		{
			this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
			destinationIsCanaryIsland = entryLine.RandomLine.DestinationStateIsCanaryIsland;
		}
		readonly CusEntryLine entryLine;
		readonly bool destinationIsCanaryIsland;

		public ZString RateCode => destinationIsCanaryIsland ? UniversalReferenceConstants.RefCusRateCode.RetailerSurcharge : UniversalReferenceConstants.RefCusRateCode.EquivalenceSurcharge;

		public IEnumerable<IDutyCalculationIntermediateResult> CalculateExtraFees()
		{
			var retailerFees = new List<IDutyCalculationIntermediateResult>();
			var invLine = entryLine.RandomLine;
			var code = invLine.IsPVPApplicable ? invLine.ZG_ExciseCode : invLine.JI_ZZF_NKTaxType;
			if (!code.IsEmpty && GetMandatoryElementInEntryLineFees(destinationIsCanaryIsland) != null)
			{
				var factory = entryLine.Factory;
				var valuationDate = ZDateTime.Today;

				var customsVatCode = GetCustomsVATCode(code, factory, valuationDate);
				if (!customsVatCode.IsEmpty)
				{
					var feeRate = GetFeeRate(factory, valuationDate, customsVatCode);
					if (!feeRate.IsEmpty)
					{
						var baseValue = GetBaseValue();
						retailerFees.Add(new DutyCalculationIntermediateResult(baseValue * feeRate, feeRate, baseValue, Customs.Business.UniversalReferenceConstants.MethodOfCalculation.Percentage));
					}
				}
			}

			return retailerFees;
		}

		CusEntryLineFee GetMandatoryElementInEntryLineFees(bool isCanaryIsland)
		{
			var code3IG = UniversalReferenceConstants.RefCusRateCode.IGIC;
			var codeVAT = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			if (isCanaryIsland)
			{
				return entryLine.Fees.GetElementWithThisCode(codeVAT) ?? entryLine.Fees.GetElementWithThisCode(code3IG);
			}
			else
			{
				return entryLine.Fees.GetElementWithThisCode(codeVAT);
			}
		}

		ZDecimal GetBaseValue()
		{
			var baseValue = entryLine.Fees.GetElementWithThisCode(EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)?.CF_BaseValue ?? ZDecimal.Zero;
			if (destinationIsCanaryIsland)
			{
				baseValue += entryLine.Fees.GetElementWithThisCode(UniversalReferenceConstants.RefCusRateCode.AIEM)?.CF_ChargeAmount ?? ZDecimal.Zero;
			}
			return baseValue;
		}

		ZDecimal GetFeeRate(BusinessObjectFactory factory, ZDateTime valuationDate, ZString refCusMapCode)
			=> new RefCusTaxOrFee.Loader(factory).LoadTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Spain, refCusMapCode, valuationDate)
				?.SingleOrDefault(x => x.ZZF_ZX0_NKTaxOrFeeType == UniversalReferenceConstants.RefCusTaxOrFeeType.VAT)
				?.ZZF_Value ?? ZDecimal.Zero;

		ZString GetCustomsVATCode(ZString taxTypeCode, BusinessObjectFactory factory, ZDateTime valuationDate)
			=> ZZRefCusMapCombined.MapCW1CodeToCustomsCode(factory, Core.Constants.CountryCodes.Spain, UniversalReferenceConstants.RefCusMapType.RetailerFee, taxTypeCode, valuationDate);
	}
}
