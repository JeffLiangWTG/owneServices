using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class DutyCalculatorStrategy : EU.Business.Declaration.DutyCalculatorStrategy
	{
		public DutyCalculatorStrategy(JobDeclaration declaration)
			: base(declaration)
		{
		}

		JobDeclaration Declaration => (JobDeclaration)base.declaration;

		protected override bool ShouldDutyCalculationIncludeNonParticipatingMinMaxResults => true;

		protected override void CalculateEntryLineVatFee(EU.Business.Declaration.CusEntryLine entryLine)
		{
			CalculatePreVatEntryLineExtraFees(entryLine);
			base.CalculateEntryLineVatFee(entryLine);
		}

		protected override IEnumerable<IExtraFeeCalculator> GetExtraFeeCalculatorCollectionCore(EU.Business.Declaration.CusEntryLine entryLine)
		{
			var esEntryLine = (CusEntryLine)entryLine;
			if (Declaration.IsImport)
			{
				yield return new NonRecycledPlasticTaxCalculator(esEntryLine);
			}
		}

		void CalculatePreVatEntryLineExtraFees(EU.Business.Declaration.CusEntryLine entryLine)
		{
			var esEntryLine = (CusEntryLine)entryLine;
			var randomLine = esEntryLine.RandomLine;

			if (randomLine.CurrentExciseRate != null)
			{
				IExtraFeeCalculator exciseCalculator = new ExciseCalculator(esEntryLine);
				CalculateExtraSystemFeesIfApplicable(esEntryLine, exciseCalculator);
				IExtraFeeCalculator specialExciseCalculator = new ExciseSpecialRateCalculator(esEntryLine);
				CalculateExtraSystemFeesIfApplicable(esEntryLine, specialExciseCalculator);
			}
		}

		protected override IEnumerable<IExtraFeeCalculator> GetNonVatableExtraFeeCalculatorCollectionCore(EU.Business.Declaration.CusEntryLine entryLine)
		{
			var esEntryLine = (CusEntryLine)entryLine;
			var randomLine = esEntryLine.RandomLine;
			if (!randomLine.ZG_AIEMType.IsEmpty && randomLine.DestinationStateIsCanaryIsland)
			{
				yield return new AIEMTaxCalculator(esEntryLine);
			}
			var importerIsRetailer = ESOrgImpAddInfo.Get(randomLine.Importer)?.ZO_Retailer ?? false;
			if (randomLine.IsImport && importerIsRetailer)
			{
				yield return new RetailerFeesCalculator(esEntryLine);
			}
		}

		protected override void SetChargeAmount(EU.Business.Declaration.CusEntryLine entryLine, EU.Business.Declaration.CusEntryLineFee entryLineFee, IDutyCalculationIntermediateResult intermediateResult)
		{
			var chargeType = entryLineFee.CF_ChargeType;

			if (IsExciseFeeExempted(((CusEntryLine)entryLine).RandomLine, chargeType))
			{
				entryLineFee.CF_ChargeAmount = 0;
				entryLineFee.CF_Rate = 0;
			}
			else if (intermediateResult.MethodOfCalculation != MethodOfCalculation.Percentage && Regex.IsMatch(chargeType, @"^[012456789A]"))
			{
				var spanishCustomsValue = (ZDecimal)entryLine.InvoiceLines.Sum(x => ((JobComInvoiceLine)x).JI_Calc_ESCustomsValue);
				entryLineFee.CF_ChargeAmount = spanishCustomsValue.IsEmpty ? spanishCustomsValue : (ZDecimal)(intermediateResult.ParticipatingAmount * entryLine.CL_CustomsValue / spanishCustomsValue);
			}
			else
			{
				base.SetChargeAmount(entryLine, entryLineFee, intermediateResult);
			}
		}

		protected override void CalculateExtraSystemFeesIfApplicable(EU.Business.Declaration.CusEntryLine entryLine, IExtraFeeCalculator extraFeeCalculator)
		{
			var invoiceLine = ((CusEntryLine)entryLine).RandomLine;
			if (!IsExciseExemption_EOrBOrN(invoiceLine.ZG_ExciseExemption) || extraFeeCalculator.RateCode.StartsWith("1"))
			{
				base.CalculateExtraSystemFeesIfApplicable(entryLine, extraFeeCalculator);
			}
		}

		bool IsExciseExemption_EOrBOrN(ZString exciseExemption) => exciseExemption == ExciseExemptionList.Codes.E || exciseExemption == ExciseExemptionList.Codes.B || exciseExemption == ExciseExemptionList.Codes.N;

		bool IsExciseFeeExempted(JobComInvoiceLine invoiceLine, ZString chargeType)
		{
			var exciseCode = invoiceLine.ZG_ExciseCode;
			var exciseSpecialCode = invoiceLine.SpecialExciseCode;
			var exciseExemption = invoiceLine.ZG_ExciseExemption;

			return !exciseExemption.IsEmpty && exciseExemption != ExciseExemptionList.Codes.NoExemption && (chargeType == exciseCode || chargeType == exciseSpecialCode);
		}
	}
}
