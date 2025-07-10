using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class JobComInvoiceLineTax : Customs.Business.JobComInvoiceLineTax
	{
		public JobComInvoiceLineTax(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override bool SupportsCloneCore() => true;

		public new JobComInvoiceLine InvoiceLine => base.InvoiceLine as JobComInvoiceLine;

		public override void OnSaving()
		{
			base.OnSaving();

			if (!InvoiceLine.IsImportOnly && JLT_Type == Constants.RateCodes.ImportDuty && JLT_MethodOfCalculation.IsEmpty)
			{
				Delete();
			}
		}

		public bool ShouldBeIncludedOnSpecialCases
		{
			get
			{
				var taxTypeList = Factory.GetCachedValue<SpecialCaseTaxTypeList>();
				var result = taxTypeList.ContainsCode(JLT_MethodOfCalculation);
				if (result && (InvoiceLine?.IsImportSiscomex ?? false))
				{
					result = JLT_Type != Constants.RateCodes.ImportDuty &&
						!((JLT_Type == Constants.RateCodes.IPI || JLT_Type == Constants.RateCodes.PIS || JLT_Type == Constants.RateCodes.Cofins)
						&& JLT_MethodOfCalculation == SpecialCaseTaxTypeList.Codes.AdValoremRate);
				}
				return result;
			}
		}

		public void SetTaxesForImportSiscomex()
		{
			var isReduced = JLT_MethodOfCalculation == SpecialCaseTaxTypeList.Codes.Reduced;

			switch (JLT_Type)
			{
				case Constants.RateCodes.ImportDuty:
					InvoiceLine.JI_PrimaryPreference = GetMappedRatePreferenceType(JLT_MethodOfCalculation);
					InvoiceLine.DutyRateIsOverridden = true;
					switch (JLT_MethodOfCalculation)
					{
						case SpecialCaseTaxTypeList.Codes.TariffAgreement:
							InvoiceLine.FTAMarginRateValue = JLT_Rate;
							break;
						case SpecialCaseTaxTypeList.Codes.Reduced:
							InvoiceLine.ReducedDutyRateValue = JLT_Rate;
							break;
						case SpecialCaseTaxTypeList.Codes.Reduction:
							InvoiceLine.ReductionMarginRateValue = JLT_Rate;
							break;
						default:
							InvoiceLine.OverriddenDutyRateValue = JLT_Rate;
							break;
					}
					break;
				case Constants.RateCodes.IPI:
					InvoiceLine.IPIRateIsOverridden = !isReduced;
					if (isReduced)
					{
						InvoiceLine.IPITaxRegime = IPITaxRegimeList.Codes.Reduction;
					}
					else
					{
						InvoiceLine.IPIVigentRateValue = JLT_Rate;
					}
					break;
				case Constants.RateCodes.PIS:
					InvoiceLine.PisRateIsOverridden = !isReduced;
					if (isReduced && !InvoiceLine.CofinsRateIsOverridden)
					{
						InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Reduction;
					}
					else if (!isReduced)
					{
						InvoiceLine.PisVigentRateValue = JLT_Rate;
					}
					break;
				case Constants.RateCodes.Cofins:
					InvoiceLine.CofinsRateIsOverridden = !isReduced;
					if (isReduced && !InvoiceLine.PisRateIsOverridden)
					{
						InvoiceLine.PisCofinsTaxRegime = TaxRegimeList.Codes.Reduction;
					}
					else if (!isReduced)
					{
						InvoiceLine.CofinsVigentRateValue = JLT_Rate;
					}
					break;
			}
		}

		string GetMappedRatePreferenceType(string taxType)
		{
			return taxType switch
			{
				SpecialCaseTaxTypeList.Codes.TariffAgreement => Constants.RatePreferenceType.FreeTradeAgreement,
				SpecialCaseTaxTypeList.Codes.Reduction => Constants.RatePreferenceType.ReductionMargin,
				SpecialCaseTaxTypeList.Codes.Reduced => Constants.RatePreferenceType.ReducedRate,
				_ => Constants.RatePreferenceType.ExTariff,
			};
		}
	}
}
