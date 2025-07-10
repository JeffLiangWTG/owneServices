using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	static class AggregatedFieldsCalculator
	{
		public static void CalculateAggregatedFields(JobComInvoiceLine invoiceLine)
		{
			var sima = new AggregatedDutyData();
			var cpt = new AggregatedDutyData();
			var cta = new AggregatedDutyData();
			var dty = new AggregatedDutyData();
			var gst = new AggregatedDutyData();
			var exs = new AggregatedDutyData();

			var groupedDutyAndTaxes = invoiceLine.DutiesAndTaxes.GroupBy(x => x.C1_TaxType);
			var simaTaxTypeArray = new[] { DutyAndTaxTypes.Codes.SIMADuty, DutyAndTaxTypes.Codes.ADD, DutyAndTaxTypes.Codes.CVD, DutyAndTaxTypes.Codes.SUR };

			foreach (var group in groupedDutyAndTaxes)
			{
				var taxType = group.Key;
				switch (taxType)
				{
					case DutyAndTaxTypes.Codes.SIMADuty:
					case DutyAndTaxTypes.Codes.ADD:
					case DutyAndTaxTypes.Codes.CVD:
					case DutyAndTaxTypes.Codes.SUR:
						{
							var descriptionBuilder = new ZStringBuilder();
							foreach (var dutyTax in group.Where(x => !x.C1_Rate.IsEmpty))
							{
								descriptionBuilder.Append(dutyTax.C1_Rate + dutyTax.C1_RateType);
							}
							if (sima.TaxType.IsEmpty || Array.IndexOf(simaTaxTypeArray, sima.TaxType.ToString()) > Array.IndexOf(simaTaxTypeArray, taxType.ToString()))
							{
								sima.TaxType = taxType;
								sima.ExemptCode = group.MaxOrDefault(x => x.C1_ExemptCode);
							}
							sima.Amount += group.Sum(x => x.C1_Amount);
							descriptionBuilder.Prepend(sima.RateDescription);
							sima.RateDescription = descriptionBuilder.ToStringWithDelimiterBetweenAppends(" ").Trim();
						}
						break;
					case DutyAndTaxTypes.Codes.CPT:
						DutyDataSetter(ref cpt, group);
						break;
					case DutyAndTaxTypes.Codes.CTA:
						DutyDataSetter(ref cta, group);
						break;
					case DutyAndTaxTypes.Codes.CustomsDuty:
						DutyDataSetter(ref dty, group);
						break;
					case DutyAndTaxTypes.Codes.GST:
						DutyDataSetter(ref gst, group);
						break;
					case DutyAndTaxTypes.Codes.ExciseTax:
						DutyDataSetter(ref exs, group);
						break;
					default:
						break;
				}
			}

			AggregatedFieldsSetter(sima, invoiceLine.CA_SIMExemptCodeInfo, invoiceLine.CA_SIMAmountInfo, invoiceLine.CA_SIMRateDescriptionInfo);
			AggregatedFieldsSetter(cpt, invoiceLine.CA_CPTExemptCodeInfo, invoiceLine.CA_CPTAmountInfo, invoiceLine.CA_CPTRateDescriptionInfo);
			AggregatedFieldsSetter(cta, invoiceLine.CA_CTAExemptCodeInfo, invoiceLine.CA_CTAAmountInfo, invoiceLine.CA_CTARateDescriptionInfo);
			AggregatedFieldsSetter(dty, invoiceLine.CA_DTYExemptCodeInfo, invoiceLine.CA_DTYAmountInfo, invoiceLine.CA_DTYRateDescriptionInfo);
			AggregatedFieldsSetter(gst, invoiceLine.CA_GSTExemptCodeInfo, invoiceLine.CA_GSTAmountInfo, invoiceLine.CA_GSTRateDescriptionInfo);
			AggregatedFieldsSetter(exs, invoiceLine.CA_EXSExemptCodeInfo, invoiceLine.CA_EXSAmountInfo, invoiceLine.CA_EXSRateDescriptionInfo);
		}

		static void DutyDataSetter(ref AggregatedDutyData dutyData, IGrouping<ZString, DutyAndTax> group)
		{
			var taxType = group.Key;
			var descriptionBuilder = new ZStringBuilder();
			foreach (var dutyTax in group.Where(x => !x.C1_Rate.IsEmpty))
			{
				descriptionBuilder.Append(dutyTax.C1_Rate + dutyTax.C1_RateType);
			}
			dutyData.TaxType = group.Key;
			dutyData.ExemptCode = group.MaxOrDefault(x => x.C1_ExemptCode);
			dutyData.Amount = group.Sum(x => x.C1_Amount);
			dutyData.RateDescription = descriptionBuilder.ToStringWithDelimiterBetweenAppends(" ").Trim();
		}

		static void AggregatedFieldsSetter(AggregatedDutyData dutyData, ZPropertyInfo exemptCodeInfo, ZPropertyInfo amountInfo, ZPropertyInfo rateDescriptionInfo)
		{
			exemptCodeInfo.Value = dutyData.ExemptCode;
			amountInfo.Value = dutyData.Amount;
			rateDescriptionInfo.Value = dutyData.RateDescription;
		}
	}

	struct AggregatedDutyData
	{
		public ZString TaxType;
		public ZString RateDescription;
		public ZString ExemptCode;
		public ZDecimal Amount;
	}
}
