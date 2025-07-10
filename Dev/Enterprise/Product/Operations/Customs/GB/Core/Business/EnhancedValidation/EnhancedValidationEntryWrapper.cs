using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business
{
	public class EnhancedValidationEntryWrapper : IEnhancedValidationEntryWrapper
	{
		public EnhancedValidationEntryWrapper(CusEntryHeader entry)
		{
			this.entry = entry;

			var uniqueCommodityOriginPairs = entry.InvoiceLines.Select(x => (x.JI_FormattedTariff, x.JI_CountryOfOrigin)).Distinct();
			var upToFivePairs = uniqueCommodityOriginPairs.Take(5).ToArray();

			CommodityLine1 = FormatCommodityOriginPair(upToFivePairs, 0);
			CommodityLine2 = FormatCommodityOriginPair(upToFivePairs, 1);
			CommodityLine3 = FormatCommodityOriginPair(upToFivePairs, 2);
			CommodityLine4 = FormatCommodityOriginPair(upToFivePairs, 3);
			CommodityLine5 = FormatCommodityOriginPair(upToFivePairs, 4);
			CommodityLineCount = upToFivePairs.Length;

			var taxCommodityPairs = new List<(ZString, ZString, ZString)>();
			foreach (var line in entry.AllEntryLines)
			{
				foreach (var fee in line.Fees.Cast<CusEntryLineFee>())
				{
					if (fee.CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Override)
					{
						var description = fee.ChargeTypeDescription;
						taxCommodityPairs.AddRange(line.InvoiceLines.Select(x => (fee.CF_ChargeType, description, x.JI_FormattedTariff)));
					}
				}
			}
			var upToFiveUniqueTaxCommodityPairs = taxCommodityPairs.Distinct().Take(5).ToArray();

			TaxLine1 = FormatTaxCommodityPair(upToFiveUniqueTaxCommodityPairs, 0);
			TaxLine2 = FormatTaxCommodityPair(upToFiveUniqueTaxCommodityPairs, 1);
			TaxLine3 = FormatTaxCommodityPair(upToFiveUniqueTaxCommodityPairs, 2);
			TaxLine4 = FormatTaxCommodityPair(upToFiveUniqueTaxCommodityPairs, 3);
			TaxLine5 = FormatTaxCommodityPair(upToFiveUniqueTaxCommodityPairs, 4);
			TaxLineCount = upToFiveUniqueTaxCommodityPairs.Length;
		}

		string FormatCommodityOriginPair((ZString, ZString)[] pairs, int index) => pairs.Length > index ? FormatCommodityOriginPair(pairs[index]) : null;
		string FormatCommodityOriginPair((ZString, ZString) pair) => $"{pair.Item1} from {GetCountryName(pair.Item2)}";

		string GetCountryName(string code) => entry.Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, code)?.RN_Desc ?? code;

		string FormatTaxCommodityPair((ZString, ZString, ZString)[] pairs, int index) => pairs.Length > index ? FormatTaxCommodityPair(pairs[index]) : null;
		string FormatTaxCommodityPair((ZString, ZString, ZString) pair) => pair.Item2.IsEmpty ? $"{pair.Item1} for {pair.Item3}" : $"{pair.Item1} ({pair.Item2}) for {pair.Item3}";

		public string CommodityLine1 { get; private set; }
		public string CommodityLine2 { get; private set; }
		public string CommodityLine3 { get; private set; }
		public string CommodityLine4 { get; private set; }
		public string CommodityLine5 { get; private set; }

		public string TaxLine1 { get; private set; }
		public string TaxLine2 { get; private set; }
		public string TaxLine3 { get; private set; }
		public string TaxLine4 { get; private set; }
		public string TaxLine5 { get; private set; }

		public int CommodityLineCount { get; private set; }
		public int TaxLineCount { get; private set; }

		readonly CusEntryHeader entry;
	}
}
