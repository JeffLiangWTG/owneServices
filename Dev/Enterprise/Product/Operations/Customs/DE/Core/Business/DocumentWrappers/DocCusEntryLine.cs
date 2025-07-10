using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;
using CusEntryLineFee = Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	public class DocCusEntryLine : DocBaseCusEntryLine
	{
		DocCusEntryLine(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap)
			: base(cusEntryLine, factoryToWrap)
		{
		}

		public static DocCusEntryLine New(CusEntryLine cusEntryLine, BusinessObjectFactory factoryToWrap) => cusEntryLine == null ? null : new DocCusEntryLine(cusEntryLine, factoryToWrap);

		public DocBaseJobComInvoiceLine InvoiceLine => InvoiceLineInternal;

		//Vorgelegte Unterlagen Position
		public DocSupportingDocumentCollection SupportingDocuments => supportingDocuments ??= new DocSupportingDocumentCollection(EntryLine.RandomLine.SupportingDocuments.Cast<SupportingDocument>(), Factory);
		DocSupportingDocumentCollection supportingDocuments;

		//Hinzurechnungen / Abzüge
		public CombinedChargesCollection Charges => charges ??= new CombinedChargesCollection(InvoiceLines.SelectMany(i => i.Charges.Cast<InvoiceLineCharge>()), Factory);

		CombinedChargesCollection charges;

		//Hinzurechnungen / Abzüge apportioned
		public CombinedChargesCollection ApportionCharges => apportionCharges ??= new CombinedChargesCollection(InvoiceLines.SelectMany(i => i.ApportionedCharges.Cast<InvoiceLineApportionCharge>()), Factory);

		CombinedChargesCollection apportionCharges;

		//Angaben über Abgabenbeträge Position
		public DocEntryFeeCollection Fees
		{
		get
			{
				if (fees == null)
				{
					var countryCode = EntryLine.Declaration?.CountryCode;
					if (UseConfirmedFees)
					{
						var confirmedGroupedFees = EntryLine.ConfirmedFeesReadOnly.Cast<CusEntryLineConfirmedFeeWrapper>();
						fees = new DocEntryFeeCollection(confirmedGroupedFees
							.Select(l => new EntryFee(
								description: Factory.GetGermanRateCodeDescription(l.CF_ChargeType, countryCode) ?? l.ChargeTypeDescription,
								chargeType: l.CF_ChargeType,
								chargeAmount: l.CF_ChargeAmount,
								baseValue: l.CF_BaseValue,
								methodOfCalculation: l.MethodOfCalculationDE,
								rate: l.CF_Rate))
							.ToArray(), Factory);
					}
					else
					{
						fees = new DocEntryFeeCollection(EntryLine.Fees.Cast<CusEntryLineFee>()
							.Select(l => new EntryFee(
								description: Factory.GetGermanRateCodeDescription(l.CF_ChargeType, countryCode) ?? l.ChargeTypeDescription,
								chargeType: l.CF_ChargeType,
								chargeAmount: l.CF_ChargeAmount,
								baseValue: l.CF_BaseValue,
								methodOfCalculation: l.CF_MethodOfCalculation,
								rate: l.CF_Rate))
							.ToArray(), Factory);
					}
				}
				return fees;
			}
		}
		DocEntryFeeCollection fees;

		public ZBool UseConfirmedFees { get; set; }

		public ZInt NumberOfPacks => PackagePivot?.CHC_NumberOfPacks ?? ZInt.Zero;

		public ZString PackType => PackagePivot?.Package?.CW_PackType ?? ZString.Empty;

		public ZString MarksAndNumbers => PackagePivot?.Package?.CW_MarksAndNos ?? ZString.Empty;

		public ZString GrossWeight => EntryLine.GrossWeight.Amount.ToStringRounded(3);

		public ZString GrossWeightUQ => EntryLine.GrossWeight.Unit;

		public ZString NetWeight => EntryLine.EffectiveNetWeight.Amount.ToStringRounded(3);

		public ZString NetWeightUQ => EntryLine.EffectiveNetWeight.Unit;

		public ZString Procedure => RandomInvoiceLine.JI_Procedure.SubstringSafe(0, 4);

		public ZString CessionFlag => RandomInvoiceLine.JI_CessionFlag;

		public ZString EUCode => RandomInvoiceLine.JI_Procedure.SubstringSafe(4, 3);

		public ZString AdditionalCodes => string.Join(" ", RandomInvoiceLine.JI_SupplementaryCode1, RandomInvoiceLine.JI_SupplementaryCode2).Trim();

		public ZString LinePrice => linePrice.ToStringRounded(2);

		public ZDecimal linePrice => InvoiceLines.Sum(e => e.JI_LinePrice);

		public ZString LinePriceCurrency => RandomInvoiceLine.JI_RX_NKLinePriceCurr;

		public ZString Rate => rate.ToString(6, useCommas: true);

		ZDecimal rate => InvoiceHeader.JZ_InvoiceCurrExRate;

		public ZString LinePriceEURValue
		{
			get
			{
				var result = ZDecimal.Zero;
				if (rate > ZDecimal.Zero)
				{
					result = linePrice / rate;
				}
				return result.ToStringRounded(2);
			}
		}

		public ZString NetPrice => netPrice.ToStringRounded(2);

		ZDecimal netPrice => InvoiceLines.Sum(e => e.JI_NetPrice);

		public ZString NetPriceCurrency => RandomInvoiceLine.JI_RX_NKNetPriceCurr;

		//Kurs
		public ZString NetPriceEURValue
		{
			get
			{
				var result = ZDecimal.Zero;
				if (rate > ZDecimal.Zero)
				{
					result = netPrice / rate;
				}
				return result.ToStringRounded(2);
			}
		}

		public ZString StatisticsValue => ((ZDecimal)InvoiceLines.Sum(x => x.JI_Calc_StatisticalValue)).ToStringRounded(2);

		public ZString CustomsValueAsString => CustomsValue.ToStringRounded(2);

		public ZString StatisticsValueQTY => ((ZDecimal)InvoiceLines.Sum(i => i.JI_CustomsSecondQuantity)).ToStringRounded(3);

		public ZString StatisticsValueUQ => RandomInvoiceLine.JI_CustomsSecondUnitQty;

		public ZString PrimaryPreference => RandomInvoiceLine.JI_PrimaryPreference;

		public ZString CustomsQuantityAsString => ((ZDecimal)InvoiceLines.Sum(i => i.JI_CustomsThirdQuantity)).ToStringRounded(3);

		public ZString CustomsQuantityUQ => RandomInvoiceLine.JI_CustomsThirdUnitQty;

		public ZString CustomsQuantity2 => ((ZDecimal)InvoiceLines.Sum(i => i.JI_CustomsFourthQuantity)).ToStringRounded(3);

		public ZString CustomsQuantityUQ2 => RandomInvoiceLine.JI_CustomsFourthUnitQty;

		public ZString CountryOfOrigin
		{
			get
			{
				var countryOfOrigin = RandomInvoiceLine.JI_CountryOfOrigin;
				var countryName = RefCountry.LoadFromCountryCode(Factory, countryOfOrigin)?.RN_Desc ?? ZString.Empty;
				return string.Join(" ", countryOfOrigin, countryName);
			}
		}

		public ZString PreferentialCountry
		{
			get
			{
				var countryOfSupply = RandomInvoiceLine.ZG_CountryOfSupply;
				var countryName = RefCountry.LoadFromCountryCode(Factory, countryOfSupply)?.RN_Desc ?? ZString.Empty;
				return string.Join(" ", countryOfSupply, countryName);
			}
		}

		protected override DocBaseJobComInvoiceLine CreateJobComInvoiceLine(BaseJobComInvoiceLine invoiceLineToWrap) => DocBaseJobComInvoiceLine.New(invoiceLineToWrap, Factory);

		InvoiceLinePackagePivot PackagePivot => packagePivot ??= EntryLine.PackagingDetails.FirstOrDefault();
		InvoiceLinePackagePivot packagePivot;

		CusEntryLine EntryLine => entryLine ??= (CusEntryLine)WrappedObject;
		CusEntryLine entryLine;

		JobComInvoiceLine RandomInvoiceLine => invoiceLine ??= EntryLine.RandomLine;
		JobComInvoiceLine invoiceLine;

		JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= RandomInvoiceLine.InvoiceHeader;
		JobComInvoiceHeader invoiceHeader;

		IEnumerable<JobComInvoiceLine> InvoiceLines => Factory.GetCached(ref invoiceLinesCached, () => EntryLine.InvoiceLines.Cast<JobComInvoiceLine>());
		CachedProperty<IEnumerable<JobComInvoiceLine>> invoiceLinesCached;
	}
}
