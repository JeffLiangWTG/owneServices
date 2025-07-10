using System;
using CargoWise.Common;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	partial class JobComInvCharge : IDefaultLandedCostInput
	{
		#region Apportionment

		public override ZDecimal J7_ExchangeRate
		{
			get { return base.J7_ExchangeRate; }
			set
			{
				bool isDiff = base.J7_ExchangeRate != value;
				base.J7_ExchangeRate = value;
				if (isDiff && IsZeroPercentageAndNotCopyingAndUserEnterableRate)
				{
					MarkApportionmentDirty();
				}
			}
		}

		protected bool IsZeroPercentageAndNotCopyingAndUserEnterableRate
		{
			get { return IsZeroPercentageAndNotCopying && IsJ7_ExchangeRateUserEnterable; }
		}

		protected bool IsZeroPercentageAndNotCopying
		{
			get { return !IsCopying && J7_Percentage == 0; }
		}

		[ResourceStringData("Enterprise.Customs.Common.JobComInvHeaderCharge|J7_Percentage", Caption = "% of Line Price", ShortCaption = "% of Price", FullDescription = "Is only used for some charges such as commission or discount. If you enter a value in this field it will be defaulted to every invoice line under group invoices or invoices and the amount is then calculated based on the line price.")]
		public override ZDecimal J7_Percentage
		{
			get { return base.J7_Percentage; }
			set
			{
				bool isDiff = base.J7_Percentage != value;
				base.J7_Percentage = value;
				if (isDiff && !IsCopying && J7_Percentage > 0 && !J7_IsApportionedCharge)
				{
					IsJ7_ExchangeRateUserEnterable = false;
				}
			}
		}

		//Should not add J7_IsIncludedInITOT as they are calculated while apportionment is done
		event EventHandler ApportionmentDirtyChangedEventHandler
		{
			add
			{
				J7_AmountInfo.ValueChanged += value;
				J7_RX_NKCurrencyInfo.ValueChanged += value;
				J7_ChargeTypeInfo.ValueChanged += value;
				J7_IsDutiableInfo.ValueChanged += value;
				J7_IsGSTApplicableInfo.ValueChanged += value;
				J7_PercentageInfo.ValueChanged += value;
				J7_IsNotIncludedInInvoiceInfo.ValueChanged += value;
				J7_DistributeByInfo.ValueChanged += value;
				J7_FullOrPartialApportionmentInfo.ValueChanged += value;
				J7_ChargeDescriptionInfo.ValueChanged += value;
				J7_IsStatisticalValueApplicableInfo.ValueChanged += value;
			}
			remove
			{
				throw new NotSupportedException();
			}
		}

		protected void OnApportionmentBeingDirty(object sender, EventArgs e)
		{
			MarkApportionmentDirty();
		}

		protected void MarkApportionmentDirty()
		{
			if (!IsCopying && !IsDeleted && Parent != null && Parent.InvoicesHolder != null && !J7_IsApportionedCharge && !IsMarkApportionmentDirtySuspended)
			{
				Parent.InvoicesHolder.MarkApportionmentDirty();
			}
		}

		protected bool IsMarkApportionmentDirtySuspended => isMarkApportionmentDirtySuspended != 0;

		public IDisposable SuspendMarkApportionmentDirty()
		{
			return new DisposableAction(() => isMarkApportionmentDirtySuspended++, () => isMarkApportionmentDirtySuspended--);
		}

		byte isMarkApportionmentDirtySuspended;

		void ValidateIncoTerms()
		{
			if (!IsCopying && !IsDeleted && Parent != null && Parent.InvoicesHolder != null && !J7_IsApportionedCharge)
			{
				Parent.InvoicesHolder.ValidateIncoTerms();
			}
		}

		protected bool IsIncludedInInvoiceAmountFixed
		{
			get
			{
				var incoTermAndChargeFactory = Parent?.InvoicesHolder?.IncoTermAndChargeFactory;
				var charge = incoTermAndChargeFactory?.GetCharge(J7_ChargeType);
				return charge != null && incoTermAndChargeFactory.IsIncludedInInvoiceAmountFixed(Parent.IncoTerm, charge);
			}
		}

		protected ZString IncoTerm => Parent != null ? Parent.IncoTerm : ZString.Empty;

		public void ResetDefaultIsIncludedInAmountAndIsIncludedInITOTIfDetermined()
		{
			if (!J7_IsApportionedCharge)
			{
				var incoTermAndChargeFactory = IncoTermAndChargeFactory;
				var charge = incoTermAndChargeFactory?.GetCharge(J7_ChargeType);
				if (charge != null && (charge.IsIncludedInITOTDeemedForThisCharge || ShouldResetDefaultIsIncludedInITOT(IncoTerm)))//for ADD or DED
				{
					J7_IsIncludedInITOT = GetDefaultIsIncludedInITOT(charge);
				}
				DefaultIsIncludedInInvoice(incoTermAndChargeFactory, charge);
			}
		}

		protected virtual ZBool GetDefaultIsIncludedInITOT(ICustomsChargeCode customsChargeCode)
		{
			return customsChargeCode.IsIncludedInITOTIfDeemed.HasValue && customsChargeCode.IsIncludedInITOTIfDeemed.Value;
		}

		protected virtual void DefaultIsIncludedInInvoice(IncoTermAndCustomsChargeFactory incoTermAndChargeFactory, ICustomsChargeCode charge)
		{
			if (IsIncludedInInvoiceAmountFixed || ShouldResetDefaultIsIncludedInAmount(IncoTerm, charge))
			{
				J7_Calc_IsIncludedInInvoiceAmount = charge != null && incoTermAndChargeFactory.GetDefaultIsIncludedInInvoice(Parent.IncoTerm, charge);
			}
		}

		protected virtual bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => J7_ChargeType == CustomsChargeTypeList.Codes.AdditionCharge || J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge;

		protected virtual bool ShouldResetDefaultIsIncludedInITOT(ZString incoTerm) => J7_ChargeType == CustomsChargeTypeList.Codes.AdditionCharge || J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge;

		#endregion

		#region Overrides

		public override ZBool J7_AdjustedCharge
		{
			get { return base.J7_AdjustedCharge; }
			set
			{
				base.J7_AdjustedCharge = value;

				MarkApportionmentDirty();
			}
		}

		protected bool J7_Amount_ReadOnly => GetJ7_Amount_ReadOnly();

		protected virtual bool GetJ7_Amount_ReadOnly() => J7_Percentage > 0;

		protected bool J7_RX_NKCurrency_ReadOnly => GetJ7_RX_NKCurrency_ReadOnly();

		protected virtual bool GetJ7_RX_NKCurrency_ReadOnly() => J7_Percentage > 0;

		protected bool J7_IsDutiable_ReadOnly => GetJ7_IsDutiable_ReadOnly();

		protected virtual bool GetJ7_IsDutiable_ReadOnly()
		{
			if (ShouldMakeReadOnlyWhenDeemed())
			{
				return ChargeCode?.IsDutiableDeemedForThisCharge ?? false;
			}
			return J7_ChargeType == CustomsChargeTypeList.Codes.AdditionCharge || J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge;
		}

		protected bool J7_IsGSTApplicable_ReadOnly => GetJ7_IsGSTApplicable_ReadOnly();

		protected virtual bool GetJ7_IsGSTApplicable_ReadOnly()
		{
			if (ShouldMakeReadOnlyWhenDeemed())
			{
				return ChargeCode?.IsVATibleDeemedForThisCharge ?? false;
			}
			return J7_IsDutiable || J7_ChargeType == CustomsChargeTypeList.Codes.AdditionCharge || J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge;
		}

		protected bool J7_IsStatisticalValueApplicable_ReadOnly => GetJ7_IsStatisticalValueApplicable_ReadOnly();

		protected virtual bool GetJ7_IsStatisticalValueApplicable_ReadOnly()
		{
			if (ShouldMakeReadOnlyWhenDeemed())
			{
				return ChargeCode?.IsStatisticalValueApplicableDeemed ?? false;
			}
			return false;
		}

		protected bool J7_Percentage_ReadOnly => GetJ7_Percentage_ReadOnly();

		protected virtual bool GetJ7_Percentage_ReadOnly() => false;

		bool ShouldMakeReadOnlyWhenDeemed()
		{
			var incoTermAndChargeFactory = Parent?.InvoicesHolder?.IncoTermAndChargeFactory;
			return incoTermAndChargeFactory?.MakeFlagsReadOnlyWhenDeemed ?? false;
		}

		#endregion

		#region ILandedCostInput Members

		ZString IDefaultLandedCostInput.ChargeDescription
		{
			get
			{
				return IsDeduction ? Res.GetString("19567b73-b1e9-4c48-a7ff-2ac2cc88a24e", "Deduction (or Discount) from Entry") : Res.GetString("84f7d898-151f-46fd-a604-e342bc7019d5", "{0} from Entry", ChargeCodeDescription.ToUpper());
			}
		}

		Money IDefaultLandedCostInput.AmountToDistribute
		{
			get
			{
				Money result = Money.Empty;
				if (Currency != null)
				{
					ZDecimal amount = IsDeduction ? new ZDecimal(-1 * J7_Amount) : J7_Amount;
					result = new Money(amount, Currency);
				}
				return result;
			}
		}

		bool IsDeduction
		{
			get
			{
				return J7_ChargeType == CustomsChargeTypeList.Codes.DeductionCharge
					|| IsDiscount;
			}
		}

		ZGuid IDefaultLandedCostInput.FKToChargeCode
		{
			get { return ZGuid.Empty; }
		}

		ZDecimal IDefaultLandedCostInput.ExchangeRate
		{
			get
			{
				ZDecimal result = 0m;

				if (IsJ7_ExchangeRateUserEnterable)
				{
					result = J7_ExchangeRate;
				}
				else if (CurrencyConverter != null && Currency != null)
				{
					result = CurrencyConverter.GetExchangeRate(Currency);
				}
				return result;
			}
		}

		ZBool IDefaultLandedCostInput.IsValidToImport
		{
			get
			{
				return IsValidToImportForLC;
			}
		}

		protected virtual bool IsValidToImportForLC
		{
			get
			{
				return !J7_IsApportionedCharge &&
					!ChargeCodeDescription.IsEmpty &&
					!J7_AdjustedCharge &&
					J7_Amount > 0 &&
					Currency != null &&
					!Enterprise.DocumentEngineCore.Registry.DocumentsDataRegistry.Instance.PullLandedCostingDataFromBillingTabOnly.Value &&
					(IsDeduction || !J7_IsIncludedInITOT);
			}
		}

		#endregion
	}
}
