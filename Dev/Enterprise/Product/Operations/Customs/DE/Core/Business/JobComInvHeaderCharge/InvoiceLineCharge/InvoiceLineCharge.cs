using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class InvoiceLineCharge : EU.Business.Declaration.InvoiceLineCharge, Integration.Customs.DE.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : JobComInvCharge.Schema
		{
			public const string IsJ7_ExchangeRateIATA = nameof(InvoiceLineCharge.IsJ7_ExchangeRateIATA);
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		[ResourceStringData("36E05302-98F1-4308-8CFB-B8D373529CD5", Caption = "IATA")]
		[ReadOnlyMember(nameof(IsJ7_ExchangeRateIATA_ReadOnly))]
		public ZBool IsJ7_ExchangeRateIATA
		{
			get => J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.IATARate;
			set
			{
				var oldValue = IsJ7_ExchangeRateIATA;
				if (oldValue != value)
				{
					if (value)
					{
						if (!IsCopying)
						{
							IsJ7_ExchangeRateUserEnterable = false;
						}
						J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.IATARate;
					}
					else if (oldValue)
					{
						J7_ExchangeRateType = ZString.Empty;
					}

					SetExchangeRateIfNotUserOverridden();

					if (IsZeroPercentageAndNotCopying)
					{
						MarkApportionmentDirty();
					}

					if (!IsCopying)
					{
						if (!IsValidationSuspended)
						{
							(Validation as ImportInvoiceLineChargeValidation)?.ValidateIsJ7_ExchangeRateUserEnterable();
						}
						IsJ7_ExchangeRateIATAInfo.RefreshBinding(oldValue);
					}
				}
			}
		}

		public ZPropertyInfo IsJ7_ExchangeRateIATAInfo => GetZPropertyInfo(Schema.IsJ7_ExchangeRateIATA);

		protected override ZBool IsJ7_ExchangeRateUserEnterableCore
		{
			get => base.IsJ7_ExchangeRateUserEnterableCore;
			set
			{
				base.IsJ7_ExchangeRateUserEnterableCore = value;
				if (!IsCopying && !IsValidationSuspended)
				{
					(Validation as ImportInvoiceLineChargeValidation)?.ValidateIsJ7_ExchangeRateIATA();
				}
			}
		}

		[MaxLength(30)]
		[ReadOnlyMember(nameof(J7_ChargeDescription_ReadOnly))]
		public override ZString J7_ChargeDescription
		{
			get => base.J7_ChargeDescription;
			set => base.J7_ChargeDescription = value;
		}

		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				var oldValue = base.J7_ChargeType;
				base.J7_ChargeType = value;
				if (!IsCopying && oldValue != J7_ChargeType)
				{
					DefaultCurrency();

					if (!J7_ChargeType.SupportsIATA())
					{
						IsJ7_ExchangeRateIATA = false;
					}

					if (J7_ChargeType.IsEmpty)
					{
						J7_Amount = ZDecimal.Zero;
						J7_RX_NKCurrency = ZString.Empty;
					}
					else
					{
						if (IsImport)
						{
							this.SetValuesIfNeeded();
						}
					}
				}
			}
		}

		public override ZDecimal J7_Amount
		{
			get => base.J7_Amount;
			set
			{
				if (!inCalculateNetPrice)
				{
					var oldValue = base.J7_Amount;
					base.J7_Amount = value;
					if (!IsCopying && oldValue != J7_Amount)
					{
						CalculateLineNetPrice();
					}
				}
			}
		}

		public override void Delete()
		{
			CalculateLineNetPrice(this);

			base.Delete();
		}

		public override void OnSaving()
		{
			if (IsImport)
			{
				this.SetValuesForApportionChargesIfNeeded();
			}

			base.OnSaving();
		}

		public new bool J7_IsDutiable_ReadOnly => base.J7_IsDutiable_ReadOnly && !IsImportSpecificRate;

		public new bool J7_IsStatisticalValueApplicable_ReadOnly => base.J7_IsDutiable_ReadOnly && !IsImportSpecificRate;

		public new bool J7_IsGSTApplicable_ReadOnly => base.J7_IsDutiable_ReadOnly && !IsImportSpecificRate;

		public bool IsImportSpecialRate => Factory.GetValue(ref isImportSpecialRateCached, () => IsImport && ImportChargeCodeList.IsSpecialRate(J7_ChargeType));
		CachedProperty<bool> isImportSpecialRateCached;

		protected override ZArchitecture.Core.ExchangeRateType GetCurrencyConverterDataProviderRateTypeCore() =>
			J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.IATARate ? ZArchitecture.Core.ExchangeRateType.IATA : base.GetCurrencyConverterDataProviderRateTypeCore();

		public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;

		public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);

		protected override JobComInvHeaderChargeValidation GetNewValidation()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine?.IsImport ?? false)
			{
				return new ImportInvoiceLineChargeValidation(this);
			}
			else if (invoiceLine?.IsExport ?? false)
			{
				return new ExportInvoiceLineChargeValidation(this);
			}
			else
			{
				return new InvoiceLineChargeValidation(this);
			}
		}

		protected override bool GetJ7_RX_NKCurrency_ReadOnly() => IsImportSpecialRate || IsImportDiscountRate;

		protected override bool GetJ7_IsDutiable_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsDutiable_ReadOnlyRate(J7_ChargeType) : base.GetJ7_IsDutiable_ReadOnly();

		protected override bool GetJ7_IsStatisticalValueApplicable_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsStatisticalValueApplicable_ReadOnlyRate(J7_ChargeType) : base.GetJ7_IsDutiable_ReadOnly();

		protected override bool GetJ7_IsGSTApplicable_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsGSTApplicable_ReadOnlyRate(J7_ChargeType) : base.GetJ7_IsDutiable_ReadOnly();

		protected override bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_Calc_IsIncludedInInvoiceAmount_ReadOnlyRate(J7_ChargeType) : base.GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly();

		protected override bool GetIncludedInITOTReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsIncludedInITOT_ReadOnlyRate(J7_ChargeType) : base.GetIncludedInITOTReadOnly();

		protected override bool GetJ7_Percentage_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_Percentage_ReadOnlyRate(J7_ChargeType) : base.GetJ7_Percentage_ReadOnly();

		protected override ZString DefaultChargeDescription => J7_ChargeDescription_ReadOnly ? base.DefaultChargeDescription : ZString.Empty;

		bool J7_ChargeDescription_ReadOnly => J7_ChargeType != ImportChargeCodeList.Codes._016;

		bool IsJ7_ExchangeRateIATA_ReadOnly => IsImport && IsHighValueOvrd && ChargeHelper.IsJ7_ExchangeRateIATA_ReadOnlyRate(J7_ChargeType);

		ZBool IsImport => InvoiceLine?.IsImport ?? ZBool.False;

		ZBool IsHighValueOvrd => InvoiceLine.InvoiceHeader.IsHighValueOvrd;

		ZBool IsImportDiscountRate => IsImport && J7_ChargeType == CustomsChargeTypeList.Codes.Discount;

		bool IsImportSpecificRate => Factory.GetValue(ref isImportSpecificRateCached, () => (InvoiceLine?.IsImport ?? false) && ImportChargeCodeList.IsSpecificRate(J7_ChargeType));
		CachedProperty<bool> isImportSpecificRateCached;

		void DefaultCurrency()
		{
			if (IsImportSpecialRate)
			{
				J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			}
			else if (IsImportDiscountRate)
			{
				J7_RX_NKCurrency = InvoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency;
			}
		}

		void CalculateLineNetPrice(InvoiceLineCharge chargeToBeDeleted = null)
		{
			if (IsImportDiscountRate)
			{
				inCalculateNetPrice = true;
				InvoiceLine.CalculateNetPriceIfNeeded(true, chargeToBeDeleted);
				inCalculateNetPrice = false;
			}
		}
		ZBool inCalculateNetPrice = false;
	}
}
