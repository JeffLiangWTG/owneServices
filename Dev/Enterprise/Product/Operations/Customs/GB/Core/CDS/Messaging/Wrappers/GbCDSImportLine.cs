using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DutyCalculator;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Messaging;
using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSImportLine : GbLine, ILine
	{
		public GbCDSImportLine(GbCDSImportHeader header, CusEntryLine actualEntryLine) : base(header, actualEntryLine)
		{
		}

		IEnumerable<ITax> ILine.Taxes
		{
			get
			{
				var result = new List<ITax>();
				IEnumerable<CusEntryLineFee> fees;
				if (GBCustomsDataRegistry.Instance.SendDTNTaxBaseToCDS.Value || !ConvertibleUnitsOfMeasure.WeightConversionDictionary.ContainsKey(actualEntryLine.CustomsUnitQty))
				{
					fees = actualEntryLine.Fees.Cast<CusEntryLineFee>();
				}
				else
				{
					fees = actualEntryLine.Fees.Cast<CusEntryLineFee>().Where(fee => !ConvertibleUnitsOfMeasure.WeightConversionDictionary.ContainsKey(fee.CF_MethodOfCalculation));
				}
				foreach (EU.Business.Declaration.CusEntryLineFee fee in fees)
				{
					result.Add(new Tax(fee));
				}
				return result;
			}
		}

		IEnumerable<IStatement> ILine.Statements
		{
			get
			{
				return actualEntryLine.AdditionalInfos.Where(x => !x.IsHeaderOnly).Select(x => new Statement(x));
			}
		}

		public ZString CountryOfOriginCode
		{
			get => GbHeader.EntryHeader.ConvertFromUnToChiefCountry(randomLine.JI_CountryOfOrigin.SubstringSafe(0, 2));
		}

		public ZDecimal ItemPrice
		{
			get => actualEntryLine.Header.IsMultiInvoiceCurrency
				? actualEntryLine.TotalLinePriceInLocalCurrency
				: actualEntryLine.TotalLinePrice.Amount;
		}

		public ZString ItemPriceCurrency
		{
			get => actualEntryLine.Header.IsMultiInvoiceCurrency
				? actualEntryLine.Header.LocalCurrency.Code
				: actualEntryLine.TotalLinePrice.Currency?.Code ?? ZString.Empty;
		}

		public ZString PreferenceCode => randomLine.JI_PrimaryPreference;

		public ZString QuotaOrderNumber => randomLine.JI_ConcessionOrder;

		public ZString ValuationMethod => randomLine.JI_ValuationCode;

		public ZDecimal ValueAdjustmentAmount => randomLine.JI_ValuationMarkup;

		public ZString ValueAdjustmentCode => randomLine.ZG_ValueAdjustmentCode;

		public ZBool FECCountryOfOrigin => randomLine.JI_FecORG;

		public ZString CountryOfExport
		{
			get => GbHeader.EntryHeader.ConvertFromUnToChiefCountry(
				randomLine.JI_RN_NKCountryOfExport.IsEmpty
				&& randomLine.Declaration != null
					? randomLine.Declaration.JE_GoodsOrigin
					: randomLine.JI_RN_NKCountryOfExport);
		}

		public ZString CountryOfSupply => randomLine.ZG_CountryOfSupply;

		protected override ZString GetCountryOfOriginBox34()
		{
			return GbHeader.EntryHeader.ConvertFromUnToChiefCountry(base.GetCountryOfOriginBox34());
		}
	}
}
