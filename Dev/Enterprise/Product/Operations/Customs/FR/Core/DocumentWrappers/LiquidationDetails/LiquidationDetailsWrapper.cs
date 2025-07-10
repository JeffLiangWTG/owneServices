using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.DocumentWrappers.LiquidationDetails;

[WTG.StaticAnalysis.Annotation.CodeAlive("Used in documents DataContext")]
public class LiquidationDetailsWrapper : Enterprise.DocumentWrappers.DocBaseWrapper
{
	LiquidationDetailsWrapper(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap)
		: base(entryHeader, factoryToWrap)
	{ }

	public static LiquidationDetailsWrapper New(CusEntryHeader entryHeader, BusinessObjectFactory factoryToWrap) => new LiquidationDetailsWrapper(entryHeader, factoryToWrap);

	public CusEntryHeader EntryHeader => (CusEntryHeader)base.WrappedObject;

	public ZString DeclarationReference => EntryHeader.Declaration?.JE_DeclarationReference ?? ZString.Empty;

	public LiquidationDetailsLineWrapperCollection Lines
	{
		get { return lines ?? (lines = GetLinesCore()); }
	}
	LiquidationDetailsLineWrapperCollection lines;

	protected virtual LiquidationDetailsLineWrapperCollection GetLinesCore()
	{
		return new LiquidationDetailsLineWrapperCollection(EntryHeader.MergedLines, Factory);
	}

	public ZString EntryReference => EntryHeader.CH_BGMReference;
	public ZString DeltaReference => EntryHeader.EntryNumber;
	public ZDateTime EntryDate => EntryHeader.CusEntryNumber?.CE_IssueDate ?? EntryHeader.Declaration.JE_SystemCreateTimeUtc.ToLocalBranchTime();
	public ZString EntryStatus => EntryHeader.Lookups.CH_EntryStatusList.GetDescriptionFromCode(EntryHeader.CusEntryNumber?.CE_EntryStatus ?? ZString.Empty);
	public ZString SupplierAddress => EntryHeader.Supplier.Organisation == null ? ZString.Empty : GetFullAddress(EntryHeader.Supplier.Organisation);

	public ZString ImporterAddress => EntryHeader.Importer.Organisation == null ? ZString.Empty : GetFullAddress(EntryHeader.Importer.Organisation);

	public ZString ImporterEori => EntryHeader.ImporterEoriOfMainOffice;
	public ZString DeclarantAddress => EntryHeader.DeclarantOrganisation == null ? ZString.Empty : GetFullAddress(EntryHeader.DeclarantOrganisation);

	ZString GetFullAddress(OrgHeader organisation)
	{
		var address = organisation.MainAddress;
		var sb = new ZStringBuilder();
		sb.AppendLine(address.CompanyName);
		sb.AppendLine(address.Address1);
		sb.AppendLine(address.Street);
		sb.Append(address.City);
		sb.Append(" ");
		sb.AppendLine(address.State);
		sb.Append(address.Country.Description);
		return sb.ToString();
	}

	public ZString RepresentationType => EntryHeader.Declaration.Lookups.DeclarantTypeList.GetDescriptionFromCode(EntryHeader.Declaration.JE_DeclarantType);
	public ZString DeltaMode => EntryHeader.Declaration.JE_DeltaMode;
	public ZString AgreementNumber => EntryHeader.Declaration.JE_CustomsProfile;
	public ZString DefermentNumber => EntryHeader.Declaration.JE_DefermentAccountNumber;
	public ZString CodNumber => EntryHeader.Declaration.CustomsGuarantee?.GetCustomsGuaranteeFriendlyName(EntryHeader.Declaration.DeltaMode) ?? ZString.Empty;
	public ZString VatProcedure => EntryHeader.Declaration.AddInfoLookups.DeferTypeList.GetDescriptionFromCode(EntryHeader.Declaration.ZG_VATDeferType); // description from code
	public ZString TransportMode => EntryHeader.Declaration.JE_TransportMode;
	public ZString OriginPort => EntryHeader.Declaration.JE_RL_NKPortOfLoading;
	public ZString ValuationBypass
	{
		get
		{
			var valuationBypass = EntryHeader.EntryInstruction?.ZG_BypassCode ?? ZString.Empty;
			if (valuationBypass != ZString.Empty)
			{
				valuationBypass = string.Format(" / {0}", valuationBypass);
			}

			return valuationBypass;
		}
	}
	public ZString AirRouteType => EntryHeader.Declaration.JE_AirRouteType;

