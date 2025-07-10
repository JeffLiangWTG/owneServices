using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	[SingleObjectAroundARow]
	public abstract partial class JobComInvCharge : AutoJobComInvHeaderCharge,
		ICurrencyConverterDataProviderWithFixedExRates,
		ITypeDeciderContext,
		ICurrencyConverterProvider
	{
		#region Constants

		public new class Schema : AutoJobComInvHeaderCharge.Schema
		{
			public const string ChargeCodeDescription = "ChargeCodeDescription";
			public const string IsJ7_ExchangeRateUserEnterable = "IsJ7_ExchangeRateUserEnterable";
			public const string CustomsChargeTypeDescription = "CustomsChargeTypeDescription";
			public const string NoOfDecimalsForPercentage = "NoOfDecimalsForPercentage";
			public const string J7_Calc_IsIncludedInInvoiceAmount = "J7_Calc_IsIncludedInInvoiceAmount";
		}

		#endregion

		protected JobComInvCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsAMMV()
		{
			return J7_IsSystem
				   && J7_ChargeType == CustomsChargeTypeList.Codes.AdditionCharge
				   && J7_IsDutiable
				   && J7_IsNotIncludedInInvoice;
		}

		protected override void SetPKAndDefaults()
		{
			base.SetPKAndDefaults();
			ApportionmentDirtyChangedEventHandler += new EventHandler(OnApportionmentBeingDirty);  // Can't put this in the constructor as it would violate rule CA2214
		}

		public static readonly TypeDecider TypeDecider = new JobComInvHeaderChargeTypeDecider();

		public override void OnSaving()
		{
			if (J7_IsApportionedCharge)
			{
				if (!hasSetConcurrencyPolicy && IsInDatabase)
				{
					hasSetConcurrencyPolicy = true;
					SetConcurrencyPolicyOnProperties(ConcurrencyPolicy.Ignore);
				}
			}
			base.OnSaving();
		}

		protected bool hasSetConcurrencyPolicy;

		#region New Properties

		public ICommonInvoice Parent
		{
			get
			{
				if (!IsDeleted && (fParent == null || fParent.PK != J7_ParentID))
				{
					foreach (Type bizoType in RegisteredParentTypes)
					{
						if (BusinessObjectFactory.GetTableCodeFromType(bizoType) == J7_ParentTableCode)
						{
							fParent = (ICommonInvoice)Factory.Load(bizoType, J7_ParentID);
							break;
						}
					}
					currencyConverter = null;
				}
				return IsDeleted || fParent == null || fParent.IsDeleted ? null : fParent;
			}
			set
			{
				if (fParent != value)
				{
					fParent = value;
					if (value == null)
					{
						J7_ParentID = ZGuid.Empty;
						J7_ParentTableCode = "";
					}
					else
					{
						J7_ParentID = value.PK;
						J7_ParentTableCode = ((BusinessObject)value).TablePrefix;
					}
					currencyConverter = null;
				}
			}
		}
		ICommonInvoice fParent;

		List<Type> RegisteredParentTypes
		{
			get
			{
				if (registeredParentTypes == null)
				{
					registeredParentTypes = new List<Type>();

					RegisterParentType(registeredParentTypes);
				}
				return registeredParentTypes;
			}
		}
		List<Type> registeredParentTypes;

		protected virtual void RegisterParentType(List<Type> parentTypes)
		{
#if DEBUG
			parentTypes.Add(typeof(Testing.TestDeclaration));
			parentTypes.Add(typeof(Testing.TestInvoice));
			parentTypes.Add(typeof(Testing.TestInvoiceLine));
#endif
		}

		public ZString ChargeCodeDescription
		{
			get
			{
				if (chargeCodeDescriptionCached == null)
				{
					chargeCodeDescriptionCached = new CachedProperty<ZString>(Factory, delegate
						{
							return Lookups.ChargeTypeList.GetDescriptionFromCode(J7_ChargeType);
						});
				}
				return chargeCodeDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> chargeCodeDescriptionCached;

		public ZPropertyInfo ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodeDescription); }
		}

		public override ZString J7_ExchangeRateType
		{
			get { return base.J7_ExchangeRateType; }
			set
			{
				ZBool oldIsJ7_ExchangeRateUserEnterable = IsJ7_ExchangeRateUserEnterable;
				bool isDiffAndNotCopying = !IsCopying && base.J7_ExchangeRateType != value;
				base.J7_ExchangeRateType = value;
				if (isDiffAndNotCopying)
				{
					MarkAsNeedingValidation();
					if (oldIsJ7_ExchangeRateUserEnterable != IsJ7_ExchangeRateUserEnterable)
					{
						if (IsZeroPercentageAndNotCopying)
						{
							MarkApportionmentDirty();
						}

						IsJ7_ExchangeRateUserEnterableInfo.RefreshBinding(oldIsJ7_ExchangeRateUserEnterable);
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		public ZBool IsJ7_ExchangeRateUserEnterable
		{
			get { return IsJ7_ExchangeRateUserEnterableCore; }
			set
			{
				ZBool oldValue = IsJ7_ExchangeRateUserEnterableCore;
				IsJ7_ExchangeRateUserEnterableCore = value;
				if (!IsCopying && oldValue != IsJ7_ExchangeRateUserEnterableCore && !IsJ7_ExchangeRateUserEnterableCore)
				{
					SetExchangeRateAndExchangeRateDate();
					IsJ7_ExchangeRateUserEnterableInfo.RefreshBinding(oldValue);
				}
			}
		}

		protected virtual ZBool IsJ7_ExchangeRateUserEnterableCore
		{
			get { return J7_ExchangeRateType == ChargeExchangeRateTypeList.Codes.FixedRate; }
			set
			{
				bool isDiff = IsJ7_ExchangeRateUserEnterableCore != value;
				if (value)
				{
					J7_ExchangeRateType = ChargeExchangeRateTypeList.Codes.FixedRate;
				}
				else if (IsJ7_ExchangeRateUserEnterableCore)
				{
					J7_ExchangeRateType = ZString.Empty;
				}

				if (isDiff && IsZeroPercentageAndNotCopying)
				{
					MarkApportionmentDirty();
				}
			}
		}

		protected bool IsJ7_ExchangeRateUserEnterable_ReadOnly
		{
			get { return GetIsJ7_ExchangeRateUserEnterableReadOnly(); }
		}

		public virtual ZPropertyInfo IsJ7_ExchangeRateUserEnterableInfo
		{
			get { return GetZPropertyInfo(Schema.IsJ7_ExchangeRateUserEnterable); }
		}

		protected virtual bool GetIsJ7_ExchangeRateUserEnterableReadOnly()
		{
			return J7_RX_NKCurrency.IsEmpty
				|| (Parent != null && J7_RX_NKCurrency == Parent.LocalCurrencyCode)
				|| J7_Percentage > 0;
		}

		[ReadOnlyMember(nameof(J7_ExchangeRateDateReadOnly))]
		[ResourceStringData("0E82B224-782D-451C-8F2B-95612700A9F4", Caption = "Exchange Rate Date")]
		public override ZDate J7_ExchangeRateDate { get => base.J7_ExchangeRateDate; set => base.J7_ExchangeRateDate = value; }

		public virtual bool J7_ExchangeRateDateReadOnly => !IsJ7_ExchangeRateUserEnterable;

		protected bool J7_ExchangeRate_ReadOnly
		{
			get { return GetJ7_ExchangeRateReadOnly(); }
		}

		protected virtual bool GetJ7_ExchangeRateReadOnly()
		{
			return !IsJ7_ExchangeRateUserEnterable || J7_Percentage > 0;
		}

		protected bool J7_ExchangeRateType_ReadOnly
		{
			get { return GetJ7_ExchangeRateTypeReadOnly(); }
		}

		protected virtual bool GetJ7_ExchangeRateTypeReadOnly()
		{
			return GetIsJ7_ExchangeRateUserEnterableReadOnly();
		}

		public Money Money
		{
			get
			{
				Money result = new Money(J7_Amount, Currency);
				if (Currency != null)
				{
					result = (IsJ7_ExchangeRateUserEnterable) ? ConvertToLocalAmountExact(result) : result;
				}
				return result;
			}
		}

		public Money MoneyInLocalCurrency
		{
			get { return ConvertToLocalAmountExact(Money); }
		}

		public bool IsEmpty
		{
			get { return J7_Amount.IsEmpty && J7_RX_NKCurrency.IsEmpty && J7_Percentage.IsEmpty; }
		}

		public bool IsDiscount
		{
			get { return IncoTermAndChargeFactory?.IsThisChargeDiscount(J7_ChargeType) ?? false; }
		}

		public ZInt NoOfDecimalsForPercentage
		{
			get { return 5; }
		}

		public ZPropertyInfo NoOfDecimalsForPercentageInfo
		{
			get { return GetZPropertyInfo(Schema.NoOfDecimalsForPercentage); }
		}

		public virtual bool ShouldSetCurrencyFromParent => !this.J7_IsApportionedCharge && (this.J7_Percentage > 0 || (this.J7_Amount > 0 && this.J7_RX_NKCurrency.IsEmpty));

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Overrides

		public override bool SupportsNotes
		{
			get { return false; }
		}

		public ICustomsChargeCode ChargeCode
		{
			get
			{
				ICustomsChargeCode result = null;
				if (!IsDeleted)
				{
					result = IncoTermAndChargeFactory?.GetCharge(J7_ChargeType);
				}
				return result;
			}
		}

		public override ZString J7_RX_NKCurrency
		{
			get { return base.J7_RX_NKCurrency; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_RX_NKCurrency != value;
				base.J7_RX_NKCurrency = value;
				if (isDiffAndNotCopying)
				{
					ResetExchangeRateData();
					MarkAsNeedingValidation();

					if (!J7_IsApportionedCharge)
					{
						ValidateIncoTerms();
					}
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZGuid J7_ParentID
		{
			get { return base.J7_ParentID; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_ParentID != value;

				using (GetValidationSuspender())//AU depends on Parent to determine validation object type
				{
					base.J7_ParentID = value;
				}

				if (isDiffAndNotCopying)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvHeaderChargeLookups.ChargeDistributionBy))]
		[ReadOnlyMember(nameof(J7_DistributeBy_ReadOnly_Default))]
		public override ZString J7_DistributeBy
		{
			get { return base.J7_DistributeBy; }
			set { base.J7_DistributeBy = value; }
		}

		protected bool J7_DistributeBy_ReadOnly_Default
		{
			get
			{
				if (ShouldMakeReadOnlyWhenDeemed())
				{
					return ChargeCode?.DistributeByDeemedForThisCharge ?? false;
				}
				return false;
			}
		}

		public override ZString J7_ParentTableCode
		{
			get { return base.J7_ParentTableCode; }
			set
			{
				using (GetValidationSuspender())
				{
					base.J7_ParentTableCode = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvHeaderChargeLookups.PrepaidCollectList))]
		public override ZString J7_PrepaidCollect
		{
			get { return base.J7_PrepaidCollect; }
			set { base.J7_PrepaidCollect = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvHeaderChargeLookups.ApportionmentTypeList))]
		public override ZString J7_FullOrPartialApportionment
		{
			get { return base.J7_FullOrPartialApportionment; }
			set { base.J7_FullOrPartialApportionment = value; }
		}

		RefCurrency fCurrency;
		public override RefCurrency Currency
		{
			get
			{
				if (fCurrency == null || fCurrency.RX_Code != J7_RX_NKCurrency || fCurrency.IsDeleted)
				{
					fCurrency = base.Currency;
				}
				return fCurrency;
			}
		}

		protected override JobComInvHeaderChargeValidation GetNewValidation()
		{
			return new JobComInvHeaderChargeValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			J7_FullOrPartialApportionment = ApportionmentTypeList.Codes.PartialApportionment;
			J7_IsSystem = false;
		}

		#endregion

		#region Related Objects

		public IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory => Parent?.InvoicesHolder?.IncoTermAndChargeFactory;

		#endregion

		#region Apportionment

		public virtual ZBool J7_Calc_IsIncludedInInvoiceAmount
		{
			get { return !J7_IsNotIncludedInInvoice; }
			set
			{
				J7_IsNotIncludedInInvoice = !value;
				J7_Calc_IsIncludedInInvoiceAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo J7_Calc_IsIncludedInInvoiceAmountInfo
		{
			get { return GetZPropertyInfo(Schema.J7_Calc_IsIncludedInInvoiceAmount); }
		}

		protected bool J7_Calc_IsIncludedInInvoiceAmount_ReadOnly
		{
			get { return GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly(); }
		}

		protected virtual bool GetJ7_Calc_IsIncludedInInvoiceAmountReadOnly()
		{
			return J7_IsApportionedCharge || IsIncludedInInvoiceAmountFixed;
		}

		public bool IsFullApportionment
		{
			get { return J7_FullOrPartialApportionment == ApportionmentTypeList.Codes.FullApportionment; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvHeaderChargeLookups.ChargeTypeList))]
		public override ZString J7_ChargeType
		{
			get { return base.J7_ChargeType; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_ChargeType != value;
				base.J7_ChargeType = value;
				if (isDiffAndNotCopying)
				{
					J7_ChargeDescription = DefaultChargeDescription;
					DefaultFlags();
					ResetDefaultIsIncludedInAmountAndIsIncludedInITOTIfDetermined();
					MarkAsNeedingValidation();
				}
			}
		}

		public override ZString J7_ChargeDescription
		{
			get => base.J7_ChargeDescription;
			set
			{
				if (AllowNonWesternEuropeanCharacterForChargeDescription || value.IsWesternEuropeanOrEmpty)
				{
					base.J7_ChargeDescription = value;
				}
			}
		}

		protected virtual ZString DefaultChargeDescription => ChargeCodeDescription.Left(J7_ChargeDescriptionInfo.MaxLength);

		public override ZBool J7_IsNotIncludedInInvoice
		{
			get { return base.J7_IsNotIncludedInInvoice; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_IsNotIncludedInInvoice != value;
				base.J7_IsNotIncludedInInvoice = value;
				if (isDiffAndNotCopying && (!IncoTermAndChargeFactory?.CanThisChargeBeIncludedOnLineButNotOnInvoice(J7_ChargeType) ?? true))
				{
					if (J7_IsNotIncludedInInvoice && !J7_IsApportionedCharge)
					{
						J7_IsIncludedInITOT = false;
					}

					MarkAsNeedingValidation();
				}
			}
		}

		public override ZDecimal J7_Amount
		{
			get { return base.J7_Amount; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_Amount != value;
				base.J7_Amount = value.Round(NumberOfDecimals);
				if (isDiffAndNotCopying)
				{
					DefaultCurrencyIfNecessary();

					MarkAsNeedingValidation();
				}
			}
		}

		internal const byte NumberOfDecimals = 2;
		internal const decimal UnitOfAmountToBackApportion = 0.01m;

		void DefaultCurrencyIfNecessary()
		{
			if (J7_Amount > 0 && J7_RX_NKCurrency.IsEmpty && !J7_IsApportionedCharge)
			{
				IChargeHolder parent = Parent as IChargeHolder;

				if (parent != null)
				{
					J7_RX_NKCurrency = parent.GetDefaultCurrencyCode(ChargeCode, this);
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.Common.JobComInvHeaderCharge|J7_IsIncludedInITOT", Caption = "Included in Invoice Lines", ShortCaption = "Incl. in Inv.Lines?", FullDescription = "Identify if this charge code is included in the invoice lines values or not. Entry of charge codes and this identifier allow the system to do value calculations. Example: CIF invoice, where OFT and ONS are NOT Included in Lines values, shows the lines are already at an FOB level, so the system disregards OFT and ONS in the calculation of the customs value.")]
		public override ZBool J7_IsIncludedInITOT
		{
			get { return base.J7_IsIncludedInITOT; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_IsIncludedInITOT != value;
				base.J7_IsIncludedInITOT = value;
				if (isDiffAndNotCopying && (!IncoTermAndChargeFactory?.CanThisChargeBeIncludedOnLineButNotOnInvoice(J7_ChargeType) ?? true))
				{
					if (J7_IsIncludedInITOT && !J7_IsApportionedCharge)
					{
						J7_IsNotIncludedInInvoice = false;
					}

					MarkApportionmentDirty();

					MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool J7_IsDutiable
		{
			get { return base.J7_IsDutiable; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_IsDutiable != value;
				base.J7_IsDutiable = value;
				if (isDiffAndNotCopying)
				{
					if (J7_IsDutiable && !J7_IsApportionedCharge)
					{
						J7_IsGSTApplicable = true;
					}
					MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool J7_IsGSTApplicable
		{
			get { return base.J7_IsGSTApplicable; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_IsGSTApplicable != value;
				base.J7_IsGSTApplicable = value;
				if (isDiffAndNotCopying)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool J7_IsStatisticalValueApplicable
		{
			get { return base.J7_IsStatisticalValueApplicable; }
			set
			{
				bool isDiffAndNotCopying = !IsCopying && base.J7_IsStatisticalValueApplicable != value;
				base.J7_IsStatisticalValueApplicable = value;
				if (isDiffAndNotCopying)
				{
					MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region ChargeKey

		public ChargeCodeChargeKey ChargeKey
		{
						get { return new ChargeCodeChargeKey(J7_ChargeType, J7_IsDutiable, J7_IsGSTApplicable, J7_IsStatisticalValueApplicable); }
		}

		public bool WithKey(ChargeCodeChargeKey theOther)
		{
			return ChargeKey.Equals(theOther);
		}

		public bool WithKey(MessageChargeKey theOther)
		{
			return MessageChargeKey.Equals(theOther);
		}

		public bool WithKey(ApportionChargeKey theOther)
		{
			return ApportionChargeKey.Equals(theOther);
		}

		public virtual ApportionChargeKey ApportionChargeKey
		{
			get
			{
				return new ApportionChargeKey(
					ChargeKey,
					J7_FullOrPartialApportionment,
					J7_IsIncludedInITOT ? GroupIsIncludedInLinesOptionList.Codes.Yes : GroupIsIncludedInLinesOptionList.Codes.No,
					J7_Calc_IsIncludedInInvoiceAmount ? GroupIsIncludedInLinesOptionList.Codes.Yes : GroupIsIncludedInLinesOptionList.Codes.No,
					J7_DistributeBy,
					J7_Percentage,
					J7_AdjustedCharge,
					J7_ChargeDescription.IsEmpty ? DefaultChargeDescription : J7_ChargeDescription,
					J7_IsSystem);
			}
		}

		public virtual MessageChargeKey MessageChargeKey
		{
			get
			{
				return new MessageChargeKey(
					J7_ChargeType,
					J7_IsDutiable,
					J7_IsGSTApplicable,
					J7_IsIncludedInITOT,
					J7_IsStatisticalValueApplicable);
			}
		}
		#endregion

		#region Implementation

		protected virtual void ResetExchangeRateData()
		{
			J7_ExchangeRateType = ZString.Empty;
			SetExchangeRateAndExchangeRateDate();
		}

		public void SetExchangeRateIfNotUserOverridden()
		{
			if (!IsJ7_ExchangeRateUserEnterable)
			{
				SetExchangeRateAndExchangeRateDate();
			}
		}

		void SetExchangeRateAndExchangeRateDate()
		{
			if (Parent != null && Parent.LocalCurrencyCode == J7_RX_NKCurrency)
			{
				J7_ExchangeRate = 1m;
			}
			else if (Currency != null)
			{
				var currencyConverter = CurrencyConverter;
				if (currencyConverter != null)
				{
					J7_ExchangeRate = currencyConverter.GetExchangeRateToDefault(Currency);
				}
			}
			else
			{
				J7_ExchangeRate = 0m;
			}
			J7_ExchangeRateDate = ZDate.Empty;
		}

		protected void DefaultPrepaidCollect(ICommonInvoice invoice)
		{
			if (!IsCopying)
			{
				if (IsThisPrepaid(invoice))
				{
					J7_PrepaidCollect = Constants.PaymentType.Prepaid;
				}
				else
				{
					J7_PrepaidCollect = Constants.PaymentType.Collect;
				}
			}
		}

		protected virtual bool IsThisPrepaid(ICommonInvoice invoice)
		{
			var incoTermAndChargeFactory = invoice?.InvoicesHolder?.IncoTermAndChargeFactory;
			var charge = incoTermAndChargeFactory?.GetCharge(J7_ChargeType);
			return charge != null && incoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(invoice.IncoTerm, charge);
		}

		void DefaultFlags()
		{
			using (GetValidationSuspender())
			{
				var row = ((IBusinessObjectInternals)this).Row;
				var originalState = row.RowState;

				ICustomsChargeCode chargeCode;
				if (!IsCopying && ((chargeCode = ChargeCode) != null))
				{
					J7_IsDutiable = chargeCode.IsDutiable;

					if (IsDeleted)
					{
						ErrorReporter.ReportOnce("Charge was unexpectedly deleted", string.Format(CultureInfo.CurrentCulture, "Row state: {0} - {1}, StackTrace: {2}", originalState, row.RowState, deletedStackTrace));
					}

					J7_IsGSTApplicable = chargeCode.IsVATible;
					J7_IsStatisticalValueApplicable = chargeCode.IsStatisticalValueApplicable;
					if (!string.IsNullOrEmpty(chargeCode.DistributeBy))
					{
						J7_DistributeBy = chargeCode.DistributeBy;
					}
				}
			}
		}

		public override void Delete()
		{
			deletedStackTrace = System.Environment.StackTrace;
			base.Delete();
		}

		string deletedStackTrace;

		protected bool J7_IsIncludedInITOT_ReadOnly
		{
			get { return GetIncludedInITOTReadOnly(); }
		}

		protected virtual bool GetIncludedInITOTReadOnly()
		{
			return ChargeCode?.IsIncludedInITOTDeemedForThisCharge ?? false;
		}

		#endregion

		#region ICurrencyConverterDataProviderWithFixedExRates Members

		string ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRateCurrencyCode
		{
			get { return IsJ7_ExchangeRateUserEnterable ? J7_RX_NKCurrency : ZString.Empty; }
		}

		decimal ICurrencyConverterDataProviderWithFixedExRates.FixedExchangeRate
		{
			get { return J7_ExchangeRate; }
		}

		#endregion

		#region ICurrencyConverterDataProvider Members

		ZDateTime ICurrencyConverterDataProvider.DateOfValuation
		{
			get { return Parent.CurrencyConverter.DateForRate; }
		}

		int ICurrencyConverterDataProvider.MaximumDaysToFallback
		{
			get { return Parent.CurrencyConverter.MaximumDaysToFallback; }
		}

		ExchangeRateType ICurrencyConverterDataProvider.RateType
		{
			get { return GetCurrencyConverterDataProviderRateTypeCore(); }
		}

		protected virtual ExchangeRateType GetCurrencyConverterDataProviderRateTypeCore() => Parent.CurrencyConverter.RateType;

		GlbCompany ICurrencyConverterDataProvider.Company
		{
			get { return Parent.CurrencyConverter.Company; }
		}

		ZString ICurrencyConverterDataProvider.LocalCurrencyCodeOverride
		{
			get { return Parent.CurrencyConverter.LocalCurrencyCodeOverride; }
		}

		ZBool? ICurrencyConverterDataProvider.IsReciprocalOverride
		{
			get { return Parent.CurrencyConverter.IsReciprocalOverride; }
		}

		#endregion

		#region ICurrencyConverterProvider Members

		public CurrencyConverterWithFixedExchangeRatesDataProvider CurrencyConverter
		{
			get
			{
				if (currencyConverter == null && Parent != null && Parent.CurrencyConverter != null)
				{
					currencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, this);
				}
				return currencyConverter;
			}
		}
		CurrencyConverterWithFixedExchangeRatesDataProvider currencyConverter;

		CurrencyConverter ICurrencyConverterProvider.CurrencyConverter
		{
			get { return CurrencyConverter; }
		}

		#endregion

		#region ITypeDeciderContext Members

		string ITypeDeciderContext.Country => (Parent as ITypeDeciderContext)?.Country ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

		#endregion

		protected Money ConvertToLocalAmountExact(Money amount)
		{
			return CurrencyConverter != null ? CurrencyConverter.ConvertExact(amount, CurrencyConverter.LocalCurrency) : amount;
		}

		public ZDecimal AmountInLocalCurrency => Factory.GetValue(ref amountInLocalCurrencyCached, () =>
		{
			var currencyConverter = Enterprise.MasterFiles.Business.CurrencyConverter.New(Factory, ZDateTime.Today, ExchangeRateType.Customs, false);
			return currencyConverter.ConvertExact(new Money(J7_Amount, Currency), GlbCompany.CurrentCompany.CustomsCurrency).Amount;
		});
		CachedProperty<ZDecimal> amountInLocalCurrencyCached;

		public virtual ZBool AllowNonWesternEuropeanCharacterForChargeDescription => false;
	}
}

