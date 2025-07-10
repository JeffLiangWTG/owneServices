using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class InvoiceCharge : EU.Business.Declaration.InvoiceCharge, Integration.Customs.DE.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : JobComInvCharge.Schema
		{
			public const string IsJ7_ExchangeRateIATA = nameof(InvoiceCharge.IsJ7_ExchangeRateIATA);
		}

		[ResourceStringData("B4AFD034-F14F-4E46-9859-DE489824AA38", Caption = "IATA")]
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
							Validation.ValidateIsJ7_ExchangeRateUserEnterable();
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
					Validation.ValidateIsJ7_ExchangeRateIATA();
				}
			}
		}

		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				var valueHasChanged = value != J7_ChargeType;
				base.J7_ChargeType = value;
				if (!IsCopying && valueHasChanged)
				{
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
							DefaultCurrencyIfNeeded();
						}
					}
				}
			}
		}

		protected override ZArchitecture.Core.ExchangeRateType GetCurrencyConverterDataProviderRateTypeCore() =>
			J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.IATARate ? ZArchitecture.Core.ExchangeRateType.IATA : base.GetCurrencyConverterDataProviderRateTypeCore();

		public override void OnSaving()
		{
			if (IsImport)
			{
				this.SetValuesForApportionChargesIfNeeded();
			}

			base.OnSaving();
		}

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;

		public new InvoiceChargeValidation Validation => (InvoiceChargeValidation)base.Validation;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceChargeLookups(this);

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);

		protected override bool GetJ7_IsDutiable_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsDutiable_ReadOnlyRate(J7_ChargeType) : base.GetJ7_IsDutiable_ReadOnly();

		protected override bool GetJ7_IsStatisticalValueApplicable_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsStatisticalValueApplicable_ReadOnlyRate(J7_ChargeType) : base.GetJ7_IsStatisticalValueApplicable_ReadOnly();

		protected override bool GetJ7_IsGSTApplicable_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsGSTApplicable_ReadOnlyRate(J7_ChargeType) : base.GetJ7_IsGSTApplicable_ReadOnly();

		protected override bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_Calc_IsIncludedInInvoiceAmount_ReadOnlyRate(J7_ChargeType) : base.GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly();

		protected override bool GetIncludedInITOTReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_IsIncludedInITOT_ReadOnlyRate(J7_ChargeType) : base.GetIncludedInITOTReadOnly();

		protected override bool GetJ7_Percentage_ReadOnly() => IsImport ? IsHighValueOvrd && ChargeHelper.IsJ7_Percentage_ReadOnlyRate(J7_ChargeType) : base.GetJ7_Percentage_ReadOnly();

		protected override bool GetJ7_RX_NKCurrency_ReadOnly() => (IsImport && IsTCEType) || base.GetJ7_RX_NKCurrency_ReadOnly();

		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;

		bool IsJ7_ExchangeRateIATA_ReadOnly => IsImport && IsHighValueOvrd && ChargeHelper.IsJ7_ExchangeRateIATA_ReadOnlyRate(J7_ChargeType);

		ZBool IsImport => Invoice?.IsImport ?? ZBool.False;

		ZBool IsHighValueOvrd => Invoice.IsHighValueOvrd;

		ZBool IsTCEType => J7_ChargeType == ImportChargeCodeList.Codes.TCE;

		void DefaultCurrencyIfNeeded()
		{
			if (IsTCEType)
			{
				J7_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			}
		}
	}
}
