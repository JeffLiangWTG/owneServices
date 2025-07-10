using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.ARAP.CashAdvance;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.CriticalValidation;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Integration.CalculateTaxForCharge;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.RtfConverter;
using static System.FormattableString;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	[DebuggerDisplay("{DebuggerDisplay}")]
	public abstract partial class BaseCharge : JobCharge, IHaveConstructorStackTrace
	{
		string DebuggerDisplay => string.Join("-", ChargeCode?.AC_Code ?? "null", Job?.JH_JobNum ?? "null", CostAccount?.OH_Code ?? "null", SellAccount?.OH_Code ?? "null");

		#region Schema

		public new abstract class Schema : JobCharge.Schema
		{
			public const string TotalTaxOnInvForJob = "TotalTaxOnInvForJob";
			public const string TotalAmountOnInvForJob = "TotalAmountOnInvForJob";
			public const string CurrencyCodeOnInvForJob = "CurrencyCodeOnInvForJob";
			public const string TotalTaxOnInv = "TotalTaxOnInv";
			public const string TotalAmountOnInv = "TotalAmountOnInv";
			public const string JR_OSCostAmtWithGSTAmt = "JR_OSCostAmtWithGSTAmt";
			public const string JR_OSCostCurrencyCode = "JR_OSCostCurrencyCode";
			public const string JR_OSSellCurrencyCode = "JR_OSSellCurrencyCode";
			public const string JR_CostCurrency = "JR_CostCurrency";
			public const string JR_IsCostPosted = "JR_IsCostPosted";
			public const string JR_IsApproved = "JR_IsApproved";
			public const string JR_IsRevenuePosted = "JR_IsRevenuePosted";
			public const string JR_IsPosted = "JR_IsPosted";
			public const string JR_LocalCurrencyCode = "JR_LocalCurrencyCode";
			public const string ChargeType = "ChargeType";
			public const string MarginPercentage = "MarginPercentage";
			public const string JR_Calc_OSCostGSTAmt = "JR_Calc_OSCostGSTAmt";
			public const string JR_Calc_OSSellGSTAmt = "JR_Calc_OSSellGSTAmt";
			public const string JR_Calc_OSCostExtraTaxAmt = "JR_Calc_OSCostExtraTaxAmt";
			public const string JR_Calc_OSSellExtraTaxAmt = "JR_Calc_OSSellExtraTaxAmt";
			public const string JR_Calc_OSSellAmtWithGSTName = "JR_Calc_OSSellAmtWithGST";
			public const string JR_Calc_OSCostAmtWithGSTName = "JR_Calc_OSCostAmtWithGST";
			public const string JR_Calc_LocalCostAmtWithGSTName = "JR_Calc_LocalCostAmtWithGST";
			public const string JR_Cost_LocalGSTAmountName = "JR_Cost_LocalGSTAmount";
			public const string JR_Cost_LocalWHTAmountName = "JR_Cost_LocalWHTAmount";
			public const string JR_Sell_LocalGSTAmountName = "JR_Sell_LocalGSTAmount";
			public const string JR_Sell_LocalWHTAmountName = "JR_Sell_LocalWHTAmount";
			public const string JR_SellCurrencyName = "JR_SellCurrency";
			public const string JR_JobNumberName = "JR_JobNumber";
			public const string JR_ARInvoiceNumberName = "JR_ARInvoiceNumber";
			public const string JR_JobInvoiceNumberName = "JR_JobInvoiceNumber";
			public const string JR_ChequeOrReferenceLabelName = "JR_ChequeOrReferenceLabel";
			public const string JR_LocalCurrencyDecimals = "JR_LocalCurrencyDecimals";
			public const string JR_CFXAmt = "JR_CFXAmt";
			public const string CostRecognition = "CostRecognition";
			public const string SellRecognition = "SellRecognition";
			public const string JR_ActualWeight = "JR_ActualWeight";
			public const string JR_ActualWeightUnit = "JR_ActualWeightUnit";
			public const string JR_Chargeable = "JR_Chargeable";
			public const string JR_ChargeableUnit = "JR_ChargeableUnit";
			public const string DisplaySellInvoiceAddress = "DisplaySellInvoiceAddress";
			public const string DisplaySellInvoiceContact = "DisplaySellInvoiceContact";
			public const string JR_Calc_ARInvoiceDate = "JR_Calc_ARInvoiceDate";
			public const string JR_LocalSellInvoiceAmt = "JR_LocalSellInvoiceAmt";
			public const string JR_ActualVolume = "JR_ActualVolume";
			public const string JR_ActualVolumeUnit = "JR_ActualVolumeUnit";
			public const string JR_RL_NKOrigin = "JR_RL_NKOrigin";
			public const string JR_RL_NKDestination = "JR_RL_NKDestination";
			public const string ARCashAdvanceRequestStatus = "ARCashAdvanceRequestStatus";
			public const string ARCashAdvanceRequestStatusDescription = "ARCashAdvanceRequestStatusDescription";
			public const string ARCashAdvanceRequestID = "ARCashAdvanceRequestID";
			public const string ChargeGroup = "ChargeGroup";
			public const string ChargeCodeSubGroup = "ChargeCodeSubGroup";
		}

		#endregion

		#region Charge Contexts

		internal enum Contexts { PosterCopiesExchangeRateAndAmount }

		#endregion

		protected BaseCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			this.SetConstructorStackTrace();
		}

		public static new readonly TypeDecider TypeDecider = JobCharge.TypeDecider;

		#region Exchange Rates

		IExchangeRateProvider ExchangeRateProvider => InvoicingJob;
		ChargeToExRateLinker Linker => ChargeToExRateLinker.Get(Factory);

		public IExchangeRateJobBilling CostExchangeRate
		{
			get
			{
				InitializeCostExchangeRate();
				return costExchangeRate;
			}
			set => UpdateExchangeRateIncludingEventSubscription(ref costExchangeRate, value, OnCostExchangeRateChanged, ExchangeRateKind.CostRate);
		}
		IExchangeRateJobBilling costExchangeRate;
		bool isCostExchangeRateInitialised => IsExchangeRateInitialised(ExchangeRateKind.CostRate);

		bool IsExchangeRateInitialised(ExchangeRateKind rateKind) => Linker?.IsInitialised(this, rateKind) ?? true;

		void OnCostExchangeRateChanged(object sender, EventArgs e)
		{
			if (ShouldApplyExchangeRateUpdate && !JR_IsApportioned && !IsCostPosted)
			{
				OnCostExchangeRateChangedCore(sender, e);
			}
		}

		protected virtual void OnCostExchangeRateChangedCore(object sender, EventArgs e)
		{
		}

		protected void InitializeCostExchangeRate()
		{
			if (costExchangeRate == null && !isCostExchangeRateInitialised)
			{
				UpdateCostExchangeRate();
			}
		}

		public IExchangeRateJobBilling RevenueExchangeRate
		{
			get
			{
				InitializeRevenueExchangeRate();
				return revenueExchangeRate;
			}
			set => UpdateExchangeRateIncludingEventSubscription(ref revenueExchangeRate, value, OnRevenueExchangeRateChanged, ExchangeRateKind.SellRate);
		}
		IExchangeRateJobBilling revenueExchangeRate;
		bool isRevenueExchangeRateInitialised => IsExchangeRateInitialised(ExchangeRateKind.SellRate);

		protected bool isInitialisingRevenueEnchangeRate => Linker?.IsInitialising(this, ExchangeRateKind.SellRate) ?? false;

		protected void OnRevenueExchangeRateChanged(object sender, EventArgs e)
		{
			if (ShouldApplyExchangeRateUpdate && !IsRevenuePosted)
			{
				OnRevenueExchangeRateChangedCore(sender, e);
			}
		}

		protected virtual void OnRevenueExchangeRateChangedCore(object sender, EventArgs e)
		{
			JR_OSSellExRateInfo.RefreshBinding();
			JR_OSSellGSTAmt_CalcInfo.RefreshBinding();
			JR_LocalSellAmtInfo.RefreshBinding();
		}

		protected void InitializeRevenueExchangeRate()
		{
			if (revenueExchangeRate == null && !isRevenueExchangeRateInitialised)
			{
				using (Linker?.BeginInitialising(this, ExchangeRateKind.SellRate))
				{
					UpdateRevenueExchangeRate();
				}
			}
		}

		public IExchangeRateJobBilling SellInvoiceExchangeRate
		{
			get
			{
				InitializeSellInvoiceExchangeRate();
				return sellInvoiceExchangeRate;
			}
			set
			{
				UpdateExchangeRateIncludingEventSubscription(ref sellInvoiceExchangeRate, value, OnSellInvoiceExchangeRateChanged, ExchangeRateKind.SellInvoiceRate);
			}
		}
		IExchangeRateJobBilling sellInvoiceExchangeRate;
		bool isSellInvoiceExchangeRateInitialised => IsExchangeRateInitialised(ExchangeRateKind.SellInvoiceRate);

		protected virtual void OnSellInvoiceExchangeRateChanged(object sender, EventArgs e)
		{
			if (ShouldApplyExchangeRateUpdate && !IsRevenuePosted)
			{
				OnSellInvoiceExchangeRateChangedCore(sender, e);
			}
		}

		protected virtual void OnSellInvoiceExchangeRateChangedCore(object sender, EventArgs e)
		{
		}

		protected void InitializeSellInvoiceExchangeRate()
		{
			if (sellInvoiceExchangeRate == null)
			{
				if (!isSellInvoiceExchangeRateInitialised)
				{
					UpdateSellInvoiceExchangeRate();
				}
				if (AccountingConfigurationRegistry.Instance.AllowChargeToRetrieveExchangeRateFromJobDuringPeriodicInvoicing.Value
					&& sellInvoiceExchangeRate == null
					&& Factory.HasContext(BusinessContext.PeriodicInvoicePosting))
				{
					SellInvoiceExchangeRate = GetSellInvoiceExchangeRate(false);
				}
			}
		}

		bool ShouldApplyExchangeRateUpdate => !Factory.HasContext(BusinessContext.IsUpdatedDueToChangesInDB) || !IsInDatabase;

		void UpdateExchangeRateIncludingEventSubscription(ref IExchangeRateJobBilling currentValue, IExchangeRateJobBilling newValue, EventHandler eventHandler, ExchangeRateKind rateKind)
		{
			if (currentValue?.Equals(newValue) ?? newValue == null)
			{
				if (Factory.HasContext(BusinessContext.ChargeReloader) && newValue != null)
				{
					eventHandler(null, new EventArgs());
				}
				return;
			}

			if (currentValue != null)
			{
				currentValue.Changed -= eventHandler;
			}

			currentValue = newValue;

			if (currentValue != null)
			{
				currentValue.Changed += eventHandler;
				Linker?.RemoveLinks(this, rateKind);
			}

			if (!IsDeleted)
			{
				eventHandler(null, new EventArgs());
			}
		}

		public void UpdateCostExchangeRate()
		{
			if (!IsDeleted && !JR_IsApportioned && !IsCostPosted)
			{
				CostExchangeRate = GetCostExchangeRate(true);
			}
		}

		IExchangeRateJobBilling GetCostExchangeRate(bool forceExchangeRateCreation) => ExchangeRateProvider?.GetExchangeRate(JR_RX_NKCostCurrency, JR_OH_CostAccount, ExchangeRateValidLedgerEnum.AP, forceExchangeRateCreation: forceExchangeRateCreation);

		public InvoiceCurrencyType InvoiceCurrencyTypeForAR => AccExchangeRateConfigurationRateFinder.GetInvoiceCurrencyType(Company, ExchangeRateValidLedgerEnum.AR, this);

		public void UpdateRevenueExchangeRate()
		{
			if (!IsDeleted
				&& !this.HasContext(BusinessContext.AddingDefaultApportionmentCharge)
				&& !IsRevenuePosted)
			{
				RevenueExchangeRate = GetRevenueExchangeRate(true);
			}
		}

		IExchangeRateJobBilling GetRevenueExchangeRate(bool forceExchangeRateCreation) => ExchangeRateProvider?.GetExchangeRate(JR_RX_NKSellCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, forceExchangeRateCreation, InvoiceCurrencyTypeForAR);

		public void UpdateSellInvoiceExchangeRate()
		{
			if (!IsDeleted && !IsRevenuePosted)
			{
				SellInvoiceExchangeRate = GetSellInvoiceExchangeRate(true);
			}
		}

		IExchangeRateJobBilling GetSellInvoiceExchangeRate(bool forceExchangeRateCreation) => ExchangeRateProvider?.GetExchangeRate(JR_RX_NKSellInvoiceCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, forceExchangeRateCreation, InvoiceCurrencyTypeForAR);

		protected void UpdateSellInvoiceCurrency()
		{
			if (!InvoiceSellCurrencyDefaultingSuspender.IsSuspended)
			{
				JR_RX_NKSellInvoiceCurrency = new ZString(AllowsSellInvoiceCurrency ? InvoiceTypeCalculator.GetDefaultSellInvoiceCurrency() : string.Empty);
			}
		}

		public FunctionalitySuspender InvoiceSellCurrencyDefaultingSuspender
		{
			get { return invoiceSellCurrencyDefaultingSuspender ?? (invoiceSellCurrencyDefaultingSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender invoiceSellCurrencyDefaultingSuspender;

		public void ClearExchangeRates()
		{
			CostExchangeRate = null;
			Linker?.RemoveLinks(this, ExchangeRateKind.CostRate);
			RevenueExchangeRate = null;
			Linker?.RemoveLinks(this, ExchangeRateKind.SellRate);
			SellInvoiceExchangeRate = null;
			Linker?.RemoveLinks(this, ExchangeRateKind.SellInvoiceRate);
		}

		#endregion

		public virtual bool ShouldValidateBranchAndDepartment
		{
			get { return true; }
		}

		public bool IsCommentChargeCode
		{
			get { return (ChargeCode != null && ChargeCode.AC_ChargeType == Core.Constants.ChargeType.Comment); }
		}

		public bool IsAllowedToModifyThisCharge
		{
			get
			{
				return CheckLoginPermissionIfRequired(InvoicingJob == null || !InvoicingJob.IsAllowedToModifyChargesFromOtherBranchesOrDepartments);
			}
		}

		public bool IsAllowedToViewThisCharge
		{
			get
			{
				return CheckLoginPermissionIfRequired(InvoicingJob == null || !InvoicingJob.IsAllowedToViewChargesFromOtherBranchesOrDepartments);
			}
		}

		public bool IsAllowedToViewCosts
		{
			get
			{
				return CheckLoginPermissionIfRequired(InvoicingJob == null || !InvoicingJob.IsAllowedToViewConsolCostsFromOtherBranchesOrDepartments);
			}
		}

		public bool IsAllowedToPostSellCharge
		{
			get
			{
				return !(AccountingConfigurationRegistry.Instance.RestrictPostingOfSellChargesVisibleToLoginUserOnly.Value && Globals.IsUserInteractive && !IsAllowedToViewThisCharge);
			}
		}

		[ReadOnlyMember(nameof(JR_InvoiceType_ReadOnly))]
		public override ZString JR_InvoiceType
		{
			get => base.JR_InvoiceType;
			set
			{
				var oldValue = JR_InvoiceType;
				base.JR_InvoiceType = value;
				if (oldValue != value)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_InvoiceType_PropertyValueSet,
					() =>
					{
						return Invariant($@"OldValue: {oldValue}, NewValue: {value}
{System.Environment.StackTrace}");
					});
				}
			}
		}

		protected virtual bool JR_InvoiceType_ReadOnly => false;

		bool CheckLoginPermissionIfRequired(bool securityCheckPointCheckFailed)
		{
			bool result = true;
			ZGuid branchPK = IsInDatabase ? (ZGuid)JR_GBInfo.OriginalValue : JR_GB;
			ZGuid departmentPK = IsInDatabase ? (ZGuid)JR_GEInfo.OriginalValue : JR_GE;

			if (securityCheckPointCheckFailed && branchPK.IsValid && departmentPK.IsValid)
			{
				result = AllowedToLogin(branchPK, departmentPK);
			}
			return result;
		}

		bool AllowedToLogin(ZGuid branchPK, ZGuid departmentPK)
		{
			bool result = true;
			GlbBranch branch = Factory.Load<GlbBranch>(branchPK);
			GlbDepartment department = Factory.Load<GlbDepartment>(departmentPK);

			if (branch != null && branch.IsInDatabase && department != null && department.IsInDatabase
				&& (branch.PK != GlbBranch.CurrentBranch.PK || department.PK != GlbDepartment.CurrentDepartment.PK))
			{
				result = AllowedToLogin(branch, department);
			}
			if (result && IsInDatabase && Branch != null && Branch.IsInDatabase && Department != null && Department.IsInDatabase)
			{
				result = AllowedToLogin(Branch, Department);
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
			{
				var security = new SecurityCore(GlbStaff.CurrentUser.StaffSecurityPermissionsCollection, GlbStaff.CurrentUser, branch.PK.ToGuid(), department.PK.ToGuid(), branch.GB_GC.ToGuid(), false);
				return security.Login.IsAllowed;
			});
		}

		public bool IsAllowedToOverrideSellTaxMessage
		{
			get { return InvoicingJob?.InvoicingAllowOverrideSellTaxMessage ?? false; }
		}

		public bool IsAllowedToOverrideCostTaxMessage
		{
			get { return InvoicingJob?.InvoicingAllowOverrideCostTaxMessage ?? false; }
		}

		internal bool IsAllowedToOverrideSellGovtChargeCode
		{
			get { return InvoicingJob?.InvoicingAllowOverrideSellGovtChargeCode ?? false; }
		}

		internal bool IsAllowedToOverrideCostGovtChargeCode
		{
			get { return InvoicingJob?.InvoicingAllowOverrideCostGovtChargeCode ?? false; }
		}

		public bool JR_A9_SellVATClass_ReadOnly
		{
			get
			{
				return IsRevenuePosted
					|| JR_AT_SellGSTRate.IsEmpty
					|| !IsAllowedToOverrideSellTaxMessage
					|| !AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty)
					|| !IsSellGSTApplicable
					|| IsInDatabaseAndReadyForRevenuePosting
					|| IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		public override ZGuid JR_A9_CostVATClass
		{
			get { return base.JR_A9_CostVATClass; }
			set
			{
				if (JR_A9_CostVATClass != value)
				{
					var oldValue = JR_A9_CostVATClass;
					base.JR_A9_CostVATClass = value;
					AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo((NoResString)"Tax Class", ParentConsolCost?.E6_A9_VATClass, JR_A9_CostVATClass, oldValue);
				}
			}
		}

		public bool JR_A9_CostVATClass_ReadOnly
		{
			get
			{
				return IsCostPosted ||
					   JR_AT_CostGSTRate.IsEmpty ||
					   !IsAllowedToOverrideCostTaxMessage ||
					   !AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyTaxMessage.GetFallBackValueAtAllLevels(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty) ||
					   JR_IsApportioned ||
					   !IsCostGSTApplicable ||
					   IsInDatabaseAndReadyForCostPosting ||
					   IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		protected bool JR_GB_ReadOnly
		{
			get { return IsCostPosted || IsRevenuePosted || JR_IsApportioned || IsInDatabaseAndReadyForCostPosting || IsInDatabaseAndReadyForRevenuePosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		protected bool JR_GE_ReadOnly
		{
			get { return IsCostPosted || IsRevenuePosted || JR_IsApportioned || IsInDatabaseAndReadyForCostPosting || IsInDatabaseAndReadyForRevenuePosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		protected virtual bool JR_SellGovtChargeCode_ReadOnly
		{
			get { return !IsAllowedToOverrideSellGovtChargeCode || IsMainAllFieldsReadonly || JR_IsApportioned; }
		}

		protected virtual bool JR_CostGovtChargeCode_ReadOnly
		{
			get { return !IsAllowedToOverrideCostGovtChargeCode || IsAllCostFieldsReadonly; }
		}

		#region Tax Branch

		protected virtual bool JR_GB_CostTaxBranch_ReadOnly
		{
			get
			{
				var isSecurityAllowed = InvoicingJob?.InvoicingAllowOverrideCostTaxBranch ?? false;
				return IsCostTaxBranchReadOnly || !isSecurityAllowed;
			}
		}

		protected virtual bool JR_GB_SellTaxBranch_ReadOnly
		{
			get
			{
				var isSecurityAllowed = InvoicingJob?.InvoicingAllowOverrideSellTaxBranch ?? false;
				return IsSellTaxBranchReadOnly || !isSecurityAllowed;
			}
		}

		bool IsCostTaxBranchReadOnly
		{
			get
			{
				return JR_IsApportioned ||
						!AccountingMasterFilesUtils.IsTaxBranchApplicable ||
						!IsCostGSTApplicable ||
						IsCostPosted ||
						IsInDatabaseAndReadyForCostPosting ||
						IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		bool IsSellTaxBranchReadOnly
		{
			get
			{
				return !AccountingMasterFilesUtils.IsTaxBranchApplicable ||
						!IsSellGSTApplicableCore ||
						IsRevenuePosted ||
						IsInDatabaseAndReadyForRevenuePosting ||
						IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		public bool IsCostTaxBranchActual
		{
			get
			{
				var isTaxBranchApplicableForCostCharge = AccountingMasterFilesUtils.IsTaxBranchApplicable && IsCostGSTApplicable;

				return ChargeCode != null && ChargeCode.IsComment || (isTaxBranchApplicableForCostCharge && CostTaxBranch != null) || (!isTaxBranchApplicableForCostCharge && CostTaxBranch == null);
			}
		}

		public bool IsSellTaxBranchActual
		{
			get
			{
				var isTaxBranchApplicableForSellCharge = AccountingMasterFilesUtils.IsTaxBranchApplicable && IsSellGSTApplicable;

				return ChargeCode != null && ChargeCode.IsComment || (isTaxBranchApplicableForSellCharge && SellTaxBranch != null) || (!isTaxBranchApplicableForSellCharge && SellTaxBranch == null);
			}
		}

		public void SetSellTaxBranchDefault()
		{
			JR_GB_SellTaxBranch = Job != null && AccountingMasterFilesUtils.IsTaxBranchApplicable && IsSellGSTApplicable ? Job.JH_GB_TaxBranch : ZGuid.Empty;
		}

		public void SetCostTaxBranchDefault()
		{
			if (!JR_IsApportioned)
			{
				JR_GB_CostTaxBranch = Job != null && AccountingMasterFilesUtils.IsTaxBranchApplicable && IsCostGSTApplicable ? Job.JH_GB_TaxBranch : ZGuid.Empty;
			}
		}

		#endregion

		#region JR_LocalSellInvoiceAmt

		[DecimalPlaces(nameof(CompanyLocalCurrencyDecimals))]
		[ReadOnly(true)]
		public ZDecimal JR_LocalSellInvoiceAmt
		{
			get
			{
				var result = JR_LocalSellAmt;

				if (IsRevenuePostedWithSellInvoiceCurrencyAndNotCopyingOnPosting)
				{
					return ARLine.AL_LineAmount;
				}
				else if (!JR_LocalSellAmt.IsEmpty && BillInInvoiceCurrencyWithLocalSellCurrency)
				{
					var cfxMinimum = SellInvoiceExchangeRate?.CFXMinimum ?? ZDecimal.Zero;

					if (!JR_LineCFX.IsEmpty)
					{
						result = Utilities.Round(JR_LocalSellAmt * (100m + JR_LineCFX) / 100m, CompanyLocalCurrencyDecimals);
					}

					if (!cfxMinimum.IsEmpty)
					{
						var withCfxMinimum = JR_LocalSellAmt + cfxMinimum * Math.Sign(JR_LocalSellAmt);
						if (Math.Abs(withCfxMinimum) > Math.Abs(result))
						{
							result = withCfxMinimum;
						}
					}
				}
				return result;
			}
			protected set
			{
				if (JR_LocalSellInvoiceAmt == JR_LocalSellAmt)
				{
					JR_LocalSellAmt = value;
				}
				else
				{
					ErrorReporter.ReportOnce("JR_LocalSellInvoiceAmt setter", "Should not be setting JR_LocalSellInvoiceAmt property when it includes CFX uplift calculated for Sell Invoice Currency with local Sell Currency.\r\nWe could do reverse calculation to subtract CFX uplift but the logic for it is not clear.");
				}
			}
		}

		protected bool IsRevenuePostedWithSellInvoiceCurrency
		{
			get
			{
				return BillInInvoiceCurrency && IsRevenuePosted && ARLine.AL_RX_NKTransactionCurrency == JR_RX_NKSellInvoiceCurrency;
			}
		}

		protected bool IsRevenuePostedWithSellInvoiceCurrencyAndNotCopyingOnPosting
		{
			get
			{
				return IsRevenuePostedWithSellInvoiceCurrency && !this.HasContext(Contexts.PosterCopiesExchangeRateAndAmount);
			}
		}

		#endregion

		public ZBool IsLocalClientCharge
		{
			get { return Job != null && JR_OH_SellAccount.IsValid && JR_OH_SellAccount == Job.LocalChargesPK; }
		}

		public ZBool IsAgentCharge
		{
			get
			{
				return Job != null && JR_OH_SellAccount.IsValid &&
					(JR_OH_SellAccount == Job.AgentCollectPK || JR_OH_SellAccount == InvoicingJob.GetSendingAgentPK() || JR_OH_SellAccount == InvoicingJob.GetReceivingAgentPK());
			}
		}

		#region Fields for Automatic Job Revenue Journals

		internal FunctionalitySuspender SetDefaultValuesForAutoJobRevenueJournalsSuspender
		{
			get { return setDefaultValuesForAutoJobRevenueJournalsSuspender ?? (setDefaultValuesForAutoJobRevenueJournalsSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender setDefaultValuesForAutoJobRevenueJournalsSuspender;

		protected ChargeInternalFieldsManager InternalFieldsManager => internalFieldsManager ?? (internalFieldsManager = new ChargeInternalFieldsManager(this));
		ChargeInternalFieldsManager internalFieldsManager;

		[List("Branches")]
		public override ZGuid JR_GB_InternalBranch
		{
			get { return base.JR_GB_InternalBranch; }
			set
			{
				base.JR_GB_InternalBranch = value;

				if (InternalBranch != null && InternalBranch.GB_IsActive && !SetDefaultValuesForAutoJobRevenueJournalsSuspender.IsSuspended)
				{
					using (SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
					{
						InternalFieldsManager.SetCostAndSellAccountFromInternalBranch();
					}
				}
			}
		}

		protected virtual bool JR_GB_InternalBranch_ReadOnly
		{
			get { return IsInternalJobInfoDisabled; }
		}

		[List("Departments")]
		public override ZGuid JR_GE_InternalDept
		{
			get { return base.JR_GE_InternalDept; }
			set { base.JR_GE_InternalDept = value; }
		}

		protected virtual bool JR_GE_InternalDept_ReadOnly
		{
			get { return IsInternalJobInfoDisabled; }
		}

		[List("InternalJobs")]
		public override ZGuid JR_JH_InternalJob
		{
			get { return base.JR_JH_InternalJob; }
			set
			{
				base.JR_JH_InternalJob = value;

				if (!SetDefaultValuesForAutoJobRevenueJournalsSuspender.IsSuspended)
				{
					using (SetDefaultValuesForAutoJobRevenueJournalsSuspender.GetSuspender())
					using (GetValidationSuspender())
					{
						if (InternalJob != null && InternalJob.JH_GB.IsValid)
						{
							JR_GB_InternalBranch = InternalJob.JH_GB;
						}
						if (InternalJob != null && InternalJob.JH_GE.IsValid)
						{
							JR_GE_InternalDept = InternalJob.JH_GE;
						}
						InternalFieldsManager.SetCostAndSellAccountFromInternalBranch();
					}
				}
			}
		}

		[List("Lookups.SupplyTypes")]
		public override ZString JR_CostSupplyType
		{
			get { return base.JR_CostSupplyType; }
			set
			{
				if (JR_CostSupplyType != value)
				{
					var oldValue = base.JR_CostSupplyType;
					base.JR_CostSupplyType = value;
					UpdateCostGST(shouldCalculateWHT: false);
					AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo<ZString>((NoResString)"Supply Type", ReplaceEmptyStringWithQuotes(ParentConsolCost?.E6_SupplyType ?? ZString.Empty), ReplaceEmptyStringWithQuotes(JR_CostSupplyType), ReplaceEmptyStringWithQuotes(oldValue));
				}
			}
		}

		protected virtual bool JR_CostSupplyType_ReadOnly => IsAllCostFieldsReadonly;

		[List("Lookups.SupplyTypes")]
		public override ZString JR_SellSupplyType
		{
			get { return base.JR_SellSupplyType; }
			set
			{
				if (JR_SellSupplyType != value)
				{
					base.JR_SellSupplyType = value;
					ResetSellGSTTaxDefault();
				}
			}
		}

		protected virtual bool JR_SellSupplyType_ReadOnly => IsMainAllFieldsReadonly;

		protected virtual bool JR_JH_InternalJob_ReadOnly
		{
			get { return IsInternalJobInfoDisabled; }
		}

		public bool IsInternalJobInfoDisabled
		{
			get
			{
				return !CostOrSellAccountIsOrgProxy
					|| CostAndSellAccountAreBothOrgProxies
					|| (InvoicingJob != null && !InvoicingJob.IsConsumerTypeShouldCreateCostJRJ && !InvoicingJob.IsConsumerTypeShouldCreateSellJRJ)
					|| IsInternalJobInfoDisabled_CostAccount
					|| IsInternalJobInfoDisabled_SellAccount
					|| IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		bool IsInternalJobInfoDisabled_CostAccount => CostAccountIsOrgProxy && (IsCostPosted || this.IsExcludedFromAutoJRJ(CostAccount) || IsInDatabaseAndReadyForCostPosting);

		bool IsInternalJobInfoDisabled_SellAccount => SellAccountIsOrgProxy && (IsRevenuePosted || this.IsExcludedFromAutoJRJ(SellAccount) || IsInDatabaseAndReadyForRevenuePosting);

		public bool IsOrgProxyAccountPosted
		{
			get
			{
				return (CostAccountIsOrgProxy && IsCostPosted) || (SellAccountIsOrgProxy && IsRevenuePosted);
			}
		}

		public bool IsConsumerTypeShoudCreateCostOrSellJRJ
		{
			get
			{
				return InvoicingJob != null && (InvoicingJob.IsConsumerTypeShouldCreateCostJRJ || InvoicingJob.IsConsumerTypeShouldCreateSellJRJ);
			}
		}

		public bool CostAndSellAccountAreBothOrgProxies
		{
			get { return CostAccountIsOrgProxy && SellAccountIsOrgProxy; }
		}

		public bool CostOrSellAccountIsOrgProxy
		{
			get { return CostAccountIsOrgProxy || SellAccountIsOrgProxy; }
		}

		public bool CostAccountIsOrgProxy
		{
			get { return Factory.GetCachedValue(GetOrgProxyKey(JR_OH_CostAccount), () => CostAccount != null ? CostAccount.IsProxyOrg(CalculatedCompany) : ZBool.False); }
		}

		public bool SellAccountIsOrgProxy
		{
			get { return Factory.GetCachedValue(GetOrgProxyKey(JR_OH_SellAccount), () => SellAccount != null ? SellAccount.IsProxyOrg(CalculatedCompany) : ZBool.False); }
		}

		string GetOrgProxyKey(ZGuid orgPk) => $"{orgPk.ToStringKey()}{JR_GC.ToStringKey()}";

		#endregion

		#region DisplaySellInvoiceAddress

		public OrgAddress DisplaySellInvoiceAddressBO
		{
			get { return DisplaySellInvoiceAddress != Guid.Empty ? Factory.Load<OrgAddress>(DisplaySellInvoiceAddress) : null; }
		}

		[List("DisplaySellInvoiceAddresses")]
		[RelatedBusinessObject("DisplaySellInvoiceAddressBO")]
		public ZGuid DisplaySellInvoiceAddress
		{
			get { return JR_OA_SellInvoiceAddress.IsValid ? JR_OA_SellInvoiceAddress : DefaultSellInvoiceAddressPK; }
			set { JR_OA_SellInvoiceAddress = value; }
		}

		public ZPropertyInfo DisplaySellInvoiceAddressInfo
		{
			get { return GetWrappedZPropertyInfo(BaseCharge.Schema.DisplaySellInvoiceAddress, x => JR_OA_SellInvoiceAddressInfo); }
		}

		ZGuid DefaultSellInvoiceAddressPK
		{
			get
			{
				var result = ZGuid.Empty;
				if (SellAccount != null)
				{
					if (Job != null && JR_OH_SellAccount == Job.LocalChargesPK)
					{
						result = Job.JH_OA_LocalChargesAddr;
					}
					else if (Job != null && JR_OH_SellAccount == Job.AgentCollectPK)
					{
						result = Job.JH_OA_AgentCollectAddr;
					}
					else
					{
						var address = SellAccount.AddressForSendingARDocuments;
						result = address != null ? address.PK : ZGuid.Empty;
					}
				}

				return result;
			}
		}

		public OrgAddressDependentCollection DisplaySellInvoiceAddresses
		{
			get
			{
				OrgAddressDependentCollection fAddresses = new OrgAddressDependentCollection(Factory);
				OrgHeader parent = Factory.Load<OrgHeader>(JR_OH_SellAccount);
				if (parent != null)
				{
					ZQuery filter = new ZQuery(OrgAddressSchema.OA_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fAddresses = new OrgAddressDependentCollection(parent, filter);
					fAddresses.Load();
				}
				return fAddresses;
			}
		}

		public bool DisplaySellInvoiceAddress_ReadOnly
		{
			get { return AddressAndContactOverrideReadyOnly; }
		}

		bool AddressAndContactOverrideReadyOnly
		{
			get { return !JR_OH_SellAccount.IsValid || JR_OH_SellAccountInfo.ReadOnly; }
		}

		public ZAddress DisplaySellInvoiceAddress_ZAddress
		{
			get
			{
				if (displaySellInvoiceAddress_ZAddress == null)
				{
					displaySellInvoiceAddress_ZAddress = new ZAddress(DisplaySellInvoiceAddressInfo);
					displaySellInvoiceAddress_ZAddress.DefaultAddressType = AddressType.ARM;
				}
				return displaySellInvoiceAddress_ZAddress;
			}
		}

		ZAddress displaySellInvoiceAddress_ZAddress;

		#endregion

		#region DisplaySellInvoiceContact

		public OrgContact DisplaySellInvoiceContactBO
		{
			get { return DisplaySellInvoiceContact != Guid.Empty ? Factory.Load<OrgContact>(DisplaySellInvoiceContact) : null; }
		}

		[List("DisplaySellInvoiceContacts")]
		[RelatedBusinessObject("DisplaySellInvoiceContactBO")]
		public ZGuid DisplaySellInvoiceContact
		{
			get { return JR_OC_SellInvoiceContact.IsValid ? JR_OC_SellInvoiceContact : DefaultSellInvoiceContactPK; }
			set { JR_OC_SellInvoiceContact = value; }
		}

		public ZPropertyInfo DisplaySellInvoiceContactInfo
		{
			get { return GetWrappedZPropertyInfo(BaseCharge.Schema.DisplaySellInvoiceContact, x => JR_OC_SellInvoiceContactInfo); }
		}

		public OrgContactDependentCollection DisplaySellInvoiceContacts
		{
			get
			{
				OrgContactDependentCollection fContacts = new OrgContactDependentCollection(Factory);
				OrgHeader parent = Factory.Load<OrgHeader>(JR_OH_SellAccount);
				if (parent != null)
				{
					ZQuery filter = new ZQuery(OrgContactSchema.OC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fContacts = new OrgContactDependentCollection(parent, filter);
					fContacts.Load();
				}
				return fContacts;
			}
		}

		public bool DisplaySellInvoiceContact_ReadOnly
		{
			get { return AddressAndContactOverrideReadyOnly; }
		}

		ZGuid DefaultSellInvoiceContactPK
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (!JR_OH_SellAccount.IsEmpty &&
					Job != null && JR_OH_SellAccount == Job.LocalChargesPK && !IsRevenuePosted)
				{
					result = Job.JH_OC_LocalBillingContact;
				}

				return result;
			}
		}

		#endregion

		#region IHaveConstructorStackTrace member

		StackTrace IHaveConstructorStackTrace.ConstructorStackTrace { get; set; }

		#endregion

		#region Saving

		public override bool IsSavedByFactory
		{
			get { return base.IsSavedByFactory && (IsDeleted || !IsForConsolCostForIncompleteInvoice); }
		}

		bool IsForConsolCostForIncompleteInvoice
		{
			get { return ParentConsolCost != null && ParentConsolCost.IsForIncompleteInvoice; }
		}

		protected override ZStringBuilder GetIsSavedByFactoryEvaluationInfoCore()
		{
			var msgBuilder = base.GetIsSavedByFactoryEvaluationInfoCore();
			msgBuilder.Append(EvaluateIsSavedByFactoryAndCollectInfo(() => IsDeleted, nameof(IsDeleted)));
			msgBuilder.Append(EvaluateIsSavedByFactoryAndCollectInfo(() => IsForConsolCostForIncompleteInvoice, nameof(IsForConsolCostForIncompleteInvoice)));
			return msgBuilder;
		}

		bool ShouldCallOnFactorySavingMethods
		{
			get { return !IsDeleted && !IsForConsolCostForIncompleteInvoice && !Factory.HasContext(BusinessContext.IncompleteInvoiceSaving) && !Factory.HasContext(BusinessContext.PreviewInvoice); }
		}

		#endregion

		#region Default Values + OnLoaded

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JR_GB = GlbBranch.CurrentBranch.PK;
			JR_GE = GlbDepartment.CurrentDepartment.PK;
			JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		#endregion

		#region Parent Job

		public override JobHeader Job
		{
			get { return Factory.Load<Job>(JR_JH); }
		}

		public Job InvoicingJob
		{
			get { return (Job)Job; }
		}

		#endregion

		#region Internal Job

		public Job InternalInvoicingJob
		{
			get { return (Job)InternalJob; }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			ClearExchangeRates();

			if (!IsDeleted)
			{
				RecordUnexpectedDelete();

				if (!HasReversedAccrual)
				{
					ReverseAccrual(ZDateTime.Now);
				}

				if (!HasReversedWIP)
				{
					ReverseWIP(ZDateTime.Now);
				}

				PaymentBases.DeleteAll();
				if (IsDisbursementCharge && InvoicingJob != null && !InvoicingJob.AllowSaveNonZeroBalanceDisbursements)
				{
					InvoicingJob.ShouldRaiseErrorForNonZeroBalanceDisbursementChargesEvenIfNoChanges = true;
				}
			}

			base.Delete();
		}

		protected override void DeleteForDataRefresh()
		{
			ClearExchangeRates();
			base.DeleteForDataRefresh();
		}

		void RecordUnexpectedDelete()
		{
			if (!IsInDatabase && JR_IsApportioned && ParentConsolCost?.ParentAPInvoice != null)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ApportionmentChargeLinkedToInvoiceLineDeleted, () =>
				{
					if (!ParentConsolCost.ParentAPInvoice.IsReportingDeletedApportionmentChargesSuspended && ParentConsolCost.ParentAPInvoice.Lines.Cast<InvoicingLineBase>().Any(x => (x.ApportionmentChargeImportedFrom_PKForErrorReporting == PK)))
					{
						return System.Environment.StackTrace;
					}
					return null;
				});
			}

			DeleteApportionmentChargesWhenSaveJobConsolCostMonitor monitor;
			if ((monitor = Factory.ServiceContainer.GetService<DeleteApportionmentChargesWhenSaveJobConsolCostMonitor>()) != null &&
				monitor.TryGetRelativeJobConsolCostPK(this, out ZGuid jobConsolCostPK))
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(
					jobConsolCostPK,
					CriticalValidationInfoCollectorServiceKeyType.DeleteNewApportionedChargeWhenSaveJobConsolCost,
					() => FormattableString.Invariant($@"Charge Delete:
{this.GetJobChargeInfo()}
{System.Environment.StackTrace}"));
			}
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get
			{
				bool result = base.CanDelete;

				var checker = ObjectFactory.Get<IAccCashAdvanceFunctionalityChecker>();
				if ((IsInDatabase && !IsAllowedToModifyThisCharge) ||
					IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity ||
					(checker.IsReceivablesCashAdvanceFunctionalityEnabled && ARCashAdvanceRequirement.HasActiveCashAdvanceRequestLine) ||
					(checker.IsPayablesCashAdvanceFunctionalityEnabled && APCashAdvanceRequirement.HasActiveCashAdvanceRequestLine))
				{
					result = false;
				}

				return result;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;

				if (IsInDatabase && !IsAllowedToModifyThisCharge)
				{
					result = ResString.GetMultilingualString("d7e0426c-e339-41b0-8c4b-740a7a2d6b5e", "You cannot create a new charge / modify / delete charge against a branch for which you do not have login permission.");
				}
				else if (IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity)
				{
					result = AccountingConstants.JobIsReadyForFinancialClosureWithoutModifySecurityErrorMessage;
				}
				else if (ARCashAdvanceRequirement.HasActiveCashAdvanceRequestLine)
				{
					var chargeCode = ChargeCode?.AC_Code ?? ZString.Empty;
					result = ResString.GetMultilingualString("dfb370c8-26ba-4931-a83b-ff95aa08e8f9", "Unable to delete the {0} charge as it has an active AR Advance Payment. Please cancel the Advance Payment if you need to delete this charge.", chargeCode);
				}
				else if (APCashAdvanceRequirement.HasActiveCashAdvanceRequestLine)
				{
					var chargeCode = ChargeCode?.AC_Code ?? ZString.Empty;
					result = ResString.GetMultilingualString("8994e2e6-e21d-41dc-8892-b01231e58f7a", "Unable to delete the {0} charge as it has an active AP Advance Payment. Please cancel the Advance Payment if you need to delete this charge.", chargeCode);
				}

				return result;
			}
		}

		#endregion

		#region CLearPaymentDetails()

		public void ClearPaymentDetails()
		{
			using (GetValidationSuspender())
			{
				JR_PaymentType = ZString.Empty;
				JR_AB = ZGuid.Empty;
				JR_AK = ZGuid.Empty;
				JR_ChequeNo = ZString.Empty;

				if (ParentConsolCost != null)
				{
					using (ParentConsolCost.GetValidationSuspender())
					{
						ParentConsolCost.E6_PaymentType = ZString.Empty;
						ParentConsolCost.E6_AB_BankAccount = ZGuid.Empty;
						ParentConsolCost.E6_AK_ChequeBook = ZGuid.Empty;
						ParentConsolCost.E6_ChequeOrReference = ZString.Empty;
					}
				}
			}
		}

		#endregion

		#region WIP and Accrual

		public Accrual Accrual
		{
			get
			{
				Accrual accrual = (APLine != null && APLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual) ? Factory.Load<Accrual>(APLine.PK) : null;
				if (accrual != null)
				{
					accrual.SetRelatedJobCharge(this);
				}
				return accrual;
			}
		}

		public WIP WIP
		{
			get
			{
				WIP wip = (ARLine != null && ARLine.AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP) ? Factory.Load<WIP>(ARLine.PK) : null;
				if (wip != null)
				{
					wip.SetRelatedJobCharge(this);
				}
				return wip;
			}
		}

		#region Creating

		internal bool IsCreditorValidToCreateAccrualWhenAccrualMustHaveCreditor
		{
			get { return !AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.GetValueWithoutFallback(JR_GC.ToGuid(), Guid.Empty, Guid.Empty) || (CostAccount?.OH_IsCreditor ?? false); }
		}

		bool IsAutoJRJ
		{
			get
			{
				return this.HasContext(BusinessContext.AutoJobRevenueJournal)
					|| (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && this.InternalFieldsPointToAnotherEntity());
			}
		}

		internal bool ShouldCreateAccrual
		{
			get
			{
				var invoicingJob = InvoicingJob;
				return invoicingJob != null
					&& invoicingJob.ConsumerTypeShouldCreateAccrual(JR_InvoiceType)
					&& (AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.Value || invoicingJob.AreTransactionsCreated())
					&& (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value ? JR_LocalCostAmt != 0 : JR_LocalCostAmt > 0)
					&& (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() || !CostAccountIsOrgProxy || !ShouldCreateCostJRJ);
			}
		}

		internal bool ShouldCreateCostJRJ
		{
			get
			{
				var shouldCreate = JR_AL_APLine.IsEmpty || Accrual != null;
				shouldCreate = shouldCreate && !JR_LocalCostAmt.IsEmpty && !JR_OSCostAmt.IsEmpty;
				shouldCreate = shouldCreate && CostAccountIsOrgProxy;
				shouldCreate = shouldCreate && IsAutoJRJ;
				shouldCreate = shouldCreate && InvoicingJob != null && InvoicingJob.IsConsumerTypeShouldCreateCostJRJ;

				return shouldCreate;
			}
		}

		internal bool IsDebtorValidToCreateWIPWhenWIPMustHaveDebtor
		{
			get { return !AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.GetValueWithoutFallback(JR_GC.ToGuid(), Guid.Empty, Guid.Empty) || (SellAccount?.OH_IsDebtor ?? false); }
		}

		internal bool ShouldCreateWIP
		{
			get
			{
				var invoicingJob = InvoicingJob;
				return invoicingJob != null
					&& invoicingJob.ConsumerTypeShouldCreateWIP(JR_InvoiceType)
					&& (AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.Value || invoicingJob.AreTransactionsCreated())
					&& (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.Value ? JR_LocalSellAmt != 0 : JR_LocalSellAmt > 0)
					&& (!AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() || !SellAccountIsOrgProxy || !ShouldCreateSellJRJ);
			}
		}

		internal bool ShouldCreateSellJRJ
		{
			get
			{
				var shouldCreate = JR_AL_ARLine.IsEmpty || WIP != null;
				shouldCreate = shouldCreate && !JR_LocalSellAmt.IsEmpty && !JR_OSSellAmt.IsEmpty;
				shouldCreate = shouldCreate && SellAccountIsOrgProxy;
				shouldCreate = shouldCreate && IsAutoJRJ;
				shouldCreate = shouldCreate && InvoicingJob.IsConsumerTypeShouldCreateSellJRJ;

				return shouldCreate;
			}
		}

		internal bool ShouldCreateJRJ => ShouldCreateCostJRJ || ShouldCreateSellJRJ;

		protected void CreateAccrualCore()
		{
			Accrual accrual = Factory.New<Accrual>();

			using (accrual.GetValidationSuspender())
			{
				accrual.SetValues(InvoicingJob, this);
				JR_AL_APLine = accrual.PK;

				this.Accrual.UpdateAL_PostDate();
			}
		}

		protected void CreateWIPCore()
		{
			WIP wip = Factory.New<WIP>();

			using (wip.GetValidationSuspender())
			{
				wip.SetValues(InvoicingJob, this);
				JR_AL_ARLine = wip.PK;

				this.WIP.UpdateAL_PostDate();
			}
		}

		internal ZDateTime WIPAccrualCreationDate
		{
			get
			{
				if (!wipAccrualCreationDate.HasValue)
				{
					wipAccrualCreationDate = ZDateTime.Now;
				}
				return wipAccrualCreationDate.Value;
			}
			set { wipAccrualCreationDate = value; }
		}
		ZDateTime? wipAccrualCreationDate;

		#endregion

		#region Reversing

		internal bool ShouldReverseAccrual
		{
			get
			{
				return !IsCostPosted && Accrual != null &&
						(JR_AC != Accrual.AL_AC ||
						JR_LocalCostAmt != Accrual.AL_LocalExTaxAmount ||
						JR_OH_CostAccount != Accrual.AL_OH ||
						JR_GE != Accrual.AL_GE ||
						JR_GB != Accrual.AL_GB);
			}
		}

		internal bool ShouldReverseWIP
		{
			get
			{
				return !IsRevenuePosted && WIP != null &&
						(JR_LocalSellInvoiceAmt - JR_CFXAmt != WIP.AL_LocalExTaxAmount ||
						JR_AC != WIP.AL_AC ||
						JR_OH_SellAccount != WIP.AL_OH ||
						JR_GE != WIP.AL_GE ||
						JR_GB != WIP.AL_GB);
			}
		}

		public void ReverseAccrual(ZDateTime reverseDate, bool forceReverseDate = false)
		{
			if (Accrual != null)
			{
				using (Accrual.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
				{
					WIPAccrualReverseDate = reverseDate;

					if (forceReverseDate)
					{
						using (Accrual.GetValidationSuspender())
						{
							Accrual.AL_ReverseDate = reverseDate;
						}
					}
					else
					{
						Accrual.UpdateAL_ReverseDate();
					}

					if (Accrual.IsReversed)
					{
						ClearCostLink();
					}
				}
			}
		}

		public void ReverseWIP(ZDateTime reverseDate, bool forceReverseDate = false)
		{
			if (WIP != null)
			{
				using (WIP.SetReverseDateBeforeUnlinkChargeErrorSuspender.GetSuspender())
				{
					WIPAccrualReverseDate = reverseDate;

					if (forceReverseDate)
					{
						using (WIP.GetValidationSuspender())
						{
							WIP.AL_ReverseDate = reverseDate;
						}
					}
					else
					{
						WIP.UpdateAL_ReverseDate();
					}

					if (WIP.IsReversed)
					{
						ClearRevenueLink();
					}
				}
			}
		}

		internal ZDateTime WIPAccrualReverseDate
		{
			get;
			set;
		}

		bool HasReversedAccrual
		{
			get
			{
				var result = false;
				if (Accrual != null)
				{
					result = Accrual.IsReversed;
				}
				else
				{
					var accrual = GetAccrualFromHistory();
					result = accrual != null && accrual.IsReversed;
				}
				return result;
			}
		}

		bool HasReversedWIP
		{
			get
			{
				var result = false;
				if (WIP != null)
				{
					result = WIP.IsReversed;
				}
				else
				{
					var wip = GetWIPFromHistory();
					result = wip != null && wip.IsReversed;
				}
				return result;
			}
		}

		#endregion

		#region JR_IsCostPosted

		public ZBool JR_IsCostPosted
		{
			get { return IsCostPosted; }
		}

		public ZPropertyInfo JR_IsCostPostedInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_IsCostPosted); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region JR_IsApproved

		public ZBool JR_IsApproved
		{
			get { return IsApproved; }
		}

		public ZPropertyInfo JR_IsApprovedInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_IsApproved); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region JR_IsRevenuePosted

		public ZBool JR_IsRevenuePosted
		{
			get { return IsRevenuePosted; }
		}

		public ZPropertyInfo JR_IsRevenuePostedInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_IsRevenuePosted); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		protected AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		AccountingPeriodCalculator fPeriodCalculator;

		internal bool CanRecognizeProfitOnWIPsAndAccruals
		{
			get
			{
				IRevenueRecognition recognitionOption;
				return (ShouldCreateAccrual || ShouldCreateWIP) && InvoicingJob != null &&
						(AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.Value ||
						((recognitionOption = InvoicingJob.GetRevenueRecognitionOption(ChargeCode)) != null &&
						(recognitionOption.RecognitionDateOptionCode == RevenueRecognitionLookups.RecognitionDateOptionCodes.Immediate ||
						recognitionOption.RecognitionDateOptionCode == RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate)));
			}
		}

		#endregion

		#region Lookups

		public new BaseChargeLookups Lookups => (BaseChargeLookups)base.Lookups;

		protected override JobChargeLookups GetNewLookups()
		{
			return new BaseChargeLookups(this);
		}

		#region Witholding Tax

		AccWithholdingCollection fWHTCollection;
		public AccWithholdingCollection WHTCollection
		{
			get
			{
				if (fWHTCollection == null)
				{
					fWHTCollection = new AccWithholdingCollection(Factory, GlbCompany.CurrentCompany);
				}

				return fWHTCollection;
			}
		}

		#endregion

		#region GST

		AccTaxRateCollection fGSTCollection;
		public AccTaxRateCollection GSTCollection
		{
			get
			{
				if (fGSTCollection == null)
				{
					ZQuery taxRatesFilter = new ZQuery(AccTaxRateSchema.AT_IsActive, true);
					fGSTCollection = new VATAccTaxRateCollection(Factory, taxRatesFilter);
				}
				return fGSTCollection;
			}
		}

		#endregion

		#region Bank Accounts

		public AccBankAccountCollection BankAccounts
		{
			get { return FindboxLookupCollections.GetBankAccounts(Factory, Branch); }
		}

		#endregion

		#region Cheque Books

		public AccChequeBookCollection ChequeBooks
		{
			get
			{
				return Factory.GetCachedValue(FindboxLookupCollections.CachingKey + "BaseCharge" + JR_AB.ToStringKey(),
					() =>
					{
						var fChequeBooks = new ActiveChequeBookCollection(Factory, BankAccount);
						fChequeBooks.SetOverrideNotificationWhenAdditionalFilterNotMet(Res.GetString("e607d415-ff56-4e85-b88c-f73d607a1383", "This check book cannot be chosen because it belongs to another bank account, another branch or/and is inactive. Please choose another check book"));
						return fChequeBooks;
					});
			}
		}

		#endregion

		#region Branches

		public GlbBranchDependentCollection Branches
		{
			get { return FindboxLookupCollections.GetCompanyBranchesCollection(Factory); }
		}

		#endregion

		#region Departments

		public GlbDepartmentCollection Departments
		{
			get { return FindboxLookupCollections.GetDepartmentCollection_ActiveOnly(Factory); }
		}

		#endregion

		#region InternalJobs

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter ID used only in code")]
		public const string DefaultGatewayJobsFilterName = "Gateway Jobs for Shipment #";

		public JobCollection InternalJobs
		{
			get
			{
				var jobs = new JobCollection(Factory, new ZQuery(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK));
				jobs.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(DefaultGatewayJobsFilterName, "Property", JR_Calc_RelatedJobNumber));
				return jobs;
			}
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		#endregion

		#region Creditors

		public CreditorCollection Creditors
		{
			get { return FindboxLookupCollections.GetCreditorCollection(Factory); }
		}

		public override OrgHeader CostAccount
		{
			get
			{
				var costAccount = base.CostAccount;
				if (costAccount != null && !costAccount.ReadOnly)
				{
					costAccount.SetReadOnlyIncludingChildren(true);
				}
				return costAccount;
			}
		}

		#endregion

		#region Debtors

		public OrganisationsFindBoxCollection Debtors
		{
			get
			{
				return Factory.GetCachedValue(FindboxLookupCollections.CachingKey + DebtorCollectionType.ToString(), GetOrganisationsFindBoxByForDebtorCollectionType());
			}
		}

		GetValueDelegate<OrganisationsFindBoxCollection> GetOrganisationsFindBoxByForDebtorCollectionType()
		{
			return delegate
			{ return DebtorCollectionType == DebtorCollectionTypeEnum.OneOffQuotation ? GetOrganisationsFindBox() : GetDebtorCollection(); };
		}

		OrganisationsFindBoxCollection GetOrganisationsFindBox()
		{
			return FindboxLookupCollections.GetOrganisationsFindBoxCollection(Factory);
		}

		OrganisationsFindBoxCollection GetDebtorCollection()
		{
			return FindboxLookupCollections.GetDebtorCollection(Factory);
		}

		DebtorCollectionTypeEnum DebtorCollectionType
		{
			get { return InvoicingJob != null && InvoicingJob.JobType == JobInvoicingConsumerTypes.OneOffQuotation ? DebtorCollectionTypeEnum.OneOffQuotation : DebtorCollectionTypeEnum.other; }
		}

		enum DebtorCollectionTypeEnum
		{
			OneOffQuotation,
			other
		}

		public override OrgHeader SellAccount
		{
			get
			{
				var sellAccount = base.SellAccount;
				if (sellAccount != null && !sellAccount.ReadOnly)
				{
					sellAccount.SetReadOnlyIncludingChildren(true);
				}
				return sellAccount;
			}
		}

		#endregion

		#region Payment Types

		[List("PaymentTypes")]
		public virtual CodeDescriptionPairList PaymentTypes
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.PaymentMethod); }
		}

		#endregion

		#endregion

		#region Properties

		#region Readonly properties

		[ReadOnly(true)]
		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public override ZDecimal JR_OSSellWHTAmt
		{
			get { return base.JR_OSSellWHTAmt; }
			set { base.JR_OSSellWHTAmt = value; }
		}

		#endregion

		#region JR_OSSellGSTAmt_Calc

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public override ZDecimal JR_OSSellGSTAmt_Calc => CalculateSellGSTAmounts().OSSellGSTAmount;

		(ZDecimal OSSellGSTAmount, ZDecimal LocalSellGSTAmount) CalculateSellGSTAmounts()
		{
			var arlineDetails = new LineDetails();
			var arLine = ARLine;
			if (arLine != null)
			{
				arlineDetails = new LineDetails
				{
					Type = arLine.AL_LineType,
					CurrencyCode = arLine.AL_RX_NKTransactionCurrency,
					LocalTaxAmount = arLine.AL_GSTVAT,
					OSAmount = arLine.AL_OSAmount
				};
			}

			var chargeSellDetails = new ChargeSellDetails
			{
				InvoiceType = JR_InvoiceType,
				SellExRate = JR_OSSellExRate,
				SellCurrencyCode = JR_RX_NKSellCurrency,
				SellInvoiceCurrencyCode = JR_RX_NKSellInvoiceCurrency,
				LocalCurrencyCode = (ZString)Company?.GC_RX_NKLocalCurrency,
				SellOSAmount = JR_OSSellAmt,
				LocalSellAmount = JR_LocalSellAmt,
				SellTaxRatePK = JR_AT_SellGSTRate,
				TaxRate = SellGSTRate?.GetRate(JR_SellTaxDate),
				EffectiveExtraTaxRate = SellGSTRate?.GetEffectiveExtraRate(JR_SellTaxDate),
				ChargeCodePK = JR_AC,
				CompanyPK = JR_GC,
				CountryCode = (ZString)Company?.GC_RN_NKCountryCode,
			};

			return TaxCalculatorHelperForCharge.CalculateSellGSTAmounts(Factory, arlineDetails, chargeSellDetails);
		}

		#endregion

		#region Related Job

		public ZString JobReference
		{
			get { return Job != null ? Job.JH_JobNum : ZString.Empty; }
		}

		public ZPropertyInfo JobReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(JobReference)); }
		}

		#endregion

		#region Is Agent Charge to be negated and journalled to AR Invoice

		public bool IsAgentRelatedCharge
		{
			get
			{
				bool result = false;
				if (JR_E6.IsValid && ParentConsolCost != null)
				{
					result = ParentConsolCost.E6_Calc_IncludeOnAgentInvoice;
				}
				return result;
			}
		}

		public new JobConsolCost ParentConsolCost
		{
			get { return (JobConsolCost)base.ParentConsolCost; }
		}

		#endregion

		#region GatewaySellHeader

		public new JobConsolCost GatewayConsolCost
		{
			get { return (JobConsolCost)base.GatewayConsolCost; }
		}

		#endregion

		#region JR_ChargeableUnit

		[ReadOnly(true)]
		public ZString JR_ChargeableUnit
		{
			get
			{
				ZString result = ZString.Empty;
				if (ParentConsolCost != null && ParentConsolCost.Consol != null)
				{
					result = ParentConsolCost.Consol.CostSupporter.TotalChargeableUnit;
				}
				else if (!ChargeableUnitForRevenueApportionment.IsEmpty)
				{
					result = ChargeableUnitForRevenueApportionment;
				}
				else if (ShipmentInfo != null)
				{
					result = ShipmentInfo.InvoicingSupporter.ActualChargeableUnit;
				}
				return result;
			}
		}

		public ZPropertyInfo JR_ChargeableUnitInfo
		{
			get { return GetZPropertyInfo(nameof(JR_ChargeableUnit)); }
		}

		#endregion

		public ZString ChargeableUnitForRevenueApportionment
		{
			get;
			set;
		}

		#region JR_Chargeable

		[DecimalPlaces(nameof(WeightVolumeDecimals))]
		public ZDecimal JR_Chargeable
		{
			get
			{
				ZDecimal result = 0m;

				if (ShipmentInfo != null)
				{
					ZString resultChargeableUnit = JR_ChargeableUnit;

					if (resultChargeableUnit == ZString.Empty || ShipmentInfo.InvoicingSupporter.ActualChargeableUnit == resultChargeableUnit)
					{
						result = ShipmentInfo.InvoicingSupporter.ActualChargeable;
					}
					else
					{
						result = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
						{
							Weight = new ZWeight(ShipmentInfo.InvoicingSupporter.ActualWeight, ShipmentInfo.InvoicingSupporter.ActualWeightUnit),
							Volume = new ZVolume(ShipmentInfo.InvoicingSupporter.ActualVolume, ShipmentInfo.InvoicingSupporter.ActualVolumeUnit),
							LoadingLength = new Quantity(ShipmentInfo.InvoicingSupporter.ActualLoadingMeters, Constants.LoadingLength.LoadingMeters),
							TargetUnit = resultChargeableUnit,
							ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(ShipmentInfo.InvoicingSupporter, resultChargeableUnit)
						}).Chargeable.Amount;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JR_ChargeableInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Chargeable); }
		}
		#endregion

		#region ActualWeight

		[DecimalPlaces(nameof(WeightVolumeDecimals))]
		public ZDecimal JR_ActualWeight
		{
			get { return (ShipmentInfo != null) ? ShipmentInfo.InvoicingSupporter.ActualWeight : ZDecimal.Zero; }
		}

		public ZPropertyInfo JR_ActualWeightInfo
		{
			get { return GetZPropertyInfo(Schema.JR_ActualWeight); }
		}

		public ZString JR_ActualWeightUnit
		{
			get { return (ShipmentInfo != null) ? ShipmentInfo.InvoicingSupporter.ActualWeightUnit : ZString.Empty; }
		}

		public ZPropertyInfo JR_ActualWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JR_ActualWeightUnit); }
		}

		#endregion

		#region ActualVolume

		public ZDecimal JR_ActualVolume
		{
			get { return (ShipmentInfo != null) ? ShipmentInfo.InvoicingSupporter.ActualVolume : ZDecimal.Zero; }
		}

		public ZPropertyInfo JR_ActualVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.JR_ActualVolume); }
		}

		public ZString JR_ActualVolumeUnit
		{
			get { return (ShipmentInfo != null) ? ShipmentInfo.InvoicingSupporter.ActualVolumeUnit : ZString.Empty; }
		}

		public ZPropertyInfo JR_ActualVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.JR_ActualVolumeUnit); }
		}

		#endregion

		public virtual IJobInvoicingPlugIn ShipmentInfo
		{
			get { return InvoicingJob.PlugInData; }
		}

		#region JR_AL_APLine

		public override ZGuid JR_AL_APLine
		{
			get { return base.JR_AL_APLine; }
			set
			{
				if (CanChangeAPLine(value) && JR_AL_APLine != value)
				{
					var oldValue = JR_AL_APLine;
					HandleAPLineChanging(value);
					base.JR_AL_APLine = value;
					HandleAPLineChanged(oldValue);

					if (IsCostPosted)
					{
						if (!JR_IsCostTaxAmountOverridden)
						{
							using (StopGSTAmountOfUnApportionedChargeFromBeingOverridden.GetSuspender())
							{
								JR_IsCostTaxAmountOverridden = true;
							}
						}

						if (!value.IsEmpty && IsCostPosted && CostExchangeRate != null)
						{
							var costExRate = CostExchangeRate ?? GetCostExchangeRate(false);
							if (costExRate != null && costExRate.Rate != 0)
							{
								costExRate.EnsureWillNotBeAutoDeleted();
							}
							CostExchangeRate = null;
						}
					}
				}
			}
		}

		bool CanChangeAPLine(ZGuid newValue)
		{
			return !IsCostPosted;
		}

		void HandleAPLineChanging(ZGuid newValue)
		{
			if (Accrual != null && Accrual.AL_ReverseDate.IsEmpty)
			{
				var message = (NoResString)@"Accrual must have Reverse Data set before detaching from dbo.JobCharge" + System.Environment.NewLine + Accrual.GetTransactionLineInfo() + System.Environment.NewLine + this.GetJobChargeInfo();
				ExceptionReporter.Instance.ReportDeveloperException("AccrualMustHaveReverseDataSetBeforeDetachingFromJobCharge", message, new Exception(message));

				if (!WIPAccrualReverseDate.IsEmpty)
				{
					Accrual.AL_ReverseDate = WIPAccrualReverseDate;
				}
				else
				{
					Accrual.AL_ReverseDate = ZDateTime.Now;
				}
			}
		}

		void HandleAPLineChanged(ZGuid oldValue)
		{
			if (Accrual != null)
			{
				if (Accrual.IsReversed)
				{
					var message = Res.GetString("a50fdb87-baf7-4b87-ac51-9cc6585109b5", @"The new Accrual has been reversed {0}", System.Environment.NewLine + Accrual.GetTransactionLineInfo() + System.Environment.NewLine + this.GetJobChargeInfo());
					ErrorReporter.ReportOnce("The new Accrual has been reversed", message);
				}
				Accrual.SetRelatedJobCharge(this);
			}
		}

		public void UpdateLastAccrualReverseDate(ZDateTime reverseDate)
		{
			var lastAccrual = Accrual ?? GetAccrualFromHistory();

			if (lastAccrual != null && lastAccrual.IsReversed)
			{
				if (reverseDate.IsEmpty)
				{
					throw new ArgumentException("Accrual Reverse Date cannot be cleared by setting empty date");
				}

				lastAccrual.AL_ReverseDate = reverseDate;
			}
		}

		Accrual GetAccrualFromHistory()
		{
			Accrual result = null;
			if (!APLinePreviousValueFromHistory.IsEmpty)
			{
				var lineFromHistory = Factory.Load<TransactionLine>(APLinePreviousValueFromHistory);
				result = lineFromHistory != null && lineFromHistory.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual ? lineFromHistory as Accrual : null;
			}
			return result;
		}

		#endregion

		#region JR_AL_ARLine

		public override ZGuid JR_AL_ARLine
		{
			get { return base.JR_AL_ARLine; }
			set
			{
				if (CanChangeARLine(value) && JR_AL_ARLine != value)
				{
					var oldValue = JR_AL_ARLine;
					HandleARLineChanging(value);
					base.JR_AL_ARLine = value;
					HandleARLineChanged(oldValue);

					if (!value.IsEmpty && IsRevenuePosted)
					{
						if ((string)ARLine.TransactionHeader?.AH_TransactionType != TransactionTypes.JobRevenueJournal)
						{
							var revenueExRate = RevenueExchangeRate ?? GetRevenueExchangeRate(false);
							if (revenueExRate != null && revenueExRate.Rate != 0)
							{
								revenueExRate.EnsureWillNotBeAutoDeleted();
							}
						}
						RevenueExchangeRate = null;
					}
				}
			}
		}

		bool CanChangeARLine(ZGuid newValue)
		{
			return !IsRevenuePosted;
		}

		void HandleARLineChanging(ZGuid newValue)
		{
			if (WIP != null && WIP.AL_ReverseDate.IsEmpty)
			{
				var message = (NoResString)@"WIP must have Reverse Data set before detaching from dbo.JobCharge" + System.Environment.NewLine + WIP.GetTransactionLineInfo() + System.Environment.NewLine + this.GetJobChargeInfo();
				ExceptionReporter.Instance.ReportDeveloperException("WIPMustHaveReverseDataSetBeforeDetachingFromJobCharge", message, new Exception(message));

				if (!WIPAccrualReverseDate.IsEmpty)
				{
					WIP.AL_ReverseDate = WIPAccrualReverseDate;
				}
				else
				{
					WIP.AL_ReverseDate = ZDateTime.Now;
				}
			}
		}

		void HandleARLineChanged(ZGuid oldValue)
		{
			if (WIP != null)
			{
				if (WIP.IsReversed)
				{
					var message = Res.GetString("7358a948-de34-41ab-9f71-7094a34ca671", @"The new WIP has been reversed {0}", System.Environment.NewLine + WIP.GetTransactionLineInfo() + System.Environment.NewLine + this.GetJobChargeInfo());
					ErrorReporter.ReportOnce("The new WIP has been reversed", message);
				}
				WIP.SetRelatedJobCharge(this);
			}
		}

		public void UpdateLastWIPReverseDate(ZDateTime reverseDate)
		{
			var lastWIP = WIP ?? GetWIPFromHistory();

			if (lastWIP != null && lastWIP.IsReversed)
			{
				if (reverseDate.IsEmpty)
				{
					throw new ArgumentException("WIP Reverse Date cannot be cleared by setting empty date");
				}

				lastWIP.AL_ReverseDate = reverseDate;
			}
		}

		WIP GetWIPFromHistory()
		{
			WIP result = null;
			if (!ARLinePreviousValueFromHistory.IsEmpty)
			{
				var lineFromHistory = Factory.Load<TransactionLine>(ARLinePreviousValueFromHistory);
				result = lineFromHistory != null && lineFromHistory.AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP ? lineFromHistory as WIP : null;
			}
			return result;
		}

		#endregion

		#region JR_GE

		[List("Departments")]
		public override ZGuid JR_GE
		{
			get { return base.JR_GE; }
			set
			{
				var hasChanged = JR_GE != value;
				base.JR_GE = value;

				if (hasChanged)
				{
					SetSupplyType();
					SetCreditorFromOverride();
					UpdateSellInvoiceCurrency();
				}
			}
		}

		#endregion

		#region JR_GC

		public override ZGuid JR_GC
		{
			get { return base.JR_GC; }
			set
			{
				var oldValue = JR_GC;
				base.JR_GC = value;

				if (value != GlbCompany.CurrentCompany.PK)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_JCNotEqualToCurrentCompany, () =>
					{
						return Invariant($@"OldValue: {oldValue}, NewValue: {value}
{System.Environment.StackTrace}");
					});
				}
			}
		}

		#endregion

		#region JR_RX_NKCostCurrency

		public sealed override ZString JR_RX_NKCostCurrency
		{
			get { return base.JR_RX_NKCostCurrency; }
			set
			{
				if (JR_RX_NKCostCurrency != value)
				{
					var oldCurrency = JR_RX_NKCostCurrency;

					using (AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(this, CostOSPropertiesRequiringRounding(), oldCurrency, value, CostRoundingErrorReproterFunctionalitySuspender))
					using (SetEstimatedCostSuspender.GetSuspender())
					{
						SetJR_RX_NKCostCurrencyCore(value);
					}
				}
			}
		}

		protected virtual void SetJR_RX_NKCostCurrencyCore(ZString value)
		{
			base.JR_RX_NKCostCurrency = value;
		}

		internal virtual string[] CostOSPropertiesRequiringRounding()
		{
			return Array.Empty<string>();
		}

		internal FunctionalitySuspender CostRoundingErrorReproterFunctionalitySuspender
		{
			get
			{
				if (costRoundingErrorReproterFunctionalitySuspender == null)
				{
					costRoundingErrorReproterFunctionalitySuspender = new FunctionalitySuspender(
						() => AccountingValuesRoundingHelper.ReportErrorIfPropertiesNotRounded(this, CostOSPropertiesRequiringRounding(), originalCostCurrencyForRounding, JR_RX_NKCostCurrency, null),
						true,
						() => originalCostCurrencyForRounding = JR_RX_NKCostCurrency
						);
				}

				return costRoundingErrorReproterFunctionalitySuspender;
			}
		}
		FunctionalitySuspender costRoundingErrorReproterFunctionalitySuspender;

		ZString originalCostCurrencyForRounding;

		#endregion

		#region JR_RX_NKSellCurrency

		public sealed override ZString JR_RX_NKSellCurrency
		{
			get { return base.JR_RX_NKSellCurrency; }
			set
			{
				if (JR_RX_NKSellCurrency != value)
				{
					var oldCurrency = JR_RX_NKSellCurrency;

					using (AccountingValuesRoundingHelper.GetActionForChangeInDecimalPlaces(this, SellOSPropertiesRequiringRounding(), oldCurrency, value, SellRoundingErrorReproterFunctionalitySuspender))
					{
						SetJR_RX_NKSellCurrencyCore(value);
					}
				}
			}
		}

		protected virtual void SetJR_RX_NKSellCurrencyCore(ZString value)
		{
			base.JR_RX_NKSellCurrency = value;
		}

		internal virtual string[] SellOSPropertiesRequiringRounding()
		{
			return Array.Empty<string>();
		}

		internal FunctionalitySuspender SellRoundingErrorReproterFunctionalitySuspender
		{
			get
			{
				if (sellRoundingErrorReproterFunctionalitySuspender == null)
				{
					sellRoundingErrorReproterFunctionalitySuspender = new FunctionalitySuspender(
						() => AccountingValuesRoundingHelper.ReportErrorIfPropertiesNotRounded(this, SellOSPropertiesRequiringRounding(), originalSellCurrencyForRounding, JR_RX_NKSellCurrency, null),
						true,
						() => originalSellCurrencyForRounding = JR_RX_NKSellCurrency
						);
				}

				return sellRoundingErrorReproterFunctionalitySuspender;
			}
		}

		FunctionalitySuspender sellRoundingErrorReproterFunctionalitySuspender;

		ZString originalSellCurrencyForRounding;

		#endregion

		#region JR_GB

		[List("Branches")]
		public override ZGuid JR_GB
		{
			get { return base.JR_GB; }
			set
			{
				bool hasChanged = JR_GB != value;
				base.JR_GB = value;
				JR_ABInfo.RefreshBinding();
				JR_AKInfo.RefreshBinding();

				if (hasChanged)
				{
					if (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.GetValueWithoutFallback(Company?.PK.ToGuid() ?? Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
					{
						DefaultCostPlaceOfSupply();
						DefaultSellPlaceOfSupply();
					}

					if (GlbCompany.CurrentCompany.GC_IsGSTRegistered && !IsUsedByConsolCostImporter
						&& !(AccountingMasterFilesUtils.IsTaxBranchApplicable && AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value))
					{
						SetGSTandWHTRates();
					}

					if (AutoJRJRegistryStatusHelper.IsAutoJRJWithTaxRegistrationNumberEnabled())
					{
						if (SellAccountIsOrgProxy)
						{
							InternalFieldsManager.ResetInternalFields(JR_OH_SellAccount, true);
						}
						else if (CostAccountIsOrgProxy)
						{
							InternalFieldsManager.ResetInternalFields(JR_OH_CostAccount);
						}
					}

					UpdateSellInvoiceCurrency();
				}
			}
		}

		#endregion

		#region JR_GB_CostTaxBranch

		[List("Branches")]
		public override ZGuid JR_GB_CostTaxBranch
		{
			get { return base.JR_GB_CostTaxBranch; }
			set
			{
				bool hasChanged = JR_GB_CostTaxBranch != value;
				base.JR_GB_CostTaxBranch = value;
				if (hasChanged
					&& AccountingMasterFilesUtils.IsTaxBranchApplicable && AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value)
				{
					SetCostGSTRates();
				}
			}
		}

		#endregion

		#region JR_GB_SellTaxBranch

		[List("Branches")]
		public override ZGuid JR_GB_SellTaxBranch
		{
			get { return base.JR_GB_SellTaxBranch; }
			set
			{
				bool hasChanged = JR_GB_SellTaxBranch != value;
				base.JR_GB_SellTaxBranch = value;
				if (hasChanged
					&& AccountingMasterFilesUtils.IsTaxBranchApplicable && AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.Value)
				{
					SetSellGSTRates();
				}
			}
		}

		#endregion

		#region JR_AB

		[List("BankAccounts")]
		public override ZGuid JR_AB
		{
			get { return base.JR_AB; }
			set
			{
				if (JR_AB != value)
				{
					base.JR_AB = value;
					if (IsCashAccount)
					{
						JR_PaymentType = ReceiptTypes.Cash;
					}
					JR_AKInfo.RefreshBinding();
				}
			}
		}

		internal bool IsCashAccount => BankAccount != null && BankAccount.AB_AccountType == AccountTypeCodeDescriptionPairList.Codes.CSH;

		#endregion

		#region JR_AT_CostGSTRate

		[List("GSTCollection")]
		public override ZGuid JR_AT_CostGSTRate
		{
			get { return base.JR_AT_CostGSTRate; }
			set
			{
				if (JR_AT_CostGSTRate != value)
				{
					var oldValue = base.JR_AT_CostGSTRate;
					var oldCode = base.CostGSTRate?.AT_Code ?? ZString.Empty;
					base.JR_AT_CostGSTRate = value;
					if (!JR_AT_CostGSTRate.IsValid)
					{
						JR_CostTaxDate = ZDate.Empty;
					}
					OnCostTaxRateChanged();
					AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo((NoResString)"Tax Rate", ParentConsolCost?.E6_AT_TaxRate, base.JR_AT_CostGSTRate, oldValue);

					if (this.HasContext(BusinessContext.AutoJobRevenueJournal) && JR_AT_CostGSTRate.IsValid)
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeTaxRateNotEmptyWhenAutoJobRevenueJournal,
							() => Invariant(
$@"Job Consol Cost Tax Rate : '{ParentConsolCost?.TaxRate?.AT_Code ?? ZString.Empty}'
Charge's Tax Rate is changed from '{oldCode}' to '{base.CostGSTRate?.AT_Code ?? ZString.Empty}'.
Call stack:
{new StackTrace()}"));
					}
				}

				SetCostTaxRateAndMessage(true);

				if (!IsValidationSuspended)
				{
					Validation.ValidateJR_A9_CostVATClass();
				}
			}
		}

		protected bool JR_AT_CostGSTRate_ReadOnly => IsCostGSTFieldReadOnly;

		protected virtual void UpdateJR_AT_CostGSTRateReadOnly()
		{
			if (!PreventReadOnlyFromChangingValues && !JR_IsApportioned && !IsCostGSTApplicable)
			{
				JR_AT_CostGSTRate = ZGuid.Empty;
			}
			JR_AT_CostGSTRateInfo.RefreshBinding();
			JR_CostTaxDateInfo.RefreshBinding();
		}

		public FunctionalitySuspender ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender =>
			apportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender ??
			(apportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender = new FunctionalitySuspender(CheckAndReportCostTaxDatesMismatch));

		FunctionalitySuspender apportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender;

		void CheckAndReportCostTaxDatesMismatch()
		{
			if (!IsCostPosted && JR_E6.IsValid && JR_AT_CostGSTRate.IsValid && ParentConsolCost != null && ParentConsolCost.E6_TaxDate != base.JR_CostTaxDate)
			{
				var message = FormattableString.Invariant($@"{JobChargeSchema.JR_CostTaxDate.Name} = {JR_CostTaxDate}. But {JobConsolCostSchema.E6_TaxDate.Name} = {ParentConsolCost.E6_TaxDate}

{this.GetJobChargeInfo()}

{ParentConsolCost.GetJobConsolCostInfo()},

Stacktrace:
{System.Environment.StackTrace}");
				ErrorReporter.ReportOnce("ApportionedChargeWithMismatched_TaxDate_1", message);
			}
		}

		#endregion

		#region JR_CostTaxDate

		public override ZDate JR_CostTaxDate
		{
			get => base.JR_CostTaxDate;
			set
			{
				if (base.JR_CostTaxDate != value)
				{
					using (ApportionedChargeTaxDateMismatchWithConsolCostTaxDateDelayedCheckSuspender.GetSuspender())
					{
						var oldValue = base.JR_CostTaxDate;
						base.JR_CostTaxDate = value;
						OnCostTaxRateChanged();
						AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo((NoResString)"cost tax date", ParentConsolCost?.E6_TaxDate, JR_CostTaxDate, oldValue);
					}
				}
			}
		}

		protected bool JR_CostTaxDate_ReadOnly => IsCostGSTFieldReadOnly;

		public void SetCostTaxDateSafe(ZDate value) => JR_CostTaxDate = (JR_AT_CostGSTRate.IsValid ? value : ZDate.Empty);

		#endregion

		#region JR_AW_CostWHTRate

		[List("WHTCollection")]
		public override ZGuid JR_AW_CostWHTRate
		{
			get { return base.JR_AW_CostWHTRate; }
			set
			{
				if (JR_AW_CostWHTRate != value)
				{
					base.JR_AW_CostWHTRate = value;
					UpdateOsCostWHTAmount();
				}
			}
		}

		#endregion

		#region JR_AT_SellGSTRate

		[List("GSTCollection")]
		public override ZGuid JR_AT_SellGSTRate
		{
			get { return base.JR_AT_SellGSTRate; }
			set
			{
				if (JR_AT_SellGSTRate != value)
				{
					base.JR_AT_SellGSTRate = value;
					if (!JR_AT_SellGSTRate.IsValid)
					{
						JR_SellTaxDate = ZDate.Empty;
					}
					JR_Calc_OSSellAmtWithGSTInfo.RefreshBinding();
				}

				ZGuid overrideSellInvTaxMsg = ZGuid.Empty;
				ZGuid overrideSellTaxRate = ZGuid.Empty;
				if (Job != null)
				{
					overrideSellTaxRate = Job.GetGSTID(this, CostSell.Revenue, SellPlaceOfSupplyLocation, out overrideSellInvTaxMsg);
				}

				SetSellTaxInvoiceMessage(overrideSellTaxRate, overrideSellInvTaxMsg);

				Validation?.ValidateJR_OSSellGSTAmt_Calc();

				if (!IsValidationSuspended)
				{
					Validation.ValidateJR_A9_SellVATClass();
				}
			}
		}

		protected bool JR_AT_SellGSTRate_ReadOnly => IsSellGSTFieldReadOnly;

		protected void UpdateJR_AT_SellGSTRateReadOnly()
		{
			if (!PreventReadOnlyFromChangingValues && !IsSellGSTApplicable)
			{
				JR_AT_SellGSTRate = ZGuid.Empty;
			}

			JR_AT_SellGSTRateInfo.RefreshBinding();
			JR_SellTaxDateInfo.RefreshBinding();
		}

		#endregion

		#region JR_SellTaxDate

		public override ZDate JR_SellTaxDate
		{
			get => base.JR_SellTaxDate;
			set
			{
				base.JR_SellTaxDate = value;
				JR_Calc_OSSellAmtWithGSTInfo.RefreshBinding();
			}
		}

		protected virtual bool JR_SellTaxDate_ReadOnly => IsSellGSTFieldReadOnly;

		public void SetSellTaxDateSafe(ZDate value) => JR_SellTaxDate = (JR_AT_SellGSTRate.IsValid ? value : ZDate.Empty);

		#endregion

		#region JR_AW_SellWHTRate

		[List("WHTCollection")]
		public override ZGuid JR_AW_SellWHTRate
		{
			get { return base.JR_AW_SellWHTRate; }
			set
			{
				if (JR_AW_SellWHTRate != value)
				{
					base.JR_AW_SellWHTRate = value;
					UpdateOsSellWHTAmount();
				}
			}
		}

		#endregion

		#region JR_PaymentType

		[List("PaymentTypes")]
		public override ZString JR_PaymentType
		{
			get { return base.JR_PaymentType; }
			set { base.JR_PaymentType = value; }
		}

		#endregion

		#region JR_PaymentDate

		public override ZDateTime JR_PaymentDate
		{
			get => base.JR_PaymentDate;
			set
			{
				if (JR_PaymentDate != value)
				{
					var oldValue = base.JR_PaymentDate;
					base.JR_PaymentDate = value;

					if (IsInDatabase)
					{
						AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo((NoResString)"Payment Date", ParentConsolCost?.E6_PaymentDate, JR_PaymentDate, oldValue);
					}
				}
			}
		}

		#endregion

		#region JR_AK

		[List("ChequeBooks")]
		public override ZGuid JR_AK
		{
			get { return base.JR_AK; }
			set { base.JR_AK = value; }
		}

		#endregion

		#region JR_AC

		public override ZGuid JR_AC
		{
			get { return base.JR_AC; }
			set
			{
				if (JR_AC != value)
				{
					base.JR_AC = value;

					if (ChargeCode == null)
					{
						JR_Desc = ZString.Empty;
					}
					else
					{
						SetChargeBranch();
						SetChargeDepartment();
						SetChargeDebtor();
						SetChargeCreditor();
						SetInitialChargeDescription();
						SetSupplyType();

						if (!IsUsedByConsolCostImporter)
						{
							SetProfitShareIncludedFlag();
						}

						DefaultCostPlaceOfSupply();
						DefaultSellPlaceOfSupply();
					}
					if (GlbCompany.CurrentCompany.GC_IsGSTRegistered)
					{
						SetGSTandWHTRates();
					}
					SetGovtChargeCode();
				}
			}
		}

		protected class UpdateCreditorFunctionalitySuspender : ServiceContainerSuspenderHelper.FunctionalitySuspenderService
		{
		}

		public IDisposable GetSuspenderForConsolCostImporter()
		{
			return new SuspenderForConsolCostImporter(this);
		}

		bool IsUsedByConsolCostImporter;

		class SuspenderForConsolCostImporter : IDisposable
		{
			public SuspenderForConsolCostImporter(BaseCharge charge)
			{
				this.charge = charge;
				charge.IsUsedByConsolCostImporter = true;
			}

			readonly BaseCharge charge;

			public void Dispose()
			{
				charge.IsUsedByConsolCostImporter = false;
			}
		}

		public void SetProfitShareIncludedFlag()
		{
			if (InvoicingJob != null && InvoicingJob.ProfitShareAgreement != null && ChargeCode != null)
			{
				JR_IsIncludedInProfitShare = InvoicingJob.ProfitShareAgreement.IsChargeGroupProfitShared(ChargeCode, InvoicingJob.PaymentTerm);
			}
		}

		public IDisposable GetSuspenderForCreateProfitShareCharges()
		{
			return new SuspenderForCreateProfitShareCharges(this);
		}

		public bool IsCreateProfitShareCharges
		{
			get => isCreateProfitShareCharges;
		}

		bool isCreateProfitShareCharges;

		class SuspenderForCreateProfitShareCharges : IDisposable
		{
			public SuspenderForCreateProfitShareCharges(BaseCharge charge)
			{
				this.charge = charge;
				charge.isCreateProfitShareCharges = true;
			}

			readonly BaseCharge charge;

			public void Dispose()
			{
				charge.isCreateProfitShareCharges = false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public void SetInitialChargeDescription()
		{
			if (ChargeCode != null)
			{
				JR_Desc = ChargeCode.GetLocalLanguageDescription(IsLocalClient)
					?? ChargeCode.AC_Desc;
			}
			else
			{
				JR_Desc = ZString.Empty;
			}
		}

		// OneOffQuote is based on ClientDocAddress, otherwise Debtor
		protected bool IsLocalClient => Job?.Parent is QuotedBooking quotedBooking && quotedBooking.IsOneOffQuote
			? quotedBooking.ClientDocAddress?.Organisation?.IsLocalClosestPort ?? false
			: IsDebtorInSameCountry;

		void SetChargeBranch()
		{
			if (ChargeCode != null && InvoicingJob != null)
			{
				var job = InvoicingJob;
				job.InitializeParentFromGenericJobWithSettingDefaults();
				var branchToOverride = ChargeCode.GetOverriddenBranch(job.JobType, job.Direction, job.TransportMode,
					defaultingRule => job.PlugInData == null ? null : job.PlugInData.InvoicingSupporter.GetOrganisationByBranchDefaultingRule(defaultingRule));
				if (branchToOverride != null)
				{
					JR_GB = branchToOverride.PK;
				}
			}
		}

		void SetSupplyType()
		{
			var supplyType = InvoicingJob?.GetSupplyType(ChargeCode, ChargeType, JR_GE) ?? ZString.Empty;

			if (!supplyType.IsEmpty)
			{
				JR_CostSupplyType = supplyType;
				JR_SellSupplyType = supplyType;
			}
		}

		void SetChargeDepartment()
		{
			GlbDepartment dept = DepartmentFromJobTypeAndChargeCodeType;
			JR_GE = dept != null ? dept.PK : ZGuid.Empty;
		}

		void SetGSTandWHTRates()
		{
			SetCostGSTRates();
			SetSellGSTRates();
			if (!JR_IsApportioned)
			{
				JR_AW_CostWHTRate = CostWHTId;
				UpdateJR_AW_CostWHTRateReadOnly();
			}

			if (!IsRevenuePosted)
			{
				JR_AW_SellWHTRate = LocalWHTId;
			}
		}

		void SetCostGSTRates()
		{
			if (UpdateCostTaxInfoSuspender.IsSuspended)
			{
				return;
			}

			if (!JR_IsApportioned)
			{
				JR_AT_CostGSTRate = Job != null ? Job.GetGSTID(this, CostSell.Cost, CostPlaceOfSupplyLocation, out _) : ZGuid.Empty;
				UpdateJR_AT_CostGSTRateReadOnly();
			}
		}

		void SetSellGSTRates()
		{
			if (!IsRevenuePosted)
			{
				SetSellGSTTaxDefault();
			}
		}

		#region SetChargeDebtor

		void SetChargeDebtor()
		{
			if (!JR_OH_SellAccount.IsValid)
			{
				ResetChargeDebtor();
			}
		}

		internal bool IsValidDebtorForDefaulting(ZGuid debtorPK)
		{
			if (debtorPK.IsEmpty)
			{
				return false;
			}

			var shouldNotSetInvalidDebtor = OrgNotDebtorCheckEnabled && !AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.Value;
			var shouldNotSetInvalidOrInactiveDebtor = Factory.HasAnyOfContexts(BusinessContextSets.GetDoNotDefaultInvalidOrInactiveDebtorSet());
			var shouldNotSetOrgProxyAsDebtorDueToAutoJobRevenueJournalRestrictions = AutoJRJRegistryStatusHelper.IsAutoJRJEnabled() && CostAccountIsOrgProxy;

			if (shouldNotSetInvalidDebtor || shouldNotSetInvalidOrInactiveDebtor || shouldNotSetOrgProxyAsDebtorDueToAutoJobRevenueJournalRestrictions)
			{
				var org = Factory.Load<OrgHeader>(debtorPK);
				if (shouldNotSetInvalidDebtor && !org.OH_IsDebtor)
				{
					return false;
				}

				if (shouldNotSetInvalidOrInactiveDebtor && (!org.OH_IsDebtor || org.IsCancelled))
				{
					return false;
				}

				if (shouldNotSetOrgProxyAsDebtorDueToAutoJobRevenueJournalRestrictions && org.IsProxyOrg(CalculatedCompany))
				{
					return false;
				}
			}

			return true;
		}

		public void ResetChargeDebtor()
		{
			if (InvoicingJob != null && ChargeCode != null)
			{
				var debtorPK = InvoicingJob.GetDebtorPK(this);
				JR_OH_SellAccount = IsValidDebtorForDefaulting(debtorPK) ? debtorPK : ZGuid.Empty;
			}
		}

		#endregion

		#region SetChargeCreditor

		void SetChargeCreditor()
		{
			if (JR_OH_CostAccount.IsEmpty)
			{
				if (!ServiceContainerSuspenderHelper.FunctionalitySuspender<UpdateCreditorFunctionalitySuspender>.IsSuspended(Factory))
				{
					if (!SetCreditorFromOverrideWithoutSuspendCheck())
					{
						var creditorPK = InvoicingJob?.GetCreditorPK(ChargeCode, ChargeType, JR_InvoiceType, ZGuid.Empty) ?? ZGuid.Empty;
						JR_OH_CostAccount = IsValidCreditorForDefaulting(creditorPK) ? creditorPK : ZGuid.Empty;
					}
				}
			}
		}

		void SetCreditorFromOverride()
		{
			if (!ServiceContainerSuspenderHelper.FunctionalitySuspender<UpdateCreditorFunctionalitySuspender>.IsSuspended(Factory))
			{
				SetCreditorFromOverrideWithoutSuspendCheck();
			}
		}

		bool SetCreditorFromOverrideWithoutSuspendCheck()
		{
			var chargeCode = ChargeCode;
			if (chargeCode != null)
			{
				var invJob = InvoicingJob;
				if (invJob != null)
				{
					var creditorOverride = chargeCode.CreditorOverrides.GetCreditorOverride(
											invJob.PlugInData?.InvoicingSupporter,
											CostSell.Cost,
											invJob.JobType,
											invJob.Direction,
											invJob.TransportMode,
											JR_GE,
											invJob.OverseasAgentIsApplicable,
											JobHeaderHelper.GetOverseasCreditorPK(invJob));
					if (creditorOverride != null)
					{
						JR_OH_CostAccount = creditorOverride.Value;
						return true;
					}
				}
			}
			return false;
		}

		protected bool IsValidCreditorForDefaulting(ZGuid creditorPK)
		{
			if (!creditorPK.IsValid)
			{
				return false;
			}

			var shouldNotSetInvalidOrInactiveCreditor = Factory.HasAnyOfContexts(BusinessContextSets.GetDoNotDefaultInvalidCreditorSet());

			if (shouldNotSetInvalidOrInactiveCreditor)
			{
				var org = Factory.Load<OrgHeader>(creditorPK);
				if (!(org?.OH_IsCreditor ?? false))
				{
					return false;
				}
			}

			return true;
		}
		#endregion

		#region SetGovtChargeCode

		void SetGovtChargeCode()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				return;
			}

			if (ParentConsolCost == null)
			{
				if (InvoicingJob == null)
				{
					var govtChargeCode = ChargeCode?.AC_GovtChargeCode ?? ZString.Empty;
					JR_CostGovtChargeCode = govtChargeCode;
					JR_SellGovtChargeCode = govtChargeCode;
				}
				else
				{
					JR_CostGovtChargeCode = ChargeCode?.GetFallbackGovtChargeCode(InvoicingJob.GetConfigurationMatcherParameters(CostSell.Cost)) ?? ZString.Empty;
					JR_SellGovtChargeCode = ChargeCode?.GetFallbackGovtChargeCode(InvoicingJob.GetConfigurationMatcherParameters(CostSell.Revenue)) ?? ZString.Empty;
				}
			}
			else
			{
				var isApplyingParentGovtChargeCode = IsApplyingParentGovtChargeCode;
				using (new DisposableAction(
					() => IsApplyingParentGovtChargeCode = true,
					() => IsApplyingParentGovtChargeCode = isApplyingParentGovtChargeCode
				))
				{
#if DEBUG
					if (Globals.IsTest)
					{
						InvokeMethodWhenSetGovtChargeCodeWithConsolCost_ForTestOnly?.Invoke();
					}
#endif
					JR_CostGovtChargeCode = ParentConsolCost.E6_CostGovtChargeCode;
					JR_SellGovtChargeCode = ParentConsolCost.E6_SellGovtChargeCode;
				}
			}
		}

		public bool IsApplyingParentGovtChargeCode { get; private set; }

#if DEBUG
		internal Action InvokeMethodWhenSetGovtChargeCodeWithConsolCost_ForTestOnly;
#endif

		#endregion

		static GlbDepartment CalculateDepartmentFromDepartmentFilterlist(BusinessObjectFactory factory, GlbDepartment department, AccChargeCode chargeCode)
		{
			var filteredDepartment = department;
			var deptFilterList = chargeCode.AC_DepartmentFilterList;
			if (!deptFilterList.IsEmpty)
			{
				if (((deptFilterList.Split(',').Length == 1) &&
					(deptFilterList != "ALL")))
				{
					filteredDepartment = factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, deptFilterList));
				}
			}
			return filteredDepartment;
		}

		public static GlbDepartment CalculateDepartmentFromJobTypeAndChargeCodeType(
			BusinessObjectFactory factory,
			Job invoicingJob,
			AccChargeCode chargeCode,
			GlbDepartment currentDepartment)
		{
			var department = currentDepartment;

			if (chargeCode != null && department != null)
			{
				if (chargeCode.IsCustomsCharge
					&& (CalculateIsQuoteBookingJob(factory, invoicingJob) || CalculateIsFreightShipmentJob(factory, invoicingJob)))
				{
					string mappedDepartmentCode = CachedDepartmentMappings(factory).GetMapping(department.GE_Code);
					var mappedDepartment = factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, mappedDepartmentCode));

					if (mappedDepartment != null && mappedDepartment.GE_IsActive)
					{
						department = mappedDepartment;
					}
				}
				else
				{
					department = CalculateDepartmentFromDepartmentFilterlist(factory, department, chargeCode);
				}
			}

			return department;
		}

		GlbDepartment DepartmentFromJobTypeAndChargeCodeType
			=> CalculateDepartmentFromJobTypeAndChargeCodeType(Factory, InvoicingJob, ChargeCode, Department);

		static DepartmentMappingCollection CachedDepartmentMappings(BusinessObjectFactory factory)
		{
			var companyPK = Env.CurrentCompanyPK;
			return factory.GetCachedValue("InvoicingDepartmentMapping" + companyPK, () =>
				new DepartmentMappingCollection(AccountingConfigurationRegistry.Instance.InvoicingDepartmentMapping.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty)));
		}

		static bool CalculateIsFreightShipmentJob(BusinessObjectFactory factory, Job invoicingJob)
		{
			bool result = false;

			if (invoicingJob != null && invoicingJob.IsFreight)
			{
				CommonShipment shipment = factory.Load<CommonShipment>(invoicingJob.JH_ParentID);
				result = shipment != null && shipment.JS_IsForwardRegistered;
			}

			return result;
		}

		static bool CalculateIsQuoteBookingJob(BusinessObjectFactory factory, Job invoicingJob)
		{
			bool result = false;

			if (invoicingJob != null)
			{
				ViewQuotedBooking quotedBooking = factory.Load<ViewQuotedBooking>(invoicingJob.JH_ParentID);
				result = quotedBooking != null;
			}

			return result;
		}

		public bool IsDebtorInSameCountry => SellAccount != null && SellAccount.IsLocalClosestPort;

		public bool ShouldDefaultLocalChargeDescription => ChargeCode != null
			&& ChargeCode.ShouldDefaultLocalChargeDescription(IsDebtorInSameCountry);

		public bool IsFreight
		{
			get { return ChargeCode != null && ChargeCode.IsFreight; }
		}

		public bool IsFOBCharge
		{
			get { return ChargeCode != null && ChargeCode.IsFOBCharge; }
		}

		#region Deferred Charges

		public ZBool HasDeferredConfiguration
		{
			get
			{
				ZBool result = false;

				var sellAccount = SellAccount;
				var invoicingJob = sellAccount != null ? InvoicingJob : null;
				var chargeCode = invoicingJob != null ? ChargeCode : null;
				var companyData = sellAccount?.CompanyData;
				if (chargeCode != null && companyData != null)
				{
					var invoiceTypes = companyData.GetApplicableInvoiceTypes(invoicingJob.TransportMode, invoicingJob.ServiceDirection, invoicingJob.ServiceLevel, true, invoicingJob.JobType?.Code ?? string.Empty);

					if (invoiceTypes != null && invoiceTypes.Count > 0)
					{
						OrgInvoiceType type = invoiceTypes.Values.First();

						if (type.DeferredCharges.Count == 0 || type.PI_Calc_IsInclude == InvoiceTypeChargeInclusionTypeList.Codes.ALL)
						{
							result = true; // all charges are deferred if no specific charge codes / groups are indicated
						}
						else
						{
							var filter = new ZQuery();
							filter.AddToFilter(OrgInvTypeDeferredChargesSchema.PO_AC, chargeCode.PK);
							filter.AddToFilter(JoinCondition.Or, OrgInvTypeDeferredChargesSchema.PO_ChargeGroup, chargeCode.AC_ChargeGroup);
							var chargeFound = type.DeferredCharges.Find(filter).Length > 0;

							result = type.PI_Calc_IsInclude == InvoiceTypeChargeInclusionTypeList.Codes.INC ? chargeFound : !chargeFound;
						}
					}
				}

				return result;
			}
		}

		public ZBool IsDeferredCharge
		{
			get { return InvoiceTypeCalculationProvider.IsDeferredInvoiceType(JR_InvoiceType); }
		}

		public ZBool IsDisbursementInvoiceCharge
		{
			get { return InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(JR_InvoiceType); }
		}

		#endregion

		#endregion

		#region Charge Group

		public ZString ChargeGroup
		{
			get { return ChargeCode?.AC_ChargeGroup ?? ZString.Empty; }
		}

		public ZPropertyInfo ChargeGroupInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroup); }
		}

		public ZString ChargeCodeSubGroup
		{
			get { return ChargeCode?.AC_ChargeSubGroup ?? ZString.Empty; }
		}

		public ZPropertyInfo ChargeCodeSubGroupInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodeSubGroup); }
		}

		#endregion

		public override ZGuid JR_JH
		{
			get { return base.JR_JH; }
			set
			{
				base.JR_JH = value;
				SetChargeBranch();
			}
		}

		#region JR_CFXAmt

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_CFXAmt
		{
			get
			{
				if (IsCFXPosted)
				{
					return -CFXLine.AL_LineAmount;
				}

				if (!IsApplyCFX || JR_LineCFX == 0m)
				{
					return ZDecimal.Zero;
				}

				return CalculateCFXAmt();
			}
		}

		public ZPropertyInfo JR_CFXAmtInfo
		{
			get { return GetZPropertyInfo(Schema.JR_CFXAmt); }
		}

		protected internal abstract ZDecimal CalculateCFXAmt();

		#endregion

		#region JR_RL_NKOrigin

		[ResourceStringData("a68cf980-2c45-49d0-a18e-439c298f5600", Caption = "Origin")]
		public ZString JR_RL_NKOrigin => ShipmentInfo?.InvoicingSupporter?.Origin?.RL_Code ?? ZString.Empty;

		public ZPropertyInfo JR_RL_NKOriginInfo => GetZPropertyInfo(Schema.JR_RL_NKOrigin);

		#endregion

		#region JR_RL_NKDestination

		[ResourceStringData("32e9277b-3ca1-49b7-80d7-e9bb155cadad", Caption = "Destination", ShortCaption = "Dest.")]
		public ZString JR_RL_NKDestination => ShipmentInfo?.InvoicingSupporter?.Destination?.RL_Code ?? ZString.Empty;

		public ZPropertyInfo JR_RL_NKDestinationInfo => GetZPropertyInfo(Schema.JR_RL_NKDestination);

		#endregion

		#region RevenueRecognitionType

		public ZString CostRecognition
		{
			get
			{
				if (APLine == null)
				{
					return InvoicingJob == null ? string.Empty : InvoicingJob.GetRevenueRecognitionType(ChargeCode);
				}
				else
				{
					return APLine.AL_RevRecognitionType;
				}
			}
		}

		public ZPropertyInfo CostRecognitionInfo
		{
			get { return GetZPropertyInfo(Schema.CostRecognition); }
		}

		public bool IsCostRecognized
		{
			get { return (IsCostPosted && !APLine.AL_ReverseDate.IsEmpty) || (Accrual != null && !ShouldReverseAccrual) || (!ShouldCreateAccrual && !ShouldCreateCostJRJ); }
		}

		public ZString SellRecognition
		{
			get
			{
				if (ARLine == null)
				{
					return InvoicingJob == null ? string.Empty : InvoicingJob.GetRevenueRecognitionType(ChargeCode);
				}
				else
				{
					return ARLine.AL_RevRecognitionType;
				}
			}
		}

		public ZPropertyInfo SellRecognitionInfo
		{
			get { return GetZPropertyInfo(Schema.SellRecognition); }
		}

		public bool IsSellRecognized
		{
			get { return (IsRevenuePosted && !ARLine.AL_ReverseDate.IsEmpty) || (WIP != null && !ShouldReverseWIP) || (!ShouldCreateWIP && !ShouldCreateSellJRJ); }
		}

		#endregion

		#endregion

		#region JR_OH_CostAccount

		[List("Creditors")]
		public override ZGuid JR_OH_CostAccount
		{
			get { return base.JR_OH_CostAccount; }
			set
			{
				var oldValue = JR_OH_CostAccount;
				if (oldValue != value)
				{
					base.JR_OH_CostAccount = value;

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OH_CostAccount_PropertyValueSet,
					() =>
					{
						return Invariant($@"OldValue: {oldValue}, NewValue: {value}
{System.Environment.StackTrace}");
					});

					if (InvoicingJob != null && !JR_IsApportioned && !IsCostPosted)
					{
						InvoicingJob.AddCurrency(CostCurrency, JR_OH_CostAccount, ExchangeRateValidLedgerEnum.AP);
					}

					if (!SellAccountIsOrgProxy)
					{
						InternalFieldsManager.ResetInternalFields(value);
					}

					if (!IsCreateProfitShareCharges && !JR_CostRated && !JR_E6.IsValid && !DefaultCostCurrencyFromCostAccount.IsEmpty && DefaultCostCurrencyFromCostAccount != JR_RX_NKCostCurrency && !Factory.HasContext(BusinessContext.ChargeProcessingForAPTransactionPosting))
					{
						JR_RX_NKCostCurrency = DefaultCostCurrencyFromCostAccount;
					}

					using (UpdateCostTaxInfoSuspender.GetSuspender())
					{
						SetCostTaxBranchDefault();
						UpdateCostExchangeRate();
						DefaultCostPlaceOfSupply();
					}

					UpdateCostGST(shouldCalculateWHT: true);

					if (JR_IsApportioned)
					{
						CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ChargeSetsCreditorDifferentToConsolCost,
							() => ParentConsolCost != null && ParentConsolCost.E6_OH_Creditor != JR_OH_CostAccount ? Invariant(
$@"Charge account '{JR_OH_CostAccount}' Consol Cost account '{ParentConsolCost.E6_OH_Creditor}'.
Call stack:
{new StackTrace().ToString()}")
							: null);
					}
				}
			}
		}

		#endregion

		#region JR_CostReference

		public override ZString JR_CostReference
		{
			get
			{
				return base.JR_CostReference;
			}
			set
			{
				var oldValue = base.JR_CostReference;
				bool hasChanged = JR_CostReference != value;
				if (hasChanged)
				{
					base.JR_CostReference = value;
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_CostReference_PropertyValueSet,
					() =>
					{
						return Invariant($@"OldValue: {oldValue}, NewValue: {value}
{System.Environment.StackTrace}");
					});

					AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo<ZString>((NoResString)"Cost Reference", ReplaceEmptyStringWithQuotes(ParentConsolCost?.E6_CostReference ?? ZString.Empty), ReplaceEmptyStringWithQuotes(JR_CostReference), ReplaceEmptyStringWithQuotes(oldValue));
				}
			}
		}

		#endregion

		#region JR_APInvoiceDate

		public override ZDateTime JR_APInvoiceDate
		{
			get
			{
				return base.JR_APInvoiceDate;
			}
			set
			{
				var oldValue = base.JR_APInvoiceDate;
				bool hasChanged = JR_APInvoiceDate != value;
				base.JR_APInvoiceDate = value;

				if (hasChanged)
				{
					if (JR_E6.IsValid && JR_APInvoiceDate.IsValid && ParentConsolCost != null &&
					ParentConsolCost.E6_AH_APInvoice.IsValid && ParentConsolCost.E6_InvoiceDate.IsValid &&
					JR_APInvoiceDate.ToSmallDateTimeFloor() != ParentConsolCost.E6_InvoiceDate.ToSmallDateTimeFloor())
					{
						ReportJR_APInvoiceDateIsNotEqualE6_InvoiceDate();
					}

					AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo((NoResString)"Invoice Date", ParentConsolCost?.E6_InvoiceDate, JR_APInvoiceDate, oldValue);
				}
			}
		}

		void ReportJR_APInvoiceDateIsNotEqualE6_InvoiceDate()
		{
			var message = string.Format(CultureInfo.InvariantCulture, (NoResString)"AP Invoice Date on Apportionment Charge was set to a different value '{0}' than Invoice Date '{1}' on its posted parent Consol Cost.\r\nThis error is related to the Issue 00866041.\r\nParentConsolCost={2}.\r\nCharge with incorrect data={3}.",
				JR_APInvoiceDate, ParentConsolCost.E6_InvoiceDate, ParentConsolCost.GetJobConsolCostInfo(), this.GetJobChargeInfo());

			var key = "APInvoiceDateOnAppChargeSetToDifferentValueThanOnPostedParentConsolCost";
			ErrorReporter.ReportOnce(key, message);
		}

		#endregion

		#region PaymentBases

		public void AddPaymentBases(IEnumerable<PaymentBasis> rateInfoBases, bool isCost)
		{
			if (HasChanges || !IsInDatabase)
			{
				RemoveOldBases(isCost);
				rateInfoBases.ConvertToJobPaymentBases(isCost, PaymentBases.AddNew);
			}
		}

		void RemoveOldBases(bool isCost)
		{
			if (isCost)
			{
				foreach (var costBasis in CostPaymentBases.Where(x => x.PBS_JR.IsValid).ToArray())
				{
					costBasis.Delete();
				}
			}
			else
			{
				foreach (var sellBasis in SellPaymentBases.Where(x => x.PBS_JR.IsValid).ToArray())
				{
					sellBasis.Delete();
				}
			}
		}

		[ChildEditable(false)]
		public ChargePaymentBasisCollection PaymentBases
		{
			get
			{
				if (paymentBases == null)
				{
					paymentBases = new ChargePaymentBasisCollection(this);
					paymentBases.Load();
					RegisterEditableChildObject(paymentBases);
				}

				return paymentBases;
			}
		}
		ChargePaymentBasisCollection paymentBases;

		public IReadOnlyCollection<JobPaymentBasis> CostPaymentBases
		{
			get
			{
				if (JR_E6.IsValid && ParentConsolCost != null)
				{
					return ParentConsolCost.PaymentBases.Cast<JobPaymentBasis>().ToList();
				}

				return PaymentBases.Cast<JobPaymentBasis>().Where(x => x.PBS_IsCost).ToList();
			}
		}

		public IReadOnlyCollection<JobPaymentBasis> SellPaymentBases => PaymentBases.Cast<JobPaymentBasis>().Where(x => !x.PBS_IsCost).ToList();

		#endregion

		#region Was Auto Rated for this OperationalJobCode

		public override bool CanReautorate(CostSell costOrSell, params ZString[] operationalJobCodes)
		{
			var isCost = costOrSell == CostSell.Cost;

			var ratingBehavior = isCost ? JR_Calc_CostRatingBehavior : JR_Calc_SellRatingBehavior;
			if (ratingBehavior != JobChargeLookups.ReAutorateCharge)
			{
				return false;
			}

			var hasPaymentBases = isCost ? CostPaymentBases.Any() : SellPaymentBases.Any();
			if (!hasPaymentBases)
			{
				return true;
			}

			if (!operationalJobCodes.Any() || operationalJobCodes.All(x => x.IsEmpty))
			{
				return true;
			}

			return isCost ? WasCostAutoRated(operationalJobCodes) : WasSellAutoRated(operationalJobCodes);
		}

		bool WasCostAutoRated(params ZString[] operationalJobCodes)
		{
			var result = CostPaymentBases.Any(cbs => operationalJobCodes.Contains(cbs.PBS_AdapterID));
			if (!result)
			{
				result = CostPaymentBases.Any(cbs => cbs.PBS_AdapterID == JR_CostReference);
			}
			if (!result && !CostPaymentBases.Any())
			{
				result = SellPaymentBases.Any(sbs => operationalJobCodes.Contains(sbs.PBS_AdapterID));
			}

			return result;
		}

		bool WasSellAutoRated(params ZString[] operationalJobCodes)
		{
			var result = SellPaymentBases.Any(sbs => operationalJobCodes.Contains(sbs.PBS_AdapterID));
			if (!result && !SellPaymentBases.Any())
			{
				result = CostPaymentBases.Any(cbs => operationalJobCodes.Contains(cbs.PBS_AdapterID));
			}

			return result;
		}

		#endregion

		#region Light Validation

		protected sealed override bool EnableLightValidationIfAvailable
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value;
			}
		}

		#endregion

		internal protected bool LocalSellAmtOverridenAndUserNeedToCheck;

		internal protected ZDecimal LocalSellAmtPreviousValue;

		public new void CancelChanges()
		{
			var potentialProblemProperties = new List<string>();
			ZString chargeInfoBeforeCancelChanges = ZString.Empty;

			var setOfForeignKeyColumnsAlreadyFixed = ForeignKeyColumnsAlreadyFixed;
			if (this.HasContext(BusinessContext.DeletingConsolCost))
			{
				setOfForeignKeyColumnsAlreadyFixed.Add(JobChargeSchema.JR_E6.Name);
			}

			var foreignKeyPropertiesWithChanges = ZPropertyInfoHash.Cast<ZPropertyInfo>().Where(info => info.PropertyType == typeof(ZGuid) && info.IsPersistent && info.HasChanges);
			foreach (var property in foreignKeyPropertiesWithChanges)
			{
				if (!setOfForeignKeyColumnsAlreadyFixed.Contains(property.Name) && ((ZGuid)property.Value).IsValid)
				{
					var bizObjs = Factory.GetBizOsForPK(((ZGuid)property.Value).ToGuid()).Where(x => !x.IsInDatabase);
					if (bizObjs.Any())
					{
						if (potentialProblemProperties.Count == 0)
						{
							chargeInfoBeforeCancelChanges = this.GetJobChargeInfo();
						}

						potentialProblemProperties.Add(property.Name);
					}
				}
			}

			var orphanedWIP = WIP;
			var orphanedACR = Accrual;

			base.CancelChanges();

			if (potentialProblemProperties.Count > 0)
			{
				potentialProblemProperties.Sort((a, b) => b.CompareTo(a));

				var key = string.Format(CultureInfo.InvariantCulture, "Charge_CancelChanges_PotentiallyOrphanedObject_{0}", potentialProblemProperties[0]);
				var message = string.Format(CultureInfo.InvariantCulture, (NoResString)@"A foreign key business object has been created and might become orphaned after cancelling changes on charge. 
Please take appropriate steps to prevent this causing a critical validation error and update ForeignKeyColumnsAlreadyFixed list after your changes.
Job charge foreign key column : {0}
Charge info before cancelling changes : {1}
Charge info after cancelling changes : {2}", string.Join(", ", potentialProblemProperties), chargeInfoBeforeCancelChanges, this.GetJobChargeInfo());
				ErrorReporter.ReportOnce(key, message);
			}

			if (orphanedWIP != null && !orphanedWIP.IsInDatabase && !orphanedWIP.IsDeleted)
			{
				orphanedWIP.Delete();
			}
			if (WIP != null)
			{
				WIP.CancelChanges();
			}

			if (orphanedACR != null && !orphanedACR.IsInDatabase && !orphanedACR.IsDeleted)
			{
				orphanedACR.Delete();
			}
			if (Accrual != null)
			{
				Accrual.CancelChanges();
			}
		}

		static HashSet<ZString> ForeignKeyColumnsAlreadyFixed
		{
			get
			{
				return new HashSet<ZString>() { JobChargeSchema.JR_AL_APLine.Name, JobChargeSchema.JR_AL_ARLine.Name };
			}
		}

		protected bool IsMainAllFieldsReadonly
		{
			get { return IsRevenuePosted || IsInDatabaseAndReadyForRevenuePosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		protected bool IsBaseSellFieldsReadonly
		{
			get { return AccountingConfigurationRegistry.Instance.PreventOperatorFromChangingRatedLine.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK) && JR_SellRated; }
		}

		protected bool JR_OH_SellAccount_ReadOnly
		{
			get
			{
				return IsBaseSellFieldsReadonly && !JR_OH_SellAccount.IsEmpty && JR_OH_SellAccount.IsValid ||
						 IsMainAllFieldsReadonly;
			}
		}

		[List("Debtors")]
		public override ZGuid JR_OH_SellAccount
		{
			get { return base.JR_OH_SellAccount; }
			set
			{
				var oldValue = JR_OH_SellAccount;
				bool hasChanged = oldValue != value;

				base.JR_OH_SellAccount = value;

				if (hasChanged)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OH_SellAccount_PropertyValueSet,
					() =>
					{
						return Invariant($@"OldValue: {oldValue}, NewValue: {value}
{System.Environment.StackTrace}");
					});

					if (InvoicingJob != null && !IsRevenuePosted)
					{
						var invoiceCurrencyType = InvoiceCurrencyTypeForAR;
						InvoicingJob.AddCurrency(SellInvoiceCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
						InvoicingJob.AddCurrency(SellCurrency, JR_OH_SellAccount, ExchangeRateValidLedgerEnum.AR, invoiceCurrencyType);
					}

					if (!CostAccountIsOrgProxy)
					{
						InternalFieldsManager.ResetInternalFields(value, true);
					}
					if (!IsCreateProfitShareCharges && !JR_SellRated && !DefaultSellCurrencyFromSellAccount.IsEmpty && DefaultSellCurrencyFromSellAccount != JR_RX_NKSellCurrency)
					{
						JR_RX_NKSellCurrency = DefaultSellCurrencyFromSellAccount;
					}
					UpdateRevenueExchangeRate();

					if (!SetUpdateInvoiceTargetSuspender.IsSuspended)
					{
						UpdateInvoiceTarget();
					}

					displaySellInvoiceAddress_ZAddress = null;

					UpdateSellInvoiceCurrency();

					DefaultSellPlaceOfSupply();
				}

				SetSellTaxBranchDefault();
				ResetSellGSTTaxDefault();
				RunSellAccountCalculations();
				InvoiceTypeCalculator.UpdateInvoiceType();
			}
		}

		internal ZString DefaultSellCurrencyFromSellAccount
		{
			get
			{
				return SellAccount != null && !SellAccount.CompanyData.OB_RX_NKARDDefltCurrency.IsEmpty ? SellAccount.CompanyData.OB_RX_NKARDDefltCurrency : ZString.Empty;
			}
		}

		internal ZString DefaultCostCurrencyFromCostAccount
		{
			get
			{
				return CostAccount != null && !CostAccount.CompanyData.OB_RX_NKAPDefltCurrency.IsEmpty ? CostAccount.CompanyData.OB_RX_NKAPDefltCurrency : ZString.Empty;
			}
		}

		protected virtual void RunSellAccountCalculations()
		{
		}

		public void ResetSellGSTTaxDefault()
		{
			if (!IsRevenuePosted)
			{
				SetSellGSTTaxDefault();
				UpdateJR_AT_SellGSTRateReadOnly();
			}
		}

		void SetSellGSTTaxDefault()
		{
			JR_AT_SellGSTRate = Job != null ? Job.GetGSTID(this, CostSell.Revenue, SellPlaceOfSupplyLocation, out _) : ZGuid.Empty;
		}

		void SetSellTaxInvoiceMessage(ZGuid overrideSellTaxRate, ZGuid overrideSellInvTaxMsg)
		{
			if (JR_AT_SellGSTRate == ZGuid.Empty)
			{
				JR_A9_SellVATClass = ZGuid.Empty;
			}
			else
			{
				if (overrideSellTaxRate == JR_AT_SellGSTRate && overrideSellInvTaxMsg != ZGuid.Empty)
				{
					JR_A9_SellVATClass = overrideSellInvTaxMsg;
				}
				else
				{
					JR_A9_SellVATClass = SellTaxRateInvoiceMessage;
				}
			}
		}

		protected void SetCostTaxRateAndMessage(bool skipRateSetting)
		{
			if (UpdateCostTaxInfoSuspender.IsSuspended)
			{
				return;
			}

			ZGuid overrideCostTaxRate = ZGuid.Empty;
			ZGuid overrideCostInvTaxMsg = ZGuid.Empty;
			if (Job != null)
			{
				overrideCostTaxRate = Job.GetGSTID(this, CostSell.Cost, CostPlaceOfSupplyLocation, out overrideCostInvTaxMsg);
			}
			if (!skipRateSetting)
			{
				using (UpdateCostTaxInfoSuspender.GetSuspender())
				{
					JR_AT_CostGSTRate = overrideCostTaxRate;
				}
			}
			if (JR_AT_CostGSTRate == ZGuid.Empty)
			{
				JR_A9_CostVATClass = ZGuid.Empty;
			}
			else
			{
				if (overrideCostTaxRate == JR_AT_CostGSTRate && overrideCostInvTaxMsg != ZGuid.Empty)
				{
					JR_A9_CostVATClass = overrideCostInvTaxMsg;
				}
				else
				{
					JR_A9_CostVATClass = CostGSTRate?.AT_A9_DefaultVatClass ?? ZGuid.Empty;
				}
			}
		}

		#region Places of Supply Defaults

		void DefaultCostPlaceOfSupply()
		{
			if (!JR_IsApportioned && !IsCostPosted)
			{
				(var posType, var posCode) = GetDefaultPlaceOfSupply(CostSell.Cost, CostAccount);
				if (JR_CostPlaceOfSupply != posCode)
				{
					if (!posType.IsEmpty)
					{
						JR_CostPlaceOfSupply = posCode;
					}
					else if (posCode.IsEmpty)
					{
						JR_CostPlaceOfSupply = posCode;
					}
				}
			}
		}

		void DefaultSellPlaceOfSupply()
		{
			if (!IsRevenuePosted)
			{
				(var posType, var posCode) = GetDefaultPlaceOfSupply(CostSell.Revenue, SellAccount);
				if (JR_SellPlaceOfSupply != posCode)
				{
					if (!posType.IsEmpty)
					{
						JR_SellPlaceOfSupply = posCode;
					}
					else if (posCode.IsEmpty)
					{
						JR_SellPlaceOfSupply = posCode;
					}
				}
			}
		}

		(ZString posType, ZString posCode) GetDefaultPlaceOfSupply(CostSell costSell, OrgHeader org)
		{
			var placeOfSupplyEnabled = Factory.GetCachedValue("BaseCharge.GetDefaultPlaceOfSupply" + JR_GC, () => PlaceOfSupplyHelper.IsPlaceOfSupplyEnabled(Company));

			if (placeOfSupplyEnabled && ChargeCode != null && org != null && InvoicingJob?.PlugInData != null)
			{
				return AccPlaceOfSupplyHelper.GetPlaceOfSupplyFromConfiguration(InvoicingJob.PlugInData, ChargeCode, costSell, org, costSell == CostSell.Cost ? JR_CostSupplyType : JR_SellSupplyType, Branch);
			}

			return (posType: ZString.Empty, posCode: ZString.Empty);
		}

		#endregion

		#region Invoice Type Calculator

		protected InvoiceTypeCalculator InvoiceTypeCalculator
		{
			get
			{
				if (fInvoiceTypeCalculator == null)
				{
					fInvoiceTypeCalculator = new InvoiceTypeCalculator(this);
				}
				return fInvoiceTypeCalculator;
			}
		}

		InvoiceTypeCalculator fInvoiceTypeCalculator;

		#endregion

		#region AutoRatingOverrideSuppressor

		internal IDisposable SuppressAutoRatingOverride()
		{
			return new RatingOverrideSuppressor(this);
		}

		class RatingOverrideSuppressor : IDisposable
		{
			public RatingOverrideSuppressor(BaseCharge charge)
			{
				this.charge = charge;
				charge.autoRatingOverrideSuppressCount++;
			}

			readonly BaseCharge charge;

			void IDisposable.Dispose()
			{
				charge.autoRatingOverrideSuppressCount--;
			}
		}

		protected ZBool AutoRatingOverrideSuppressed
		{
			get
			{
				if (autoRatingOverrideSuppressCount > 0)
				{
					return true;
				}

				var job = InvoicingJob;
				return job != null && job.AutoRatingOverrideSuppressed;
			}
		}
		int autoRatingOverrideSuppressCount;

		#endregion

		public bool OrgNotDebtorCheckEnabled { get; set; }

		#region JR_CostRatingOverride

		public override ZBool JR_CostRatingOverride
		{
			get { return base.JR_CostRatingOverride; }
			set
			{
				if (!AutoRatingOverrideSuppressed)
				{
					base.JR_CostRatingOverride = value;

					if (!value)
					{
						JR_CostRatingOverrideComment = ZString.Empty;
					}

					JR_CostRatingOverrideCommentInfo.RefreshBinding();
					Validation.ValidateJR_CostRatingOverrideComment();
				}
			}
		}

		public void SetCostRatingOverrideAlways(ZBool value) => base.JR_CostRatingOverride = value;

		public virtual bool JR_CostRatingOverride_ReadOnly
		{
			get { return IsInDatabaseAndReadyForCostPosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		#endregion

		#region JR_CostRatingOverrideComment

		protected virtual bool JR_CostRatingOverrideComment_ReadOnly
		{
			get { return !JR_CostRatingOverride; }
		}

		#endregion

		#region JR_SellRatingOverride

		public override ZBool JR_SellRatingOverride
		{
			get { return base.JR_SellRatingOverride; }
			set
			{
				if (!AutoRatingOverrideSuppressed)
				{
					base.JR_SellRatingOverride = value;

					if (!value)
					{
						JR_SellRatingOverrideComment = ZString.Empty;
					}

					JR_SellRatingOverrideCommentInfo.RefreshBinding();
					Validation.ValidateJR_SellRatingOverrideComment();
				}
			}
		}

		public void SetSellRatingOverrideAlways(ZBool value) => base.JR_SellRatingOverride = value;

		public virtual bool JR_SellRatingOverride_ReadOnly
		{
			get { return IsInDatabaseAndReadyForRevenuePosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		#endregion

		#region JR_SellRatingOverrideComment

		protected virtual bool JR_SellRatingOverrideComment_ReadOnly
		{
			get { return !JR_SellRatingOverride; }
		}

		#endregion

		#region JR_LocalSellAmt

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal JR_LocalSellAmt
		{
			get { return base.JR_LocalSellAmt; }
			set
			{
				var oldValue = JR_LocalSellAmt;
				base.JR_LocalSellAmt = value;
				UpdateOsSellWHTAmount();

				if (value != 0m && AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					MarkAsNeedingValidation();
				}

				if (this is ApportionSplitCharge && oldValue != JR_LocalSellAmt)
				{
					AddChangeInfoToCiriticalValidationCollectorIfRequires();
				}
			}
		}

		#endregion

		#region JR_OSSellAmt

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public override ZDecimal JR_OSSellAmt
		{
			get { return base.JR_OSSellAmt; }
			set
			{
				var oldValue = JR_OSSellAmt;
				
				var shouldUpdateAgentDeclaredSellAmount =
					(JR_AgentDeclaredSellAmt == 0m || JR_AgentDeclaredSellAmt == JR_OSSellAmt);
				bool hasRatedAmountChanged = base.JR_OSSellAmt != value;

				base.JR_OSSellAmt = value;
				SetAgentDeclaredSellAmountFromForeignAmount(value, shouldUpdateAgentDeclaredSellAmount);
				if (hasRatedAmountChanged)
				{
					MarkSellRatedAmountAsChanged();
				}

				UpdateOsSellWHTAmount();

				if (this is ApportionSplitCharge && oldValue != JR_OSSellAmt)
				{
					AddChangeInfoToCiriticalValidationCollectorIfRequires();
				}
			}
		}

		void SetAgentDeclaredSellAmountFromForeignAmount(ZDecimal newValue, bool shouldUpdate)
		{
			if (shouldUpdate)
			{
				JR_AgentDeclaredSellAmt = newValue;
			}
		}

		#endregion

		#region JR_OSCostAmtWithGSTAmt

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public ZDecimal JR_OSCostAmtWithGSTAmt
		{
			get { return JR_OSCostAmt + JR_OSCostGSTAmt_Calc; }
		}

		public ZPropertyInfo JR_OSCostAmtWithGSTAmtInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_OSCostAmtWithGSTAmt); }
		}

		#endregion

		#region JR_OSCostAmt

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override ZDecimal JR_OSCostAmt
		{
			get { return base.JR_OSCostAmt; }
			set
			{
				SetAgentDeclaredCostAmountFromForeignAmount(value);
				bool hasRatedAmountChanged = base.JR_OSCostAmt != value;

				base.JR_OSCostAmt = value;
				if (hasRatedAmountChanged)
				{
					MarkCostRatedAmountAsChanged();
				}
				if (!SetEstimatedCostSuspender.IsSuspended && !IsCostPosted && (JR_EstimatedCost.IsEmpty || !IsInDatabase))
				{
					SetEstimatedCost(JR_OSCostAmt);
				}

				UpdateOsCostGSTAmount();
				UpdateOsCostWHTAmount();
				JR_Calc_OSCostAmtWithGSTInfo.RefreshBinding();
				UpdateTotals();
			}
		}

		void SetAgentDeclaredCostAmountFromForeignAmount(ZDecimal newValue)
		{
			if (JR_AgentDeclaredCostAmt == 0m || JR_AgentDeclaredCostAmt == JR_OSCostAmt)
			{
				JR_AgentDeclaredCostAmt = newValue;
			}
		}

		#endregion

		#region JR_OSCostExRate

		[ReadOnly(true)]
		public sealed override ZDecimal JR_OSCostExRate
		{
			get { return base.JR_OSCostExRate; }
			set
			{
				using (SetEstimatedCostSuspender.GetSuspender())
				{
					SetJR_OSCostExRateChangedCore(value);
				}
				RecalculateEstimatedCostProportionally();
			}
		}

		protected virtual void SetJR_OSCostExRateChangedCore(ZDecimal value)
		{
			base.JR_OSCostExRate = value;
		}

		#endregion

		#region JR_APInvoiceNum

		public override ZString JR_APInvoiceNum
		{
			get { return base.JR_APInvoiceNum; }
			set
			{
				if (JR_APInvoiceNum != value)
				{
					var oldValue = base.JR_APInvoiceNum;
					base.JR_APInvoiceNum = value;
					AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo<ZString>((NoResString)"AP Invoice Number", ReplaceEmptyStringWithQuotes(ParentConsolCost?.E6_InvoiceNum ?? ZString.Empty), ReplaceEmptyStringWithQuotes(JR_APInvoiceNum), ReplaceEmptyStringWithQuotes(oldValue));
				}
			}
		}

		#endregion

		#region JR_OSCostGSTAmt_Calc

		[ReadOnly(true)]
		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override ZDecimal JR_OSCostGSTAmt_Calc
		{
			get { return GSTCalculationStrategy.GetJR_OSCostGSTAmt(); }
			set
			{
				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, JR_OSCostGSTAmt_Calc != value))
				{
					GSTCalculationStrategy.SetJR_OSCostGSTAmt(value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateJR_OSCostGSTAmt_Calc();
					}
					JR_Cost_LocalGSTAmountInfo.RefreshBinding();
				}
			}
		}

		JobChargeOSGSTCalculationStrategy GSTCalculationStrategy
		{
			get
			{
				JobChargeOSGSTCalculationStrategy gstCalculationStrategy = null;

				if (JR_IsCostTaxAmountOverridden)
				{
					gstCalculationStrategy = new JobChargeOSGSTCalculationWhenOverridden(this);
				}
				else
				{
					gstCalculationStrategy = new JobChargeOSGSTCalculationWhenNotOverridden(this);
				}

				return gstCalculationStrategy;
			}
		}

		public bool IsGSTAmountEqualToCalculatedGST => JR_OSCostGSTAmt_Calc == new JobChargeOSGSTCalculationWhenNotOverridden(this).GetJR_OSCostGSTAmt();

		#endregion

		#region JR_OSCostWHTAmt

		[ReadOnly(true)]
		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public override ZDecimal JR_OSCostWHTAmt
		{
			get { return base.JR_OSCostWHTAmt; }
			set
			{
				if (AccountingValuesRoundingHelper.PropertyHasChanges(this, JR_OSCostWHTAmt != AccountingUtils.Round(value, CostCurrency)))
				{
					base.JR_OSCostWHTAmt = AccountingUtils.Round(value, CostCurrency);
					JR_Cost_LocalWHTAmountInfo.RefreshBinding();
				}
			}
		}

		#endregion

		#region Calc Extra Tax Amounts

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Calc_LocalCostExtraTaxAmt
		{
			get { return TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalTaxAmount(Factory, JR_GC, JR_Cost_LocalGSTAmount, CostGSTRate, CostGSTRate?.GetRate(JR_CostTaxDate), CostGSTRate?.GetEffectiveExtraRate(JR_CostTaxDate)); }
		}

		#region JR_Calc_OSCostExtraTaxAmt

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public ZDecimal JR_Calc_OSCostExtraTaxAmt
		{
			get { return TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, JR_OSCostAmt, CostGSTRate, CostGSTRate?.GetEffectiveExtraRate(JR_CostTaxDate), CostCurrency, JR_GC); }
		}

		public ZPropertyInfo JR_Calc_OSCostExtraTaxAmtInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_OSCostExtraTaxAmt); }
		}

		#endregion

		#region JR_Calc_OSSellExtraTaxAmt

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public ZDecimal JR_Calc_OSSellExtraTaxAmt
		{
			get { return JR_Calc_LocalSellExtraTaxAmt.IsEmpty ? ZDecimal.Zero : TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, JR_OSSellAmt, SellGSTRate, SellGSTRate?.GetEffectiveExtraRate(JR_SellTaxDate), SellCurrency, JR_GC); }
		}

		public ZPropertyInfo JR_Calc_OSSellExtraTaxAmtInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_OSSellExtraTaxAmt); }
		}

		#endregion

		#region JR_Calc_LocalSellExtraTaxAmt

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Calc_LocalSellExtraTaxAmt
		{
			get
			{
				ZDecimal result = 0M;
				if (SellGSTRate != null)
				{
					var taxDate = JR_SellTaxDate;
					var rate = SellGSTRate.GetRate(taxDate);
					var effectiveExtraRate = SellGSTRate.GetEffectiveExtraRate(taxDate);
					var totalRate = effectiveExtraRate + rate;
					if (totalRate != 0)
					{
						var oSextra = JR_OSSellGSTAmt_Calc * effectiveExtraRate / totalRate;
						result = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, JR_GC, JR_Sell_LocalGSTAmount, SellGSTRate, rate, effectiveExtraRate, osExtraTaxAmount: oSextra, localExtraTaxAmount: ZDecimal.Zero, exchangeRate: JR_OSSellExRate);
					}
				}

				return result;
			}
		}

		public ZPropertyInfo JR_Calc_LocalSellExtraTaxAmtInfo
		{
			get { return GetZPropertyInfo(nameof(JR_Calc_LocalSellExtraTaxAmt)); }
		}

		#endregion

		#region JR_Calc_OSCostGSTAmt

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public ZDecimal JR_Calc_OSCostGSTAmt
		{
			get { return TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, JR_OSCostGSTAmt_Calc, CostGSTRate, CostGSTRate?.GetRate(JR_CostTaxDate), CostGSTRate?.GetEffectiveExtraRate(JR_CostTaxDate), CostCurrency, JR_GC); }
		}

		public ZPropertyInfo JR_Calc_OSCostGSTAmtInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_OSCostGSTAmt); }
		}

		#endregion

		#region JR_Calc_OSSellGSTAmt

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public ZDecimal JR_Calc_OSSellGSTAmt
		{
			get { return TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, JR_OSSellGSTAmt_Calc, SellGSTRate, SellGSTRate?.GetRate(JR_SellTaxDate), SellGSTRate?.GetEffectiveExtraRate(JR_SellTaxDate), SellCurrency, JR_GC); }
		}

		public ZPropertyInfo JR_Calc_OSSellGSTAmtInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_OSSellGSTAmt); }
		}

		#endregion

		#endregion

		#region JR_LocalCurrencyDecimals

		public ZInt JR_LocalCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		public ZPropertyInfo JR_LocalCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo(Schema.JR_LocalCurrencyDecimals); }
		}

		#endregion

		#region OS Cost Currency Code

		public ZString JR_OSCostCurrencyCode
		{
			get { return JR_RX_NKCostCurrency; }
		}

		public ZPropertyInfo JR_OSCostCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_OSCostCurrencyCode); }  // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region Cost

		#region JR_LocalCostAmt

		public override ZDecimal JR_LocalCostAmt
		{
			get { return base.JR_LocalCostAmt; }
			set
			{
				var oldValue = base.JR_LocalCostAmt;

				base.JR_LocalCostAmt = value;
				UpdateOsCostWHTAmount();

				if (value != 0m && AccountingConfigurationRegistry.Instance.EnableLightValidationForChargeAndConsolCost.Value)
				{
					MarkAsNeedingValidation();
				}

				if (oldValue != base.JR_LocalCostAmt)
				{
					AddLocalCostAmountChangeInfoToCiriticalValidationCollectorIfRequires(oldValue);
				}

				if (value == 0 && ParentConsolCost != null && ParentConsolCost.E6_OSCostAmount != 0)
				{
					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeLocalAmountSetToZero, () =>
					{
						return System.Environment.StackTrace;
					});
				}
			}
		}

		protected void AddLocalCostAmountChangeInfoToCiriticalValidationCollectorIfRequires(ZDecimal localCostAmountValueBeforeChange)
		{
			if (JR_IsApportioned && ParentConsolCost != null)
			{
				var charges = Factory.Load<ApportionSplitCharge>(new ApportionmentSplitChargeCollection(ParentConsolCost).CompleteFilter);

				var buildReportData = new Func<string>(() =>
				{
					var infoBuilder = new ZStringBuilder(FormattableString.Invariant($"Local Cost Amount is changed from {localCostAmountValueBeforeChange} to {JR_LocalCostAmt} of charge with PK: {PK}."));
					charges.ForEach(c => infoBuilder.Append(c.GetJobChargeInfo()));
					infoBuilder.Append((NoResString)"Stacktrace -->");
					infoBuilder.Append(new StackTrace(true).ToString());
					return infoBuilder.ToStringWithNewLineBetweenAppends();
				});

				if (ParentConsolCost.E6_LocalCostAmount != charges.Sum(x => x.JR_LocalCostAmt))
				{
					var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
					infoCollector.AddLastInfoWhenAllowed(ParentConsolCost.PK, CriticalValidationInfoCollectorServiceKeyType.LocalCostAmountChangedThatCausedUnApportionedAmount, buildReportData);
				}
			}

			if (Accrual != null && Accrual.AL_LineAmount != JR_LocalCostAmt)
			{
				var buildReportData = new Func<string>(() =>
				{
					var infoBuilder = new ZStringBuilder(FormattableString.Invariant($"Local Cost Amount is changed from {localCostAmountValueBeforeChange} to {JR_LocalCostAmt} of charge with PK: {PK}."));
					infoBuilder.Append(FormattableString.Invariant($"Accrual Line Amount: {Accrual.AL_LineAmount}"));
					infoBuilder.Append(this.GetJobChargeInfo());
					infoBuilder.Append((NoResString)"Stacktrace -->");
					infoBuilder.Append(new StackTrace(true).ToString());
					return infoBuilder.ToStringWithNewLineBetweenAppends();
				});

				var infoCollector = CriticalValidationInfoCollectorService.GetOrCreateService(Factory);
				infoCollector.AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalCostAmtNotEqualRelatedAccrualAmount, buildReportData);
			}
		}

		#endregion

		#region JR_Cost_LocalGSTAmount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Cost_LocalGSTAmount
		{
			get
			{
				return Utilities.Round(JR_Cost_LocalGSTAmountHighPrecision, GlbCompany.CurrentCompany.LocalCurrency.Decimals);
			}
		}

		ZDecimal JR_Cost_LocalGSTAmountHighPrecision
		{
			get
			{
				var osTaxAmountFromExTaxAmount =
							TaxAmountCalculator.GetOSTaxAmount(Factory, JR_OSCostAmt, CostGSTRate, CostGSTRate?.GetRate(JR_CostTaxDate), CostGSTRate?.GetEffectiveExtraRate(JR_CostTaxDate), CostCurrency, Company.PK);

				return JR_OSCostGSTAmt_Calc == osTaxAmountFromExTaxAmount
					? TaxAmountCalculator.GetLocalTaxAmount(Factory, JR_GC, JR_LocalCostAmt, CostGSTRate, CostGSTRate?.GetRate(JR_CostTaxDate), CostGSTRate?.GetEffectiveExtraRate(JR_CostTaxDate), JR_OSCostGSTAmt_Calc, JR_OSCostExRate, withoutRounding: true)
					: TaxAmountCalculator.GetLocalTaxAmountFromOSTaxAmount(Factory, JR_GC, JR_OSCostGSTAmt_Calc, JR_OSCostExRate, withoutRounding: true);
			}
		}

		public ZPropertyInfo JR_Cost_LocalGSTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Cost_LocalGSTAmountName); }
		}

		#endregion

		#region JR_Cost_LocalWHTAmount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Cost_LocalWHTAmount
		{
			get
			{
				return TaxAmountCalculator.GetLocalWithholdingTaxAmountFromLocalExTaxAmount(JR_LocalCostAmt, CostWHTRate);
			}
		}

		public ZPropertyInfo JR_Cost_LocalWHTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Cost_LocalWHTAmountName); }
		}
		#endregion

		#region JR_EstimatedCost

		protected virtual bool JR_EstimatedCost_ReadOnly
		{
			get { return (InvoicingJob != null && !InvoicingJob.AllowOverrideEstimatedCost) || JR_OSCostAmtInfo.ReadOnly; }
		}

		internal void SetEstimatedCost(ZDecimal value)
		{
			JR_EstimatedCost = value;
			if (CostCurrency != null)
			{
				LastEstCostCalculationExRate = JR_OSCostExRate;
			}
			else
			{
				LastEstCostCalculationExRate = ZDecimal.Zero;
			}
		}

		internal void RecalculateEstimatedCostProportionally()
		{
			if (!LastEstCostCalculationExRate.HasValue)
			{
				LastEstCostCalculationExRate = (ZDecimal)JR_OSCostExRateInfo.OriginalValue;
			}
			if (!JR_OSCostExRate.IsEmpty && CostCurrency != null)
			{
				var newEstimatedCostValue = JR_EstimatedCost;
				if (!LastEstCostCalculationExRate.Value.IsEmpty)
				{
					newEstimatedCostValue = GlbCompany.CurrentCompany.GC_IsReciprocal ? (ZDecimal)((JR_EstimatedCost * LastEstCostCalculationExRate.Value) / JR_OSCostExRate) : (ZDecimal)((JR_EstimatedCost / LastEstCostCalculationExRate.Value) * JR_OSCostExRate);
				}

				SetEstimatedCost(newEstimatedCostValue);
			}
		}

		internal FunctionalitySuspender SetEstimatedCostSuspender
		{
			get { return setEstimatedCostSuspender ?? (setEstimatedCostSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender setEstimatedCostSuspender;

		internal FunctionalitySuspender SetNZEntryFeeChargeTaxAmountSuspender
		{
			get { return setNZEntryFeeChargeTaxAmountSuspender ?? (setNZEntryFeeChargeTaxAmountSuspender = new FunctionalitySuspender()); }
		}
		FunctionalitySuspender setNZEntryFeeChargeTaxAmountSuspender;

		LastEstCostCalculation LastEstCostCalculationValues
		{
			get { return LastEstCostCalculationValuesProvider.GetValue(this); }
		}

		ZDecimal? LastEstCostCalculationExRate
		{
			get { return LastEstCostCalculationValues.ExRate; }
			set
			{
				var lastValues = LastEstCostCalculationValues;
				lastValues.ExRate = value;
				LastEstCostCalculationValuesProvider.SetValue(this, lastValues);
			}
		}

		struct LastEstCostCalculation
		{
			public ZDecimal? ExRate;
		}

		class LastEstCostCalculationValuesProvider : BizoDataRowRelatedValue<LastEstCostCalculation>
		{
			public static void SetValue(BusinessObject bizoForDataRow, LastEstCostCalculation value)
			{
				var service = bizoForDataRow.Factory.ServiceContainer.GetService<LastEstCostCalculationValuesProvider>();
				if (service == null)
				{
					service = new LastEstCostCalculationValuesProvider();
					bizoForDataRow.Factory.ServiceContainer.AddService(service);
				}

				service.SetDataRowRelatedValue(bizoForDataRow, value);
			}

			public static LastEstCostCalculation GetValue(BusinessObject bizoForDataRow)
			{
				var service = bizoForDataRow.Factory.ServiceContainer.GetService<LastEstCostCalculationValuesProvider>();
				LastEstCostCalculation value;
				if (service != null && service.TryGetDataRowRelatedValue(bizoForDataRow, out value))
				{
					return value;
				}

				return new LastEstCostCalculation();
			}
		}

		#endregion

		#endregion

		#region Invoice Amounts

		TransactionHeader fAPTransactionHeader;
		TransactionHeader APTransactionHeader
		{
			get
			{
				if (APLine != null && fAPTransactionHeader == null)
				{
					fAPTransactionHeader = Factory.Load<TransactionHeader>(APLine.AL_AH);
				}
				return fAPTransactionHeader;
			}
		}

		#region Total Tax On Invoice For Job

		protected ZDecimal fTotalTaxOnInvForJob;
		[DecimalPlaces(nameof(CurrencyOnInvForJobDecimals))]
		public ZDecimal TotalTaxOnInvForJob
		{
			get
			{
				if (IsCostPosted)
				{
					return APTransactionHeader.AH_OSTaxAmount;
				}
				else
				{
					SumInvoiceAmountOnJob();
					return fTotalTaxOnInvForJob;
				}
			}
		}

		public ZPropertyInfo TotalTaxOnInvForJobInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.TotalTaxOnInvForJob); }  // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region Total Amount On Invoice For Job

		protected ZDecimal fTotalAmountOnInvForJob;
		[DecimalPlaces(nameof(CurrencyOnInvForJobDecimals))]
		public ZDecimal TotalAmountOnInvForJob
		{
			get
			{
				if (IsCostPosted)
				{
					return APTransactionHeader.AH_OSTotalAmount;
				}
				else
				{
					SumInvoiceAmountOnJob();
					return fTotalAmountOnInvForJob;
				}
			}
		}

		public ZPropertyInfo TotalAmountOnInvForJobInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.TotalAmountOnInvForJob); }  // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal TotalLocalAmountWithGSTOnInvForJob
		{
			get
			{
				if (IsCostPosted)
				{
					return APTransactionHeader.AH_LocalTotalAmount;
				}
				else
				{
					SumInvoiceAmountOnJob();
					return totalLocalAmountWithGSTOnInvForJob;
				}
			}
		}
		ZDecimal totalLocalAmountWithGSTOnInvForJob;

		#endregion

		#region CurrencyCodeOnInvForJob

		protected ZString CurrencyCodeOnInvForJob_innerValue = ZString.Empty;
		public ZString CurrencyCodeOnInvForJob
		{
			get
			{
				if (IsCostPosted)
				{
					return APTransactionHeader.AH_RX_NKTransactionCurrency;
				}
				else
				{
					SumInvoiceAmountOnJob();
					return CurrencyCodeOnInvForJob_innerValue.IsEmpty ? JR_OSCostCurrencyCode : CurrencyCodeOnInvForJob_innerValue;
				}
			}
		}

		public ZPropertyInfo CurrencyCodeOnInvForJobInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.CurrencyCodeOnInvForJob); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region CurrencyOnInvForJobDecimals

		public int CurrencyOnInvForJobDecimals
		{
			get => (CurrencyCodeOnInvForJob == JR_OSCostCurrencyCode) ? OSCostCurrencyDecimals : LocalCurrencyDecimals;
		}

		#endregion

		#region CalculateAPInvoiceOnJobCharges

		protected virtual bool CalculateAPInvoiceOnJobCharges
		{
			get { return InvoicingJob != null; }
		}

		#endregion

		void SumInvoiceAmountOnJob()
		{
			totalLocalAmountWithGSTOnInvForJob = 0;
			ZDecimal totalOSGSTInvoiceAmount = 0;
			ZDecimal totalOSInvoiceAmount = 0;
			ZDecimal totalLocalGSTInvoiceAmount = 0;
			ZDecimal totalLocalInvoiceAmount = 0;
			bool isLocalCurrencyInvoice = false;
			ZString prevChargeCurrecyCode = ZString.Empty;

			BusinessObject[] invoiceCharges = Array.Empty<BusinessObject>();
			if (CalculateAPInvoiceOnJobCharges && !JR_APInvoiceNum.IsEmpty && JR_OH_CostAccount.IsValid)
			{
				ZQuery filter = new ZQuery(JobChargeSchema.JR_APInvoiceNum, SQLComparisonOperator.Equal, JR_APInvoiceNum);
				filter.AddToFilter(JobChargeSchema.JR_OH_CostAccount, SQLComparisonOperator.Equal, JR_OH_CostAccount);

				invoiceCharges = InvoicingJob.Charges.Find(filter);
				foreach (BaseCharge invoiceCharge in invoiceCharges)
				{
					totalOSInvoiceAmount += invoiceCharge.JR_OSCostAmtWithGSTAmt;
					totalOSGSTInvoiceAmount += invoiceCharge.JR_OSCostGSTAmt_Calc;
					totalLocalInvoiceAmount += invoiceCharge.JR_Calc_LocalCostAmtWithGST;
					totalLocalGSTInvoiceAmount += invoiceCharge.JR_Cost_LocalGSTAmount;
					totalLocalAmountWithGSTOnInvForJob += invoiceCharge.JR_Calc_LocalCostAmtWithGST;

					if (!isLocalCurrencyInvoice && !prevChargeCurrecyCode.IsEmpty && prevChargeCurrecyCode != invoiceCharge.JR_OSCostCurrencyCode)
					{
						isLocalCurrencyInvoice = true;
					}
					prevChargeCurrecyCode = invoiceCharge.JR_OSCostCurrencyCode;
				}
			}

			fTotalAmountOnInvForJob = isLocalCurrencyInvoice ? totalLocalInvoiceAmount : totalOSInvoiceAmount;
			fTotalTaxOnInvForJob = isLocalCurrencyInvoice ? totalLocalGSTInvoiceAmount : totalOSGSTInvoiceAmount;
			CurrencyCodeOnInvForJob_innerValue = isLocalCurrencyInvoice ? JR_LocalCurrencyCode : JR_OSCostCurrencyCode;
		}

		#region LineAmountsOnInvoiceForJob

		[DecimalPlaces(nameof(CurrencyOnInvForJobDecimals))]
		public ZDecimal LineTotalAmountOnInvoiceForJob
		{
			get { return CurrencyCodeOnInvForJob == JR_OSCostCurrencyCode ? JR_OSCostAmtWithGSTAmt : JR_Calc_LocalCostAmtWithGST; }
		}

		[DecimalPlaces(nameof(CurrencyOnInvForJobDecimals))]
		public ZDecimal LineGSTAmountOnInvoiceForJob
		{
			get { return CurrencyCodeOnInvForJob == JR_OSCostCurrencyCode ? JR_OSCostGSTAmt_Calc : JR_Cost_LocalGSTAmount; }
		}

		[DecimalPlaces(nameof(CurrencyOnInvForJobDecimals))]
		public ZDecimal LineWHTAmountOnInvoiceForJob
		{
			get { return CurrencyCodeOnInvForJob == JR_OSCostCurrencyCode ? JR_OSCostWHTAmt : JR_Cost_LocalWHTAmount; }
		}

		[DecimalPlaces(nameof(CurrencyOnInvForJobDecimals))]
		public ZDecimal LineExtraTaxAmountOnInvoiceForJob
		{
			get { return CurrencyCodeOnInvForJob == JR_OSCostCurrencyCode ? JR_Calc_OSCostExtraTaxAmt : JR_Calc_LocalCostExtraTaxAmt; }
		}

		#endregion

		#endregion

		#region Sell

		#region JR_IsPosted

		public ZBool JR_IsPosted
		{
			get { return IsRevenueCharge ? (bool)JR_IsRevenuePosted : JR_IsCostPosted && JR_IsRevenuePosted; }
		}

		public ZPropertyInfo JR_IsPostedInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_IsPosted); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region JR_Calc_OSSellAmtWithGST

		[DecimalPlaces(nameof(OSSellCurrencyDecimals))]
		public ZDecimal JR_Calc_OSSellAmtWithGST
		{
			get
			{
				return JR_OSSellAmt + JR_OSSellGSTAmt_Calc;
			}
		}

		public ZPropertyInfo JR_Calc_OSSellAmtWithGSTInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_OSSellAmtWithGSTName); }
		}

		#endregion

		#region JR_Calc_OSCostAmtWithGST

		[DecimalPlaces(nameof(OSCostCurrencyDecimals))]
		public ZDecimal JR_Calc_OSCostAmtWithGST
		{
			get
			{
				return JR_OSCostAmt + JR_OSCostGSTAmt_Calc;
			}
		}

		public ZPropertyInfo JR_Calc_OSCostAmtWithGSTInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_OSCostAmtWithGSTName); }
		}

		#endregion

		#region JR_Calc_LocalCostAmtWithGST

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Calc_LocalCostAmtWithGST
		{
			get { return JR_LocalCostAmt + JR_Cost_LocalGSTAmount; }
		}

		public ZPropertyInfo JR_Calc_LocalCostAmtWithGSTInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_LocalCostAmtWithGSTName); }
		}

		public ZDecimal GetLocalCostAmtWithHighPrecisionGST()
		{
			return JR_LocalCostAmt + JR_Cost_LocalGSTAmountHighPrecision;
		}

		#endregion

		#region JR_Sell_LocalGSTAmount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Sell_LocalGSTAmount => CalculateSellGSTAmounts().LocalSellGSTAmount;

		public ZPropertyInfo JR_Sell_LocalGSTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Sell_LocalGSTAmountName); }
		}
		#endregion

		#region JR_Sell_LocalWHTAmount

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public ZDecimal JR_Sell_LocalWHTAmount
		{
			get
			{
				return TaxAmountCalculator.GetLocalWithholdingTaxAmountFromLocalExTaxAmount(JR_LocalSellAmt, SellWHTRate);
			}
		}

		public ZPropertyInfo JR_Sell_LocalWHTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Sell_LocalWHTAmountName); }
		}
		#endregion

		#endregion

		#region JR_CostCurrency
		[MaxLength(3)]
		public ZString JR_CostCurrency
		{
			get { return JR_RX_NKCostCurrency; }
		}

		public ZPropertyInfo JR_CostCurrencyInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_CostCurrency); } // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}
		#endregion

		#region JR_SellCurrency

		public ZString JR_SellCurrency
		{
			get { return JR_RX_NKSellCurrency; }
		}

		public ZPropertyInfo JR_SellCurrencyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JR_SellCurrencyName, x => JR_RX_NKCostCurrencyInfo); }
		}

		public bool IsSellUSD
		{
			get { return JR_OSSellCurrencyCode == Core.Constants.CurrencyCodes.UnitedStates; }
		}

		#endregion

		#region OS Sell Currency Code

		public ZString JR_OSSellCurrencyCode
		{
			get { return SellCurrency != null ? SellCurrency.RX_Code : ZString.Empty; }
		}

		public ZPropertyInfo JR_OSSellCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Charge.Schema.JR_OSSellCurrencyCode); }  // TODO LJM: non-persistent property that doesn't exist in new schema classes; leave them alone until Geoff looks at this
		}

		#endregion

		#region LocalCurrencyCode

		[MaxLength(3)]
		public ZString JR_LocalCurrencyCode => (Company ?? GlbCompany.CurrentCompany).GC_RX_NKLocalCurrency;

		public ZPropertyInfo JR_LocalCurrencyCodeInfo => GetZPropertyInfo(Schema.JR_LocalCurrencyCode);

		#endregion

		#region JR_JobNumber

		public virtual ZString JR_JobNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (JR_JH.IsValid)
				{
					Job aJob = this.Factory.Load<Job>(JR_JH);
					if (aJob != null)
					{
						result = aJob.JH_JobNum;
					}
				}
				return result;
			}
			set { }
		}

		public ZPropertyInfo JR_JobNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JR_JobNumberName); }
		}

		#endregion

		#region JR_JobLocalRef

		public ZString JR_JobLocalRef
		{
			get { return Job != null ? Job.JH_JobLocalReference : ZString.Empty; }
		}

		public ZPropertyInfo JR_JobLocalRefInfo
		{
			get { return GetZPropertyInfo(nameof(JR_JobLocalRef)); }
		}

		#endregion

		#region JR_ARInvoiceNumber

		public ZString JR_ARInvoiceNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (ARLine != null)
				{
					if (ARLine.TransactionHeader != null)
					{
						result = ARLine.TransactionHeader.AH_TransactionNum;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JR_ARInvoiceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JR_ARInvoiceNumberName); }
		}
		#endregion

		#region JR_JobInvoiceNumber

		public ZString JR_JobInvoiceNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (ARLine != null)
				{
					if (ARLine.TransactionHeader != null)
					{
						result = ARLine.TransactionHeader.AH_ConsolidatedInvoiceRef;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo JR_JobInvoiceNumberInfo
		{
			get { return GetZPropertyInfo(Schema.JR_JobInvoiceNumberName); }
		}

		#endregion

		#region JR_ChequeOrReferenceLabel

		public ZString JR_ChequeOrReferenceLabel
		{
			get { return (IsCheque) ? Res.GetString("7e675880-ecd8-42cc-97b9-9b93ba574bd4", "Cheque #:") : Res.GetString("723786e4-3094-4f9b-9359-1c3d17411737", "Reference #:"); }
		}

		public ZPropertyInfo JR_ChequeOrReferenceLabelInfo
		{
			get { return GetZPropertyInfo(Schema.JR_ChequeOrReferenceLabelName); }
		}
		#endregion

		#region JR_Calc_ARInvoiceDate

		public ZDateTime JR_Calc_ARInvoiceDate
		{
			get
			{
				var invoiceDate = new InvoiceAndDueDateCalculator(InvoicingJob != null ? InvoicingJob.PlugInData : null,
																														ZDateTime.Now,
																														SellAccount,
																														InvoicingJob != null ? InvoicingJob.JobType : null,
																														InvoicingJob != null ? InvoicingJob.Direction : ZString.Empty,
																														InvoicingJob != null ? InvoicingJob.TransportMode : ZString.Empty,
																														InvoicingJob != null ? InvoicingJob.JH_GB : ZGuid.Empty,
																														InvoicingJob != null ? InvoicingJob.JH_GE : ZGuid.Empty,
																														LedgerTypes.AccountsReceivable,
																														JR_InvoiceType,
																														InvoicingJob).InvoiceDate;

				return invoiceDate.IsEmpty ? ZDateTime.Now : invoiceDate;
			}
		}

		public ZPropertyInfo JR_Calc_ARInvoiceDateInfo
		{
			get { return GetZPropertyInfo(Schema.JR_Calc_ARInvoiceDate); }
		}

		#endregion

		#region JobChargeTarget

		JobChargeTargetManager JCTManager => jctManager ?? (jctManager = new JobChargeTargetManager(this));
		JobChargeTargetManager jctManager;

		internal ZGuid RelatedJobID => JCTManager.RelatedJobID;
		internal virtual IJobInvoicingPlugIn RelatedJob => JCTManager.RelatedJob;

		[ResourceStringData("BaseCharge|JR_Calc_InvoiceTarget",
			Caption = "Intercompany Invoice Target Job",
			MediumCaption = "Invoice Target",
			ShortCaption = "Inv. Target",
			FullDescription = "Enter or select a Target Job Number to be used for Intercompany Invoicing. This revenue charge will be imported in the receiving company as cost on the Target Job. If left blank, cost will be imported on this operational job (default behavior).")]
		[List(nameof(Lookups) + "." + nameof(BaseChargeLookups.InvoiceTargetJobNumbers))]
		public ZString JR_Calc_InvoiceTarget
		{
			get => JCTManager.InvoiceTargetJobNumber;
			set
			{
				if (JR_Calc_InvoiceTarget != value)
				{
					JCTManager.InvoiceTargetJobNumber = value;
					SetNonPersistentPropertyValue(JR_Calc_InvoiceTargetInfo, ref jr_calc_InvoiceTarget, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJR_Calc_InvoiceTarget();
					}

					JR_Calc_InvoiceTargetInfo.RefreshBinding();
				}
			}
		}
		ZString? jr_calc_InvoiceTarget;

		public ZPropertyInfo JR_Calc_InvoiceTargetInfo
		{
			get { return GetZPropertyInfo(nameof(JR_Calc_InvoiceTarget)); }
		}

		public bool JR_Calc_InvoiceTarget_ReadOnly => !this.InvoiceTargetsEnabled();

		[List("Lookups.RelatedJobNumbers")]
		[ResourceStringData("BaseCharge|JR_Calc_RelatedJobNumber", Caption = "Related Job Number", MediumCaption = "Related Job", ShortCaption = "Rel. Job", FullDescription = "Enter or select a Related Job Number this charge is associated with. Related Job Number can be used to default other values on the charge.")]
		public override ZString JR_Calc_RelatedJobNumber
		{
			get => JCTManager.RelatedJobNumber;
			set
			{
				if (JR_Calc_RelatedJobNumber != value)
				{
					InvoicingJob?.InvalidateGroupValidation();

					JCTManager.RelatedJobNumber = value;
					SetNonPersistentPropertyValue(JR_Calc_RelatedJobNumberInfo, ref jr_calc_RelatedJobNumber, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateJR_Calc_RelatedJobNumber();
					}

					ResetDebtorAndUpdateTarget();

					if (SellAccountIsOrgProxy && !CostAccountIsOrgProxy)
					{
						InternalFieldsManager.ResetInternalFields(JR_OH_SellAccount, true);
					}

					JR_Calc_RelatedJobNumberInfo.RefreshBinding();
				}
			}
		}
		ZString? jr_calc_RelatedJobNumber;

		public virtual bool JR_Calc_RelatedJobNumber_ReadOnly => IsRevenuePosted;

		public ZPropertyInfo JR_Calc_RelatedJobNumberInfo
		{
			get { return GetZPropertyInfo(nameof(JR_Calc_RelatedJobNumber)); }
		}

		FunctionalitySuspender SetUpdateInvoiceTargetSuspender => setUpdateInvoiceTargetSuspender ?? (setUpdateInvoiceTargetSuspender = new FunctionalitySuspender());
		FunctionalitySuspender setUpdateInvoiceTargetSuspender;

		void ResetDebtorAndUpdateTarget()
		{
			using (SetUpdateInvoiceTargetSuspender.GetSuspender())
			{
				ResetChargeDebtor();
			}

			UpdateInvoiceTarget();
		}

		void UpdateInvoiceTarget()
		{
			JR_Calc_InvoiceTarget = this.GetInvoiceTarget();
		}

		#endregion

		#region JR_IsCostTaxAmountOverridden

		public override ZBool JR_IsCostTaxAmountOverridden
		{
			get { return base.JR_IsCostTaxAmountOverridden; }
			set
			{
				if (base.JR_IsCostTaxAmountOverridden != value)
				{
					if (ParentConsolCost != null && !IsCostPosted && value != ParentConsolCost.E6_IsTaxAmountOverridden)
					{
						var message = string.Format(CultureInfo.CurrentCulture, @"Trying to set JR_IsCostTaxAmountOverridden to a value which does not match with linked Consol Cost.
Current Value of JR_IsCostTaxAmountOverridden: {0}, E6_IsTaxAmountOverridden: {1}", JR_IsCostTaxAmountOverridden, ParentConsolCost.E6_IsTaxAmountOverridden);
						ErrorReporter.ReportOnce("ApportionedChargeWithMismatchedJR_IsCostTaxAmountOverriddeValue_2", message);
					}

					if (value)
					{
						if (ParentConsolCost != null || StopGSTAmountOfUnApportionedChargeFromBeingOverridden.IsSuspended)
						{
							base.JR_IsCostTaxAmountOverridden = true;
						}
						else
						{
							var message = string.Format(CultureInfo.CurrentCulture, @"Trying to set JR_IsCostTaxAmountOverridden to true for an unapportioned job charge, which is not allowed.
Current Value of JR_IsCostTaxAmountOverridden: {0}, JR_E6: {1}", JR_IsCostTaxAmountOverridden, JR_E6);
							ErrorReporter.ReportOnce("SettingJR_IsCostTaxAmountOverriddenToTrueForUnapportionedCharge_2", message);
						}
					}
					else
					{
						base.JR_IsCostTaxAmountOverridden = false;
					}

					GSTCalculationStrategy.ResetOSCostGSTAmount();
				}
			}
		}

		public FunctionalitySuspender StopGSTAmountOfUnApportionedChargeFromBeingOverridden => stopGSTAmountOfUnApportionedChargeFromBeingOverridden ?? (stopGSTAmountOfUnApportionedChargeFromBeingOverridden = new FunctionalitySuspender());
		FunctionalitySuspender stopGSTAmountOfUnApportionedChargeFromBeingOverridden;

		#endregion

		#region JR_E6

		public override ZGuid JR_E6
		{
			get { return base.JR_E6; }
			set
			{
				var oldValue = JR_E6;
				if (oldValue != value)
				{
					base.JR_E6 = value;

					if (JR_IsApportioned && ParentConsolCost != null)
					{
						JR_IsCostTaxAmountOverridden = ParentConsolCost.E6_IsTaxAmountOverridden;
					}
					else
					{
						JR_IsCostTaxAmountOverridden = false;
					}

					CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_E6_PropertyValueSet,
					() =>
					{
						return Invariant($@"OldValue: {oldValue}, NewValue: {value}
{System.Environment.StackTrace}");
					});
				}
			}
		}

		#endregion

		#region JR_LineCFX

		[ReadOnlyMember(nameof(JR_LineCFX_ReadOnly))]
		public override ZDecimal JR_LineCFX
		{
			get
			{
				return base.JR_LineCFX;
			}
			set
			{
				if (JR_LineCFX != value)
				{
					base.JR_LineCFX = value;

					if (this is ApportionSplitCharge)
					{
						AddChangeInfoToCiriticalValidationCollectorIfRequires();
					}
				}
			}
		}

		protected virtual bool JR_LineCFX_ReadOnly => false;

		protected void AddChangeInfoToCiriticalValidationCollectorIfRequires()
		{
			if (WIP != null && !WIP.IsInDatabase)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddLastInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeLocalSellAmtNotEqualRelatedWIPAmount, () =>
				{
					var chargeAmount = JR_LocalSellInvoiceAmt;
					var chargeCFX = JR_CFXAmt;
					var chargeTotal = chargeAmount - chargeCFX;
					var lineAmount = -WIP.AL_LineAmount;

					if (chargeTotal != lineAmount)
					{
						var developerInfo = new ZStringBuilder();
						developerInfo.AppendLine(Invariant($"{chargeAmount} - {chargeCFX} = {chargeTotal} != {lineAmount}"));
						developerInfo.AppendLine((NoResString)"ExchangeRate info: ");

						developerInfo.AppendLine(Invariant($"SellInvoiceRateWithoutCFX: {GetSellInvoiceRateWithoutCFXInfo()}"));
						developerInfo.AppendLine(Invariant($"SellInvoiceExchangeRate: {SellInvoiceExchangeRate?.Rate ?? 1m}"));
						developerInfo.AppendLine(Invariant($"SellRateWithoutCFX: {GetSellRateWithoutCFXInfo()}"));
						developerInfo.AppendLine(Invariant($"RevenueExchangeRate: {RevenueExchangeRate?.Rate ?? 1m}"));
						developerInfo.AppendLine(Invariant($"Stacktrace --> {new StackTrace(true)}"));
						return developerInfo.ToString();
					}
					return null;
				});
			}
		}

		protected virtual string GetSellInvoiceRateWithoutCFXInfo() => (NoResString)"Not available";

		protected virtual string GetSellRateWithoutCFXInfo() => (NoResString)"Not available";

		#endregion

		#region JR_CostPlaceOfSupply

		public override ZString JR_CostPlaceOfSupply
		{
			get => base.JR_CostPlaceOfSupply;
			set
			{
				if (JR_CostPlaceOfSupply != value)
				{
					base.JR_CostPlaceOfSupply = value;
					UpdateCostGST(shouldCalculateWHT: false);
				}
			}
		}

		public override ILocation CostPlaceOfSupplyLocation => PlaceOfSupplyHelper.TryConvertToLocation(Company, JR_CostPlaceOfSupply) ?? InvoicingJob?.FixedPlaceOfSupply;

		#endregion

		#region JR_SellPlaceOfSupply

		public override ZString JR_SellPlaceOfSupply
		{
			get => base.JR_SellPlaceOfSupply;
			set
			{
				if (JR_SellPlaceOfSupply != value)
				{
					base.JR_SellPlaceOfSupply = value;
					ResetSellGSTTaxDefault();
					RunSellAccountCalculations();
				}
			}
		}

		public override ILocation SellPlaceOfSupplyLocation => PlaceOfSupplyHelper.TryConvertToLocation(Company, JR_SellPlaceOfSupply) ?? InvoicingJob?.FixedPlaceOfSupply;

		#endregion

		#region JR_AgentDeclaredCostAmtLocal

		protected virtual bool JR_AgentDeclaredCostAmtLocal_ReadOnly => false;

		#endregion

		#region JR_AgentDeclaredSellAmtLocal

		protected virtual bool JR_AgentDeclaredSellAmtLocal_ReadOnly => false;

		#endregion

		#region IsCheque
		protected internal bool IsCheque
		{
			get { return JR_PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque; }
		}
		#endregion

		#region Charge Type
		[MaxLength(3)]
		public ZString ChargeType
		{
			get
			{
				ZString result = ZString.Empty;

				if (!JR_ChargeType.IsEmpty)
				{
					result = JR_ChargeType;
				}
				else
				{
					var chargeCode = ChargeCode;
					if (chargeCode != null)
					{
						var chargeTypeOverride = JobInvoicing.Job.GetChargeTypeInformation(chargeCode, InvoicingJob);
						if (chargeTypeOverride != null)
						{
							result = chargeTypeOverride.AN_ChargeType;
							chargeTypeOverride = null;
						}
					}
				}

				return result;
			}
		}

		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal MarginPercentage
		{
			get
			{
				ZDecimal result = 0m;

				if (!JR_ChargeType.IsEmpty)
				{
					result = JR_MarginPercentage;
				}
				else if (ChargeCode != null)
				{
					var chargeTypeOverride = JobInvoicing.Job.GetChargeTypeInformation(ChargeCode, InvoicingJob);
					if (chargeTypeOverride != null)
					{
						result = chargeTypeOverride.AN_MarginPercentage;
						chargeTypeOverride = null;
					}
				}

				return result;
			}
		}

		public ZPropertyInfo MarginPercentageInfo
		{
			get { return GetZPropertyInfo(BaseCharge.Schema.MarginPercentage); }
		}

		public ZPropertyInfo ChargeTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeType); }
		}

		public bool IsRevenueCharge
		{
			get { return ChargeType == Core.Constants.ChargeType.Revenue; }
		}

		public bool IsOverheadCharge
		{
			get { return ChargeType == Core.Constants.ChargeType.Overhead; }
		}

		public bool IsDisbursementCharge
		{
			get { return ChargeType == Core.Constants.ChargeType.Disbursement; }
		}

		public bool IsMarginCharge
		{
			get { return ChargeType == Core.Constants.ChargeType.Margin; }
		}

		public bool IsManualJobAccrualCharge
		{
			get { return ChargeType == Core.Constants.ChargeType.ManualJobAccrual; }
		}

		#endregion

		#region Cash Advance

		public ICashAdvanceRequirement ARCashAdvanceRequirement => arCashAdvanceRequirement ?? (arCashAdvanceRequirement = new ARCashAdvanceRequirement(this));
		ICashAdvanceRequirement arCashAdvanceRequirement;

		public ICashAdvanceRequirement APCashAdvanceRequirement => apCashAdvanceRequirement ?? (apCashAdvanceRequirement = new APCashAdvanceRequirement(this));
		ICashAdvanceRequirement apCashAdvanceRequirement;

		internal bool IsARCashAdvanceOnOrBeyondRequeustedStage => ARCashAdvanceRequirement.IsRequested || HasARPaidOrInvoicedCashAdvanceRequestLine;

		internal bool IsAPCashAdvanceOnOrBeyondRequeustedStage => APCashAdvanceRequirement.IsRequested || HasAPPaidOrInvoicedCashAdvanceRequestLine;

		internal bool HasARPaidOrInvoicedCashAdvanceRequestLine => ARCashAdvanceRequirement.IsPaid || ARCashAdvanceRequirement.IsInvoiced;

		internal bool HasAPPaidOrInvoicedCashAdvanceRequestLine => APCashAdvanceRequirement.IsPaid || APCashAdvanceRequirement.IsInvoiced;

		#endregion

		public ZDecimal GetContainersCostShare()
		{
			ZDecimal costAmnt;
			var consolCost = ParentConsolCost;
			if (consolCost != null)
			{
				var paymentBases = consolCost.PaymentBases.Cast<JobPaymentBasis>();
				var consol = consolCost.Consol;
				if (consol != null
					&& ShipmentInfo != null
					&& ShipmentInfo.InvoicingSupporter.TryGetContainersCostShare(ChargeCode, paymentBases, consol.PK, out costAmnt))
				{
					return costAmnt;
				}
			}

			return 0m;
		}

		public bool HasContainerCostShare()
		{
			var consolCost = ParentConsolCost;
			if (consolCost != null)
			{
				var consol = consolCost.Consol;
				return consol != null
						&& ShipmentInfo != null
						&& ShipmentInfo.InvoicingSupporter.HasContainerCostShare(ChargeCode);
			}
			else
			{
				return false;
			}
		}

		protected bool PreventReadOnlyFromChangingValues;

		protected void UpdateJR_AW_CostWHTRateReadOnly()
		{
			JR_AW_CostWHTRateInfo.RefreshBinding();
			if (!PreventReadOnlyFromChangingValues && !IsCostWHTApplicable)
			{
				JR_AW_CostWHTRate = ZGuid.Empty;
			}
		}

		protected virtual bool JR_AW_CostWHTRate_ReadOnly
		{
			get { return IsCostWHTFieldReadOnly; }
		}

		void MarkSellRatedAmountAsChanged()
		{
			if (!AutoRatingOverrideSuppressed)
			{
				JR_SellRatingOverride = true;
				RemoveOldBases(false);
				shouldDisableSellLog = JR_SellRatingOverride;
			}
		}

		protected bool shouldDisableSellLog;

		void MarkCostRatedAmountAsChanged()
		{
			if (!AutoRatingOverrideSuppressed)
			{
				JR_CostRatingOverride = true;
				RemoveOldBases(true);
				shouldDisableCostLog = JR_CostRatingOverride;
			}
		}

		protected bool IsAllCostFieldsReadonly
		{
			get { return IsRevenueCharge || IsCostPosted || JR_IsApportioned || IsRevenuePostedWithManualJobRevenueJournal || IsInDatabaseAndReadyForCostPosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		protected bool shouldDisableCostLog;

		public ZDecimal TotalLocalRevenueAmount => JR_LocalSellAmt + JR_Sell_LocalGSTAmount;

		internal void AddAttributes(RateAttributeSet attributes)
		{
			if (attributes != null)
			{
				JobChargeAttributes.RemoveAndDeleteAll();

				foreach (var attribute in attributes.Attributes)
				{
					var attrib = JobChargeAttributes.AddNew();
					attrib.EC_Name = attribute.Code;
					attrib.EC_Value = attribute.Value;
					attrib.EC_Amount = attribute.Amount;
				}
			}
		}

		internal void ReplaceAttributes(JobChargeAttribCollection fromAttributes)
		{
			JobChargeAttributes.RemoveAndDeleteAll();

			foreach (JobChargeAttrib fromAttribute in fromAttributes)
			{
				var newAttrib = JobChargeAttributes.AddNew();
				newAttrib.EC_Name = fromAttribute.EC_Name;
				newAttrib.EC_Value = fromAttribute.EC_Value;
				newAttrib.EC_Amount = fromAttribute.EC_Amount;
			}
		}

		#region Autorating Calculation Description

		public void SetRevenueCalculationDescription(AutoRateInfo rateInfo)
		{
			SetCalculationDescription(rateInfo, CostRevenueType.Revenue);
		}

		public void SetCostCalculationDescription(AutoRateInfo rateInfo)
		{
			SetCalculationDescription(rateInfo, CostRevenueType.Cost);
		}

		[ReadOnly(true)]
		public ZBlob RevenueCalculationDescription
		{
			get { return GetCalculationStmNote(SellCalculationNote); }
			set
			{
				if (ChargeCode != null)
				{
					sellCalculationNote = this.CreateOrUpdateSellCalculationNote(SellCalculationNote, value);
				}
				RevenueCalculationDescriptionInfo.RefreshBinding();
			}
		}

		public string RevenueCalculationDescriptionString
		{
			get
			{
				var description = RevenueCalculationDescription;
				return ORtfTextUtil.IsRtf(description) ? ORtfTextUtil.RtfToText(description) : description.ToUTF8();
			}
		}

		public ZBlob RevenueCalculationDescription_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(RevenueCalculationDescription);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				RevenueCalculationDescription = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZPropertyInfo RevenueCalculationDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(RevenueCalculationDescription)); }
		}

		StmNote SellCalculationNote
		{
			get
			{
				if (sellCalculationNote == null || sellCalculationNote.IsDeleted)
				{
					sellCalculationNote = this.GetSellCalculationNote();
				}
				return sellCalculationNote;
			}
		}
		StmNote sellCalculationNote;

		[ReadOnly(true)]
		public ZBlob CostCalculationDescription
		{
			get { return GetCalculationStmNote(CostCalculationNote); }
			set
			{
				if (ChargeCode != null)
				{
					costCalculationNote = this.CreateOrUpdateCostCalculationNote(CostCalculationNote, value);
				}
				CostCalculationDescriptionInfo.RefreshBinding();
			}
		}

		public string CostCalculationDescriptionString
		{
			get
			{
				var description = CostCalculationDescription;
				return ORtfTextUtil.IsRtf(description) ? ORtfTextUtil.RtfToText(description) : description.ToUTF8();
			}
		}
		public ZBlob CostCalculationDescription_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(CostCalculationDescription);
			}

			set
			{
				var htmlToRtfConverter = new HtmlToRtfConverter();
				CostCalculationDescription = ZBlob.FromUTF8(htmlToRtfConverter.Convert(value.ToUTF8()));
			}
		}

		public ZPropertyInfo CostCalculationDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CostCalculationDescription)); }
		}

		StmNote CostCalculationNote
		{
			get
			{
				if (costCalculationNote == null || costCalculationNote.IsDeleted)
				{
					costCalculationNote = this.GetCostCalculationNote();
				}
				return costCalculationNote;
			}
		}
		StmNote costCalculationNote;

		ZBlob GetCalculationStmNote(StmNote note)
		{
			if (ChargeCode == null || note == null)
			{
				return ZBlob.Empty;
			}

			return note.ST_NoteData;
		}

		enum CostRevenueType
		{
			Cost,
			Revenue
		}

		bool IsDisbursementChargeWithAnyPostedAmount
		{
			get { return IsDisbursementCharge && (IsCostPosted || IsRevenuePosted); }
		}

		internal ZString GetCombinedDescription(AutoRateInfo rateInfo)
		{
			var result = !rateInfo.InvoiceLineDescription.IsEmpty && !ShouldDefaultLocalChargeDescription && rateInfo.IsCustomsDisbursementOrDeferredCharge(Job?.JH_GC)
						? new ZStringBuilder(rateInfo.InvoiceLineDescription) : new ZStringBuilder(JR_Desc);

			if (!rateInfo.AdditionalInvoiceLineDescription.IsEmpty && !IsDisbursementChargeWithAnyPostedAmount)
			{
				if (!JR_Desc.Contains(rateInfo.AdditionalInvoiceLineDescription))
				{
					if (!result.IsEmpty)
					{
						result.AppendLine();
					}

					result.Append(rateInfo.AdditionalInvoiceLineDescription);
				}

				if (result.Length > JR_DescInfo.MaxLength)
				{
					string truncationMessage = Res.GetString("6695c905-2bc2-4a02-9d2c-dec6a2d98fcb", "*\r\n\r\n* Truncated to fit");
					ZString tooLongDescription = result.ToString();
					result = new ZStringBuilder(tooLongDescription.Left(JR_DescInfo.MaxLength - truncationMessage.Length) + truncationMessage);
				}
			}
			return result.ToString();
		}

		void SetCalculationDescription(AutoRateInfo rateInfo, CostRevenueType costRevenue)
		{
			if (ChargeCode != null)
			{
				var value = ZBlob.FromUTF8(rateInfo.Description);

				if (rateInfo != null && rateInfo.IsDisbursement)
				{
					RevenueCalculationDescription = value;
					CostCalculationDescription = value;
				}
				else
				{
					if (costRevenue == CostRevenueType.Revenue)
					{
						RevenueCalculationDescription = value;
					}
					else if (costRevenue == CostRevenueType.Cost)
					{
						CostCalculationDescription = value;
					}
				}
			}
		}

		#endregion

		#region Margins

		public ZDecimal GetMarginAmountUp(ZDecimal amount, ZString currency)
		{
			return MarginPercentage != 0 ? (ZDecimal)AccountingUtils.Round(amount / MarginPercentage * 100, currency) : 0;
		}

		public ZDecimal GetMarginAmountDown(ZDecimal amount, ZString currency)
		{
			return (ZDecimal)AccountingUtils.Round(amount / 100 * MarginPercentage, currency);
		}

		#endregion

		#region Calculation of GST and WHT Amounts

		protected bool IsCostAccountWHTRegistered
		{
			get { return (CostAccount != null && CostAccount.MiscServ != null) ? CostAccount.MiscServ.OM_APWHTApplicable : ZBool.False; }
		}

		protected bool IsCostAccountGSTRegistered
		{
			get
			{
				if (CostAccount != null && CostAccount.CompanyData != null)
				{
					if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled())
					{
						return !(CostAccountIsOrgProxy && !this.IsExcludedFromAutoJRJ(CostAccount)) && CostAccount.CompanyData.IsAPTaxApplicable;
					}
					else
					{
						return CostAccount.CompanyData.IsAPTaxApplicable;
					}
				}
				else
				{
					return false;
				}
			}
		}

		protected ZBool IsSellAccountWHTRegistered
		{
			get { return (SellAccount != null && SellAccount.MiscServ != null) ? SellAccount.MiscServ.OM_ARWHTApplicable : ZBool.False; }
		}

		protected ZBool IsSellAccountGSTRegistered
		{
			get
			{
				if (SellAccount != null && SellAccount.CompanyData != null)
				{
					if (AutoJRJRegistryStatusHelper.IsAutoJRJEnabled())
					{
						return !(SellAccountIsOrgProxy && !this.IsExcludedFromAutoJRJ(SellAccount)) && SellAccount.CompanyData.IsARTaxApplicable;
					}
					else
					{
						return SellAccount.CompanyData.IsARTaxApplicable;
					}
				}
				else
				{
					return false;
				}
			}
		}

		#region GST & WHT Rate fetching

		protected ZGuid CostWHTId
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (CostAccount != null && ChargeCode != null && GlbCompany.CurrentCompany.GC_IsWHTRegistered && CostAccount.MiscServ.OM_APWHTApplicable)
				{
					result = ChargeCode.AC_AW_WithholdingTaxRate;
				}
				return result;
			}
		}

		protected ZGuid LocalWHTId
		{
			get
			{
				ZGuid result = ZGuid.Empty;
				if (SellAccount != null && ChargeCode != null && GlbCompany.CurrentCompany.GC_IsWHTRegistered && SellAccount.MiscServ.OM_ARWHTApplicable)
				{
					result = ChargeCode.AC_AW_WithholdingTaxRate;
				}
				return result;
			}
		}

		#endregion

		public bool IsCostGSTRateActual
		{
			get { return ChargeCode != null && ChargeCode.IsComment || (IsCostGSTApplicable && CostGSTRate != null) || (!IsCostGSTApplicable && CostGSTRate == null); }
		}

		protected internal bool IsCostGSTApplicable
		{
			get { return JR_OH_CostAccount.IsValid && IsCostAccountGSTRegistered && GlbCompany.CurrentCompany.GC_IsGSTRegistered && !IsRevenueCharge; }
		}

		protected internal bool IsCostButNotGSTRegistered
		{
			get { return JR_OH_CostAccount.IsValid && !IsCostAccountGSTRegistered && !IsRevenueCharge; }
		}

		protected internal bool IsCostGSTFieldReadOnly
		{
			get
			{
				return JR_IsApportioned ||
					   !IsCostGSTApplicable ||
					   !AccountingUtils.IsUserCanChangeGST(LedgerTypes.AccountsPayable) ||
					   !(InvoicingJob?.InvoicingAllowOverrideCostTaxId ?? false) ||
					   IsCostPosted ||
					   IsInDatabaseAndReadyForCostPosting ||
					   IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		protected internal bool IsCostWHTApplicable
		{
			get { return JR_OH_CostAccount.IsValid && IsCostAccountWHTRegistered && GlbCompany.CurrentCompany.GC_IsWHTRegistered && !IsRevenueCharge; }
		}

		protected internal bool IsCostWHTFieldReadOnly
		{
			get { return JR_IsApportioned || !IsCostWHTApplicable || !AccountingUtils.IsUserCanChangeWHT(LedgerTypes.AccountsPayable) || IsCostPosted || IsInDatabaseAndReadyForCostPosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		bool IsSellGSTApplicableCore
		{
			get { return JR_OH_SellAccount.IsValid && IsSellAccountGSTRegistered && GlbCompany.CurrentCompany.GC_IsGSTRegistered; }
		}

		public bool IsSellGSTRateActual
		{
			get { return ChargeCode != null && ChargeCode.IsComment || (IsSellGSTApplicable && SellGSTRate != null) || (!IsSellGSTApplicable && SellGSTRate == null); }
		}

		protected internal bool IsSellGSTApplicable
		{
			get { return IsSellGSTApplicableCore && this.ChargeType != Core.Constants.ChargeType.Comment; }
		}

		protected internal bool IsSellButNotGSTRegistered
		{
			get { return JR_OH_SellAccount.IsValid && !IsSellAccountGSTRegistered && this.ChargeType != Core.Constants.ChargeType.Comment; }
		}

		protected virtual bool IsSellGSTFieldReadOnly
		{
			get
			{
				return !IsSellGSTApplicable ||
					   !AccountingUtils.IsUserCanChangeGST(LedgerTypes.AccountsReceivable) ||
					   !(InvoicingJob?.InvoicingAllowOverrideSellTaxId ?? false) ||
					   IsRevenuePosted ||
					   IsInDatabaseAndReadyForRevenuePosting ||
					   IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity;
			}
		}

		protected internal bool IsSellWHTApplicable
		{
			get { return JR_OH_SellAccount.IsValid && IsSellAccountWHTRegistered && GlbCompany.CurrentCompany.GC_IsWHTRegistered; }
		}

		protected bool IsSellWHTFieldReadOnly
		{
			get { return !IsSellWHTApplicable || !AccountingUtils.IsUserCanChangeWHT(LedgerTypes.AccountsReceivable) || IsRevenuePosted || IsInDatabaseAndReadyForRevenuePosting || IsInDatabaseAndReadyForFinancialClosureWithoutModifySecurity; }
		}

		#endregion

		#region Validation

		public new BaseChargeValidation Validation
		{
			get { return (BaseChargeValidation)base.Validation; }
		}

		protected override JobChargeValidation GetNewValidation()
		{
			return new BaseChargeValidation(this);
		}

		#endregion

		#region Helpers

		public void ClearRevenueLink()
		{
			var isReversing = IsRevenuePosted && ARLine.IsInDatabase;

			base.JR_AL_ARLine = ZGuid.Empty;
			JR_ChargeType = ZString.Empty;
			JR_MarginPercentage = 0m;

			if (isReversing && !HasARPaidOrInvoicedCashAdvanceRequestLine)
			{
				UpdateRevenueExchangeRate();
			}
		}

		public void ClearRevenueAmount()
		{
			if (!IsARCashAdvanceOnOrBeyondRequeustedStage)
			{
				using ((IsDisbursementCharge && JR_IsAPCashAdvance && this is Charge c) ? c.Calculations.SuspendCalculations() : DisposableAction.NoAction)
				{
					JR_LocalSellAmt = 0;
					JR_OSSellAmt = 0;
				}
			}
		}

		public void ClearSellAddress()
		{
			JR_OA_SellInvoiceAddress = ZGuid.Empty;
		}

		public void ClearCostLink()
		{
			var isReversing = IsCostPosted && APLine.IsInDatabase;
			base.JR_AL_APLine = ZGuid.Empty;

			if (!JR_IsApportioned && JR_IsCostTaxAmountOverridden)
			{
				JR_IsCostTaxAmountOverridden = false;
			}
			if (isReversing)
			{
				UpdateCostExchangeRate();
			}
		}

		public void ClearCostAmount()
		{
			if (!IsAPCashAdvanceOnOrBeyondRequeustedStage)
			{
				using ((IsDisbursementCharge && JR_IsARCashAdvance && this is Charge c) ? c.Calculations.SuspendCalculations() : DisposableAction.NoAction)
				{
					JR_OSCostAmt = 0;
					JR_LocalCostAmt = 0;
				}
			}
		}

		public void ClearCostLinkOnlyTemporary()
		{
			base.JR_AL_APLine = ZGuid.Empty;
		}

		public IDisposable ClearRevenueLinkOnlyTemporary()
		{
			var previousValue = base.JR_AL_ARLine;
			return new DisposableAction(() => base.JR_AL_ARLine = ZGuid.Empty, () => JR_AL_ARLine = previousValue);
		}

		/// <summary>
		///	Calculates poposed revenue amount based on cost amount and other inputs.
		///	Rules are:
		///		a) DSB charge must be exactly the same amount as Cost
		///		b) MRG charge must be set to Cost amount / Margin percentage * 100
		///		c) MRG 0%, REV, etc should be set to 0
		///		d) MRG 0% during AP posting, should be set to Cost amount
		/// </summary>
		/// <returns>Returns proposed revenue amount based on cost field value</returns>
		public ZDecimal GetRevenueAmountBasedOnCost()
		{
			ZDecimal result = 0m;

			if (IsMarginCharge)
			{
				var marginPercent = ZDecimal.Zero;
				if (ChargeCode != null)
				{
					var chargeTypeOverride = JobInvoicing.Job.GetChargeTypeInformation(ChargeCode, InvoicingJob);
					marginPercent = chargeTypeOverride.AN_MarginPercentage;
					chargeTypeOverride = null;
				}

				if (marginPercent == 0 && Factory.HasContext(BusinessContext.NewChargeCostIsGoingToBePostedInTransformer))
				{
					result = JR_OSCostAmt;
				}

				if (marginPercent > 0)
				{
					result = GetMarginAmountUp(JR_OSCostAmt, JR_RX_NKCostCurrency);
				}
			}
			else if (IsDisbursementCharge)
			{
				result = JR_OSCostAmt;
			}

			return result;
		}

		public ZDecimal GetCostAmountBasedOnSell()
		{
			ZDecimal result = 0m;

			if (IsMarginCharge)
			{
				var marginPercent = ZDecimal.Zero;
				if (ChargeCode != null)
				{
					var chargeTypeOverride = JobInvoicing.Job.GetChargeTypeInformation(ChargeCode, InvoicingJob);
					marginPercent = chargeTypeOverride.AN_MarginPercentage;
					chargeTypeOverride = null;
				}

				InvoicingBase aPtransaction = APTransactionHeader as InvoicingBase;
				if (marginPercent == 0 && aPtransaction != null && aPtransaction.SubmittedFromInvoicingForm)
				{
					result = JR_OSSellAmt;
				}

				if (marginPercent > 0)
				{
					result = GetMarginAmountDown(JR_OSSellAmt, JR_RX_NKSellCurrency);
				}
			}
			else if (IsDisbursementCharge)
			{
				result = JR_OSSellAmt;
			}

			return result;
		}

		public ZDecimal GetLocalRevenueAmountBasedOnCost()
		{
			ZDecimal result = 0m;

			if (IsMarginCharge)
			{
				var marginPercent = ZDecimal.Zero;
				if (ChargeCode != null)
				{
					var chargeTypeOverride = JobInvoicing.Job.GetChargeTypeInformation(ChargeCode, InvoicingJob);
					marginPercent = chargeTypeOverride.AN_MarginPercentage;
					chargeTypeOverride = null;
				}

				if (marginPercent == 0 && Factory.HasContext(BusinessContext.NewChargeCostIsGoingToBePostedInTransformer))
				{
					result = JR_LocalCostAmt;
				}

				if (marginPercent > 0)
				{
					result = GetMarginAmountUp(JR_LocalCostAmt, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
				}
			}
			else if (IsDisbursementCharge)
			{
				result = JR_LocalCostAmt;
			}

			return result;
		}

		/// <summary>
		/// Updates the cheque number field without modifying AccChequeBook objects (i.e. doesn't update Next cheque number field)
		/// </summary>
		/// <param name="newValue">New Cheque Number</param>
		public void UpdateChequeNumberWithoutUpdatingChequeBook(ZString newValue)
		{
			if (JR_ChequeNo != newValue)
			{
				base.JR_ChequeNo = newValue;
			}
		}

		/// <summary>
		/// Updates the cheque book field without triggering the update of the cheque number field
		/// </summary>
		/// <param name="newValue">New PK for the ChequeBook</param>
		public void UpdateChequeBook(ZGuid newValue)
		{
			if (JR_AK != newValue)
			{
				base.JR_AK = newValue;
			}
		}

		#region Tax Calculations

		#region OS Sell Tax

		internal void UpdateOsSellWHTAmount() => JR_OSSellWHTAmt = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalExTaxAmount(JR_LocalSellAmt, SellWHTRate, JR_OSSellExRate, SellCurrency);

		internal ZGuid SellTaxRateInvoiceMessage
		{
			get { return (SellGSTRate != null) ? SellGSTRate.AT_A9_DefaultVatClass : ZGuid.Empty; }
		}

		#endregion

		#region OS Cost Tax

		internal void UpdateOsCostGSTAmount()
		{
			GSTCalculationStrategy.ResetOSCostGSTAmount();
		}

		protected void OnCostTaxRateChanged()
		{
			if (CostTaxRecalculationSuspender.IsSuspended)
			{
				return;
			}

			GSTCalculationStrategy.ResetGSTOverriddenFlag();
			GSTCalculationStrategy.ResetOSCostGSTAmount();
		}

		internal FunctionalitySuspender CostTaxRecalculationSuspender => costTaxRecalculationSuspender ?? (costTaxRecalculationSuspender = new FunctionalitySuspender());
		FunctionalitySuspender costTaxRecalculationSuspender;

		internal void UpdateOsCostWHTAmount() => JR_OSCostWHTAmt = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalExTaxAmount(JR_LocalCostAmt, CostWHTRate, JR_OSCostExRate, CostCurrency);

		public IDisposable SuspendCostTaxCalulation(bool isUpdateValueFromConsolCost)
		{
			var suspenderCostTaxInfo = UpdateCostTaxInfoSuspender.GetSuspender();
			var originCostTaxId = JR_AT_CostGSTRate;
			var originCostTaxMessage = JR_A9_CostVATClass;

			var suspenderUpdateTotals = UpdateTotalSuspender.GetSuspender();
			var originCostAmount = JR_OSCostAmt;
			return new DisposableAction(
				() =>
				{
					suspenderCostTaxInfo.Dispose();
					suspenderUpdateTotals.Dispose();
					UpdateTaxInfo();
				});

			void UpdateTaxInfo()
			{
				if (UpdateCostTaxInfoSuspender.IsSuspended)
				{
					return;
				}

				if (isUpdateValueFromConsolCost && ParentConsolCost != null)
				{
					UpdateCostTaxInfoFromConsol();
				}
				UpdateJR_AT_CostGSTRateReadOnly();

				if (IsCostTaxInfoChanged() || IsCostAmountChanged())
				{
					UpdateTotals();
				}
			}

			bool IsCostTaxInfoChanged()
			{
				return originCostTaxId != JR_AT_CostGSTRate || originCostTaxMessage != JR_A9_CostVATClass;
			}

			bool IsCostAmountChanged()
			{
				return originCostAmount != JR_OSCostAmt;
			}
		}

		void UpdateCostGST(bool shouldCalculateWHT)
		{
			if (UpdateCostTaxInfoSuspender.IsSuspended)
			{
				return;
			}

			if (ParentConsolCost != null)
			{
				UpdateCostTaxInfoFromConsol();
			}
			else
			{
				SetCostTaxRateAndMessage(false);
			}
			UpdateJR_AT_CostGSTRateReadOnly();

			if (shouldCalculateWHT)
			{
				UpdateCostWHTDetails();
			}

			UpdateTotals();

			void UpdateCostWHTDetails()
			{
				JR_AW_CostWHTRate = CostWHTId;
				UpdateJR_AW_CostWHTRateReadOnly();
			}
		}

		void UpdateCostTaxInfoFromConsol()
		{
			using (UpdateCostTaxInfoSuspender.GetSuspender())
			{
				JR_AT_CostGSTRate = ParentConsolCost.E6_AT_TaxRate;
				SetCostTaxDateSafe(ParentConsolCost.E6_TaxDate);
				JR_A9_CostVATClass = ParentConsolCost.E6_A9_VATClass;
			}
		}

		//will change it to private at WI00516928
		public FunctionalitySuspender UpdateCostTaxInfoSuspender => updateCostTaxInfoSuspender ?? (updateCostTaxInfoSuspender = new FunctionalitySuspender());
		FunctionalitySuspender updateCostTaxInfoSuspender;

		#endregion

		#endregion

		public void ResetUnpostedSellTaxDefault()
		{
			if (!IsRevenuePosted)
			{
				if (!IsSellGSTRateActual)
				{
					ResetSellGSTTaxDefault();
				}
				if (!(ChargeCode?.IsComment ?? true))
				{
					SetSellTaxBranchDefault();
				}
			}
		}

		public ITotalProvider TotalProvider
		{
			get
			{
				return
#if DEBUG
				Globals.IsTest && totalProvider_TestOnly != null ? totalProvider_TestOnly :
#endif
				InvoicingJob;
			}
		}

		public FunctionalitySuspender UpdateTotalSuspender => updateTotalSuspender ?? (updateTotalSuspender = new FunctionalitySuspender());
		FunctionalitySuspender updateTotalSuspender;

		protected virtual void UpdateTotals()
		{
			if (!UpdateTotalSuspender.IsSuspended)
			{
				TotalProvider?.UpdateTotals();
			}
		}

#if DEBUG

		public void SubstituteTotalProvider_TestOnly(ITotalProvider totalProvider)
		{
			totalProvider_TestOnly = totalProvider;
		}
		ITotalProvider totalProvider_TestOnly;

#endif

		protected ZString GetChargeableRate(ZString? apportionmentMethod, ZDecimal localAmount)
		{
			string result = null;
			if (ApportionmentCreator.IsApportionmentMethodPerChargeableUnit(apportionmentMethod))
			{
				result = (JR_Chargeable == 0 ? ZDecimal.Zero : (ZDecimal)Utilities.Round(localAmount / JR_Chargeable, 4)).ToStringTrimZeros();
			}

			return result ?? Res.GetString("NotApplicable", "Not Applicable");
		}

		#endregion

		#region Context change when login company not equal to job company

		protected override void RunPreSaveValidationCore()
		{
			if (JR_GC == GlbCompany.CurrentCompany.PK)
			{
				base.RunPreSaveValidationCore();
			}
			else
			{
				this.SetContext(BusinessContext.HasBeenValidatedByDifferentCompany);
			}
		}

		protected sealed override void OnSavingCore()
		{
			base.OnSavingCore();
			RunInUserContextForCorrectCompany(() => OnSavingInCompanyContext());
		}

		protected sealed override void OnFactorySaving()
		{
			if (ShouldCallOnFactorySavingMethods)
			{
				base.OnFactorySaving();
				RunInUserContextForCorrectCompany(() => OnFactorySavingInCompanyContext());
			}
		}

		protected virtual void OnFactorySavingInCompanyContext()
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value || InvoicingJob == null)
			{
				return;
			}

			if (!IsInDatabase)
			{
				var chargeCode = ChargeCode;
				if (chargeCode != null)
				{
					if (chargeCode.AC_GovtChargeCode != JR_CostGovtChargeCode)
					{
						var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Cost Government Charge Code changed from Default value: '{0}'. New Value: '{1}'", chargeCode.AC_GovtChargeCode, JR_CostGovtChargeCode);
						AddLogToAppropriateBO(logReference);
					}
					if (chargeCode.AC_GovtChargeCode != JR_SellGovtChargeCode)
					{
						var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Sell Government Charge Code changed from Default value: '{0}'. New Value: '{1}'", chargeCode.AC_GovtChargeCode, JR_SellGovtChargeCode);
						AddLogToAppropriateBO(logReference);
					}
				}
			}
			else
			{
				if ((ZString)JR_CostGovtChargeCodeInfo.OriginalValue != JR_CostGovtChargeCode)
				{
					var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Cost Government Charge Code Edited. New Value: '{0}', Old Value: '{1}'", JR_CostGovtChargeCode, JR_CostGovtChargeCodeInfo.OriginalValue);
					AddLogToAppropriateBO(logReference);
				}
				if ((ZString)JR_SellGovtChargeCodeInfo.OriginalValue != JR_SellGovtChargeCode)
				{
					var logReference = string.Format(CultureInfo.InvariantCulture, (NoResString)"Sell Government Charge Code Edited. New Value: '{0}', Old Value: '{1}'", JR_SellGovtChargeCode, JR_SellGovtChargeCodeInfo.OriginalValue);
					AddLogToAppropriateBO(logReference);
				}
			}
		}

		void AddLogToAppropriateBO(string logReference)
		{
			if (!InvoicingJob.IsGatewayBillingJob())
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				InvoicingJob.Logs.AddNew(Events.EditedARecord, logReference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
			else
			{
				var obj = InvoicingJob.Parent as EnterpriseBusinessObject;
				if (obj != null)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					obj.Logs.AddNew(Events.EditedARecord, logReference);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		protected sealed override void OnFactorySavingBeforeTransactionCore()
		{
			var hasChangesBeforeCaller = this.HasChanges;

#if DEBUG
			if (Globals.IsTest)
			{
				OnFactorySavingBeforeTransactionCore_InvokeTestOnly?.Invoke(this);
			}
#endif
			if (ShouldCallOnFactorySavingMethods)
			{
				base.OnFactorySavingBeforeTransactionCore();
				RunInUserContextForCorrectCompany(() => OnFactorySavingBeforeTransactionCoreInCompanyContext());
			}

			this.RunSingleActionPerTransaction("ReportJobChargeIsChangedByDifferentCompany", () => ReportJobChargeIsChangedByDifferentCompany(nameof(OnFactorySavingBeforeTransactionCore), hasChangesBeforeCaller));
		}

		void ReportJobChargeIsChangedByDifferentCompany(string callerMethod, bool hasChangesBeforeCaller)
		{
#if DEBUG
			if (Globals.IsTest && MasterFiles.Business.Testing.SuspendToTestReportJobChargeIsChangedByDifferentCompanyAttribute.IsActive)
			{
				return;
			}
#endif
			if (!HasChanges || !hasChangesBeforeCaller || JR_GC == GlbCompany.CurrentCompany.PK)
			{
				return;
			}

			/*
			 * For now, we understand that the non-login company's charge could be loaded as common requirement by such as Forwarding Team
			 * When you deal this issue
			 *  If you find BusinessContext.InvoicingPlugInGUI, please find what cause the change(s) and stop the changing.
			 *  --> It is because CW1 GUI should only change login company's data.
			 *  ** Suspicious code list
			 *  *** ChargeWithCost -> OnFactorySavingInCompanyContext -> ProcessAccrualAndWIP()
			 * 
			 *  If you do not find BusinessContext.InvoicingPlugInGUI, please take note and assign the WI to very senior developer.
			 *  --> It is because the issue can be caused by service task or other unknown process.
			 *  --> And we need to analysis the behavior more detail.
			 */

			var service = CriticalValidationInfoCollectorService.GetService(Factory);
			var messageBuilder = new StringBuilder();
			messageBuilder.AppendLine((NoResString)"Charge is not designed to be changed in different company.");
			messageBuilder.AppendLine(this.GetAllPropertyValues());
			messageBuilder.AppendLine();
			messageBuilder.AppendLine($"Constructor StackTrace:\r\n{CurrentJobChargeConstructorStackTrace}");
			messageBuilder.AppendLine(CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_InvoiceType_PropertyValueSet));
			messageBuilder.AppendLine(CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OH_CostAccount_PropertyValueSet));
			messageBuilder.AppendLine(CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_CostReference_PropertyValueSet));
			messageBuilder.AppendLine(CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_OH_SellAccount_PropertyValueSet));
			messageBuilder.AppendLine(CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_E6_PropertyValueSet));
			messageBuilder.AppendLine(CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.JobChargeJR_JCNotEqualToCurrentCompany));
			messageBuilder.AppendLine(CriticalValidationInfoCollectorService.GetService(Factory).GetInfoSafe(PK, CriticalValidationInfoCollectorServiceKeyType.OsSellExRateGetChangedDuringRunningNonAccountingCode));

			messageBuilder.AppendLine($"UserContextSwitchLog:\r\n{GetUserContextSwitchLog()}");
			messageBuilder.AppendLine($"Parent JobConsolCost Constructor StackTrace:\r\n{GetCtorJobConsolCost()}");

			ExceptionReporter.Instance.ReportDeveloperException("JobChargeIsChangedByDifferentCompany_4", messageBuilder.ToString(),
				new DeveloperNotificationException((NoResString)"Charge was changed by a company different to the one that the charge belongs to."));

			string GetCtorJobConsolCost()
			{
				var cachedJobConsolCost = JR_E6.IsValid
					? (IHaveConstructorStackTrace)Factory.LoadTop1<JobConsolCost>(new ZQuery(JobConsolCostSchema.PK, JR_E6) { FetchOnlyFromLocalCache = true })
					: null;
				return cachedJobConsolCost != null
					? cachedJobConsolCost.ConstructorStackTrace.ToString()
					: (NoResString)"Parent ConsolCost had not be loaded or not existed or the \"Collect Constructor Call Stack Details\" registry is not enabled.";
			}

			string GetUserContextSwitchLog()
			{
				if (Env.Instance.UserContextLogger is UserContextSwitchLogger logger)
				{
					return string.Join(System.Environment.NewLine, logger.Logs);
				}

				return (NoResString)"No UserContextSwitchLog";
			}
		}

		protected virtual void OnFactorySavingBeforeTransactionCoreInCompanyContext()
		{
		}

