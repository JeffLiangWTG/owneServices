using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.DocumentWrappers;
using Enterprise.MasterFiles.Business;
using FRCusEntryLine = Enterprise.Customs.FR.Business.Declaration.CusEntryLine;
using FRCusEntryLineFee = Enterprise.Customs.FR.Business.Declaration.CusEntryLineFee;
using FRJobComInvoiceLine = Enterprise.Customs.FR.Business.Declaration.JobComInvoiceLine;

namespace Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails;

public class LiquidationDetailsLineWrapper : DocBaseWrapper
{
	LiquidationDetailsLineWrapper(FRCusEntryLine entryLine, BusinessObjectFactory factoryToWrap)
		: base(entryLine, factoryToWrap)
	{ }

	public static LiquidationDetailsLineWrapper New(FRCusEntryLine entryLine, BusinessObjectFactory factoryToWrap) => new LiquidationDetailsLineWrapper(entryLine, factoryToWrap);

	public FRCusEntryLine EntryLine => (FRCusEntryLine)base.WrappedObject;

	public ZString LineNumber => EntryLine.CL_LineNumber.ToString();
	public ZString Commodity => EntryLine.FormattedTariff;
	public ZString Description => EntryLine.EffectiveDescription;

	public ZString PackageSummary
	{
		get
		{
			var pw = new PackingWrapper(EntryLine);
			return (pw.Count.ToString() + " " + pw.Type).PadRight(14);
		}
	}

	public ZString Origin => EntryLine.RandomLine.JI_CountryOfOrigin.PadRight(8);
	public ZString Preference => EntryLine.RandomLine.JI_PrimaryPreference.PadRight(12);

	CurrencyConverter CurrencyConverter { get { return currencyConverter ?? (currencyConverter = EntryLine.Header.CurrencyConverter); } }
	CurrencyConverter currencyConverter;

	public ZString ItemPrice
	{
		get
		{
			var itemPrice = EntryLine.Header.IsMultiInvoiceCurrency ? CurrencyConverter.ConvertRounded(EntryLine.CL_InvoiceMoney, CurrencyConverter.LocalCurrency).Amount : EntryLine.CL_InvoiceAmount;
			return itemPrice.ToString().PadRight(16);
		}
	}

	public ZString ItemPriceCurrency
	{
		get
		{
			var itempriceCurrency = EntryLine.Header.IsMultiInvoiceCurrency ? Core.Constants.CurrencyCodes.EuropeanUnion : EntryLine.CL_RX_NKInvoiceAmountCurrency.ToString();
			return itempriceCurrency.PadRight(10);
		}
	}

	public ZString SupplementaryQty => new SupplementaryUnitWrapper(EntryLine).Qty.ToString().PadRight(15);
	public ZString SupplementaryUnitQty => new SupplementaryUnitWrapper(EntryLine).Code.PadRight(12);
	public IEnumerable<ZString> CEAdditionalCodes => EntryLine.RandomLine.CEAdditionalCodes;
	public IEnumerable<ZString> FRAdditionalCodes => EntryLine.RandomLine.FRAdditionalCodes;
	public ZString AdditionalCodesAsString => string.Format("{0}{1}{2} ", string.Join(", ", FRAdditionalCodes), FRAdditionalCodes.Any() && CEAdditionalCodes.Any() ? " - " : string.Empty, string.Join(", ", CEAdditionalCodes)).PadRight(20);

	public ZString SpecialProvision => CachedValueHelper.GetValue(ref specialProvision, () =>
	{
		var invoiceLines = EntryLine.InvoiceLines.Cast<FRJobComInvoiceLine>();
		var codes = EntryLine.Declaration.SupportingDocuments.Cast<SupportingDocument>()
			.Union(invoiceLines.SelectMany(invLine => invLine.InvoiceHeader.SupportingDocuments.Cast<SupportingDocument>()))
			.Union(invoiceLines.SelectMany(invLine => invLine.SupportingDocuments.Cast<SupportingDocument>()))
			.Where(doc => doc.CSI_IsDTP && doc.CSI_Code != string.Empty)
			.Select(doc => doc.CSI_Code)
			.Distinct()
			.ToArray();
		return ZString.Join(", ", codes).PadRight(18);
	});
	CachedValue<ZString> specialProvision;

	public ZString GrossMassInKg => EntryLine.EffectiveGrossWeight.InKilogramsSafe.ToString().PadRight(16);
	public ZString NettMassInKg => EntryLine.EffectiveNetWeight.InKilogramsSafe.ToString().PadRight(16);
	public ZString TariffBypass => EntryLine.RandomLine.JI_TariffBypassCode;
	public ZDecimal CustomsValue => EntryLine.CL_ConfirmedOrCalculatedCustomsValue;
	public ZDecimal StatisticalValue => EntryLine.CL_ConfirmedOrCalculatedStatisticalValue;

	public ZDecimal VatValue => EntryLine.CL_ConfirmedOrCalculatedValueForVAT;

	public ZDecimal BaseVatableValue => EntryLine.CL_ConfirmedOrCalculatedValueForVAT > 0 ? (ZDecimal)(EntryLine.CL_ConfirmedOrCalculatedValueForVAT + EntryLine.DutyAmountByConfirmedFee) : EntryLine.CL_ConfirmedOrCalculatedValueForVAT;

	public ZString ItemTaxBreakdown => CachedValueHelper.GetValue(ref itemTaxBreakdown, () =>
	{
		var feesGroupBy = EntryLine.ConfirmedFees.Cast<FRCusEntryLineFee>().GroupBy(x => x.NationalFeeTypeCode);
		return LiquidationDetailsHelper.GetTaxBreakdown(feesGroupBy, EntryLine.CL_LineNumber == 1 ? EntryLine.Header.ConfirmedCharges.Cast<CusEntryHeaderCharges>() : null);
	});
	CachedValue<ZString> itemTaxBreakdown;

	public ZString ItemChargesBreakdown => CachedValueHelper.GetValue(ref itemChargesBreakdown, () =>
	{
		var charges = EntryLine.InvoiceLines.OfType<FRJobComInvoiceLine>().SelectMany(x => x.Charges.Cast<InvoiceLineCharge>());
		var aportionedCharges = EntryLine.InvoiceLines.OfType<FRJobComInvoiceLine>().SelectMany(x => x.ApportionedCharges);
		return LiquidationDetailsHelper.GetLineChargeBreakdown(charges, aportionedCharges);
	});
	CachedValue<ZString> itemChargesBreakdown;
}
