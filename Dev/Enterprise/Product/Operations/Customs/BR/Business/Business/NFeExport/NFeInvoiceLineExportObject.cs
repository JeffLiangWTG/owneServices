using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NFeInvoiceLineExportObject : NonPersistentBusinessObject
	{
		public static class Schema
		{
			public const string InvoiceLineNumber = "InvoiceLineNumber";
			public const string InvoiceNumber = "InvoiceNumber";
			public const string ProductCode = "ProductCode";
			public const string TariffCode = "TariffCode";
			public const string InvoiceQuantity = "InvoiceQuantity";
			public const string InvoiceQuantityUQ = "InvoiceQuantityUQ";
			public const string CustomsQuantityUQ = "CustomsQuantityUQ";
			public const string CustomsQuantity = "CustomsQuantity";
			public const string GoodsDescription = "GoodsDescription";
			public const string SupplierName = "SupplierName";
			public const string ManufacturerName = "ManufacturerName";
			public const string FOBValue = "FOBValue";
			public const string FreightValue = "FreightValue";
			public const string InsuranceValue = "InsuranceValue";
			public const string CIFValue = "CIFValue";
			public const string DutyTaxRegime = "DutyTaxRegime";
			public const string DutyBaseAmount = "DutyBaseAmount";
			public const string DutyRate = "DutyRate";
			public const string DutyAmount = "DutyAmount";
			public const string IPITaxRegime = "IPITaxRegime";
			public const string IPIBaseAmount = "IPIBaseAmount";
			public const string IPIRate = "IPIRate";
			public const string IPIAmount = "IPIAmount";
			public const string IPISpecialRateUQ = "IPISpecialRateUQ";
			public const string IPISpecialRateQuantity = "IPISpecialRateQuantity";
			public const string IPISpecialRateAmount = "IPISpecialRateAmount";
			public const string PISCofinsTaxRegime = "PISCofinsTaxRegime";
			public const string PISBaseAmount = "PISBaseAmount";
			public const string PISRate = "PISRate";
			public const string PISAmount = "PISAmount";
			public const string PISSpecialRateUQ = "PISSpecialRateUQ";
			public const string PISSpecialRateQuantity = "PISSpecialRateQuantity";
			public const string PISSpecialRateAmount = "PISSpecialRateAmount";
			public const string CofinsBaseAmount = "CofinsBaseAmount";
			public const string CofinsRate = "CofinsRate";
			public const string CofinsAmount = "CofinsAmount";
			public const string CofinsSpecialRateUQ = "CofinsSpecialRateUQ";
			public const string CofinsSpecialRateQuantity = "CofinsSpecialRateQuantity";
			public const string CofinsSpecialRateAmount = "CofinsSpecialRateAmount";
			public const string ICMSTaxRegime = "ICMSTaxRegime";
			public const string ICMSLegalBase = "ICMSLegalBase";
			public const string ICMSBaseAmount = "ICMSBaseAmount";
			public const string ICMSRate = "ICMSRate";
			public const string ICMSReductionPercentage = "ICMSReductionPercentage";
			public const string ICMSAmount = "ICMSAmount";
			public const string FCPRate = "FCPRate";
			public const string FCPAmount = "FCPAmount";
			public const string SiscomexUsageFee = "SiscomexUsageFee";
			public const string AntidumpingBaseAmount = "AntidumpingBaseAmount";
			public const string AntidumpingRate = "AntidumpingRate";
			public const string AntidumpingAmount = "AntidumpingAmount";
			public const string AntidumpingSpecialRateUQ = "AntidumpingSpecialRateUQ";
			public const string AntidumpingSpecialRateQuantity = "AntidumpingSpecialRateQuantity";
			public const string AntidumpingSpecialRateAmount = "AntidumpingSpecialRateAmount";
			public const string Addition = "Addition";
			public const string Nve = "Nve";
			public const string ManufacturerIndicator = "ManufacturerIndicator";
			public const string IcmsTotalAmountReduction = "IcmsTotalAmountReduction";
			public const string AfrmmAmount = "AfrmmAmount";
			public const string ImportLicenseFineAmount = "ImportLicenseFineAmount";
			public const string EICAmount = "EICAmount";
			public const string OrderNumber = "OrderNumber";
			public const string OrderLineNumberAndSubLine = "OrderLineNumberAndSubLine";
			public const string ConcessionActNumber = "ConcessionActNumber";
			public const string Permits = "Permits";
			public const string Complement = "Complement";
			public const string GrossWeight = "GrossWeight";
			public const string GrossWeightUQ = "GrossWeightUQ";
			public const string NetWeight = "NetWeight";
			public const string NetWeightUQ = "NetWeightUQ";
		}

		public NFeInvoiceLineExportObject(JobComInvoiceLine invoiceLine) : base(invoiceLine.Factory)
		{
			this.InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		public readonly JobComInvoiceLine InvoiceLine;

		public static NFeInvoiceLineExportObject New(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new NFeInvoiceLineExportObject(invoiceLine);

		#region InvoiceLineNumber

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|InvoiceLineNumber", Caption = "Line No.")]
		public ZShort InvoiceLineNumber => InvoiceLine.JI_LineNo;

		#endregion

		#region InvoiceNumber

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|InvoiceNumber", Caption = "Invoice No.")]
		public ZString InvoiceNumber => InvoiceLine.JI_Calc_Invoice;

		#endregion

		#region ProductCode

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ProductCode", Caption = "Product Code")]
		public ZString ProductCode => InvoiceLine.JI_PartNo;

		#endregion

		#region TariffCode

		[ReadOnly(true)]
		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|TariffCode", Caption = "Tariff Code")]
		public ZString TariffCode => InvoiceLine.JI_Tariff;

		#endregion

		#region GoodsDescription

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|GoodsDescription", Caption = "Goods Description")]
		public ZString GoodsDescription
		{
			get => (fGoodsDescription.IsEmpty && InvoiceLine != null) ? InvoiceLine.FullGoodsDescription : fGoodsDescription;
			set => SetNonPersistentPropertyValue(GoodsDescriptionInfo, ref fGoodsDescription, value);
		}

		ZString fGoodsDescription;

		public ZPropertyInfo GoodsDescriptionInfo => GetZPropertyInfo(Schema.GoodsDescription);

		#endregion

		#region SupplierName

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|SupplierName", Caption = "Supplier Name")]
		public ZString SupplierName => InvoiceLine.Supplier?.OH_FullName ?? ZString.Empty;

		#endregion

		#region ManufacturerName

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ManufacturerName", Caption = "Manufacturer Name")]
		public ZString ManufacturerName => InvoiceLine.ManufacturerAddress?.Header?.OH_FullName ?? ZString.Empty;

		#endregion

		#region InvoiceQuantity

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|InvoiceQuantity", Caption = "Inv. Qty")]
		public ZDecimal InvoiceQuantity => InvoiceLine.JI_InvoiceQuantity;

		#endregion

		#region InvoiceQuantityUQ

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|InvoiceQuantityUQ", Caption = "UQ")]
		public ZString InvoiceQuantityUQ => InvoiceLine.JI_InvoiceUQ;

		#endregion

		#region CustomsQuantityUQ

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CustomsQuantityUQ", Caption = "UQ")]
		public ZString CustomsQuantityUQ => InvoiceLine.JI_CustomsUnitQty;

		#endregion

		#region CustomsQuantity

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CustomsQuantity", Caption = "Customs Qty")]
		public ZDecimal CustomsQuantity => InvoiceLine.JI_CustomsQuantity;

		#endregion

		#region FOBValue

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|FOBValue", Caption = "FOB")]
		public ZDecimal FOBValue => InvoiceLine.JI_Calc_FOB_InLocalCurrency;

		#endregion

		#region FreightValue

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|FreightValue", Caption = "Freight")]
		public ZDecimal FreightValue => InvoiceLine.OverseasFreightInLocalCurrency;

		#endregion

		#region InsuranceValue

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|InsuranceValue", Caption = "Insurance")]
		public ZDecimal InsuranceValue => InvoiceLine.OverseasInsuranceInLocalCurrency;

		#endregion

		#region CIFValue

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CIFValue", Caption = "Customs Value")]
		public ZDecimal CIFValue => InvoiceLine.JI_Calc_CIF_InLocalCurrency;

		#endregion

		#region GrossWeight

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|GrossWeight", Caption = "Gross Weight")]
		public ZDecimal GrossWeight => ((ZDecimal)Core.Constants.Weight.ConvertSafe(InvoiceLine.JI_Weight, InvoiceLine.JI_WeightUQ, Core.Constants.Weight.Kilograms)).Round(3).Normalize();

		#endregion

		#region GrossWeightUQ

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|GrossWeightUQ", Caption = "UQ")]
		public ZString GrossWeightUQ => Core.Constants.Weight.Kilograms;

		#endregion

		#region NetWeight

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|NetWeight", Caption = "Net Weight")]
		public ZDecimal NetWeight => ((ZDecimal)Core.Constants.Weight.ConvertSafe(InvoiceLine.JI_NetWeight, InvoiceLine.JI_NetWeightUQ, Core.Constants.Weight.Kilograms)).Round(3).Normalize();

		#endregion

		#region NetWeightUQ

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|NetWeightUQ", Caption = "UQ")]
		public ZString NetWeightUQ => Core.Constants.Weight.Kilograms;

		#endregion

		#region Duty

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|DutyTaxRegime", Caption = "Duty Tax Regime")]
		public ZString DutyTaxRegime => InvoiceLine.Lookups.DutyTaxRegimeList.GetDescriptionFromCode(InvoiceLine.DutyTaxRegime);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|DutyBaseAmount", Caption = "Duty Base Amount")]
		public ZDecimal DutyBaseAmount => InvoiceLine.JI_Calc_DutyBaseAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|DutyRate", Caption = "Duty Rate")]
		public ZDecimal DutyRate => InvoiceLine.DutyVigentRateValue;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|DutyAmount", Caption = "Duty Amount")]
		public ZDecimal DutyAmount => InvoiceLine.JI_Calc_DutyAmount.Round(AmountDecimalPlaces);

		#endregion

		#region IPI

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IPITaxRegime", Caption = "IPI Tax Regime")]
		public ZString IPITaxRegime => InvoiceLine.Lookups.IPITaxRegimeList.GetDescriptionFromCode(InvoiceLine.IPITaxRegime);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IPIBaseAmount", Caption = "IPI Base Amount")]
		public ZDecimal IPIBaseAmount => InvoiceLine.JI_Calc_IPIBaseAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IPIRate", Caption = "IPI Rate")]
		public ZDecimal IPIRate => InvoiceLine.IPIVigentRateValue;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IPIAmount", Caption = "IPI Amount")]
		public ZDecimal IPIAmount => InvoiceLine.JI_Calc_IPIAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IPISpecialRateUQ", Caption = "IPI UQ (Specific Rate)")]
		public ZString IPISpecialRateUQ => InvoiceLine.IPICalculateByUQRate?.UnitOfMeasure ?? ZString.Empty;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IPISpecialRateQuantity", Caption = "IPI Qty. (Specific Rate)")]
		public ZDecimal IPISpecialRateQuantity => InvoiceLine.IPICalculateByUQRate?.Quantity.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IPISpecialRateAmount", Caption = "IPI Amount (Specific Rate)")]
		public ZDecimal IPISpecialRateAmount => InvoiceLine.IPICalculateByUQRate?.RateOrUnitValue.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		#endregion

		#region PIS

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|PISCofinsTaxRegime", Caption = "PIS/COFINS Tax Regime")]
		public ZString PISCofinsTaxRegime => InvoiceLine.Lookups.PisCofinsTaxRegimeList.GetDescriptionFromCode(InvoiceLine.PisCofinsTaxRegime);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|PISBaseAmount", Caption = "PIS Base Amount")]
		public ZDecimal PISBaseAmount => InvoiceLine.JI_Calc_PISBaseAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|PISRate", Caption = "PIS Rate")]
		public ZDecimal PISRate => InvoiceLine.PisVigentRateValue;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|PISAmount", Caption = "PIS Amount")]
		public ZDecimal PISAmount => InvoiceLine.JI_Calc_PISAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|PISSpecialRateUQ", Caption = "PIS UQ (Specific Rate)")]
		public ZString PISSpecialRateUQ => InvoiceLine.PISCalculateByUQRate?.UnitOfMeasure ?? ZString.Empty;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|PISSpecialRateQuantity", Caption = "PIS Qty. (Specific Rate)")]
		public ZDecimal PISSpecialRateQuantity => InvoiceLine.PISCalculateByUQRate?.Quantity.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|PISSpecialRateAmount", Caption = "PIS Amount (Specific Rate)")]
		public ZDecimal PISSpecialRateAmount => InvoiceLine.PISCalculateByUQRate?.RateOrUnitValue.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		#endregion

		#region Cofins

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CofinsBaseAmount", Caption = "COFINS Base Amount")]
		public ZDecimal CofinsBaseAmount => InvoiceLine.JI_Calc_CofinsBaseAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CofinsRate", Caption = "COFINS Rate")]
		public ZDecimal CofinsRate => InvoiceLine.CofinsVigentRateValue;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CofinsAmount", Caption = "COFINS Amount")]
		public ZDecimal CofinsAmount => InvoiceLine.JI_Calc_CofinsAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CofinsSpecialRateUQ", Caption = "COFINS UQ (Specific Rate)")]
		public ZString CofinsSpecialRateUQ => InvoiceLine.CofinsCalculateByUQRate?.UnitOfMeasure ?? ZString.Empty;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CofinsSpecialRateQuantity", Caption = "COFINS Qty. (Specific Rate)")]
		public ZDecimal CofinsSpecialRateQuantity => InvoiceLine.CofinsCalculateByUQRate?.Quantity.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|CofinsSpecialRateAmount", Caption = "COFINS Amount (Specific Rate)")]
		public ZDecimal CofinsSpecialRateAmount => InvoiceLine.CofinsCalculateByUQRate?.RateOrUnitValue.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		#endregion

		#region ICMS

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ICMSTaxRegime", Caption = "ICMS Tax Regime")]
		public ZString ICMSTaxRegime => InvoiceLine.Lookups.ICMSTaxRegimeList.GetDescriptionFromCode(InvoiceLine.ICMSTaxRegime);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ICMSLegalBase", Caption = "ICMS Legal Base")]
		public ZString ICMSLegalBase => InvoiceLine.Lookups.ICMSLegalBaseList.GetDescriptionFromCode(InvoiceLine.ICMSLegalBase);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ICMSBaseAmount", Caption = "ICMS Base Amount")]
		public ZDecimal ICMSBaseAmount => InvoiceLine.JI_Calc_ICMSBaseAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ICMSRate", Caption = "ICMS Rate")]
		public ZDecimal ICMSRate => InvoiceLine.JI_ICMSRate;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ICMSReductionPercentage", Caption = "ICMS Reduction of (%)")]
		public ZDecimal ICMSReductionPercentage => InvoiceLine.JI_ICMSBaseValueReductionPercentage;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ICMSAmount", Caption = "ICMS Amount")]
		public ZDecimal ICMSAmount => InvoiceLine.JI_Calc_ICMSAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|IcmsTotalAmountReduction", Caption = "ICMS Total Amount Reduction")]
		public ZDecimal IcmsTotalAmountReduction => InvoiceLine.JI_ICMSTotalAmountReductionPercentage;

		#endregion

		#region FCP

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|FCPRate", Caption = "% ICMS FCP")]
		public ZDecimal FCPRate => InvoiceLine.ICMSFCPRateValue;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|FCPAmount", Caption = "FCP Amount")]
		public ZDecimal FCPAmount => InvoiceLine.JI_Calc_FCPAmount.Round(AmountDecimalPlaces);

		#endregion

		#region Siscomex Fee

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|SiscomexUsageFee", Caption = "SISCOMEX Usage Fee")]
		public ZDecimal SiscomexUsageFee => InvoiceLine.JI_Calc_SiscomexUsageAmount;

		#endregion

		#region Antidumping

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|AntidumpingBaseAmount", Caption = "Antidumping Base Amount")]
		public ZDecimal AntidumpingBaseAmount => InvoiceLine.JI_Calc_AntidumpingBaseAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|AntidumpingRate", Caption = "Antidumping Rate")]
		public ZDecimal AntidumpingRate => InvoiceLine.AntidumpingRateValue;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|AntidumpingAmount", Caption = "Antidumping Amount")]
		public ZDecimal AntidumpingAmount => InvoiceLine.JI_Calc_AntidumpingAmount.Round(AmountDecimalPlaces);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|AntidumpingSpecialRateUQ", Caption = "Antidumping UQ (Specific Rate)")]
		public ZString AntidumpingSpecialRateUQ => InvoiceLine.AntidumpingCalculateByUQRate?.UnitOfMeasure ?? ZString.Empty;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|AntidumpingSpecialRateQuantity", Caption = "Antidumping Qty. (Specific Rate)")]
		public ZDecimal AntidumpingSpecialRateQuantity => InvoiceLine.AntidumpingCalculateByUQRate?.Quantity.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|AntidumpingSpecialRateAmount", Caption = "Antidumping Amount (Specific Rate)")]
		public ZDecimal AntidumpingSpecialRateAmount => InvoiceLine.AntidumpingCalculateByUQRate?.RateOrUnitValue.Round(AmountDecimalPlaces) ?? ZDecimal.Zero;

		#endregion

		#region AfrmmAmount

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|AfrmmAmount", Caption = "AFRMM Amount")]
		public ZDecimal AfrmmAmount => InvoiceLine.JI_Calc_AfrmmAmount.Round(4);

		#endregion

		#region ImportLicenseFineAmount

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ImportLicenseFineAmount", Caption = "Import License Fine Amount")]
		public ZDecimal ImportLicenseFineAmount => InvoiceLine.JI_Calc_ImportLicenseFineAmount.Round(AmountDecimalPlaces);

		#endregion

		#region EICAmount

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|EICAmount", Caption = "EIC - Other Expenses to ICMS Amount")]
		public ZDecimal EICAmount => InvoiceLine.JI_Calc_EICAmount.Round(AmountDecimalPlaces);

		#endregion

		#region Addition

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|Addition", Caption = "Addition")]
		public ZString Addition => InvoiceLine.CusEntryLine?.CL_LineNumber.ToString() ?? ZString.Empty;

		#endregion

		#region Nve

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|Nve", Caption = "NVE")]
		public ZString Nve
		{
			get
			{
				return string.Join(",", InvoiceLine.NVECusCodeDataCollection.Cast<NveCusCodeData>().Select(x => $"{x.CY_Code};{x.Attribute};{x.Specification}").OrderBy(x => x));
			}
		}

		#endregion

		#region ManufacturerIndicator

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ManufacturerIndicator", Caption = "Manufacturer Indicator")]
		public ZString ManufacturerIndicator => InvoiceLine.Lookups.ManufacturerIndicatorList.GetDescriptionFromCode(InvoiceLine.JI_ManufacturerIndicator);

		#endregion

		#region OrderNumber

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|OrderNumber", Caption = "Order Number")]
		public ZString OrderNumber => InvoiceLine.JI_OrderNumber;

		#endregion

		#region OrderLineNumberAndSubLine

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|OrderLineNumberAndSubLine", Caption = "Order Line No.")]
		public ZString OrderLineNumberAndSubLine => InvoiceLine.JI_Calc_OrderLineNumberAndSubLine;

		#endregion

		#region ConcessionActNumber

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|ConcessionActNumber", Caption = "Concession Act Number")]
		public ZString ConcessionActNumber => InvoiceLine.AttachedImportLicenseLine?.DrawbackCANumber ?? ZString.Empty;

		#endregion

		#region Permits

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|Permits", Caption = "Permits")]
		public ZString Permits
		{
			get
			{
				return string.Join(",", InvoiceLine.Permits.Cast<Permit>().Select(x => $"{x.CSI_ReferenceNumber};{x.CSI_Quantity};{x.Lookups.UQList.GetDescriptionFromCode(x.CSI_UnitOfQuantity)}").OrderBy(x => x));
			}
		}

		#endregion

		#region Complement

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeInvoiceLineExportObject|Complement", Caption = "Complement")]
		public ZString Complement => InvoiceLine.ComplementaryDescription;

		#endregion

		const int AmountDecimalPlaces = 2;
	}
}