#if DEBUG
		public Action<JobCharge> OnFactorySavingBeforeTransactionCore_InvokeTestOnly;
#endif

		public sealed override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			RunInUserContextForCorrectCompany(() => OnSavedInCompanyContext(saveSucceeded));
		}

		protected virtual void OnSavedInCompanyContext(bool saveSucceeded)
		{
		}

		protected sealed override void OnFactorySaved(bool saveSucceeded)
		{
			if (ShouldCallOnFactorySavingMethods)
			{
				base.OnFactorySaved(saveSucceeded);
				RunInUserContextForCorrectCompany(() => OnFactorySavedInCompanyContext(saveSucceeded));
			}
		}

		protected virtual void OnFactorySavedInCompanyContext(bool saveSucceeded)
		{
		}

		protected void RunInUserContextForCorrectCompany(Action method)
		{
			var newTempUsercontext = GetUserContextForCorrectCompany();
			if (newTempUsercontext != Env.CurrentUserContext)
			{
				using (SetTemporaryUserContext())
				{
					method();
				}
			}
			else
			{
				method();
			}

			IDisposable SetTemporaryUserContext()
			{
				if (Job != null &&
				newTempUsercontext.Company.PK == Job.JH_GC.ToGuid() &&
				newTempUsercontext.Branch.PK != Guid.Empty &&
				newTempUsercontext.Department.PK != Guid.Empty)
				{
					return Job.AddInterCompanyJobOperationBusinessContextAfterLoginToJobCompany(newTempUsercontext);
				}
				else
				{
					return Env.SetTemporaryUserContext(newTempUsercontext);
				}
			}
		}

		protected virtual IUserContext GetUserContextForCorrectCompany()
		{
			IUserContext result = Env.CurrentUserContext;

			if (!IsDeleted && Branch != null && Branch.IsInDatabase && Env.CurrentCompany.PK != Branch.GB_GC)
			{
				Guid departmentPK = JR_GE.IsValid ? JR_GE.ToGuid() : result.Department.PK;

				var strategy = new DefaultErrorReportStrategy() { IsSilentReport = true };
				result = new UserContext(result.User.LoginName, Branch.PK.ToGuid(), departmentPK, strategy, factory: Factory);

				if (strategy.NotificationMessages.Count > 0)
				{
					throw new ZCannotSaveException(strategy.NotificationMessages[0], "Cannot Save the Charge");
				}
			}

			return result;
		}

		protected virtual void OnSavingInCompanyContext()
		{
			// When posting revenue, we want to lock down the charge type and margin percentage
			if (IsRevenuePosted && JR_ChargeType.IsEmpty)
			{
				JR_MarginPercentage = MarginPercentage;
				JR_ChargeType = ChargeType;
			}
		}

		#endregion

		#region ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails

		void AddApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetailsInfo<T>(string propertyDescription, T? consolCostPropertyValue, T? chargePropertyValue, T? oldChargePropertyValue)
			where T : struct
		{
			if (JR_IsApportioned &&
				ParentConsolCost != null &&
				((consolCostPropertyValue.HasValue != chargePropertyValue.HasValue) ||
				(consolCostPropertyValue.HasValue && chargePropertyValue.HasValue && !consolCostPropertyValue.Value.Equals(chargePropertyValue.Value))))
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Factory).AddInfoWhenAllowed(PK, CriticalValidationInfoCollectorServiceKeyType.ApportionSplitChargeInvoiceDetailsNotEqualToParentConsolCostInvoiceDetails, () =>
				{
					var infoBuilder = new ZStringBuilder(Invariant($"Charge's {propertyDescription} is changed from {oldChargePropertyValue} to {chargePropertyValue}. ConsolCost's {propertyDescription} is {consolCostPropertyValue}."));
					infoBuilder.Append((NoResString)"StackTrace -->");
					infoBuilder.Append(System.Environment.StackTrace);
					return infoBuilder.ToStringWithNewLineBetweenAppends();
				});
			}
		}

		static ZString ReplaceEmptyStringWithQuotes(ZString text) => text.IsEmpty ? (ZString)"''" : text;

		#endregion
	}
}
