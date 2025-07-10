using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Accounting.Helpers;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.TaxFramework.Business
{
	public class AccTaxTransaction : AutoAccTaxTransaction, ISupportCriticalValidation, ICanApplyDataRefresh, IDataVersionLoggingSupported
	{
		public AccTaxTransaction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			taxTransactionToPivotsAmountsDistributor_constructorInitializedOnly = new TaxTransactionToPivotsAmountsDistributor();
			glMovementProcessor_constructorInitializedOnly = new GLMovementProcessor();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATT_LocalTaxBaseAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATT_LocalTaxAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATT_OSTaxBaseAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATT_OSTaxAmount), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATT_RealisationDate), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATT_IsCancelled), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(ATT_AH_MatchTransaction), ConcurrencyPolicy.Strict);

			roundingMethodApplier = ObjectFactory.Get<ITaxFrameworkDependencyFactory>().GetRoundingMethodApplier();
		}

		readonly IRoundingMethodApplier roundingMethodApplier;

		#region IDataVersionLoggingSupported

		bool IDataVersionLoggingSupported.IsDataVersionsAutoLogged => (IsInDatabase && ATT_Ledger == TaxConfigurationLedgers.AccountsPayable.Code) || IsSystemCalculatedValuesInitializedAndHasChanges();

		DataVersionLogValueFormatter IDataVersionLoggingSupported.DataVersionLogValueFormatter => new TaxTransactionEditableFieldsDataVersionLogFormatter(GetSystemCalculatedValue);

		#endregion

		protected override ZString HumanReadableNameCore => Res.GetString("B382A3CE-5027-4CC3-B4DB-0C4D84176B72", "Tax record");

		public override bool CanDelete => !IsInDatabase && ATT_Ledger == TaxConfigurationLedgers.AccountsPayable.Code && Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed;

		public override MultilingualString ReasonForNotAbleToDelete => ResString.GetMultilingualString("D4BC234C-8F48-41F6-9B81-47E0E9F6A843", "Only unsaved records with AP ledger can be deleted.");

		public bool ATT_IsCancelled_ReadOnly => !IsInDatabase || !(ATT_TaxSuperType == TaxSuperTypeList.StandardPaymentRetention.Code && ATT_Ledger == TaxConfigurationLedgers.AccountsPayable.Code && ATT_RealisationDate.IsEmpty && Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed) || (TransactionHeader?.AH_IsCancelled ?? false);

		public ZBool IsInDatabase_ForBinding => IsInDatabase;

		public ZPropertyInfo IsInDatabase_ForBindingInfo => GetZPropertyInfo(nameof(IsInDatabase_ForBinding));

		public ZString ATT_AH_MatchTransaction_ForBinding
		{
			get
			{
				var matchTransactionInfoAsString = ZString.Empty;
				if (MatchTransaction != null)
				{
					matchTransactionInfoAsString = string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", MatchTransaction.AH_Ledger, MatchTransaction.AH_TransactionType, MatchTransaction.AH_TransactionNum);
				}
				return matchTransactionInfoAsString;
			}
		}

		public ZString TaxAuthorityCode => TaxConfiguration != null ? TaxConfiguration.ETC_TaxAuthorityCode : ZString.Empty;

		public override ZGuid ATT_ETC
		{
			get => base.ATT_ETC;
			set
			{
				if (!Factory.HasContext(BusinessContext.CopyingPersistentValues))
				{
					if (ATT_LocalTaxAmount != 0 || ATT_OSTaxAmount != 0)
					{
						throw new InvalidOperationException("Cannot modify ATT_ETC when Local and OS tax amounts are not 0.");
					}
				}

				base.ATT_ETC = value;
			}
		}

		[ReadOnly(true)]
		public override ZString ATT_RX_NKOSTaxCurrency
		{
			get => base.ATT_RX_NKOSTaxCurrency;
			set
			{
				if (!Factory.HasContext(BusinessContext.CopyingPersistentValues))
				{
					if (ATT_OSTaxBaseAmount != 0 || ATT_OSTaxAmount != 0)
					{
						throw new InvalidOperationException("Cannot modify currency when OS tax base amount and OS tax amount are not 0.");
					}
				}

				base.ATT_RX_NKOSTaxCurrency = value;
			}
		}

		public bool IsOSTaxCurrencyLocal => ATT_RX_NKOSTaxCurrency == (Company?.GC_RX_NKLocalCurrency ?? ZString.Empty);

		[ReadOnly(true)]
		public override ZGuid ATT_AT_TaxID
		{
			get => base.ATT_AT_TaxID;
			set
			{
				bool hasChanged = ATT_AT_TaxID != value;
				base.ATT_AT_TaxID = value;

				if (hasChanged)
				{
					Validation.ValidateATT_Rate();
				}
			}
		}

		public bool ATT_TaxAuthorityServiceCode_ReadOnly => !IsAPTaxTransactionEditable;
		public bool ATT_TaxAuthorityServiceCodeDescription_ReadOnly => !IsAPTaxTransactionEditable;

		[ReadOnly(true)]
		public override ZString ATT_TaxSystemCode { get => base.ATT_TaxSystemCode; set => base.ATT_TaxSystemCode = value; }

		[ReadOnly(true)]
		public override ZBool ATT_AffectsSourceTransactionTotal { get => base.ATT_AffectsSourceTransactionTotal; set => base.ATT_AffectsSourceTransactionTotal = value; }

		public bool ATT_TaxDate_ReadOnly => !IsAPTaxTransactionEditable;

		[ReadOnly(true)]
		public override ZDate ATT_PostDate { get => base.ATT_PostDate; set => base.ATT_PostDate = value; }

		[ReadOnly(true)]
		public override ZString ATT_Ledger { get => base.ATT_Ledger; set => base.ATT_Ledger = value; }

		[ReadOnly(true)]
		public override ZString ATT_TaxSuperType { get => base.ATT_TaxSuperType; set => base.ATT_TaxSuperType = value; }

		#region Rates

		[DecimalPlaces(3)]
		public ZDecimal ATT_Rate
		{
			get => CalculateTaxRate();
			set
			{
				bool hasChanged = ATT_Rate != value;

				if (hasChanged)
				{
					(ATT_RateNumerator, ATT_RateDenominator) = GetNumeratorDenaminatorFromRate(value);

					Validation.ValidateATT_Rate();
				}

				ATT_RateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ATT_RateInfo => GetZPropertyInfo(nameof(ATT_Rate));

		public bool ATT_Rate_ReadOnly => !IsAPTaxTransactionEditable;

		public override ZInt ATT_RateNumerator
		{
			get => base.ATT_RateNumerator;
			set
			{
				bool hasChanged = ATT_RateNumerator != value;
				base.ATT_RateNumerator = value;

				if (hasChanged)
				{
					ATT_RateInfo.RefreshBinding();
					Validation.ValidateATT_Rate();
				}
			}
		}

		public override ZInt ATT_RateDenominator
		{
			get => base.ATT_RateDenominator;
			set
			{
				bool hasChanged = ATT_RateDenominator != value;
				base.ATT_RateDenominator = value;

				if (hasChanged)
				{
					ATT_RateInfo.RefreshBinding();
					Validation.ValidateATT_Rate();
				}
			}
		}

		ZDecimal CalculateTaxRate()
		{
			return ATT_RateDenominator != 0 ? (new ZDecimal(ATT_RateNumerator) / ATT_RateDenominator) : 0;
		}

		(ZInt numerator, ZInt denominator) GetNumeratorDenaminatorFromRate(ZDecimal taxRate)
		{
			ZInt integerPart = (ZInt)taxRate.Round(0);

			ZInt numerator, denominator;
			if (integerPart == taxRate)
			{
				numerator = integerPart;
				denominator = 1;
			}
			else
			{
				var factor = (ZInt)Math.Pow(10, taxRate.Normalize().DecimalPlaces);
				numerator = (ZInt)(taxRate * factor);
				denominator = factor;
			}

			return (numerator, denominator);
		}

		internal ZDecimal EffectiveRate => effectiveRate ?? ZDecimal.Zero;
		ZDecimal? effectiveRate;

		internal void SetEffectiveRateOnTaxRecordCreation(ZDecimal rate)
		{
			if (effectiveRate.HasValue && effectiveRate.Value != rate)
			{
				throw new InvalidOperationException($"{nameof(EffectiveRate)} is already set.");
			}

			effectiveRate = rate;
		}

		internal void CalculateTaxAmountFromTaxBaseAmounts()
		{
			if (!effectiveRate.HasValue)
			{
				throw new InvalidOperationException($"{nameof(EffectiveRate)} is not set.");
			}
			ATT_OSTaxAmount = ATT_OSTaxBaseAmount * EffectiveRate;
			ATT_LocalTaxAmount = ATT_LocalTaxBaseAmount * EffectiveRate;
		}

		internal ZDecimal GetLocalTaxAmountWithoutSign() => ATT_LocalTaxAmount * Math.Sign(EffectiveRate);

		#endregion

		#region Exchange Rate

		public ZDecimal ExchangeRate
		{
			get
			{
				var exchangeRate = ZDecimal.Zero;

				if (Company?.GC_IsReciprocal ?? false)
				{
					exchangeRate = ATT_OSTaxAmount != 0 ? ATT_LocalTaxAmount / ATT_OSTaxAmount : 0;
				}
				else
				{
					exchangeRate = ATT_LocalTaxAmount != 0 ? ATT_OSTaxAmount / ATT_LocalTaxAmount : 0;
				}
				return Utilities.Round(exchangeRate, GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces);
			}
		}

		#endregion

		#region Amounts

		[DecimalPlaces(nameof(LocalCurrencyDecimals)), ReadOnly(true)]
		public override ZDecimal ATT_LocalTaxBaseAmount {
			get => base.ATT_LocalTaxBaseAmount;
			set
			{
				if (Factory.HasAnyOfContexts(BusinessContextSets.SettingExactValuesToTaxRecordProperties))
				{
					base.ATT_LocalTaxBaseAmount = value;
				}
				else
				{
					base.ATT_LocalTaxBaseAmount = Utilities.Round(value, LocalCurrencyDecimals);
				}
			}
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals)), ReadOnly(true)]
		public override ZDecimal ATT_LocalTaxAmount
		{
			get => base.ATT_LocalTaxAmount;
			set
			{
				if (Factory.HasAnyOfContexts(BusinessContextSets.SettingExactValuesToTaxRecordProperties))
				{
					base.ATT_LocalTaxAmount = value;
				}
				else
				{
					if (TaxConfiguration == null)
					{
						throw new InvalidOperationException("Could not find a valid tax configuration.");
					}
					var roundedValue = roundingMethodApplier.ApplyRounding(TaxConfiguration.ETC_TaxAmountRounding, value, false, LocalCurrencyDecimals);
					bool hasChanged = base.ATT_LocalTaxAmount != roundedValue;
					var previousValue = base.ATT_LocalTaxAmount;
					base.ATT_LocalTaxAmount = roundedValue;
					if (hasChanged && TaxParent != null)
					{
						UpdateParentTaxAmountIfApplicable(() => TaxParent.LocalTaxAmount += base.ATT_LocalTaxAmount - previousValue);
					}
				}
			}
		}

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal ATT_OSTaxBaseAmount
		{
			get => base.ATT_OSTaxBaseAmount;
			set
			{
				if (Factory.HasAnyOfContexts(BusinessContextSets.SettingExactValuesToTaxRecordProperties))
				{
					base.ATT_OSTaxBaseAmount = value;
				}
				else
				{
					bool hasChanged = base.ATT_OSTaxBaseAmount != value;
					var previousValue = base.ATT_OSTaxBaseAmount;
					base.ATT_OSTaxBaseAmount = Utilities.Round(value, OSCurrencyDecimals);

					if (hasChanged)
					{
						if (previousValue != ZDecimal.Zero && base.ATT_LocalTaxBaseAmount != ZDecimal.Zero)
						{
							taxBaseAmountRatio = base.ATT_LocalTaxBaseAmount / previousValue;
						}

						ATT_LocalTaxBaseAmount = GetPropotionalValue(taxBaseAmountRatio, value);
					}

					Validation.ValidateATT_OSTaxBaseAmount();
				}
			}
		}

		ZDecimal taxBaseAmountRatio;

		public bool ATT_OSTaxBaseAmount_ReadOnly => !IsAPTaxTransactionEditable;

		[DecimalPlaces(nameof(OSCurrencyDecimals))]
		public override ZDecimal ATT_OSTaxAmount
		{
			get => base.ATT_OSTaxAmount;
			set
			{
				if (Factory.HasAnyOfContexts(BusinessContextSets.SettingExactValuesToTaxRecordProperties))
				{
					base.ATT_OSTaxAmount = value;
				}
				else
				{
					if (TaxConfiguration == null)
					{
						throw new InvalidOperationException("Could not find a valid tax configuration.");
					}
					var roundedValue = roundingMethodApplier.ApplyRounding(TaxConfiguration.ETC_TaxAmountRounding, value, !IsOSTaxCurrencyLocal, OSCurrencyDecimals);
					bool hasChanged = base.ATT_OSTaxAmount != roundedValue;
					var previousValue = base.ATT_OSTaxAmount;
					base.ATT_OSTaxAmount = roundedValue;

					if (hasChanged)
					{
						if (previousValue != ZDecimal.Zero && base.ATT_LocalTaxAmount != ZDecimal.Zero)
						{
							taxAmountRatio = base.ATT_LocalTaxAmount / previousValue;
						}

						ATT_LocalTaxAmount = GetPropotionalValue(taxAmountRatio, roundedValue);

						if (TaxParent != null)
						{
							UpdateParentTaxAmountIfApplicable(() => TaxParent.OSTaxAmount += base.ATT_OSTaxAmount - previousValue);
						}
					}

					Validation.ValidateATT_OSTaxAmount();
				}
			}
		}
		ZDecimal taxAmountRatio;

		ZDecimal GetPropotionalValue(ZDecimal ratio, ZDecimal newValue)
		{
			return ratio * newValue;
		}

		void UpdateParentTaxAmountIfApplicable(Action updateParentTaxAmount)
		{
			if (ATT_AffectsSourceTransactionTotal
				&& !Factory.HasAnyOfContexts(BusinessContextSets.TaxRecordCreation))
			{
				using (Factory.SetTempContext(Integration.Accounting.BusinessContext.MakingChangesToOtherTaxes))
				{
					updateParentTaxAmount();
				}
			}
		}

		public bool ATT_OSTaxAmount_ReadOnly => !IsAPTaxTransactionEditable;

		int LocalCurrencyDecimals => Company?.GetLocalDecimals() ?? 0;

		int OSCurrencyDecimals => OSTaxCurrency != null ? OSTaxCurrency.Decimals : LocalCurrencyDecimals;

		#endregion

		#region Tax Parent
		public ITaxRecordParent TaxParent
		{
			get
			{
				if (taxParent == null || taxParent.PK != ATT_AH)
				{
					var taxRecordParentLoader = ObjectFactory.Get<ITaxFrameworkBOLoader>();
					taxParent = taxRecordParentLoader.LoadTaxRecordParent(Factory, ATT_AH);
				}

				return taxParent;
			}
		}
		ITaxRecordParent taxParent;
		#endregion

		[List("Lookups.TaxBasisList"), ReadOnly(true)]
		public override ZString ATT_Basis { get => base.ATT_Basis; set => base.ATT_Basis = value; }

		[ReadOnly(true)]
		public override ZDate ATT_RealisationDate
		{
			get => base.ATT_RealisationDate;
			set
			{
				if (base.ATT_RealisationDate != value)
				{
					if (base.ATT_IsCancelled || base.ATT_RealisationDate.IsEmpty || (!IsInDatabase && ATT_Basis == TaxBasisList.Posting.Code))
					{
						base.ATT_RealisationDate = value;
					}
					else
					{
						ErrorReporter.ReportOnce("TaxFramework_TaxRecordRealiser", string.Format(CultureInfo.InvariantCulture, @"Attempted to set Realisation Date on an AccTaxTransaction record which already have been realised.
New value: {0}
{1}", value, CriticalValidationInfoExtensions.GetAllPropertyValues(this)));
					}
				}
			}
		}

		[ReadOnly(true)]
		public override ZGuid ATT_A9_TaxMessage { get => base.ATT_A9_TaxMessage; set => base.ATT_A9_TaxMessage = value; }

		public TransactionLineForOtherTaxesDisplayCollection TransactionLinesLinkedToOtherTaxesCollection
		{
			get
			{
				if (transactionLinesLinkedToOtherTaxesCollection == null)
				{
					var loader = ObjectFactory.Get<ITaxFrameworkBOLoader>();
					var linePKs = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, PK)).Select(p => p.ATP_AL_TransactionLine);
					transactionLinesLinkedToOtherTaxesCollection = new TransactionLineForOtherTaxesDisplayCollection(Factory);
					transactionLinesLinkedToOtherTaxesCollection.AddRange(loader.LoadTransactionLineForOtherTaxesDisplay(Factory, new ZQuery(AccTransactionLinesSchema.PK, linePKs)));
				}
				return transactionLinesLinkedToOtherTaxesCollection;
			}
		}
		TransactionLineForOtherTaxesDisplayCollection transactionLinesLinkedToOtherTaxesCollection;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (!IsInDatabase)
			{
				if (ATT_OSTaxBaseAmount == 0 && ATT_LocalTaxBaseAmount == 0 && ATT_OSTaxAmount == 0 && ATT_LocalTaxAmount == 0)
				{
					ATT_IsCancelled = true;
					if (ATT_Basis == TaxBasisList.Matching.Code)
					{
						ATT_RealisationDate = ATT_PostDate;
					}
				}
				else if (!IsCancelled)
				{
					TaxTransactionToPivotsAmountsDistributor.AdjustPivotsLocalTaxAmountsToMatchTaxTransactionLocalTaxAmount(this);
				}
			}
		}

		ITaxTransactionToPivotsAmountsDistributor TaxTransactionToPivotsAmountsDistributor => taxTransactionToPivotsAmountsDistributor_constructorInitializedOnly;
		ITaxTransactionToPivotsAmountsDistributor taxTransactionToPivotsAmountsDistributor_constructorInitializedOnly;

		IGLMovementProcessor GLMovementProcessor => glMovementProcessor_constructorInitializedOnly;
		IGLMovementProcessor glMovementProcessor_constructorInitializedOnly;

		public override void OnSaving()
		{
			base.OnSaving();
			((ISupportCriticalValidation)this).CriticalValidation.RegisterOnSavingCheck();

			GLMovementProcessor.CreateGLMovements(this);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (!saveSucceeded)
			{
				GLMovementProcessor.DeleteGLMovementsNotInDB(this);
			}
		}

		public override void Delete()
		{
			if (!IsInDatabase)
			{
				base.Delete();
			}
			else
			{
				var message = new ZStringBuilder();
				message.Append((NoResString)"Tax Record in database cannot be deleted.");
				message.Append(this.GetAllPropertyValues());
				ErrorReporter.ReportOnce("AccTaxTransactionInDBCannotBeDeleted", message.ToStringWithNewLineBetweenAppends());
			}
		}

		ICriticalValidation ISupportCriticalValidation.CriticalValidation => new AccTaxTransactionCriticalValidation(this);

		void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext()
		{
			CriticalValidationHelpers.SetConflictWithCriticalFieldsBusinessContext(this);
		}

		#region Can Apply Data Refresh

		bool ICanApplyDataRefresh.CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher)
		{
			return ObjectFactory.Get<IDataRefreshBusUpdateActionDecider>().ShouldApplyDataRefreshBusUpdate(action, this, publisher, GetPropertiesWithStrictConcurrency());
		}

		ZPropertyInfo[] GetPropertiesWithStrictConcurrency()
		{
			return new[]
			{
				ATT_AH_MatchTransactionInfo,
				ATT_IsCancelledInfo,
				ATT_LocalTaxAmountInfo,
				ATT_LocalTaxBaseAmountInfo,
				ATT_OSTaxAmountInfo,
				ATT_OSTaxBaseAmountInfo,
				ATT_RealisationDateInfo,
			};
		}

		#endregion

		bool IsAPTaxTransactionEditable => !IsInDatabase && ATT_Ledger == TaxConfigurationLedgers.AccountsPayable.Code && EditableTaxSuperTypes.Contains(ATT_TaxSuperType) && Environment.Env.Security.PayableAllowOverrideTaxTransaction.IsAllowed;
		readonly HashSet<ZString> EditableTaxSuperTypes = new HashSet<ZString> { TaxSuperTypeList.Perceptions.Code, TaxSuperTypeList.RetentionInInvoice.Code, TaxSuperTypeList.SalesTax.Code, TaxSuperTypeList.TurnoverTax.Code, TaxSuperTypeList.ValueAddedTax.Code };

		#region Data Version Log Formatter

		class TaxTransactionEditableFieldsDataVersionLogFormatter : DataVersionLogValueFormatter
		{
			public TaxTransactionEditableFieldsDataVersionLogFormatter(Func<ZPropertyInfo, string> getOriginalValue)
			{
				GetOriginalValue = getOriginalValue;
			}

			protected override string GetStringFromPropertyValueCore(ZPropertyInfo property, DataRow row, bool useOriginalValue)
			{
				if (useOriginalValue)
				{
					var originalValue = GetOriginalValue(property);
					if (originalValue != null)
					{
						return originalValue;
					}
				}

				return base.GetStringFromPropertyValueCore(property, row, useOriginalValue);
			}

			Func<ZPropertyInfo, string> GetOriginalValue { get; }
		}

		#endregion

		#region System Calculated Values

		internal void SetTaxTransactionsSystemCalculatedValues(ZDecimal oSTaxBaseAmount, ZDecimal oSTaxAmount, ZInt rateNumerator, ZInt rateDenominator, ZDate taxDate, ZString taxAuthorityServiceCode, ZString taxAuthorityServiceCodeDescription)
		{
			if (Factory.HasAnyOfContexts(BusinessContextSets.TaxRecordCreation))
			{
				SystemCalculatedValuesWithSetters_Nullable = new TaxTransactionSystemCalculatedValues();
				SystemCalculatedValuesWithSetters_Nullable.OSTaxBaseAmount = oSTaxBaseAmount;
				SystemCalculatedValuesWithSetters_Nullable.OSTaxAmount = oSTaxAmount;
				SystemCalculatedValuesWithSetters_Nullable.RateNumerator = rateNumerator;
				SystemCalculatedValuesWithSetters_Nullable.RateDenominator = rateDenominator;
				SystemCalculatedValuesWithSetters_Nullable.TaxDate = taxDate;
				SystemCalculatedValuesWithSetters_Nullable.TaxAuthorityServiceCode = taxAuthorityServiceCode;
				SystemCalculatedValuesWithSetters_Nullable.TaxAuthorityServiceCodeDescription = taxAuthorityServiceCodeDescription;
			}
		}

		bool IsSystemCalculatedValuesInitializedAndHasChanges()
		{
			return !IsInDatabase
				&& SystemCalculatedValuesWithSetters_Nullable != null
				&& !(SystemCalculatedValuesWithSetters_Nullable.OSTaxBaseAmount.Equals(ATT_OSTaxBaseAmount)
					&& SystemCalculatedValuesWithSetters_Nullable.OSTaxAmount.Equals(ATT_OSTaxAmount)
					&& SystemCalculatedValuesWithSetters_Nullable.RateNumerator.Equals(ATT_RateNumerator)
					&& SystemCalculatedValuesWithSetters_Nullable.RateDenominator.Equals(ATT_RateDenominator)
					&& SystemCalculatedValuesWithSetters_Nullable.TaxDate.Equals(ATT_TaxDate)
					&& SystemCalculatedValuesWithSetters_Nullable.TaxAuthorityServiceCode.Equals(ATT_TaxAuthorityServiceCode)
					&& SystemCalculatedValuesWithSetters_Nullable.TaxAuthorityServiceCodeDescription.Equals(ATT_TaxAuthorityServiceCodeDescription));
		}

		string GetSystemCalculatedValue(ZPropertyInfo property)
		{
			if (!IsInDatabase && SystemCalculatedValuesWithSetters_Nullable != null)
			{
				switch (property.Name)
				{
					case nameof(ATT_OSTaxBaseAmount):
						return SystemCalculatedValuesWithSetters_Nullable.OSTaxBaseAmount.ToString();
					case nameof(ATT_OSTaxAmount):
						return SystemCalculatedValuesWithSetters_Nullable.OSTaxAmount.ToString();
					case nameof(ATT_RateNumerator):
						return SystemCalculatedValuesWithSetters_Nullable.RateNumerator.ToString();
					case nameof(ATT_RateDenominator):
						return SystemCalculatedValuesWithSetters_Nullable.RateDenominator.ToString();
					case nameof(ATT_TaxDate):
						return SystemCalculatedValuesWithSetters_Nullable.TaxDate.ToString();
					case nameof(ATT_TaxAuthorityServiceCode):
						return SystemCalculatedValuesWithSetters_Nullable.TaxAuthorityServiceCode.ToString();
					case nameof(ATT_TaxAuthorityServiceCodeDescription):
						return SystemCalculatedValuesWithSetters_Nullable.TaxAuthorityServiceCodeDescription.ToString();
				}
			}

			return null;
		}

		class TaxTransactionSystemCalculatedValues : IReadOnlyTaxTransactionSystemCalculatedValues
		{
			public ZDecimal OSTaxBaseAmount { get; set; }
			public ZDecimal OSTaxAmount { get; set; }
			public ZInt RateNumerator { get; set; }
			public ZInt RateDenominator { get; set; }
			public ZDate TaxDate { get; set; }
			public ZString TaxAuthorityServiceCode { get; set; }
			public ZString TaxAuthorityServiceCodeDescription { get; set; }
		}

		internal IReadOnlyTaxTransactionSystemCalculatedValues GetSystemCalculatedValuesIfAvailable() => SystemCalculatedValuesWithSetters_Nullable;

		TaxTransactionSystemCalculatedValues SystemCalculatedValuesWithSetters_Nullable;

		#endregion

		#region Validation

		protected override AccTaxTransactionValidation GetNewValidation()
		{
			if (ATT_IsCancelled
				|| Factory.HasContext(Integration.Accounting.BusinessContext.SavingIncompleteTransaction))
			{
				return new AccTaxTransactionEmptyValidation(this);
			}
			return base.GetNewValidation();
		}

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var taxConfiguration = Factory.NewWithValidTestData<AccTaxConfiguration>();
			taxConfiguration.ETC_RN_NKCountry = GlbCompany.CurrentCompany.Country.Code;
			ATT_ETC = taxConfiguration.PK;
			ATT_GC = GlbCompany.CurrentCompany.PK;
			ATT_GB = GlbBranch.CurrentBranch.PK;
			ATT_GE_Department = GlbDepartment.CurrentDepartment.PK;
			ATT_Ledger = LedgerTypes.AccountsReceivable;
			ATT_Basis = TaxBasisList.PostingOnMatching.Code;
			ATT_RX_NKOSTaxCurrency = CurrencyCodes.Australia;
			ATT_OSTaxBaseAmount = 10;
			ATT_LocalTaxBaseAmount = 10;
			ATT_OSTaxAmount = 10;
			ATT_LocalTaxAmount = 10;
			ATT_TaxSuperType = TaxSuperTypeList.Perceptions.Code;

			var pivots = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, PK));
			if (pivots.Length == 0)
			{
				var pivot = Factory.New<AccTaxRecordTransactionLinePivot>();
				pivot.ATP_ATT = PK;
				pivot.FillWithValidTestData();
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}

		public void SubstituteTaxTransactionToPivotsAmountsDistributor_ForTestOnly(ITaxTransactionToPivotsAmountsDistributor replacement) => taxTransactionToPivotsAmountsDistributor_constructorInitializedOnly = replacement;
		public ITaxTransactionToPivotsAmountsDistributor TaxTransactionToPivotsAmountsDistributor_ExposedForTestOnly => TaxTransactionToPivotsAmountsDistributor;

		public void SubstituteGLMovementProcessor_ForTestOnly(IGLMovementProcessor replacement) => glMovementProcessor_constructorInitializedOnly = replacement;
		public IGLMovementProcessor GLMovementProcessor_ExposedForTestOnly => GLMovementProcessor;

#endif
	}
}
