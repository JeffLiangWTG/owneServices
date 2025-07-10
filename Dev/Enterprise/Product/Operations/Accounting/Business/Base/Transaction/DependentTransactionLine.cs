using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using AccGenericCharge = Enterprise.Accounting.Business.GenericCharge.GenericCharge;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public abstract partial class DependentTransactionLine : TransactionLine, ISupportMultiSubAccounts
	{
		#region Schema

		public new abstract class Schema : TransactionLine.Schema
		{
			public const string AL_Calc_FirstSubClassParentId = "AL_Calc_FirstSubClassParentId";
			public const string AL_Calc_FirstSubClassParent = "AL_Calc_FirstSubClassParent";
			public const string AL_Calc_SecondSubClassParentId = "AL_Calc_SecondSubClassParentId";
			public const string AL_Calc_SecondSubClassParent = "AL_Calc_SecondSubClassParent";
			public const string AL_LocalGSTAmount = "AL_LocalGSTAmount";
			public const string AL_OSGSTAmount = "AL_OSGSTAmount";
			public const string AL_LocalExtraTaxAmount = "AL_LocalExtraTaxAmount";
			public const string AL_OSExtraTaxAmount = "AL_OSExtraTaxAmount";
			public const string AL_OSEDUPrimaryAmount = "AL_OSEDUPrimaryAmount";
			public const string AL_LocalEDUPrimaryAmount = "AL_LocalEDUPrimaryAmount";
			public const string AL_OSEDUSecondaryAmount = "AL_OSEDUSecondaryAmount";
			public const string AL_LocalEDUSecondaryAmount = "AL_LocalEDUSecondaryAmount";
			public const string BranchName = "BranchName";
			public const string DepartmentDescription = "DepartmentDescription";
			public const string TaxBranchName = "TaxBranchName";
		}

		#endregion

		public DependentTransactionLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new DependentTransactionLineFetchStrategy(this);
		}

		#region Overrides

		protected override bool IsTransactionHeaderReversingCore
		{
			get { return MasterTransactionHeader != null && (MasterTransactionHeader.IsReversing || MasterTransactionHeader.IsReversed || MasterTransactionHeader.IsReverseTransaction); }
		}

		public override void OnLoaded()
		{
			base.OnLoaded();

			if (AL_GSTVAT == 0m && (TaxRate == null || (TaxRate != null && !TaxRate.IsVATRemittedByCustomer)))
			{
				fAL_OSExtraTaxAmount = 0m;
			}
			else
			{
				using (SuspendSettingHasChanges())
				{
					if (ShouldCalculateOSExtraTaxFromOSExTax)
					{
						CalculateGSTAndExtraTaxFromOSExTaxAmount();
					}
					else
					{
						CalculateGSTAndExtraTaxFromOSTaxAmount();
					}
				}
			}
		}

		protected override ZDecimal CalculateOSTaxAmount()
		{
			return RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), TransactionCurrency, Company.PK, withoutRounding: true));
		}

		internal ZDecimal CalculateExpectedOSTaxAmount()
		{
			return CalculateOSTaxAmount();
		}

		public bool IsOutsideExpectedTaxAmount()
		{
			ZDecimal marginOfError = 0.05M;
			ZDecimal expectedTaxAmount = CalculateExpectedOSTaxAmount();
			ZDecimal minExpectedTaxAmount = expectedTaxAmount * (1 - marginOfError);
			ZDecimal maxExpectedTaxAmount = expectedTaxAmount * (1 + marginOfError);
			return Math.Abs(AL_OSTaxAmount) > Math.Abs(maxExpectedTaxAmount) || Math.Abs(AL_OSTaxAmount) < Math.Abs(minExpectedTaxAmount);
		}

		protected override void UpdateAL_OSTaxAmountCore()
		{
			base.UpdateAL_OSTaxAmountCore();
			AL_OSGSTAmount = RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, AL_TaxRateCalc, TransactionCurrency, Company.PK, withoutRounding: true));
			AL_OSExtraTaxAmount = RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, GetEffectiveExtraRate(), TransactionCurrency, Company.PK, withoutRounding: true));
		}

		protected override void UpdateAL_LocalTaxAmountCore()
		{
			base.UpdateAL_LocalTaxAmountCore();
			AL_LocalGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalExTaxAmount(Factory, AL_GC, AL_LocalExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSGSTAmount, AL_ExchangeRate);
			AL_LocalExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalExTaxAmount(Factory, AL_GC, AL_LocalExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSExtraTaxAmount, ZDecimal.Zero, AL_ExchangeRate);
		}

		#region Overriden Amount Properties

		#region AL_LocalExTaxAmount

		public override ZDecimal AL_LocalExTaxAmount
		{
			get { return base.AL_LocalExTaxAmount; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					bool hasChanged = AL_LocalExTaxAmount != value;
					base.AL_LocalExTaxAmount = value;
					if (hasChanged && MasterTransactionHeader != null)
					{
						MasterTransactionHeader.UpdateAH_LocalExTaxAmount();
					}
				}
				if (LineValidation_MightBeNull != null)
				{
					LineValidation_MightBeNull.ValidateAL_OSExTaxAmount();
				}
			}
		}

		#endregion

		#region AL_LocalTaxAmount

		public override ZDecimal AL_LocalTaxAmount
		{
			get { return base.AL_LocalTaxAmount; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					bool hasChanged = AL_LocalTaxAmount != value;
					base.AL_LocalTaxAmount = value;
					if (hasChanged)
					{
						if (!ShouldCalculateOSExtraTaxFromOSExTax)
						{
							CalculateGSTAndExtraTaxFromLocalTaxAmount();
						}

						if (MasterTransactionHeader != null)
						{
							MasterTransactionHeader.UpdateAH_LocalTaxAmount();
						}
					}
				}
			}
		}

		#endregion

		#region AL_LocalWHTAmount

		public override ZDecimal AL_LocalWHTAmount
		{
			get { return base.AL_LocalWHTAmount; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					bool hasChanged = AL_LocalWHTAmount != value;
					base.AL_LocalWHTAmount = value;
					if (hasChanged && MasterTransactionHeader != null)
					{
						MasterTransactionHeader.UpdateAH_LocalWHTAmount();
					}
				}
			}
		}

		#endregion

		#region AL_OSExTaxAmount

		protected override ZDecimal AL_OSExTaxAmountCore
		{
			get { return base.AL_OSExTaxAmountCore; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					bool hasChanged = AL_OSExTaxAmountCore != value;
					base.AL_OSExTaxAmountCore = value;
					if (hasChanged && MasterTransactionHeader != null)
					{
						MasterTransactionHeader.UpdateAH_OSExTaxAmount();
						MasterTransactionHeader.UpdateAH_LocalExTaxAmount();
					}
				}
			}
		}

		#endregion

		#region AL_OSTaxAmount

		public override ZDecimal AL_OSTaxAmount
		{
			get { return base.AL_OSTaxAmount; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					bool hasChanged = AL_OSTaxAmount != value;
					base.AL_OSTaxAmount = value;
					if (hasChanged)
					{
						if (!ShouldCalculateOSExtraTaxFromOSExTax)
						{
							CalculateGSTAndExtraTaxFromOSTaxAmount();
						}

						if (MasterTransactionHeader != null)
						{
							MasterTransactionHeader.UpdateAH_OSTaxAmount();
							MasterTransactionHeader.UpdateAH_LocalTaxAmount();
						}
					}
				}
			}
		}

		#endregion

		#region AL_OSWHTAmount

		public override ZDecimal AL_OSWHTAmount
		{
			get { return base.AL_OSWHTAmount; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					bool hasChanged = AL_OSWHTAmount != value;
					base.AL_OSWHTAmount = value;
					if (hasChanged && MasterTransactionHeader != null)
					{
						MasterTransactionHeader.UpdateAH_OSWHTAmount();
					}
				}
			}
		}

		#endregion

		#region AL_OverseasTotal

		public override ZDecimal AL_OverseasTotal
		{
			get { return base.AL_OverseasTotal; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					bool hasChanged = AL_OverseasTotal != value;
					base.AL_OverseasTotal = value;
					if (hasChanged && MasterTransactionHeader != null)
					{
						MasterTransactionHeader.UpdateAH_OSTotalAmount();
					}
				}
			}
		}

		protected virtual bool AL_OverseasTotal_ReadOnly => true;

		#endregion

		#endregion

		public override ZDecimal AL_ExchangeRate
		{
			get { return base.AL_ExchangeRate; }
			set
			{
				base.AL_ExchangeRate = value;

				if (!IsLocalAmountRecalculationSuspended)
				{
					RecalculateLocalAmounts();
					RecalculateTaxAmounts();
				}
			}
		}

		protected virtual bool AL_ExchangeRate_ReadOnly
		{
			get { return AL_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency; }
		}

		#endregion

		#region TaxRateOverrideCalculator

		public TaxRateOverrideCalculator TaxRateOverrideCalculator
		{
			get
			{
				if (taxRateOverrideCalculator == null)
				{
					taxRateOverrideCalculator = CreateTaxRateOverrideCalculator();
				}
				return taxRateOverrideCalculator;
			}
		}
		TaxRateOverrideCalculator taxRateOverrideCalculator;

		protected virtual TaxRateOverrideCalculator CreateTaxRateOverrideCalculator()
		{
			return new TaxRateOverrideCalculator(Factory, () => null);
		}

		#endregion

		public override ZGuid AL_AG
		{
			get { return base.AL_AG; }
			set
			{
				base.AL_AG = value;

				SubAccountHelper.SetSubClassParentTableCode(this);
			}
		}

		public override ZGuid AL_JH
		{
			get { return base.AL_JH; }
			set
			{
				if (AL_JH != value)
				{
					base.AL_JH = value;

					SubAccountHelper.SetSubClassParentTableCode(this);
				}
			}
		}

		public ZString BranchName
		{
			get { return base.Branch != null ? Branch.GB_BranchName : ZString.Empty; }
		}

		public ZPropertyInfo BranchNameInfo
		{
			get { return GetZPropertyInfo(Schema.BranchName); }
		}

		#region Tax Branch

		protected bool AL_GB_TaxBranch_ReadOnly { get; } = true;

		public ZString TaxBranchName
		{
			get { return base.TaxBranch != null ? TaxBranch.GB_BranchName : ZString.Empty; }
		}

		protected bool TaxBranchName_ReadOnly { get; } = true;

		public ZPropertyInfo TaxBranchNameInfo
		{
			get { return GetZPropertyInfo(Schema.TaxBranchName); }
		}

		internal FunctionalitySuspender TaxBranchCalculationSuspender => taxBranchCalculatitonSuspender ?? (taxBranchCalculatitonSuspender = new FunctionalitySuspender());
		FunctionalitySuspender taxBranchCalculatitonSuspender;

		#endregion

		public ZString DepartmentDescription
		{
			get { return Department != null ? Department.GE_Desc : ZString.Empty; }
		}

		public ZPropertyInfo DepartmentDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.DepartmentDescription); }
		}

		public override ZString AL_PlaceOfSupply
		{
			get
			{
				return base.AL_PlaceOfSupply;
			}
			set
			{
				if (AL_PlaceOfSupply != value)
				{
					base.AL_PlaceOfSupply = value;

					if ((MasterTransactionHeader?.NeedPlaceOfSupplyAtHeaderLevel ?? false) && MasterTransactionHeader.AH_PlaceOfSupply != value)
					{
						MasterTransactionHeader.AH_PlaceOfSupply = value;
					}
				}
			}
		}

		public virtual bool IsEnforceBranchLevelPostingValidationApplicable
		{
			get { return true; }
		}

		#region Master TransactionHeader

		protected TransactionHeaderWithLines fMasterTransactionHeader;
		internal protected virtual TransactionHeaderWithLines MasterTransactionHeader
		{
			get
			{
				if (fMasterTransactionHeader == null)
				{
					foreach (BusinessObjectCollection parentCollection in ((IBusinessObjectInternals)this).ParentCollections)
					{
						BusinessObjectCollection originalParentCollection;

						var parentCollectionAsView = parentCollection as FilteredInvoicingLineBaseCollectionView;
						if (parentCollectionAsView != null)
						{
							originalParentCollection = parentCollectionAsView.CollectionToFilter;
						}
						else
						{
							originalParentCollection = parentCollection;
						}

						var parentCollectionAsDependentLineCollection = originalParentCollection as DependentTransactionLineCollection;
						if (parentCollectionAsDependentLineCollection != null)
						{
							fMasterTransactionHeader = parentCollectionAsDependentLineCollection.ParentTransactionHeader;
							break;
						}
					}
				}
				return fMasterTransactionHeader;
			}
		}

		FunctionalitySuspenderWrapper TransactionHeaderAmountsUpdateSuspender
		{
			get
			{
				return new FunctionalitySuspenderWrapper(() =>
					{
						return MasterTransactionHeader == null ? null : MasterTransactionHeader.GetHeaderAmountsUpdateSuspender();
					});
			}
		}

		#endregion

		#region Additional GST and Extra Tax fields

		#region AL_LocalGSTAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalGSTAmount
		{
			get { return fAL_LocalGSTAmount * Multiplier; }
			set
			{
				var valueToSet = value * Multiplier;
				if (fAL_LocalGSTAmount != valueToSet)
				{
					SetNonPersistentPropertyValue(AL_LocalGSTAmountInfo, ref fAL_LocalGSTAmount, RoundAmountToCurrencyDecimals(value * Multiplier));
				}
			}
		}
		ZDecimal fAL_LocalGSTAmount;

		public ZPropertyInfo AL_LocalGSTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalGSTAmount); }
		}

		#endregion

		#region AL_OSGSTAmount

		ZDecimal fAL_OSGSTAmount;
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSGSTAmount
		{
			get { return fAL_OSGSTAmount * Multiplier; }
			set
			{
				var valueToSet = value * Multiplier;
				if (fAL_OSGSTAmount != valueToSet)
				{
					SetNonPersistentPropertyValue(AL_OSGSTAmountInfo, ref fAL_OSGSTAmount, RoundAmountToCurrencyDecimals(valueToSet), !InvertSigns);
					AL_LocalGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, AL_GC, AL_LocalTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), value, AL_ExchangeRate);

					if (!IsValidationSuspended)
					{
						if (LineValidation_MightBeNull != null)
						{
							LineValidation_MightBeNull.ValidateAL_OSGSTAmount();
						}
					}

					AL_OSGSTAmountInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AL_OSGSTAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSGSTAmount); }
		}

		#endregion

		#region Taxes and Extra Taxes From OSExTaxAmount

		void CalculateGSTAndExtraTaxFromOSExTaxAmount()
		{
			var osGSTAmount = ZDecimal.Zero;
			var osExtraTaxAmount = ZDecimal.Zero;

			if (!AL_OSExTaxAmount.IsEmpty)
			{
				osGSTAmount = RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSGSTAmountFromOSExTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, AL_TaxRateCalc, TransactionCurrency, Company.PK, withoutRounding: true));
				osExtraTaxAmount = RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSExtraTaxAmountFromOSExTaxAmount(Factory, AL_OSExTaxAmount, TaxRate, GetEffectiveExtraRate(), TransactionCurrency, Company.PK, withoutRounding: true));
			}

			AL_OSGSTAmount = osGSTAmount;
			AL_OSExtraTaxAmount = osExtraTaxAmount;
		}

		bool ShouldCalculateOSExtraTaxFromOSExTax => TaxRate != null && TaxRate.IsMexicoNeedExtraType;
		bool ShouldCalculateLocalExtraTaxFromTaxAmount => TaxRate != null && TaxRate.IsMexicoNeedExtraType;

		#endregion

		void CalculateGSTAndExtraTaxFromOSTaxAmount()
		{
			AL_OSGSTAmount = CalculateOSGST();
			AL_OSExtraTaxAmount = CalculateOSExtraTaxAmountFromTaxAmount();
		}

		void CalculateGSTAndExtraTaxFromLocalTaxAmount()
		{
			AL_LocalGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, AL_GC, AL_LocalTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSGSTAmount, AL_ExchangeRate);
			AL_LocalExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromLocalTaxAmount(Factory, AL_GC, AL_LocalTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate());
		}

		ZDecimal CalculateOSGST()
		{
			if (!AL_OSTaxAmount.IsEmpty)
			{
				return RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSGSTAmountFromOSTaxAmount(Factory, AL_OSTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), TransactionCurrency, Company.PK, withoutRounding: true));
			}
			else if (AL_OSTaxAmount.IsEmpty && TaxRate != null && TaxRate.IsVATRemittedByCustomer)
			{
				if (!AL_GSTVATExtra.IsEmpty)
				{
					return Company.GetExchangeRate().LocalToForeign(AL_GSTVATExtra * Multiplier * -1, AL_ExchangeRate, AL_RX_NKTransactionCurrency);
				}
				return RoundAmountToCurrencyDecimals(AL_OSExTaxAmount * AL_TaxRateCalc / 100);
			}
			return ZDecimal.Zero;
		}

		ZDecimal CalculateOSExtraTaxAmountFromTaxAmount()
		{
			if (!AL_OSTaxAmount.IsEmpty)
			{
				return RoundAmountToCurrencyDecimals(TaxAmountCalculator.GetOSExtraTaxAmountFromOSTaxAmount(Factory, AL_OSTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_LocalExtraTaxAmount, AL_ExchangeRate, TransactionCurrency, Company.PK, true, true));
			}
			else if (AL_OSTaxAmount.IsEmpty && TaxRate != null && TaxRate.IsVATRemittedByCustomer)
			{
				if (!AL_GSTVATExtra.IsEmpty)
				{
					return Company.GetExchangeRate().LocalToForeign(AL_GSTVATExtra * Multiplier, AL_ExchangeRate, AL_RX_NKTransactionCurrency);
				}
				return RoundAmountToCurrencyDecimals(-AL_OSExTaxAmount * AL_TaxRateCalc / 100);
			}
			return ZDecimal.Zero;
		}

		#region AL_LocalExtraTaxAmount

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalExtraTaxAmount
		{
			get
			{
				if (TaxRate != null && TaxRate.IsLocalExtraTaxAmountValuePersistent)
				{
					return AL_GSTVATExtra * Multiplier;
				}
				else
				{
					return fAL_LocalExtraTaxAmount * Multiplier;
				}
			}
			set
			{
				bool valueChanged = false;
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					var valueToSet = value * Multiplier;

					if (TaxRate != null && TaxRate.IsLocalExtraTaxAmountValuePersistent)
					{
						if (AL_GSTVATExtra != valueToSet)
						{
							AL_GSTVATExtra = RoundAmountToCurrencyDecimals(valueToSet);
							valueChanged = true;
						}
					}
					else
					{
						if (fAL_LocalExtraTaxAmount != valueToSet)
						{
							SetNonPersistentPropertyValue(AL_LocalExtraTaxAmountInfo, ref fAL_LocalExtraTaxAmount, RoundAmountToCurrencyDecimals(valueToSet));
							valueChanged = true;
						}
						AL_GSTVATExtra = ZDecimal.Zero;
					}

					if (valueChanged)
					{
						MasterTransactionHeader?.UpdateAH_LocalExtraTaxAmount();
					}

					if (!IsValidationSuspended)
					{
						LineValidation_MightBeNull?.ValidateAL_LocalExtraTaxAmount();
					}
				}
			}
		}
		ZDecimal fAL_LocalExtraTaxAmount;

		public ZPropertyInfo AL_LocalExtraTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalExtraTaxAmount); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalQSTAmount
		{
			get { return RoundAmountToLocalDecimals(AL_OSQSTAmount, AL_ExchangeRate); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalEDUAmount
		{
			get { return RoundAmountToLocalDecimals(AL_OSEDUAmount, AL_ExchangeRate); }
		}

		#endregion

		#region AL_OSExtraTaxAmount

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSExtraTaxAmount
		{
			get { return fAL_OSExtraTaxAmount * Multiplier; }
			set
			{
				using (TransactionHeaderAmountsUpdateSuspender.GetSuspender())
				{
					var valueToSet = value * Multiplier;
					if (fAL_OSExtraTaxAmount != valueToSet)
					{
						CalculateAndSetAL_LocalExtraTaxAmount(value, (ShouldCalculateLocalExtraTaxFromTaxAmount
							|| this.HasContext(BusinessContext.RecalculatingLineTaxAmountForAdjustingTaxAtHeaderLevel)));
						SetNonPersistentPropertyValue(AL_OSExtraTaxAmountInfo, ref fAL_OSExtraTaxAmount, RoundAmountToCurrencyDecimals(valueToSet));
						if (MasterTransactionHeader != null)
						{
							MasterTransactionHeader.UpdateAH_OSExtraTaxAmount();
							MasterTransactionHeader.UpdateAH_LocalExtraTaxAmount();
						}
					}
				}
			}
		}

		protected void CalculateAndSetAL_LocalExtraTaxAmount(ZDecimal value, bool alwaysRecalculate = false)
		{
			var localExtraTaxAmount = alwaysRecalculate ? ZDecimal.Zero : AL_LocalExtraTaxAmount;
			AL_LocalExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, AL_GC, AL_LocalTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), value, localExtraTaxAmount, AL_ExchangeRate);
		}

		ZDecimal fAL_OSExtraTaxAmount;

		public ZPropertyInfo AL_OSExtraTaxAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSExtraTaxAmount); }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSQSTAmount
		{
			get
			{
				return TaxRate != null && (TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.QuebecQST || TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.QuebecQSTExcludingGSTInQSTBase) ?
					AL_OSExtraTaxAmount : ZDecimal.Zero;
			}
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSEDUAmount
		{
			get
			{
				return TaxRate != null && TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax ?
					AL_OSExtraTaxAmount : ZDecimal.Zero;
			}
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSEDUPrimaryAmount
		{
			get
			{
				return TaxRate != null && TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax ?
					RoundAmountToCurrencyDecimals(AL_OSExtraTaxAmount * EDUPrimaryPart / (EDUPrimaryPart + EDUSecondaryPart)) :
					ZDecimal.Zero;
			}
		}

		public ZPropertyInfo AL_OSEDUPrimaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSEDUPrimaryAmount); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalEDUPrimaryAmount
		{
			get { return Company.GetExchangeRate().ForeignToLocal(AL_OSEDUPrimaryAmount, AL_ExchangeRate); }
		}

		public ZPropertyInfo AL_LocalEDUPrimaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalEDUPrimaryAmount); }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSEDUSecondaryAmount
		{
			get
			{
				return TaxRate != null && TaxRate.AT_ExtraTaxRateType == AccTaxRate.ExtraTypes.IndiaPrimaryAndSecondaryEducationTax ?
					RoundAmountToCurrencyDecimals(AL_OSExtraTaxAmount * EDUSecondaryPart / (EDUPrimaryPart + EDUSecondaryPart)) :
					ZDecimal.Zero;
			}
		}

		public ZPropertyInfo AL_OSEDUSecondaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_OSEDUSecondaryAmount); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AL_LocalEDUSecondaryAmount
		{
			get { return RoundAmountToLocalDecimals(AL_OSEDUSecondaryAmount, AL_ExchangeRate); }
		}

		public ZPropertyInfo AL_LocalEDUSecondaryAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AL_LocalEDUSecondaryAmount); }
		}

		readonly ZDecimal EDUPrimaryPart = 2;
		readonly ZDecimal EDUSecondaryPart = 1;

		#endregion

		#endregion

		#region AlternateGLAccount

		AccAlternateGLAccount AlternateGLAccount
		{
			get
			{
				AccAlternateGLAccount accAlternateGLAccount = null;
				var gLAccountSelectionAndEntry = AccountingMasterFilesRegistry.Instance.GLAccountSelectionAndEntry.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

				if (gLAccountSelectionAndEntry != Guid.Empty && GLHeader != null)
				{
					var attributesInDissection = GLHeader.AlternateGLAccountDissections
						.Cast<AccAlternateGLAccountDissection>()
						.Where(x => x.ADC_AAC_AlternateChart == gLAccountSelectionAndEntry && x.ADC_SeparateNumbering)
						.Select(x => x.ADC_Attribute);
					var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccountAttribute));
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AG_GLHeader, AL_AG);
					query.AddToFilter(AccAlternateGLAccountAttributeSchema.AAA_AAC_AlternateChart, gLAccountSelectionAndEntry);
					var attributes = Factory.Load<AccAlternateGLAccountAttribute>(query);
					if (attributes.Any())
					{
						if (!attributesInDissection.Any())
						{
							accAlternateGLAccount = attributes.First().AlternateGLAccount;
						}
						else
						{
							var attributesGroups = attributes.GroupBy(x => x.AAA_Sequence);
							foreach (var group in attributesGroups)
							{
								if (CheckAttributesGroupIsMatch(attributesInDissection, group))
								{
									accAlternateGLAccount = group.First().AlternateGLAccount;
									break;
								}
							}
						}
					}
				}

				return accAlternateGLAccount;
			}
		}

		bool CheckAttributesGroupIsMatch(IEnumerable<ZString> attributesInDissection, IEnumerable<AccAlternateGLAccountAttribute> attributesInGroup)
		{
			var isMatch = true;

			foreach (var attributeInDissection in attributesInDissection)
			{
				var attribute = attributesInGroup.FirstOrDefault(x => x.AAA_Attribute == attributeInDissection);
				switch (attributeInDissection)
				{
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG:

						isMatch = isMatch && attribute != null && attribute.AAA_AttributeValueID == ORGValue;

						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG:

						isMatch = isMatch && attribute != null && attribute.AAA_Value == OCGValue;

						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO:

						isMatch = isMatch && attribute != null && attribute.AAA_Value == LFOValue;

						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE:

						isMatch = isMatch && attribute != null && attribute.AAA_Value == LFEValue;

						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC:

						isMatch = isMatch && attribute != null && attribute.AAA_Value == TICValue;

						break;
					case AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR:

						isMatch = isMatch && attribute != null && attribute.AAA_Value == SPRValue;

						break;
				}
			}

			return isMatch;
		}

		ZGuid ORGValue
		{
			get
			{
				var result = ZGuid.Empty;

				if (AccTransactionLineDissectionAttributes.Any())
				{
					result = AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(y => y.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG).ALD_AttributeValueID;
				}
				else
				{
					result = TransactionHeader?.AH_OH ?? ZGuid.Empty;
				}

				return result;
			}
		}

		string OCGValue
		{
			get
			{
				var result = AccountingMasterFilesConstants.NAV.Code;

				if (AccTransactionLineDissectionAttributes.Any())
				{
					result = AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(y => y.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG).ALD_AttributeValue.ToString();
				}
				else if (TransactionHeader?.Header != null)
				{
					result = AccountingMasterFilesRegistry.Instance.ConsolidatedAccountingCategoryList.Value.Cast<CodeDescriptionWithGroup>().FirstOrDefault(x => x.Code == TransactionHeader.Header.CompanyData.OB_ARConsolidatedAccountingCategory)?.Group.ToString() ?? AccountingMasterFilesConstants.NAV.Code;
				}

				return result;
			}
		}

		string LFOValue
		{
			get
			{
				var result = AccountingMasterFilesConstants.NAV.Code;

				if (AccTransactionLineDissectionAttributes.Any())
				{
					result = AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(y => y.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO).ALD_AttributeValue.ToString();
				}
				else if (Company.Country.Code == GlbCompany.CurrentCompany.Country.Code)
				{
					result = AccountingMasterFilesConstants.LFOCodes.LOC;
				}
				else
				{
					result = AccountingMasterFilesConstants.LFOCodes.FOR;
				}

				return result;
			}
		}

		string LFEValue
		{
			get
			{
				var result = AccountingMasterFilesConstants.NAV.Code;

				if (AccTransactionLineDissectionAttributes.Any())
				{
					result = AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(y => y.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE).ALD_AttributeValue.ToString();
				}
				else if (Company.Country.Code == GlbCompany.CurrentCompany.Country.Code)
				{
					result = AccountingMasterFilesConstants.LFECodes.LOC;
				}
				else if (Company.Country.RN_EconomicGrouping == EconomicGroupList.Codes.EuropeanUnion)
				{
					result = AccountingMasterFilesConstants.LFECodes.WEU;
				}
				else
				{
					result = AccountingMasterFilesConstants.LFECodes.OEU;
				}

				return result;
			}
		}

		string TICValue
		{
			get
			{
				var result = AccountingMasterFilesConstants.NAV.Code;

				if (AccTransactionLineDissectionAttributes.Any())
				{
					result = AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(y => y.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC).ALD_AttributeValue.ToString();
				}
				else if (TransactionHeader != null && TransactionHeader.AH_Ledger != LedgerTypes.General && AL_AT.IsValid)
				{
					if (AL_TaxExtraRateNumerator > 0)
					{
						result = AccountingMasterFilesConstants.TICCodes.ETI;
					}
					else if (AL_TaxExtraRateNumerator == 0)
					{
						result = AccountingMasterFilesConstants.TICCodes.STI;
					}
				}

				return result;
			}
		}

		string SPRValue
		{
			get
			{
				var result = AccountingMasterFilesConstants.NAV.Code;

				if (AccTransactionLineDissectionAttributes.Any())
				{
					result = AccTransactionLineDissectionAttributes.Cast<AccTransactionLineDissectionAttribute>().FirstOrDefault(y => y.ALD_Attribute == AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR).ALD_AttributeValue.ToString();
				}
				else if (TransactionHeader != null)
				{
					if (TransactionHeader.AH_TransactionType == TransactionTypes.CreditNote
						|| ((TransactionHeader.AH_TransactionType == TransactionTypes.DirectReceipt || TransactionHeader.AH_TransactionType == TransactionTypes.DirectPayment)
							&& TransactionHeader.AH_TransactionBelongsToGroup.IsValid
							&& TransactionHeader.IsCancelled))
					{
						result = AccountingMasterFilesConstants.SPRCodes.SPR;
					}
					else if (TransactionHeader.AH_TransactionType == TransactionTypes.DirectReceipt || TransactionHeader.AH_TransactionType == TransactionTypes.DirectPayment || TransactionHeader.AH_TransactionType == TransactionTypes.Invoice)
					{
						result = AccountingMasterFilesConstants.SPRCodes.SPS;
					}
				}

				return result;
			}
		}

		public string AlternateGLAccountNumber => AlternateGLAccount?.AGA_AccountNum ?? string.Empty;

		public string AlternateGLAccountDescription => AlternateGLAccount?.AGA_Description ?? string.Empty;

		#endregion

		#region Implementation

		protected virtual bool AL_OSTaxAmount_ReadOnly
		{
			get { return !(IsCurrentCompanyGSTRegistered && AllowUserGSTOverride && IsTaxRateValidForTaxAmountCalculation); }
		}

		protected bool IsTaxRateValidForTaxAmountCalculation
		{
			get { return AL_TaxRateCalc > 0; }
		}

		protected void RecalculateTaxAmounts()
		{
			AL_OSTaxAmount = CalculateOSTaxAmount();
			AL_OSWHTAmount = TaxAmountCalculator.GetOSWithholdingTaxAmountFromLocalExTaxAmount(AL_LocalExTaxAmount, Withholding, AL_ExchangeRate, TransactionCurrency);
		}

		protected void RecalculateLocalAmounts()
		{
			using (AccountingSuspenders.RunMethodSuspender runMethodSuspender = new AccountingSuspenders.RunMethodSuspender(this, delegate
				{
					AL_LocalExTaxAmount = RoundAmountToLocalDecimals(AL_OSExTaxAmount, AL_ExchangeRate);
					AL_LocalTaxAmount = TaxAmountCalculator.GetLocalTaxAmount(Factory, AL_GC, AL_LocalExTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSTaxAmount, AL_ExchangeRate);
					AL_LocalWHTAmount = TaxAmountCalculator.GetLocalWithholdingTaxAmountFromLocalExTaxAmount(AL_LocalExTaxAmount, Withholding);
					AL_LocalGSTAmount = TaxAmountCalculator.GetLocalGSTAmountFromLocalTaxAmount(Factory, AL_GC, AL_LocalTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSGSTAmount, AL_ExchangeRate);
					AL_LocalExtraTaxAmount = TaxAmountCalculator.GetLocalExtraTaxAmountFromTaxAmount(Factory, AL_GC, AL_LocalTaxAmount, TaxRate, AL_TaxRateCalc, GetEffectiveExtraRate(), AL_OSExtraTaxAmount, AL_LocalExtraTaxAmount, AL_ExchangeRate);
				}))
			{
				runMethodSuspender.RunMethod();
			}
		}

		ITaxHelper TaxHelper => taxHelper_constructorInitializedOnly ?? (taxHelper_constructorInitializedOnly = new TaxHelper());
		ITaxHelper taxHelper_constructorInitializedOnly;

		public bool IsGSTMandatory => TaxHelper.IsGSTMandatory(this);

		internal bool IsCurrentCompanyGSTRegistered => GlbCompany.CurrentCompany.GC_IsGSTRegistered;

		internal bool IsCurrentOrganisationGSTRegistered
		{
			get
			{
				bool result = false;
				if (MasterTransactionHeader != null && MasterTransactionHeader.Header != null && MasterTransactionHeader.Header.MiscServ != null)
				{
					result = MasterTransactionHeader.IsMiscServTaxApplicable;
				}
				return result;
			}
		}

		protected bool AllowUserGSTOverride
		{
			get
			{
				bool result = false;

				if (MasterTransactionHeader != null)
				{
					if (MasterTransactionHeader.AH_Ledger == LedgerTypes.CashBook)
					{
						result = true;
					}
					else if (MasterTransactionHeader.Header != null && MasterTransactionHeader.Header.MiscServ != null)
					{
						if (MasterTransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable)
						{
							result =
								AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK)
								&&
								Env.Security.NewReceivablesOverrideTaxIdAllows.IsAllowed;
						}
						else
						{
							result = AccountingConfigurationRegistry.Instance.PayableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty) && Env.Security.NewPayablesOverrideTaxIDAllows.IsAllowed;
						}
					}
				}

				return result;
			}
		}

		protected bool IsCurrentChargeGLAccount
		{
			get
			{
				bool result = false;
				if (GenericTransactionCharge != null)
				{
					result = new ZBool(GenericTransactionCharge.VC_IsGLAccount);
				}
				return result;
			}
		}

		public AccGenericCharge GenericTransactionCharge
		{
			get { return fGenericTransactionCharge; }
			set { fGenericTransactionCharge = value; }
		}

		AccGenericCharge fGenericTransactionCharge;

		protected new DependentTransactionLineValidation LineValidation_MightBeNull
		{
			get { return Validation as DependentTransactionLineValidation; }
		}

		protected override AccTransactionLinesValidation GetNewValidationCore()
		{
			return new DependentTransactionLineValidation(this);
		}

		protected void CalculateBranchAndDepartmentFromJob()
		{
			if (Job != null)
			{
				this.AL_GB = Job.JH_GB;
				this.AL_GE = Job.JH_GE;
			}
		}

		#region Sub Account

		#region First Sub Account

		public IBusinessObjectCollection FirstSubAccountList => SubAccountHelper.GetSubAccountList(Factory, SubAccounts.FirstSubAccount?.AL1_SubClassParentTableCode ?? ZString.Empty);

		[List("FirstSubAccountList")]
		public ZGuid AL_Calc_FirstSubClassParentId
		{
			get => SubAccounts.FirstSubAccount?.AL1_SubClassParentId ?? ZGuid.Empty;
			set
			{
				var firstSubAccount = SubAccounts.FirstSubAccount;
				if (firstSubAccount != null)
				{
					firstSubAccount.AL1_SubClassParentId = value;
					firstSubAccount.AL1_SubClassParentIdInfo.RefreshBinding();
				}
				AL_Calc_FirstSubClassParentIdInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AL_Calc_FirstSubClassParentIdInfo => GetZPropertyInfo(nameof(AL_Calc_FirstSubClassParentId));

		public virtual bool AL_Calc_FirstSubClassParentId_ReadOnly => SubAccountHelper.IsSubAccountReadOnly(SubAccounts.FirstSubAccount);

		public ZString AL_Calc_FirstSubClassParent => SubAccounts.FirstSubAccount?.AL1_Calc_SubClassParent ?? ZString.Empty;

		public ZPropertyInfo AL_Calc_FirstSubClassParentInfo => GetZPropertyInfo(nameof(AL_Calc_FirstSubClassParent));

		#endregion

		#region Second Sub Account

		public IBusinessObjectCollection SecondSubAccountList => SubAccountHelper.GetSubAccountList(Factory, SubAccounts.SecondSubAccount?.AL1_SubClassParentTableCode ?? ZString.Empty);

		[List("SecondSubAccountList")]
		public ZGuid AL_Calc_SecondSubClassParentId
		{
			get { return SubAccounts.SecondSubAccount?.AL1_SubClassParentId ?? ZGuid.Empty; }
			set
			{
				var subAccount = SubAccounts.SecondSubAccount;
				if (subAccount != null)
				{
					subAccount.AL1_SubClassParentId = value;
					subAccount.AL1_SubClassParentIdInfo.RefreshBinding();
				}
				AL_Calc_SecondSubClassParentIdInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AL_Calc_SecondSubClassParentIdInfo => GetZPropertyInfo(nameof(AL_Calc_SecondSubClassParentId));

		public virtual bool AL_Calc_SecondSubClassParentId_ReadOnly => SubAccountHelper.IsSubAccountReadOnly(SubAccounts.SecondSubAccount);

		public ZString AL_Calc_SecondSubClassParent => SubAccounts.SecondSubAccount?.AL1_Calc_SubClassParent ?? ZString.Empty;

		public ZPropertyInfo AL_Calc_SecondSubClassParentInfo => GetZPropertyInfo(nameof(AL_Calc_SecondSubClassParent));

		#endregion

		[ChildEditable(true)]
		public TransactionLineSubAccountCollection SubAccounts
		{
			get
			{
				if (fSubAccounts == null)
				{
					fSubAccounts = new TransactionLineSubAccountCollection(this);
					fSubAccounts.Load();
					RegisterEditableChildObject(fSubAccounts);
				}
				return fSubAccounts;
			}
		}
		TransactionLineSubAccountCollection fSubAccounts;

		protected virtual bool IsMultiSubAccountsSupportedCore => false;

		#region ISupportMutilSubAccounts Implementation

		public bool IsMultiSubAccountsSupported => IsMultiSubAccountsSupportedCore;

		bool ISupportMultiSubAccounts.IsJobRelated => AL_JH.IsValid;

		ISupportSubAccountCollection ISupportMultiSubAccounts.SubAccounts => SubAccounts;

		#endregion

		#endregion

		#endregion

#if DEBUG

		public bool IsUseDefaultTaxOverrideLogic_ForTestOnly;
		public void SubstituteTaxHelper_ForTestOnly(ITaxHelper replacement) => taxHelper_constructorInitializedOnly = replacement;
		public ITaxHelper TaxHelper_ExposedForTestOnly => TaxHelper;

#endif
	}
}
