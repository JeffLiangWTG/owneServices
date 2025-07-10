using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.Messaging
{
	public class GbChiefImportLine : GbChiefLine, IImportLine
	{
		public GbChiefImportLine(GbChiefImportHeader header, CusEntryLine actualEntryLine)
			: base(header, actualEntryLine)
		{
		}

		public ZString CountryOfOriginCode
		{
			get => header.ConvertFromUnToChiefCountry(randomLine.JI_CountryOfOrigin.SubstringSafe(0, 2));
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

		// This property is a percentage so can come from randomLine
		public ZDecimal ValueAdjustmentAmount => randomLine.JI_ValuationMarkup;

		public ZString ValueAdjustmentCode => randomLine.ZG_ValueAdjustmentCode;

		public ZBool FECCountryOfOrigin => randomLine.JI_FecORG;

		public ZString CountryOfExport
		{
			get => header.ConvertFromUnToChiefCountry(
				randomLine.JI_RN_NKCountryOfExport.IsEmpty
				&& randomLine.Declaration != null
					? randomLine.Declaration.JE_GoodsOrigin
					: randomLine.JI_RN_NKCountryOfExport);
		}

		public ZString CountryOfSupply => randomLine.ZG_CountryOfSupply;
	}
}
