using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.CH.Business;

public class CusEntryLine : Customs.Business.CusEntryLine
{
	public new class Schema : Customs.Business.CusEntryLine.Schema
	{
		public const int CalcGrossWeightScaleImport = 1;
		public const int CalcGrossWeightScaleExport = 3;
		public const int CalcGrossWeightUQMaxLength = 2;
		public const int CalcNetWeightScale = 3;
		public const int CalcNetWeightUQMaxLength = 2;
		public const int CalcAdditionalQtyScale = 1;
		public const int CalcAdditionalQtyUQMaxLength = 3;
		public const int CalcCustomsNetWeightScale = 1;
		public const int CalcCustomsNetWeightUQMaxLength = 2;
	}

	public CusEntryLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new CusEntryLine Clone() => (CusEntryLine)base.Clone();

	public new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine> Fees => (CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>)base.Fees;

	public new CusEntryHeader Header => (CusEntryHeader)base.Header;

	public new JobComInvoiceLine RandomLine => base.RandomLine as JobComInvoiceLine;

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	public new CusEntryLineLookups Lookups => (CusEntryLineLookups)base.Lookups;

	public new CusEntryLineValidation Validation => (CusEntryLineValidation)base.Validation;

	protected override System.Type GetCusEntryHeaderType() => typeof(CusEntryHeader);

	protected override ICusEntryLineFeeCollection<Customs.Business.CusEntryLineFee, Customs.Business.CusEntryLine> GetCusEntryLineFeeCollection() => new CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>(this, Factory);

	protected override Customs.Business.CusEntryLineLookups GetNewLookups() => new CusEntryLineLookups(this);

	protected override Customs.Business.CusEntryLineValidation GetNewValidation() => new CusEntryLineValidation(this);

	protected override TariffFormatter GetTariffFormatter() => new TariffFormatterCH();

	public bool IsDeclarationIntegrated => Header?.IsDeclarationIntegrated ?? true;

	protected override ZDecimal GetDutyAmountCore()
	{
		return IsDeclarationIntegrated ? Fees.GetAmount(FeeTypeList.Codes.A00) : (ZDecimal)Fees.Cast<CusEntryLineFee>().Where(fee => !fee.IsVAT).Sum(x => x.CF_ChargeAmount);
	}

	protected override ZDecimal GetGSTVATAmountCore()
	{
		return IsDeclarationIntegrated ? Fees.GetAmount(FeeTypeList.Codes.B00) : (ZDecimal)Fees.Cast<CusEntryLineFee>().Where(fee => fee.IsVAT).Sum(x => x.CF_ChargeAmount);
	}

	protected override void DoMergeInvoiceLine(BaseJobComInvoiceLine baseInvoiceLine)
	{
		base.DoMergeInvoiceLine(baseInvoiceLine);
		CL_StatisticalValue += ((JobComInvoiceLine)baseInvoiceLine).JI_Calc_StatisticalValue;
	}

	public override void ResetTotalsAndCachedValues()
	{
		base.ResetTotalsAndCachedValues();
		CL_StatisticalValue = ZDecimal.Zero;
	}

	[ResourceStringData("CH.CusEntryLine.CL_AdValoremTariff", Caption = "Tariff Code")]
	public override ZString CL_AdValoremTariff
	{
		get => base.CL_AdValoremTariff;
		set => base.CL_AdValoremTariff = value;
	}

	[ResourceStringData("CH.CusEntryLine.CL_Description", Caption = "Description")]
	public override ZString CL_Description
	{
		get => base.CL_Description;
		set => base.CL_Description = value;
	}