	public ZDecimal FreightChargeFractionOutsideEU => ZDecimal.ParseSafe(EntryHeader.Declaration.IATALoadPort?.GetAttribute((ZString)UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentOutEU) ?? ZString.Empty, ZDecimal.Zero);

	public ZDecimal FreightChargeFractionInsideEU => ZDecimal.ParseSafe(EntryHeader.Declaration.IATALoadPort?.GetAttribute((ZString)UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentInEu) ?? ZString.Empty, ZDecimal.Zero);

	public ZDecimal FreightChargeFractionInFrance => ZDecimal.ParseSafe(EntryHeader.Declaration.IATALoadPort?.GetAttribute((ZString)UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.PercentDomestic) ?? ZString.Empty, ZDecimal.Zero);

	public ZString EntryTypeFriendlyName => EntryHeader.EntryTypeFriendlyName;

	public ZString TotalItems => EntryHeader.MergedLines.Count.ToString();

	public ZString SupportingDocuments => CachedValueHelper.GetValue(ref supportingDocuments, () =>
	{
		var supportingDocumentList = EntryHeader.MergedLines.Cast<CusEntryLine>().Select(x => new ArticleWrapper(EntryHeader, x))
				.SelectMany(x => x.SupportingDocuments).DistinctBy(x => ZString.Join(",", new[] { x.Code, x.RefNumber, GetFormattedDateIssue(x.DateIssue) }));
		var sb = new ZStringBuilder();
		foreach (var supportingDocument in supportingDocumentList)
		{
			sb.AppendLine(FormattableString.Invariant($"{supportingDocument.Code.Left(4).PadRight(6)}{supportingDocument.RefNumber.Left(25).PadRight(27)}{GetFormattedDateIssue(supportingDocument.DateIssue).PadRight(10)}{supportingDocument.D48Amount.ToString().PadRight(10)}").TrimEnd());
		}
		return sb.ToString();

		ZString GetFormattedDateIssue(ZDateTime dateIssue)
		{
			return dateIssue.ToString("dd/MM/yy", CultureInfo.InvariantCulture);
		}
	});

	CachedValue<ZString> supportingDocuments;

	public ZString D48Amount => CachedValueHelper.GetValue(ref d48Amount, () => $"{EntryHeader.TotalD48Amount} {Core.Constants.CurrencyCodes.EuropeanUnion}");

	CachedValue<ZString> d48Amount;

	bool ShowOnlyDeltaCalculations => !EntryHeader.EntryNumber.IsEmpty;
	public ZString ShowOnlyDeltaCalculationsAsString => ShowOnlyDeltaCalculations ? "Y" : "N";

	CurrencyConverter CurrencyConverter { get { return currencyConverter ?? (currencyConverter = EntryHeader.CurrencyConverter); } }
	CurrencyConverter currencyConverter;

	public Money InvoiceValueInDeclarationCurrencyCalculation
	{
		get
		{
			if (invoiceValueInDeclarationCurrencyCalculation == null || invoiceValueInDeclarationCurrencyCalculation.IsEmpty)
			{
				if (EntryHeader.IsMultiInvoiceCurrency)
				{
					var amount = Money.Empty;
					foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
					{
						amount = CurrencyConverter.Add(amount, CurrencyConverter.ConvertRounded(entryLine.CL_InvoiceMoney, CurrencyConverter.LocalCurrency));
					}
					invoiceValueInDeclarationCurrencyCalculation = amount;
				}
				else
				{
					var amount = Money.Empty;
					foreach (CusEntryLine entryLine in EntryHeader.MergedLines)
					{
						amount = CurrencyConverter.Add(amount, entryLine.CL_InvoiceMoney);
					}
					invoiceValueInDeclarationCurrencyCalculation = amount;
				}
			}
			return invoiceValueInDeclarationCurrencyCalculation;
		}
	}
	Money invoiceValueInDeclarationCurrencyCalculation;

	public ZDecimal InvoiceValueInDeclarationCurrency => InvoiceValueInDeclarationCurrencyCalculation.Amount;

