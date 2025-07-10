using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration.Customs.CN;

namespace Enterprise.Customs.CN.Business
{
	public partial class CusEntryLine : ICustomsEntryLine
	{
		#region Implementation of ICustomsEntryLine

		#region EntryLineNo

		public ZShort EntryLineNo => CL_LineNumber;

		#endregion

		#region TariffCode

		public ZString TariffCode => RandomLine?.JI_Tariff ?? ZString.Empty;

		public ZString CIQTariffCode => RandomLine?.JI_CIQTariff ?? ZString.Empty;

		public ZString CIQSupplementCode => CIQTariffCode.SubstringSafe(10, 3);

		public ZString CIQTariffDescription => RandomLine?.CIQTariff?.ZZ1_Description ?? ZString.Empty;

		#endregion

		#region Name Of Goods And Spec Model

		public ZString NameOfGoods => AdditionalInformationHelper.GetMergedNameOfGoods(GetAdditionalInformationHelpers());

		public ZString GoodsSpecModel => AdditionalInformationHelper.GetMergedGoodsSpecModel(GetAdditionalInformationHelpers());

		public ZPropertyInfo GoodsSpecModelInfo => GetZPropertyInfo(nameof(GoodsSpecModel));

		IEnumerable<AdditionalInformationHelper> GetAdditionalInformationHelpers() => InvoiceLines.Cast<JobComInvoiceLine>().Select(l => Header.IsChildEntry ? l.AdditionalInformation2Helper : l.AdditionalInformationHelper);

		#endregion

		#region Quantities

		public ZString TradeUnitQty => RandomLine?.JI_TradeUnitQty ?? ZString.Empty;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryLine|TradeQuantity", Caption = "Trade Quantity")]
		public ZDecimal TradeQuantity => cachedTradeQuantity?.Value ?? (cachedTradeQuantity = new CachedProperty<ZDecimal>(Factory, () => InvoiceLines.Cast<JobComInvoiceLine>().Sum(line => line.JI_TradeQuantity))).Value;

		[ResourceStringData("Enterprise.Customs.CN.Business.CusEntryLine|TradeUnitQtyDesc", Caption = "UQ")]
		public ZString TradeUnitQtyDesc => cachedCusPackListProvider.GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China).GetDescriptionFromCode(TradeUnitQty);

		public ZDecimal CustomsSecondQuantity => InvoiceLines.Sum(line => ((JobComInvoiceLine)line).JI_CustomsSecondQuantity);

		public ZString CustomsSecondUnit => RandomLine.JI_CustomsSecondUnitQty;

		public ZString CustomsSecondUnitDesc => cachedCusPackListProvider.GetCIPCustomsPackList(Factory, Core.Constants.CountryCodes.China).GetDescriptionFromCode(CustomsSecondUnit);

		CachedProperty<ZDecimal> cachedTradeQuantity;

		IRefCusPackListProvider cachedCusPackListProvider => Factory.GetCachedValue<RefCusPackListProvider>();

		#endregion

		#region GoodsDest

		public ZString GoodsDestCode => RandomLine.CountryOfExport.GetCNCountryCode();

		public ZString GoodsDestName => RandomLine.CountryOfExport.GetCNCountryName();

		#endregion

		#region GoodsOrigin

		public ZString GoodsOriginCode => RandomLine.CountryOfOrigin.GetCNCountryCode();

		public ZString GoodsOriginName => RandomLine.CountryOfOrigin.GetCNCountryName();

		public ZString OriginStateCode => RandomLine.JI_CIQOriginState;

		public ZString OriginStateName => CNRefCusCodeListLoader.GetCIQState(Factory, OriginStateCode, Declaration.DateOfValuation)?.ZZD_Description ?? ZString.Empty;

		#endregion

		#region Price

		public Money TotalPriceMoney => cachedTotalPriceMoney?.Value ?? (cachedTotalPriceMoney = new CachedProperty<Money>(Factory, () => this.CalculateTotalPrice())).Value;
		CachedProperty<Money> cachedTotalPriceMoney;

		public ZDecimal UnitPrice
		{
			get
			{
				var tradeQuantity = TradeQuantity;
				return tradeQuantity.IsEmpty ? ZDecimal.Zero : new ZDecimal(TotalPrice / tradeQuantity).Truncate(4);
			}
		}

		public ZDecimal TotalPrice => TotalPriceMoney.Amount;

		public ZString CurrencyCode => TotalPriceMoney.Currency.Code ?? ZString.Empty;

		public ZString CurrencyDesc => CNRefCusCodeListLoader.GetCurrency(Factory, CurrencyCode, Header.DateOfValuation)?.ZZD_Description ?? ZString.Empty;

		#endregion

		#region DutyMode

		public ZString DutyModeCode => (RandomLine.Declaration?.WillGenerateBothEntries ?? false) && Header.IsRecordListing ? (ZString)DutyModeList.Codes._3 : RandomLine.JI_DutyMode;

		public ZString DutyModeDesc => Factory.GetCachedValue<DutyModeList>().GetDescriptionFromCode(DutyModeCode);

		#endregion

		#region ProductManualNo

		public ZInt ProductManualNo => Header.IsChildEntry ? RandomLine.JI_ProductManualNo2 : RandomLine.JI_ProductManualNo;

		public ZString ProductCode => RandomLine.JI_PartNo;

		public ZString ProductVersion => RandomLine.JI_ProductVersion;

		#endregion

		#region DomesticDistrict

		public ZString DomesticDistrictCode => Header.IsEntering ? RandomLine.JI_DestinationDistrict : RandomLine.JI_OriginDistrict;

		public ZString DomesticDistrictName => CNRefCusCodeListLoader.GetDistrict(Factory, DomesticDistrictCode, Declaration.DateOfValuation)?.ZZD_Description ?? ZString.Empty;

		#endregion

		#region DomesticRegion

		public ZString DomesticRegionCode => Header.IsEntering ? RandomLine.JI_DestinationRegion : RandomLine.JI_OriginRegion;

		public ZString DomesticRegionName => CNRefCusCodeListLoader.GetRegion(Factory, DomesticRegionCode, Declaration.DateOfValuation)?.ZZD_Description ?? ZString.Empty;

		#endregion

		#region Certificate Of Origin

		public ZString CertOfOriginNumber => RandomLine?.CertificateOfOrigin ?? ZString.Empty;

		public ZString TradeAgreementCode => RandomLine?.TradeAgreementCode ?? ZString.Empty;

		public ZString TradeAgreementDesctiption => TradeAgreementCode.IsEmpty ? string.Empty : RandomLine?.Lookups.TradeAgreementCodeList.GetDescriptionFromCode(TradeAgreementCode);

		public ZString CertOfOriginCountry => RandomLine?.CertificateOfOriginCountry ?? ZString.Empty;

		public ZInt ItemNoOnCertOfOrigin => RandomLine?.ItemNoOnCertOfOrigin ?? ZInt.Zero;

		public ZString CertOfOriginType => RandomLine?.CertificateOfOriginType ?? ZString.Empty;

		public ZString CertOfOriginTypeDescription => CertOfOriginType.IsEmpty ? string.Empty : RandomLine?.Lookups.CertificateOfOriginTypeList.GetDescriptionFromCode(CertOfOriginType);

		#endregion

		#endregion
	}
}