	ZDecimal GetCalcGrossWeight() => ((ZDecimal)InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.JI_CustomsQuantity)).Ceiling(CalcGrosWeightDecimalPlaces);
	int CalcGrosWeightDecimalPlaces => Header.IsImport ? Schema.CalcGrossWeightScaleImport : Schema.CalcGrossWeightScaleExport;

	[ResourceStringData("CH.CusEntryLine.CalcGrossWeight", Caption = "Gross Weight")]
	[DecimalPlaces("CalcGrosWeightDecimalPlaces")]
	public ZDecimal CalcGrossWeight
	{
		get
		{
			if (calcGrossWeight == null)
			{
				calcGrossWeight = new CachedProperty<ZDecimal>(Factory, GetCalcGrossWeight);
			}
			return calcGrossWeight.Value;
		}
	}
	CachedProperty<ZDecimal> calcGrossWeight;

	public ZPropertyInfo CalcGrossWeightInfo => GetZPropertyInfo(nameof(CalcGrossWeight));

	[MaxLength(Schema.CalcGrossWeightUQMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.WeightUQList))]
	public ZString CalcGrossWeightUQ => Core.Constants.Weight.Kilograms;

	public ZPropertyInfo CalcGrossWeightUQInfo => GetZPropertyInfo(nameof(CalcGrossWeightUQ));

	ZDecimal GetCalcNetWeight() => ((ZDecimal)InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.JI_CustomsSecondQuantity)).Ceiling(Schema.CalcNetWeightScale);

	[ResourceStringData("CH.CusEntryLine.CalcNetWeight", Caption = "Net Weight")]
	[DecimalPlaces(Schema.CalcNetWeightScale)]
	public ZDecimal CalcNetWeight
	{
		get
		{
			if (calcNetWeight == null)
			{
				calcNetWeight = new CachedProperty<ZDecimal>(Factory, GetCalcNetWeight);
			}
			return calcNetWeight.Value;
		}
	}
	CachedProperty<ZDecimal> calcNetWeight;

	public ZPropertyInfo CalcNetWeightInfo => GetZPropertyInfo(nameof(CalcNetWeight));

	[MaxLength(Schema.CalcNetWeightUQMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.WeightUQList))]
	public ZString CalcNetWeightUQ => Core.Constants.Weight.Kilograms;

	public ZPropertyInfo CalcNetWeightUQInfo => GetZPropertyInfo(nameof(CalcNetWeightUQ));

	ZDecimal GetCalcAdditionalQty() => ((ZDecimal)InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => l.JI_CustomsThirdQuantity)).Ceiling(Schema.CalcAdditionalQtyScale);

	[ResourceStringData("CH.CusEntryLine.CalcAdditionalQty", Caption = "Additional Qty")]
	[DecimalPlaces(Schema.CalcAdditionalQtyScale)]
	public ZDecimal CalcAdditionalQty
	{
		get
		{
			if (calcAdditionalQty == null)
			{
				calcAdditionalQty = new CachedProperty<ZDecimal>(Factory, GetCalcAdditionalQty);
			}
			return calcAdditionalQty.Value;
		}
	}
	CachedProperty<ZDecimal> calcAdditionalQty;

	public ZPropertyInfo CalcAdditionalQtyInfo => GetZPropertyInfo(nameof(CalcAdditionalQty));

	[MaxLength(Schema.CalcAdditionalQtyUQMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.CustomsUQList))]
	public ZString CalcAdditionalQtyUQ => RandomLine.JI_CustomsThirdUnitQty;

	public ZPropertyInfo CalcAdditionalQtyUQInfo => GetZPropertyInfo(nameof(CalcAdditionalQtyUQ));

	ZDecimal GetCalcCustomsNetWeight() => ((ZDecimal)InvoiceLines.Cast<JobComInvoiceLine>().Sum(l => new ZWeight(l.JI_WeightIncludingInnerPackage, l.JI_WeightIncludingInnerPackageUQ).InKilogramsSafe)).Ceiling(Schema.CalcCustomsNetWeightScale);

	[ResourceStringData("CH.CusEntryLine.CalcCustomsNetWeight", Caption = "Customs Net Weight")]
	[DecimalPlaces(Schema.CalcCustomsNetWeightScale)]
	public ZDecimal CalcCustomsNetWeight
	{
		get
		{
			if (calcCustomsNetWeight == null)
			{
				calcCustomsNetWeight = new CachedProperty<ZDecimal>(Factory, GetCalcCustomsNetWeight);
			}
			return calcCustomsNetWeight.Value;
		}
	}
	CachedProperty<ZDecimal> calcCustomsNetWeight;

	public ZPropertyInfo CalcCustomsNetWeightInfo => GetZPropertyInfo(nameof(CalcCustomsNetWeight));

	[MaxLength(Schema.CalcCustomsNetWeightUQMaxLength)]
	[List(nameof(Lookups) + "." + nameof(CusEntryLineLookups.WeightUQList))]
	public ZString CalcCustomsNetWeightUQ => Core.Constants.Weight.Kilograms;

	public ZPropertyInfo CalcCustomsNetWeightUQInfo => GetZPropertyInfo(nameof(CalcCustomsNetWeightUQ));

	[ResourceStringData("CH.Business.CusEntryLine|ConfirmedDuty", Caption = "Confirmed Duty", FullDescription = "Sum of confirmed Duties", ShortCaption = "Conf. DTY")]
	public ZDecimal ConfirmedDuty
	{
		get { return ConfirmedFees.Where(f => f.CF_ChargeType != Core.Constants.Customs.CusEntryFeeTypes.VAT).Sum(f => f.CF_ChargeAmount); }
	}

	public ZPropertyInfo ConfirmedDutyInfo => GetZPropertyInfo(nameof(ConfirmedDuty));

	[ResourceStringData("CH.Business.CusEntryLine|ConfirmedVAT", Caption = "Confirmed VAT", FullDescription = "Sum of confirmed VAT", ShortCaption = "Conf. VAT")]
	public ZDecimal ConfirmedVAT
	{
		get { return ConfirmedFees.Where(f => f.CF_ChargeType == Core.Constants.Customs.CusEntryFeeTypes.VAT).Sum(f => f.CF_ChargeAmount); }
	}

	public ZPropertyInfo ConfirmedVATInfo => GetZPropertyInfo(nameof(ConfirmedVAT));
}