	public ZDecimal InvoiceValueInDeclarationCurrencyInEuro => InvoiceValueInDeclarationCurrencyCalculation.Currency?.Code == Core.Constants.CurrencyCodes.France ? InvoiceValueInDeclarationCurrency
		 : CurrencyConverter.ConvertRounded(InvoiceValueInDeclarationCurrencyCalculation, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.France)).Amount;

	public ZString InvoiceValueInInvoiceCurrency => EntryHeader.IsMultiInvoiceCurrency ? Core.Constants.CurrencyCodes.EuropeanUnion : EntryHeader.RandomHeader.JZ_RX_NKInvoice_Currency.ToString();
	public ZDecimal InvoiceCurrencyExchangeRate => EntryHeader.IsMultiInvoiceCurrency ? new ZDecimal(1m) : EntryHeader.RandomHeader.JZ_InvoiceCurrExRate;
	public ZDecimal CustomsValue => CachedValueHelper.GetValue(ref customsValue, () => EntryHeader.MergedLines.OfType<CusEntryLine>().Sum(x => x.CL_ConfirmedOrCalculatedCustomsValue));
	CachedValue<ZDecimal> customsValue;
	public ZDecimal StatisticalValue => CachedValueHelper.GetValue(ref statisticalValue, () => EntryHeader.MergedLines.OfType<CusEntryLine>().Sum(x => x.CL_ConfirmedOrCalculatedStatisticalValue));
	CachedValue<ZDecimal> statisticalValue;
	public ZDecimal VatableValueWithoutDuties => CachedValueHelper.GetValue(ref vatableValueWithoutDuties, () => EntryHeader.MergedLines.OfType<CusEntryLine>().Sum(x => x.CL_ConfirmedOrCalculatedValueForVAT) - EntryHeader.MergedLines.OfType<CusEntryLine>().Sum(x => x.DutyAmountByConfirmedFee));
	CachedValue<ZDecimal> vatableValueWithoutDuties;
	public ZDecimal VatableValue => CachedValueHelper.GetValue(ref vatableValue, () => EntryHeader.MergedLines.OfType<CusEntryLine>().Sum(x => x.CL_ConfirmedOrCalculatedValueForVAT));
	CachedValue<ZDecimal> vatableValue;
	public ZString IncoTerm => EntryHeader.RandomHeader.JZ_IncoTerm;
	public ZString IncoTermPlace => EntryHeader.RandomHeader.JZ_IncoTermPlace;
	public ZString IncoTermType => EntryHeader.RandomHeader.ZG_AgreedPlaceCode;
	public ZString ChargeBreakdown => CachedValueHelper.GetValue(ref chargeBreakdown, () =>
	{
		var charges = EntryHeader.InvoiceHeaders.OfType<JobComInvoiceHeader>().SelectMany(x => x.Charges.Cast<InvoiceCharge>());
		var appCharges = EntryHeader.InvoiceHeaders.OfType<JobComInvoiceHeader>().SelectMany(x => x.GroupCharges);
		return LiquidationDetailsHelper.GetHeaderChargeBreakdown(charges, appCharges);
	});
	CachedValue<ZString> chargeBreakdown;

	public ZBool AnyLineWithTariffBypass => EntryHeader.MergedLines.OfType<CusEntryLine>().Any(x => !x.RandomLine.JI_TariffBypassCode.IsEmpty);
	public ZString TaxBreakdown => CachedValueHelper.GetValue(ref taxBreakdown, () =>
	{
		var feesGroupBy = EntryHeader.MergedLines.OfType<CusEntryLine>().SelectMany(x => x.ConfirmedFees).OfType<CusEntryLineFee>().GroupBy(x => x.NationalFeeTypeCode);
		return LiquidationDetailsHelper.GetTaxBreakdown(feesGroupBy, EntryHeader.ConfirmedCharges.Cast<CusEntryHeaderCharges>(), true);
	});
	CachedValue<ZString> taxBreakdown;

	public ZString TotalFeeAmountString => FormatLineFees(TotalFeeAmount.ToString(0));
	public ZString TotalFeesCautionAmountString => FormatLineFees(TotalFeesCautionAmount.ToString(0));
	public ZString TotalFeesNonCautionAmountString => FormatLineFees(TotalFeesNonCautionAmount.ToString(0));
	public ZString TotalFeesAi2AmountString => FormatLineFees(TotalFeesAi2Amount.ToString(0));
	public ZString TotalFeesAtvaiAmountString => FormatLineFees(TotalFeesAtvaiAmount.ToString(0));
	public ZString TotalFeesCodAmountString => FormatLineFees(TotalFeesCodAmount.ToString(0));
	public ZString TotalFeesNonPercuesAmountString => FormatLineFees(TotalFeesNonPercuesAmount.ToString(0));
	public ZString TotalFeesGuaranteedAmountString => FormatLineFees(TotalFeesGuaranteedAmount.ToString(0));

	ZString FormatLineFees(ZString sumLineFees)
	{
		for (int i = sumLineFees.Length - 3; i > 0; i -= 3)
		{
			sumLineFees = sumLineFees.Insert(i, " ");
		}
		return sumLineFees;
	}

	public ZDecimal TotalFeeAmount => CachedValueHelper.GetValue(ref totalFeeAmount, () => TotalFeesCautionAmount + TotalFeesNonCautionAmount);  // 1 + 2
	CachedValue<ZDecimal> totalFeeAmount;
	public ZDecimal TotalFeesGuaranteedAmount => CachedValueHelper.GetValue(ref totalFeesGuaranteedAmount, () => GetTotalFeesAmount(new ZString[] { "5", "" }));
	CachedValue<ZDecimal> totalFeesGuaranteedAmount;
	public ZDecimal TotalFeesCautionAmount => CachedValueHelper.GetValue(ref totalFeesCautionAmount, () => GetTotalFeesAmount(new ZString[] { "1" }));
	CachedValue<ZDecimal> totalFeesCautionAmount;
	public ZDecimal TotalFeesNonCautionAmount => CachedValueHelper.GetValue(ref totalFeesNonCautionAmount, () => GetTotalFeesAmount(new ZString[] { "2" }));
	CachedValue<ZDecimal> totalFeesNonCautionAmount;
	public ZDecimal TotalFeesAi2Amount => CachedValueHelper.GetValue(ref totalFeesAi2Amount, () => GetTotalFeesAmount(new ZString[] { "3" }));
	CachedValue<ZDecimal> totalFeesAi2Amount;
	public ZDecimal TotalFeesAtvaiAmount => CachedValueHelper.GetValue(ref totalFeesAtvaiAmount, () => GetTotalFeesAmount(new ZString[] { "6" }));
	CachedValue<ZDecimal> totalFeesAtvaiAmount;

	public ZDecimal TotalFeesCodAmount => CachedValueHelper.GetValue(ref totalFeesCodAmount, () => GetTotalFeesAmount(new ZString[] { "4" }));
	CachedValue<ZDecimal> totalFeesCodAmount;

	public ZDecimal TotalFeesNonPercuesAmount => CachedValueHelper.GetValue(ref totalFeesNonPercuesAmount, () => GetTotalFeesAmount(new ZString[] { "5" })); // not quite the same as TotalFeesGuaranteedAmount 
	CachedValue<ZDecimal> totalFeesNonPercuesAmount;
	ZDecimal GetTotalFeesAmount(IEnumerable<ZString> paymentMethods)
	{
		return EntryHeader.MergedLines.OfType<CusEntryLine>().SelectMany(x => x.ConfirmedFees).OfType<CusEntryLineFee>()
			.Where(x => paymentMethods.Contains(x.CF_MethodOfPayment)).Sum(x => x.CF_ChargeAmount);
	}

	public ZString SummaryValuationCaption => ShowOnlyDeltaCalculations ? (NoResString)"DELTA Amount in €" : (NoResString)"Global calculated amount in €";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not language specific")]
	public ZString SummaryTaxationCaption => ShowOnlyDeltaCalculations ? (NoResString)"Detailed DELTA taxes amount" : "Detailed calculated taxes amount";

	public ZString GetTypeAndNumberOfPackagePackage()
	{
		var entrylines = EntryHeader.MergedLines;
		var numberOfPackage = 0;
		var typeOfPackage = ZString.Empty;
		PackingWrapper pw = null;
		foreach (CusEntryLine entryline in entrylines)
		{
			if (entryline.Package != null)
			{
				pw = new PackingWrapper(entryline);
				numberOfPackage += pw.Count;

				if (typeOfPackage.IsEmpty)
				{
					typeOfPackage = pw.Type;
				}
				else if (pw.Type != typeOfPackage)
				{
					typeOfPackage = "MLT";
					break;
				}
			}
		}
		return numberOfPackage.ToString() + " " + typeOfPackage;
	}
	public ZString NumberAndTypeOfPackage => GetTypeAndNumberOfPackagePackage();
	public ZDecimal TotalGrossWeightInKg => EntryHeader.MergedLines.OfType<CusEntryLine>().Sum(x => x.EffectiveGrossWeight.InKilogramsSafe);
	public ZDecimal TotalNetWeightInKg => EntryHeader.MergedLines.OfType<CusEntryLine>().Sum(x => x.EffectiveNetWeight.InKilogramsSafe);
}
