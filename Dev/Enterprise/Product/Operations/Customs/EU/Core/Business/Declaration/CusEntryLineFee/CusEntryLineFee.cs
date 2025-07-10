using System;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.EU.Business.Declaration
{
	[SystemDefinedValues]
	public class CusEntryLineFee : TypeSafeCusEntryLineFee
		, Integration.Customs.EU.ICusEntryLineFee
		, IEuTax
		, IDocSADHLineTaxBoxSupporter
		, IDisposable
	{
		public CusEntryLineFee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ChargeAmountRefresher.HookEvents();
		}

		public void Dispose()
		{
			chargeAmountRefresher?.UnhookEvents();
			chargeAmountRefresher = null;
			userEnteredStashSource = null;
		}

		public new CusEntryLine EntryLine => Factory.Load<CusEntryLine>(CF_CL);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new partial class Schema : AutoCusEntryLineFee.Schema
		{
			public const string NationalFeeTypeCode = "NationalFeeTypeCode";
			public const int NationalFeeTypeCodeMaxLength = 5;
		}

		public const int TaxRateDecimalPrecision = 6;

		[MaxLength(Schema.NationalFeeTypeCodeMaxLength)]
		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|NationalFeeTypeCode", Caption = "National Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.NationalFeeTypeCodeList))]
		public virtual ZString NationalFeeTypeCode
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.NationalFeeTypeCode);
			set
			{
				var oldValue = NationalFeeTypeCode;
				CheckMaximumLength(NationalFeeTypeCodeInfo, value);
				this.SetSystemDefinedValue(Schema.NationalFeeTypeCode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateNationalFeeTypeCode();
				}
				NationalFeeTypeCodeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo NationalFeeTypeCodeInfo => GetZPropertyInfo(Schema.NationalFeeTypeCode);

		#region Override Properties

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.ChargeTypeList))]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public override ZString CF_ChargeType
		{
			get => base.CF_ChargeType;
			set
			{
				var oldValue = CF_ChargeType;
				base.CF_ChargeType = value;
				if (!IsCopying && CF_ChargeType != oldValue)
				{
					DefaultMethodOfPaymentIfEmpty();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.RateOverrideReasonList))]
		public override ZString CF_RateOverrideReasonCode => base.CF_RateOverrideReasonCode;

		[ReadOnlyMember(nameof(IsActionBlank))]
		public override ZDecimal CF_BaseValue => base.CF_BaseValue;

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfCalculationList))]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public override ZString CF_MethodOfCalculation => base.CF_MethodOfCalculation;

		[ReadOnlyMember(nameof(IsActionBlank))]
		public override ZDecimal CF_Rate => base.CF_Rate;

		[ReadOnlyMember(nameof(TotalAmountReadOnly))]
		public override ZDecimal CF_ChargeAmount
		{
			get => base.CF_ChargeAmount;
			set
			{
				base.CF_ChargeAmount = ChargeAmountRounder.Round(value);
				if (EntryLine?.Header?.Declaration is JobDeclaration declaration)
				{
					var merger = new LineMerger(declaration);
					merger.PopulateTotalAmountPayableForCusEntryHeaders();
				}
			}
		}

		public virtual bool TotalAmountReadOnly => !(CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Additional ||
				CF_RateOverrideReasonCode == RateOverrideReasonList.Codes.Override);

		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfPaymentList))]
		public override ZString CF_MethodOfPayment => base.CF_MethodOfPayment;

		public virtual bool IsActionBlank => CF_RateOverrideReasonCode.IsEmpty;

		#endregion

		protected override bool ShouldResetDataOnMergingCore => false;

		protected override bool ShouldDeleteIfChargeAmountIsZero => !EntryLine?.Declaration?.Configuration.UseUniversalFeeCalculation(EntryLine.Declaration) ?? base.ShouldDeleteIfChargeAmountIsZero;

		public IFeeRounder ChargeAmountRounder => chargeAmountRounder ?? (chargeAmountRounder = GetNewChargeAmountRounder());
		IFeeRounder chargeAmountRounder;

		protected virtual IFeeRounder GetNewChargeAmountRounder() => new FeeNoRounder();

		public bool IncludeForVatCalculation => IncludeForVatCalculationCore;
		protected virtual bool IncludeForVatCalculationCore => true;

		public bool AllowNegativeAmount => AllowNegativeAmountCore;
		protected virtual bool AllowNegativeAmountCore => false;

		public bool AllowZeroOrEmptyAmount => AllowZeroOrEmptyAmountCore;
		protected virtual bool AllowZeroOrEmptyAmountCore => false;

		public void DefaultMethodOfPaymentIfEmpty()
		{
			if (CF_MethodOfPayment.IsEmpty)
			{
				CF_MethodOfPayment = GetDefaultMethodOfPaymentValue();
			}
		}
		protected virtual ZString GetDefaultMethodOfPaymentValue() => ZString.Empty;

		#region IEuTax Members

		ZBool IEuTax.IsCopying => IsCopying;

		public ZString CountryCode => EntryLine?.Declaration?.CountryCode ?? ZString.Empty;

		public ICanBeImportOrExport ImportExportParent => EntryLine;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|G4_MethodOfPayment", Caption = "Method of Payment")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfPaymentList))]
		public ZString G4_MethodOfPayment { get => CF_MethodOfPayment; set => CF_MethodOfPayment = value; }
		public ZPropertyInfo G4_MethodOfPaymentInfo => CF_MethodOfPaymentInfo;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|G4_RateDuty", Caption = "Method of Calculation")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.MethodOfCalculationList))]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public virtual ZString G4_RateDuty { get => CF_MethodOfCalculation; set => CF_MethodOfCalculation = value; }
		public ZPropertyInfo G4_RateDutyInfo => CF_MethodOfCalculationInfo;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|G4_RateSuspension", Caption = "Tax Rate")]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public ZString G4_RateSuspension { get => (CF_Rate.IsEmpty ? ZDecimal.Zero : CF_Rate).ToString("0.000000#"); set => CF_Rate = ZDecimal.ParseSafe(value, 0m); }
		public ZPropertyInfo G4_RateSuspensionInfo => CF_RateInfo;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|G4_RateOverride", Caption = "Action")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.RateOverrideReasonList))]
		public ZString G4_RateOverride { get => CF_RateOverrideReasonCode; set => CF_RateOverrideReasonCode = value; }
		public ZPropertyInfo G4_RateOverrideInfo => CF_RateOverrideReasonCodeInfo;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|G4_Type", Caption = "Type")]
		[List(nameof(Lookups) + "." + nameof(CusEntryLineFeeLookups.ChargeTypeList))]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public ZString G4_Type { get => CF_ChargeType; set => CF_ChargeType = value; }
		public ZPropertyInfo G4_TypeInfo => CF_ChargeTypeInfo;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|G4_Amount", Caption = "Total Amount")]
		[ReadOnlyMember(nameof(TotalAmountReadOnly))]
		public ZString G4_Amount { get => CF_ChargeAmount.IsEmpty ? "0.00" : CF_ChargeAmount.ToString("#.00", CultureInfo.CurrentCulture); set => CF_ChargeAmount = ZDecimal.ParseSafe(value, 0m); }
		public ZPropertyInfo G4_AmountInfo => CF_ChargeAmountInfo;

		[ResourceStringData("Enterprise.Customs.EU.Business.Declaration.CusEntryLineFee|G4_BaseAmount", Caption = "Base Amount")]
		[DecimalPlaces(2)]
		[ReadOnlyMember(nameof(IsActionBlank))]
		public ZDecimal G4_BaseAmount { get => CF_BaseValue; set => CF_BaseValue = value; }
		public ZPropertyInfo G4_BaseAmountInfo => CF_BaseValueInfo;

		public ZDecimal G4_CalculatedPercentage => throw new NotSupportedException();

		public ZDecimal G4_BaseQuantity { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		public ZString G4_BaseQuantityUQ { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

		public ZString ChargeTypeDescription => Lookups.ChargeTypeList.GetDescriptionFromCode(CF_ChargeType);

		public ZString NationalFeeTypeCodeDescription => Lookups.NationalFeeTypeCodeList.GetDescriptionFromCode(NationalFeeTypeCode);

		#endregion

		#region ISADHLineTaxBoxSupporter Members

		ZString IDocSADHLineTaxBoxSupporter.Type => CF_ChargeType;

		ZString IDocSADHLineTaxBoxSupporter.TaxBase => TaxBaseCore();

		ZString IDocSADHLineTaxBoxSupporter.Rate => TaxBoxSupporterRateCore;
		protected virtual ZString TaxBoxSupporterRateCore => FormatDecimalDefault(CF_Rate);

		ZString IDocSADHLineTaxBoxSupporter.RateDuty => RateDutyCore;
		protected virtual ZString RateDutyCore => ZString.Empty;

		ZString IDocSADHLineTaxBoxSupporter.RateOverride => ZString.Empty;

		ZString IDocSADHLineTaxBoxSupporter.AmountInDeclarationCurrency => AmountInDeclarationCurrencyCore();

		ZString IDocSADHLineTaxBoxSupporter.MethodOfPayment => CF_MethodOfPayment;

		ZString IDocSADHLineTaxBoxSupporter.NationalFeeTypeCode => NationalFeeTypeCode;

		ZString IDocSADHLineTaxBoxSupporter.DeclarationMethodOfPayment => EntryLine?.Declaration?.JE_PaymentMethod ?? ZString.Empty;

		protected virtual ZString AmountInDeclarationCurrencyCore() => FormatDecimalDefault(CF_ChargeAmount);
		protected virtual ZString TaxBaseCore() => FormatDecimalDefault(CF_BaseValue);

		protected virtual ZString FormatDecimalDefault(ZDecimal number) => number.ToString(2);

		#endregion

		public bool IsNationalIndirectTaxationFee => GetIsNationalIndirectTaxationFee();

		protected virtual bool GetIsNationalIndirectTaxationFee() => Regex.IsMatch(CF_ChargeType, @"^\d");

		public bool IsSystemAddedVatFee => CF_ChargeType == UniversalReferenceConstants.RefCusRateCodes.Vat && CF_RateOverrideReasonCode.IsEmpty;

		public IUserEnteredStashSource UserEnteredStashSource => userEnteredStashSource ?? (userEnteredStashSource = GetUserEnteredStashSourceCore());
		IUserEnteredStashSource userEnteredStashSource;

		protected virtual IUserEnteredStashSource GetUserEnteredStashSourceCore() => new CusEntryLineFeeUserEnteredStashSource(this);

		public ChargeAmountRefresher ChargeAmountRefresher => chargeAmountRefresher ?? (chargeAmountRefresher = GetNewChargeAmountRefresher());
		ChargeAmountRefresher chargeAmountRefresher;

		protected virtual ChargeAmountRefresher GetNewChargeAmountRefresher() => new ChargeAmountRefresher(this);
	}
}
