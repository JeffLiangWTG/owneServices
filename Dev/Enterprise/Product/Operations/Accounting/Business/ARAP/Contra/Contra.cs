using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComponentModel;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using OrgBalance = Enterprise.Accounting.Business.OrganisationBalance.OrganisationBalance;

namespace Enterprise.Accounting.Business.ARAP
{
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	[ProvideMetaDataProperty("PropertyDescriptions", MetaDataTypes.Description)]
	public partial class Contra : NonPersistentBusinessObjectWithLogsAndNotes, IIdentified, IPayablesAndReceivables, IObsoleteValidation, IDataExportBatchSource, IHandleDeleteError
	{
		#region Schema

		public abstract class Schema
		{
			public const string AH_Desc = "AH_Desc";
			public const string AH_TransactionNum = "AH_TransactionNum";
			public const string AH_PostDate = "AH_PostDate";
			public const string AH_InvoiceDate = "AH_InvoiceDate";
			public const string CreatedDate = "CreatedDate";
			public const string CreatingUser = "CreatingUser";
			public const string AH_ExchangeRate = "AH_ExchangeRate";
			public const string AH_InvoiceAmount = "AH_InvoiceAmount";
			public const string AH_ARAccount = "AH_ARAccount";
			public const string AH_APAccount = "AH_APAccount";
			public const string AH_Calc_LocalRX = "AH_Calc_LocalRX";
			public const string AH_ExchangeRateAmount = "AH_ExchangeRateAmount";
			public const string AH_ExchangeRateCurrencyCode = "AH_ExchangeRateCurrencyCode";
			public const string AH_RX_NKTransactionCurrency = "AH_RX_NKTransactionCurrency";
			public const string AH_OSTotal = "AH_OSTotal";
			public const string AH_Calc_RecBeforeContra = "AH_Calc_RecBeforeContra";
			public const string AH_Calc_RecAfterContra = "AH_Calc_RecAfterContra";
			public const string AH_Calc_PayBeforeContra = "AH_Calc_PayBeforeContra";
			public const string AH_Calc_PayAfterContra = "AH_Calc_PayAfterContra";
			public const string AH_NumberOfSupportingDocuments = "AH_NumberOfSupportingDocuments";
			public const string AH_NumberOfSupportingDocumentsVisible_ReadOnly = "AH_NumberOfSupportingDocumentsVisible_ReadOnly";
			public const string DisplayInvoiceAddressOverride = "DisplayInvoiceAddressOverride";
		}

		#endregion

		protected Contra(BusinessObjectFactory factory)
			: base(factory)
		{
			new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateAmountInfo, AH_InvoiceAmountInfo, AH_OSTotalInfo, ExchangeRate);
			factory.Saving += new BusinessObjectFactory.SavingEventHandler(Factory_Saving);
		}

		void Factory_Saving(BusinessObjectFactory factory)
		{
			if (!ARRow.IsInDatabase || !APRow.IsInDatabase)
			{
				var transactionNumber = NumberFountain.Generate(ARRow);
				ARRow.AH_TransactionNum = transactionNumber;
				APRow.AH_TransactionNum = transactionNumber;
			}
		}

		public static Contra New(BusinessObjectFactory factory)
		{
			return New(factory, ZString.Empty);
		}

		public static Contra New(BusinessObjectFactory factory, ZString controllerLedger)
		{
			Contra result = new Contra(factory);
			if (!controllerLedger.IsEmpty)
			{
				result.ControllerLedger = controllerLedger;
			}
			result.InitialiseNew();
			return result;
		}

		public static Contra Load(BusinessObjectFactory factory, ARContraRow aRRow, APContraRow aPRow, ZString controllerLedger)
		{
			Contra result = new Contra(aRRow.Factory);
			if (!controllerLedger.IsEmpty)
			{
				result.ControllerLedger = controllerLedger;
			}

			result.LoadInternal(aRRow, aPRow);

			return result;
		}

		#region ReadOnly

		protected bool GetPropertyReadonlyness(PropertyDescriptor property)
		{
			bool result = false;
			if (UseEditableFieldsForReadOnly && property.HasSetter())
			{
				result = !WritableProperties.Contains(property.Name);
			}
			return result || CargoWise.ComponentModel.MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		List<string> WritableProperties
		{
			get
			{
				if (writableProperties == null)
				{
					writableProperties = new List<string>();
				}
				return writableProperties;
			}
		}

		public void AddWritableProperties(string[] list)
		{
			foreach (string line in list)
			{
				WritableProperties.Add(line);
			}
			UseEditableFieldsForReadOnly = true;
			RefreshBinding();
		}

		List<string> writableProperties;

		bool UseEditableFieldsForReadOnly;

		#endregion

		#region Non Persistent Business Object Overrides

		public override string TablePrefix => AccTransactionHeaderSchema.Constants.Prefix;

		public override string TableName => AccTransactionHeaderSchema.Constants.TableName;

		public override SchemaGuidColumn PKSchemaColumn => AccTransactionHeaderSchema.PK;

		#endregion

		#region Business Object Overrides

		protected override ZGuid GetPK()
		{
			if (ControllerLedger == LedgerTypes.AccountsPayable)
			{
				return APRow.PK;
			}
			else
			{
				return ARRow.PK;
			}
		}

		protected override void AddToFactoryCache()
		{
			// Do not call base here. Called from factory methods, once AR and AP rows are set up.
		}

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return ARRow; }
		}

		public override void Delete()
		{
			// Don't do anything since Contra is nonpersistent
		}

		public override bool IsInDatabase
		{
			get { return ARRow.IsInDatabase && APRow.IsInDatabase; }
		}

		#endregion

		#region Proxy properties to AP and AR rows

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		internal AccountingNumberFountainWrapper NumberFountain
		{
			get { return AccountingNumberFountainWrapperFactory.Instance.ContraNo; }
		}

		#region AH_TransactionNum

		[MaxLength(APContraRow.Schema.AH_TransactionNumMaxLength)]
		public ZString AH_TransactionNum
		{
			get { return ARRow.AH_TransactionNum; }
		}

		public ZPropertyInfo AH_TransactionNumInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_TransactionNum); }
		}

		#endregion

		#region AH_Desc

		[MaxLength(APContraRow.Schema.AH_DescMaxLength)]
		public ZString AH_Desc
		{
			get { return ARRow.AH_Desc; }
			set
			{
				ARRow.AH_Desc = value;
				APRow.AH_Desc = value;
				AH_DescInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateAH_Desc();
				}
			}
		}

		public ZPropertyInfo AH_DescInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Desc); }
		}

		#endregion

		#region AH_PostDate

		public ZDateTime AH_PostDate
		{
			get { return ARRow.AH_PostDate; }
			set
			{
				ARRow.AH_PostDate = value;
				APRow.AH_PostDate = value;
				if (!IsValidationSuspended)
				{
					ValidateAH_PostDate();
				}
				AH_PostDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_PostDateInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_PostDate); }
		}

		protected bool AH_PostDate_ReadOnly
		{
			get { return !(AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value && UserAllowedToBackPost); }
		}

		#endregion

		#region AH_InvoiceDate

		public ZDateTime AH_InvoiceDate
		{
			get { return ARRow.AH_InvoiceDate; }
			set
			{
				ARRow.AH_InvoiceDate = value;
				APRow.AH_InvoiceDate = value;
				AH_InvoiceDateInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					ValidateAH_InvoiceDate();
				}
			}
		}

		public ZPropertyInfo AH_InvoiceDateInfo
		{
			[DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_InvoiceDate); }
		}

		#endregion

		#region CreatedDate

		public ZDateTime CreatedDate
		{
			get { return ARRow.CreatedDate; }
		}

		public ZPropertyInfo CreatedDateInfo
		{
			get { return GetZPropertyInfo(Schema.CreatedDate); }
		}

		#endregion

		#region CreatingUser

		public ZString CreatingUser
		{
			get { return ARRow.CreatingUser; }
		}

		public ZPropertyInfo CreatingUserInfo
		{
			get { return GetZPropertyInfo(Schema.CreatingUser); }
		}

		#endregion

		#region ExchangeRate

		public ZAccExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Buy, AH_ExchangeRateAmountInfo, (ZPropertyInfoString)AH_ExchangeRateCurrencyCodeInfo, null);
				}
				return fExchangeRate;
			}
		}

		ZAccExchangeRate fExchangeRate;

		#endregion

		#region AH_ExchangeRateAmount

		public ZDecimal AH_ExchangeRateAmount
		{
			get { return APRow.AH_ExchangeRate; }
			set
			{
				if (APRow.AH_ExchangeRate != value)
				{
					APRow.AH_ExchangeRate = value;
					ARRow.AH_ExchangeRate = value;

					if (!IsValidationSuspended)
					{
						ValidateAH_ExchangeRateAmount();
					}
					AH_ExchangeRateAmountInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AH_ExchangeRateAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_ExchangeRateAmount); }
		}

		#endregion

		#region AH_ExchangeRateCurrencyCode

		[MaxLength(3)]
		public ZString AH_ExchangeRateCurrencyCode
		{
			get { return APRow != null ? APRow.AH_RX_NKTransactionCurrency : ZString.Empty; }
			set
			{
				if (APRow != null && APRow.AH_RX_NKTransactionCurrency != value)
				{
					CheckMaximumLength(AH_ExchangeRateAmountInfo, value);
					APRow.AH_RX_NKTransactionCurrency = value;
					ARRow.AH_RX_NKTransactionCurrency = value;

					ValidateAH_ExchangeRateCurrencyCode();
					AH_ExchangeRateCurrencyCodeInfo.RefreshBinding();
				}
			}
		}

		public void ValidateAH_ExchangeRateCurrencyCode()
		{
			if (!IsValidationSuspended)
			{
				AH_ExchangeRateCurrencyCodeInfo.ClearAllNotifications();
				AH_ExchangeRateCurrencyCodeInfo.RunAdditionalValidation();
				ListValidation.ErrorIfInvalidCode(AH_ExchangeRateCurrencyCodeInfo, Currencies, ResString.GetMultilingualString("f3178769-e370-440c-bc9e-78820782f179", "Please enter a valid transaction currency"));
			}
		}

		public ZPropertyInfo AH_ExchangeRateCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AH_ExchangeRateCurrencyCode); }
		}

		#endregion

		#region AH_InvoiceAmount

		public ZDecimal AH_InvoiceAmount
		{
			get { return APRow.AH_LocalExTaxAmount; }
			set
			{
				ARRow.AH_LocalExTaxAmount = value;
				APRow.AH_LocalExTaxAmount = value;
				AH_InvoiceAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_InvoiceAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_InvoiceAmount); }
		}

		protected bool AH_InvoiceAmount_ReadOnly
		{
			get { return (!ah_InvoiceAmount_ReadOnly && APRow == null) || APRow.AH_LocalExTaxAmountInfo.ReadOnly; }
			set { ah_InvoiceAmount_ReadOnly = value; }
		}
		bool ah_InvoiceAmount_ReadOnly;

		#endregion

		#region AH_ARAccount

		[List("ARRow.Lookups.Headers")]
		public ZGuid AH_ARAccount
		{
			get { return ARRow.AH_OH; }
			set
			{
				ARRow.AH_OH = value;
				if (!IsValidationSuspended)
				{
					ValidateAH_ARAccount();
				}
				fOutstandingBalanceAR = null;
				AH_ARAccountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_ARAccountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_ARAccount); }
		}

		#endregion

		#region AH_APAccount

		[List("APRow.Lookups.Headers")]
		public ZGuid AH_APAccount
		{
			get { return APRow.AH_OH; }
			set
			{
				APRow.AH_OH = value;
				if (!IsValidationSuspended)
				{
					ValidateAH_APAccount();
				}
				fOutstandingBalanceAP = null;
				AH_APAccountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_APAccountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_APAccount); }
		}

		#endregion

		#region AH_Calc_LocalRX

		[List("Currencies")]
		public ZGuid AH_Calc_LocalRX
		{
			get { return APRow.AH_Calc_LocalRX; }
		}

		public ZPropertyInfo AH_Calc_LocalRXInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_LocalRX); }
		}

		#endregion

		#region AH_RX_NKTransactionCurrency

		[MaxLength(3)]
		[List("Currencies")]
		public ZString AH_RX_NKTransactionCurrency
		{
			get { return APRow.AH_RX_NKTransactionCurrency; }
		}
		public ZPropertyInfo AH_RX_NKTransactionCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.AH_RX_NKTransactionCurrency); }
		}

		#endregion

		#region AH_OSTotal

		public ZDecimal AH_OSTotal
		{
			get { return APRow.AH_OSExTaxAmount; }
			set
			{
				if (AH_OSTotal != value)
				{
					APRow.AH_OSExTaxAmount = value;
					ARRow.AH_OSExTaxAmount = value;

					RefreshAfterContraBindings();
				}
			}
		}

		public ZPropertyInfo AH_OSTotalInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSTotal); }
		}

		#endregion

		#region AH_Calc_RecBeforeContra

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_RecBeforeContra
		{
			get { return OutstandingBalanceAR.TotalOutstanding; }
		}

		public ZPropertyInfo AH_Calc_RecBeforeContraInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_RecBeforeContra); }
		}

		#endregion

		#region AH_Calc_RecAfterContra

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_RecAfterContra
		{
			get { return OutstandingBalanceAR.TotalOutstanding - AH_InvoiceAmount; }
		}

		public ZPropertyInfo AH_Calc_RecAfterContraInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_RecAfterContra); }
		}

		#endregion

		#region AH_Calc_PayBeforeContra

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_PayBeforeContra
		{
			get { return OutstandingBalanceAP.TotalOutstanding; }
		}

		public ZPropertyInfo AH_Calc_PayBeforeContraInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_PayBeforeContra); }
		}

		#endregion

		#region AH_Calc_PayAfterContra

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_PayAfterContra
		{
			get { return AH_Calc_PayBeforeContra - AH_InvoiceAmount; }
		}

		public ZPropertyInfo AH_Calc_PayAfterContraInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_PayAfterContra); }
		}

		#endregion

		#region AH_NumberOfSupportingDocuments
		public ZByte AH_NumberOfSupportingDocuments
		{
			get { return ARRow.AH_NumberOfSupportingDocuments; }
			set
			{
				if (ARRow.AH_NumberOfSupportingDocuments != APRow.AH_NumberOfSupportingDocuments)
				{
					APRow.AH_NumberOfSupportingDocuments = ARRow.AH_NumberOfSupportingDocuments;
				}

				if (ARRow.AH_NumberOfSupportingDocuments != value)
				{
					APRow.AH_NumberOfSupportingDocuments = value;
					ARRow.AH_NumberOfSupportingDocuments = value;

					if (!IsValidationSuspended)
					{
						ValidateAH_NumberOfSupportingDocuments();
					}
					AH_NumberOfSupportingDocumentsInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AH_NumberOfSupportingDocumentsInfo
		{
			get { return GetZPropertyInfo(Schema.AH_NumberOfSupportingDocuments); }
		}

		#endregion

		#region LocalCurrencySubUnitRatio

		public ZInt LocalCurrencySubUnitRatio
		{
			get { return APRow.AH_Calc_LocalRXDecimals; }
		}

		public ZPropertyInfo LocalCurrencySubUnitRatioInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCurrencySubUnitRatio)); }
		}

		#endregion

		#region OSCurrencySubUnitRatio

		public ZInt OSCurrencySubUnitRatio
		{
			get { return APRow.AH_Calc_RXDecimals; }
		}

		public ZPropertyInfo OSCurrencySubUnitRatioInfo
		{
			get { return GetZPropertyInfo(nameof(OSCurrencySubUnitRatio)); }
		}

		#endregion

		#endregion

		#region Lookups

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}

		public OrgHeaderCollection OrgDebtors
		{
			get { return ARRow.Lookups.Headers; }
		}

		public OrgHeaderCollection OrgCreditors
		{
			get { return APRow.Lookups.Headers; }
		}

		#endregion

		#region UserAllowedToBackPost

		public bool UserAllowedToBackPost
		{
			get
			{
				bool result = true;
				switch (ControllerLedger)
				{
					case LedgerTypes.AccountsReceivable:
						result = Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
						break;
					case LedgerTypes.AccountsPayable:
						result = Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed;
						break;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		#region InitialiseNew

		void InitialiseNew()
		{
			ARRow = Factory.New<ARContraRow>();
			APRow = Factory.New<APContraRow>();

			using (APRow.SuspendSettingHasChanges())
			using (ARRow.SuspendSettingHasChanges())
			{
				AH_PostDate = (ZDateTime)Env.Time.CurrentLocalDateTime;
				AH_InvoiceDate = (ZDateTime)Env.Time.CurrentLocalDateTime;

				APRow.AH_TransactionCount = 1;
				ARRow.AH_TransactionCount = 2;

				ARRow.ParentContra = this;
				APRow.ParentContra = this;

				APRow.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
				ARRow.AH_TransactionBelongsToGroup = APRow.AH_TransactionBelongsToGroup;
			}

			RegisterEditableChildObject(ARRow);
			RegisterEditableChildObject(APRow);

			AreContraRowsInDB = false;

			base.AddToFactoryCache();
		}

		#endregion

		#region OutstandingBalanceAP

		OrgBalance OutstandingBalanceAP
		{
			get
			{
				if (fOutstandingBalanceAP == null)
				{
					fOutstandingBalanceAP = new OrgBalance(AH_APAccount, LedgerTypes.AccountsPayable);
				}
				return fOutstandingBalanceAP;
			}
		}

		OrgBalance fOutstandingBalanceAP;

		#endregion

		#region OutstandingBalanceAR

		OrgBalance OutstandingBalanceAR
		{
			get
			{
				if (fOutstandingBalanceAR == null)
				{
					fOutstandingBalanceAR = new OrgBalance(AH_ARAccount, LedgerTypes.AccountsReceivable);
				}
				return fOutstandingBalanceAR;
			}
		}

		OrgBalance fOutstandingBalanceAR;

		#endregion

		public ARContraRow ARRow { get; set; }
		public APContraRow APRow { get; set; }
		public bool AreContraRowsInDB;
		public ZString ControllerLedger { get; set; }

		#region Functions

		void RefreshAfterContraBindings()
		{
			AH_OSTotalInfo.RefreshBinding();
			AH_Calc_PayAfterContraInfo.RefreshBinding();
			AH_Calc_RecAfterContraInfo.RefreshBinding();
		}

		void LoadInternal(ARContraRow aRContra, APContraRow aPContra)
		{
			ARRow = aRContra;
			ARRow.ParentContra = this;
			APRow = aPContra;
			APRow.ParentContra = this;

			SetReadOnlyIncludingChildren(true);
			AreContraRowsInDB = true;

			base.AddToFactoryCache();
		}

		#endregion

		protected IDescription GetPropertyDescriptions(PropertyDescriptor property)
		{
			switch (property.Name)
			{
				case Schema.AH_APAccount:
				case Schema.AH_ARAccount:
					return new TranslatableDescription(ResString.GetMultilingualString("8B93A17A-E4F2-45f7-8E08-C87699B2B908", "Account"));
				default:
					return null;
			}
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateAH_APAccount();
			ValidateAH_ARAccount();
			ValidateAH_Desc();
			ValidateAH_InvoiceDate();
			ValidateAH_PostDate();

			base.RunPreSaveValidationCore();
		}

		public void ValidateAH_Desc()
		{
			AH_DescInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_DescInfo);
		}

		public void ValidateAH_ExchangeRateAmount()
		{
			AH_ExchangeRateAmountInfo.ClearAllNotifications();
			if (AH_ExchangeRateAmount == 0)
			{
				AH_ExchangeRateAmountInfo.AddError(Res.GetString("1b6dffa2-7da9-47f9-91eb-45c79e707ec0", "Exchange rate cannot be 0"));
			}
			AH_ExchangeRateAmountInfo.RunAdditionalValidation();
		}

		public void ValidateAH_NumberOfSupportingDocuments()
		{
			AH_NumberOfSupportingDocumentsInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(AH_NumberOfSupportingDocumentsInfo, Res.GetString("4f2ad22e-688d-4c0c-8487-d0512d76d0ae", "Number of attachments"));
		}
		public void ValidateAH_PostDate()
		{
			AH_PostDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_PostDateInfo);

			if (!AH_PostDateInfo.HasErrors())
			{
				if (IsReverseTransaction)
				{
					if (OriginalTransaction != null && AH_PostDate.Date < OriginalTransaction.AH_PostDate.Date)
					{
						AH_PostDateInfo.AddError(Res.GetString("fe935bf2-d70d-4c81-88f1-43f55ca8a61f", "Reversing post date cannot be before the original post date of '{0}'.", OriginalTransaction.AH_PostDate.ToShortDateString()));
					}
				}

				if (!AH_PostDateInfo.HasErrors() && ARRow.AH_PostDateInfo.HasNotifications())
				{
					AH_PostDateInfo.AddAllNotificationsFrom(ARRow.AH_PostDateInfo);
				}
			}
		}

		public void ValidateAH_InvoiceDate()
		{
			AH_InvoiceDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_InvoiceDateInfo);
		}

		public void ValidateAH_ARAccount()
		{
			AH_ARAccountInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_ARAccountInfo);
			ListValidation.ErrorIfInvalidPK(AH_ARAccountInfo);
			ListValidation.ErrorIfCancelledAndEditable(AH_ARAccountInfo);
			ValidateAccountConsolidationCategoryClasses(AH_ARAccountInfo);
		}

		public void ValidateAH_APAccount()
		{
			AH_APAccountInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_APAccountInfo);
			ListValidation.ErrorIfInvalidPK(AH_APAccountInfo, OrgCreditors);
			ListValidation.ErrorIfCancelledAndEditable(AH_APAccountInfo);
			ValidateAccountConsolidationCategoryClasses(AH_APAccountInfo);
		}

		void ValidateAccountConsolidationCategoryClasses(ZPropertyInfo accountInfo)
		{
			if (!accountInfo.HasErrors() && !AH_ARAccount.IsEmpty && !AH_APAccount.IsEmpty)
			{
				var errorMessage = GetAccountConsolidationCategoryClassesErrorMessage();
				if (!errorMessage.IsEmpty)
				{
					accountInfo.AddError(errorMessage);
				}
			}
		}

		public ZString GetAccountConsolidationCategoryClassesErrorMessage()
		{
			var result = ZString.Empty;

			var apAccountConsolidatedAccountingCategoryClass = Factory.Load<OrgHeader>(AH_APAccount)?.MiscServ?.ConsolidatedAccountingCategoryClass ?? string.Empty;
			var arAccountConsolidatedAccountingCategoryClass = Factory.Load<OrgHeader>(AH_ARAccount)?.MiscServ?.ConsolidatedAccountingCategoryClass ?? string.Empty;

			if (apAccountConsolidatedAccountingCategoryClass != arAccountConsolidatedAccountingCategoryClass)
			{
				if (ControllerLedger == LedgerTypes.AccountsReceivable && !Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed)
				{
					result = Env.Security.NewReceivablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.ErrorMessageForNotAllowed;
				}
				else if (ControllerLedger == LedgerTypes.AccountsPayable && !Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.IsAllowed)
				{
					result = Env.Security.NewPayablesAllowContraTransactionWithDifferentAccountConsolidationCategoryClass.ErrorMessageForNotAllowed;
				}
			}

			return result;
		}

		#endregion

		#region IPayablesAndReceivables Members

		#region IReversing Members

		public bool IsReversing
		{
			get { return fIsReversing; }
		}
		protected bool fIsReversing;

		public bool IsReversed
		{
			get { return ((IReversing)APRow).IsReversed || ((IReversing)ARRow).IsReversed; }
		}

		public void GenerateReverseTransaction(bool mustTransform)
		{
			fIsReversing = true;

			fReverseTransaction = Contra.New(Factory);
			fReverseTransaction.IsReverseTransaction = true;
			fReverseTransaction.OriginalTransaction = this;

			fReverseTransaction.APRow.AH_RX_NKTransactionCurrency = APRow.AH_RX_NKTransactionCurrency;
			fReverseTransaction.APRow.AH_ExchangeRate = APRow.AH_ExchangeRate;
			fReverseTransaction.APRow.AH_OSExTaxAmount = APRow.AH_OSExTaxAmount * -1;
			fReverseTransaction.APRow.AH_OSTaxAmount = APRow.AH_OSTaxAmount * -1;
			fReverseTransaction.APRow.AH_LocalExTaxAmount = APRow.AH_LocalExTaxAmount * -1;
			fReverseTransaction.APRow.AH_LocalOutstandingAmount = APRow.AH_LocalTotalAmount * -1;

			fReverseTransaction.APRow.AH_AB = APRow.AH_AB;
			fReverseTransaction.APRow.AH_AG = APRow.AH_AG;
			fReverseTransaction.APRow.AH_OH = APRow.AH_OH;
			using (fReverseTransaction.APRow.GetValidationSuspender())
			{
				fReverseTransaction.APRow.AH_GB = APRow.AH_GB;
				fReverseTransaction.APRow.AH_GE = APRow.AH_GE;
			}
			fReverseTransaction.APRow.AH_TransactionCategory = APRow.AH_TransactionCategory;
			fReverseTransaction.APRow.AH_JH = APRow.AH_JH;
			fReverseTransaction.APRow.AH_CashBasisGSTIndicator = APRow.AH_CashBasisGSTIndicator;
			fReverseTransaction.APRow.AH_ChequeDrawer = APRow.AH_ChequeDrawer;
			fReverseTransaction.APRow.AH_ChequeOrReference = APRow.AH_ChequeOrReference;
			fReverseTransaction.APRow.AH_DrawerBank = APRow.AH_DrawerBank;
			fReverseTransaction.APRow.AH_DrawerBranch = APRow.AH_DrawerBranch;
			fReverseTransaction.APRow.AH_InvoiceTerm = APRow.AH_InvoiceTerm;
			fReverseTransaction.APRow.AH_InvoiceTermDays = APRow.AH_InvoiceTermDays;
			fReverseTransaction.APRow.AH_ReceiptType = APRow.AH_ReceiptType;
			fReverseTransaction.APRow.AH_DueDate = ZDateTime.Now;

			fReverseTransaction.ARRow.AH_RX_NKTransactionCurrency = ARRow.AH_RX_NKTransactionCurrency;
			fReverseTransaction.ARRow.AH_ExchangeRate = ARRow.AH_ExchangeRate;
			fReverseTransaction.ARRow.AH_OSExTaxAmount = ARRow.AH_OSExTaxAmount * -1;
			fReverseTransaction.ARRow.AH_OSTaxAmount = ARRow.AH_OSTaxAmount * -1;
			fReverseTransaction.ARRow.AH_LocalExTaxAmount = ARRow.AH_LocalExTaxAmount * -1;
			fReverseTransaction.ARRow.AH_LocalOutstandingAmount = ARRow.AH_LocalTotalAmount * -1;

			fReverseTransaction.ARRow.AH_AB = ARRow.AH_AB;
			fReverseTransaction.ARRow.AH_AG = ARRow.AH_AG;
			fReverseTransaction.ARRow.AH_OH = ARRow.AH_OH;
			using (fReverseTransaction.ARRow.GetValidationSuspender())
			{
				fReverseTransaction.ARRow.AH_GB = ARRow.AH_GB;
				fReverseTransaction.ARRow.AH_GE = ARRow.AH_GE;
			}
			fReverseTransaction.ARRow.AH_TransactionCategory = ARRow.AH_TransactionCategory;
			fReverseTransaction.ARRow.AH_JH = ARRow.AH_JH;
			fReverseTransaction.ARRow.AH_CashBasisGSTIndicator = ARRow.AH_CashBasisGSTIndicator;
			fReverseTransaction.ARRow.AH_ChequeDrawer = ARRow.AH_ChequeDrawer;
			fReverseTransaction.ARRow.AH_ChequeOrReference = ARRow.AH_ChequeOrReference;
			fReverseTransaction.ARRow.AH_DrawerBank = ARRow.AH_DrawerBank;
			fReverseTransaction.ARRow.AH_DrawerBranch = ARRow.AH_DrawerBranch;
			fReverseTransaction.ARRow.AH_InvoiceTerm = ARRow.AH_InvoiceTerm;
			fReverseTransaction.ARRow.AH_InvoiceTermDays = ARRow.AH_InvoiceTermDays;
			fReverseTransaction.ARRow.AH_ReceiptType = ARRow.AH_ReceiptType;
			fReverseTransaction.ARRow.AH_DueDate = ZDateTime.Now;
		}

		public IReversing ReverseTransaction
		{
			get { return fReverseTransaction; }
		}
		protected Contra fReverseTransaction;

		public bool IsReverseTransaction
		{
			get { return fIsReverseTransaction; }
			set
			{
				fIsReverseTransaction = value;
				APRow.IsReverseTransaction = value;
				ARRow.IsReverseTransaction = value;
			}
		}
		protected bool fIsReverseTransaction;

		public Contra OriginalTransaction
		{
			get { return fOriginalTransaction; }
			set { fOriginalTransaction = value; }
		}
		protected Contra fOriginalTransaction;

		public void SetCancellationFlag(bool cancel)
		{
			APRow.AH_IsCancelled = new ZBool(cancel);
			ARRow.AH_IsCancelled = new ZBool(cancel);
		}

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
			ZGuid groupingGuid = ARRow.AH_TransactionBelongsToGroup;
			if (fReverseTransaction != null)
			{
				fReverseTransaction.ARRow.AH_TransactionBelongsToGroup = groupingGuid;
				fReverseTransaction.APRow.AH_TransactionBelongsToGroup = groupingGuid;
			}
		}

		public void SetDescription(ZString descriptionToSet)
		{
			AH_Desc = descriptionToSet;
		}
		public void SetNumberOfSupportingDocuments(ZByte numberOfSupportingDocumentsToSet)
		{
			AH_NumberOfSupportingDocuments = numberOfSupportingDocumentsToSet;
		}

		public void ApplyWorkflowTemplatesOnReverseTransaction()
		{
		}

		public ZString ReversingCode
		{
			get { return fReversingCode; }
			set { fReversingCode = value; }
		}
		protected ZString fReversingCode;

		public ZString ReversingReason
		{
			get { return fReversingReason; }
			set
			{
				AH_Desc = new ZString(AH_Desc + " " + value).Left(AH_DescInfo.MaxLength);
				fReversingReason = value;
			}
		}
		protected ZString fReversingReason;

		public bool IsClearedInCashbook
		{
			get { return false; }
		}

		public string[] MultipleReversingErrors
		{
			get { return MultipleReversingErrors_innerValue; }
			set { MultipleReversingErrors_innerValue = value; }
		}
		string[] MultipleReversingErrors_innerValue = Array.Empty<string>();

		#endregion

		#region IMatching Members

		bool IMatching.IsAllPaidInTheSameCurrency(ZString currencyNK)
		{
			return currencyNK == AH_RX_NKTransactionCurrency;
		}

		ZDecimal IMatching.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK)
		{
			return currencyNK == AH_RX_NKTransactionCurrency ? ((IMatching)this).OSPartialPaymentAmount : 0;
		}

		ZString IMatching.RelatedTransactionDebtorsAsString { get { return ZString.Empty; } }
		ZString IMatching.CreatingUser { get { return ZString.Empty; } }
		ZString IMatching.PaymentCriticality { get { return ZString.Empty; } }
		ZDateTime IMatching.PaymentRequestedDate { get { return ZDateTime.Empty; } }
		ZString IMatching.VoyageVesselOrFlightDate { get { return ZString.Empty; } }
		ZString IMatching.ShipmentHouseBill { get { return ZString.Empty; } }
		ZString IMatching.ShipmentMasterBill { get { return ZString.Empty; } }
		ZString IMatching.RelatedClaimStatus { get { return ZString.Empty; } }
		ZString IMatching.QueryNumber { get { return ZString.Empty; } }
		ZString IMatching.InvoiceRemittanceReference { get { return ZString.Empty; } }
		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }

		bool IMatching.IsMatched
		{
			get { return ((IMatching)APRow).IsMatched || ((IMatching)ARRow).IsMatched; }
		}

		void IMatching.FullyPay(ZDateTime fullyPaidDate)
		{
			((IMatching)APRow).FullyPay(fullyPaidDate);
			((IMatching)ARRow).FullyPay(fullyPaidDate);
		}

		void IMatching.PartiallyPay()
		{
			throw new NotSupportedException("You cannot partially pay a Contra.");
		}

		void IMatching.GenerateMatchLinksCore()
		{
			APRow.GenerateMatchLinks();
			ARRow.GenerateMatchLinks();

			((IMatching)this).CurrentMatchGroup.AddRange(((IMatching)APRow).CurrentMatchGroup);
			((IMatching)APRow).CurrentMatchGroup.RemoveAll();
			((IMatching)this).CurrentMatchGroup.AddRange(((IMatching)ARRow).CurrentMatchGroup);
			((IMatching)ARRow).CurrentMatchGroup.RemoveAll();
		}

		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
			((IMatching)APRow).GeneratePaymentApprovalItems(approval);
			((IMatching)ARRow).GeneratePaymentApprovalItems(approval);

			((IMatching)this).PaymentApprovalItems.AddRange(((IMatching)APRow).PaymentApprovalItems);
			((IMatching)this).PaymentApprovalItems.AddRange(((IMatching)ARRow).PaymentApprovalItems);
		}

		PaymentApprovalItemCollection IMatching.PaymentApprovalItems
		{
			get
			{
				if (fPaymentApprovalItems == null)
				{
					fPaymentApprovalItems = new PaymentApprovalItemCollection(Factory);
				}

				return fPaymentApprovalItems;
			}
		}

		PaymentApprovalItemCollection fPaymentApprovalItems;

		UnmatchingResult IMatching.CanUnmatch(ZDecimal matchLinkAmount)
		{
			throw new NotSupportedException("Contra is made up of two rows so you should check CanUnmatch() on the AP and AR Row.");
		}

		bool ShouldReverseOnUnmatching
		{
			get { return APRow.AH_TransactionCreatedByMatching && ARRow.AH_TransactionCreatedByMatching; }
		}

		void IMatching.Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			if (ShouldReverseOnUnmatching)
			{
				// Have to unmatch the rows individually for MatchAmount to be correct
				if (APRow.LatestMatchLink != null && ARRow.LatestMatchLink != null)
				{
					((IMatching)APRow).Unmatch(APRow.LatestMatchLink.AP_Amount, APRow.LatestMatchLink.AP_OSAmount);
					((IMatching)ARRow).Unmatch(ARRow.LatestMatchLink.AP_Amount, ARRow.LatestMatchLink.AP_OSAmount);
				}

				new ReversingFactory().NewReversing(this).Reverse();
				// ** Remember: have to delete the original matchlink rows 
			}
		}

		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate)
		{
			if (ShouldReverseOnUnmatching)
			{
				if (((IMatching)APRow).MatchDate > ZDateTime.Today)
				{
					unmatchDate = ((IMatching)APRow).MatchDate;
				}

				((IMatching)APRow).ChangeUnmatchDate(unmatchDate);
				((IMatching)ARRow).ChangeUnmatchDate(unmatchDate);

				if (ReverseTransaction != null)
				{
					ReverseTransaction.PostDate = unmatchDate;
					((IMatching)this).FullyPay(unmatchDate);
					((IMatching)ReverseTransaction).FullyPay(unmatchDate);
				}
			}
		}

		TransactionMatchLinkGroup IMatching.CurrentMatchGroup
		{
			get { return fCurrentMatchGroup ?? (fCurrentMatchGroup = new TransactionMatchLinkGroup(Factory)); }
		}
		TransactionMatchLinkGroup fCurrentMatchGroup;

		TransactionMatchLinkCollection IMatching.Matchlinks
		{
			get
			{
				return matchlinks ?? (matchlinks = new TransactionMatchLinkCollection(Factory, new ZQuery(AccTransactionMatchLinkSchema.AP_AH, PK)));
			}
		}
		TransactionMatchLinkCollection matchlinks;

		ZDecimal IMatching.OSOutstandingAmount
		{
			get { return 0.0M; }
		}

		ZPropertyInfo IMatching.OSOutstandingAmountInfo
		{
			get { return GetZPropertyInfo("OSOutstandingAmount"); }
		}

		ZDecimal IMatching.OutstandingAmount
		{
			get { return 0.0M; }
		}

		ZPropertyInfo IMatching.OutstandingAmountInfo
		{
			get { return GetZPropertyInfo("OutstandingAmount"); }
		}

		ZDecimal IMatching.OriginalOutstandingAmount
		{
			get { return 0M; }
		}

		ZGuid IMatching.Organisation
		{
			get { return APRow.AH_OH; }
		}

		ZPropertyInfo IMatching.OrganisationInfo
		{
			get { return GetZPropertyInfo("Organisation"); }
		}

		ZString IMatching.Ledger
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.LedgerInfo
		{
			get { return GetZPropertyInfo("Ledger"); }
		}

		ZString IMatching.TransactionNumber
		{
			get { return AH_TransactionNum; }
		}

		ZPropertyInfo IMatching.TransactionNumberInfo
		{
			get { return GetZPropertyInfo("TransactionNumber"); }
		}

		ZDecimal IMatching.OSPartialPaymentAmount
		{
			get { return fOSPartialPaymentAmount; }
			set { fOSPartialPaymentAmount = value; }
		}
		ZDecimal fOSPartialPaymentAmount;

		ZPropertyInfo IMatching.OSPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("OSPartialPaymentAmount"); }
		}

		bool IMatching.OSPartialPaymentAmount_ReadOnly
		{
			get { return true; }
		}

		ZDecimal IMatching.LocalPartialPaymentAmount
		{
			get { return (ZDecimal)Env.CurrentCompany.ExchangeRate.ForeignToLocal(((IMatching)this).OSPartialPaymentAmount, AH_ExchangeRateAmount); }
		}

		ZPropertyInfo IMatching.LocalPartialPaymentAmountInfo
		{
			get { return GetZPropertyInfo("LocalPartialPaymentAmount"); }
		}

		ZString IMatching.TransactionType
		{
			get { return TransactionTypes.Contra; }
		}

		ZPropertyInfo IMatching.TransactionTypeInfo
		{
			get { return GetZPropertyInfo("TransactionType"); }
		}

		ZString IMatching.TransactionCategory
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.TransactionCategoryInfo
		{
			get { return GetZPropertyInfo("TransactionCategory"); }
		}

		ZGuid IMatching.BranchGuid
		{
			get { return APRow.AH_GB; }
		}

		ZPropertyInfo IMatching.BranchGuidInfo
		{
			get { return GetZPropertyInfo("BranchGuid"); }
		}

		ZGuid IMatching.DepartmentGuid
		{
			get { return APRow.AH_GE; }
		}

		ZPropertyInfo IMatching.DepartmentGuidInfo
		{
			get { return GetZPropertyInfo("DepartmentGuid"); }
		}

		[MaxLength(3)]
		ZString IMatching.CurrencyCode
		{
			get { return APRow.AH_RX_NKTransactionCurrency; }
		}

		ZPropertyInfo IMatching.PaymentCurrencyCodeInfo
		{
			get { return GetZPropertyInfo("CurrencyCode"); }
		}

		ZInt IMatching.CurrencyDecimals
		{
			get { return APRow.AH_Calc_RXDecimals; }
		}

		ZPropertyInfo IMatching.CurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("CurrencyDecimals"); }
		}

		ZInt IMatching.LoginCompanyCurrencyDecimals
		{
			get { return GlbCompany.CurrentCompany.LocalCurrency.Decimals; }
		}

		ZPropertyInfo IMatching.LoginCompanyCurrencyDecimalsInfo
		{
			get { return GetZPropertyInfo("LoginCompanyCurrencyDecimals"); }
		}

		ZString IMatching.Description
		{
			get { return AH_Desc; }
		}

		ZPropertyInfo IMatching.DescriptionInfo
		{
			get { return GetZPropertyInfo("Description"); }
		}

		ZDateTime IMatching.PostDate
		{
			get { return AH_PostDate; }
			set { AH_PostDate = value; }
		}

		ZPropertyInfo IMatching.PostDateInfo
		{
			get { return GetZPropertyInfo("PostDate"); }
		}

		bool IMatching.PostDate_ReadOnly
		{
			get { return false; }
		}

		[MaxLength(TransactionHeader.Schema.AH_ChequeOrReferenceMaxLength)]
		public ZString ChequeOrReference
		{
			get { return fChequeOrReference; }
			set
			{
				if (fChequeOrReference != value)
				{
					CheckMaximumLength(((IMatching)this).ChequeOrReferenceInfo, value);
					fChequeOrReference = value;
					ChequeOrReferenceInfo.RefreshBinding();
				}
			}
		}
		ZString fChequeOrReference;

		ZString IMatching.ChequeOrReference
		{
			get { return ChequeOrReference; }
			set { ChequeOrReference = value; }
		}

		public ZPropertyInfo ChequeOrReferenceInfo
		{
			get { return GetZPropertyInfo(nameof(ChequeOrReference)); }
		}

		ZPropertyInfo IMatching.ChequeOrReferenceInfo
		{
			get { return ChequeOrReferenceInfo; }
		}

		bool IMatching.ChequeOrReference_ReadOnly { get; set; }

		ZDecimal IMatching.ExchangeRateAmount
		{
			get { return AH_ExchangeRateAmount; }
		}

		ZPropertyInfo IMatching.ExchangeRateAmountInfo
		{
			get { return GetZPropertyInfo("ExchangeRateAmount"); }
		}

		ZString IMatching.JobLocalReference
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.JobLocalReferenceInfo
		{
			get { return GetZPropertyInfo("JobLocalReference"); }
		}

		ZString IMatching.TransactionReference
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.TransactionReferenceInfo
		{
			get { return GetZPropertyInfo("TransactionReference"); }
		}

		ZString IMatching.ConsolidatedRef
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.ConsolidatedRefInfo
		{
			get { return GetZPropertyInfo("ConsolidatedRef"); }
		}

		ZString IMatching.InvoiceBatchNumber
		{
			get { return ZString.Empty; }
		}

		ZPropertyInfo IMatching.InvoiceBatchNumberInfo
		{
			get { return GetZPropertyInfo("InvoiceBatchNumber"); }
		}

		ZDateTime IMatching.InvoiceDate
		{
			get { return AH_InvoiceDate; }
		}

		ZPropertyInfo IMatching.InvoiceDateInfo
		{
			get { return GetZPropertyInfo("InvoiceDate"); }
		}

		ZDateTime IMatching.DueDate
		{
			get { return ZDateTime.Empty; }
		}

		ZPropertyInfo IMatching.DueDateInfo
		{
			get { return GetZPropertyInfo("DueDate"); }
		}

		ZString IMatching.MatchStatus
		{
			get { return fMatchStatus; }
			set
			{
				CheckMaximumLength(((IMatching)this).MatchStatusInfo, value);
				fMatchStatus = value;
			}
		}
		ZString fMatchStatus;

		ZPropertyInfo IMatching.MatchStatusInfo
		{
			get { return GetZPropertyInfo("MatchStatus"); }
		}

		ZString IMatching.MatchStatusReasonCode
		{
			get { return fMatchStatusReasonCode; }
			set
			{
				CheckMaximumLength(((IMatching)this).MatchStatusReasonCodeInfo, value);
				fMatchStatusReasonCode = value;
			}
		}
		ZString fMatchStatusReasonCode;

		ZPropertyInfo IMatching.MatchStatusReasonCodeInfo
		{
			get { return GetZPropertyInfo("MatchStatusReasonCode"); }
		}

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusList { get { return null; } }

		ReadOnlyCodeDescriptionPairList IMatching.MatchStatusReasonCodeList { get { return null; } }

		ZDateTime IMatching.MatchDate
		{
			get { return ZDateTime.Empty; }
		}

		ZPropertyInfo IMatching.MatchDateInfo
		{
			get { return GetZPropertyInfo("MatchDate"); }
		}

		OrgHeaderCollection IMatching.Organisations
		{
			get { return ARRow.Lookups.Headers; }
		}

		GlbBranchCollection IMatching.BranchCollection
		{
			get { return FindboxLookupCollections.GetAllBranchesCollection(Factory); }
		}

		GlbDepartmentCollection IMatching.DepartmentCollection
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		ZGuid IMatching.DisplayInvoiceAddressOverride
		{
			get { return ZGuid.Empty; }
		}

		ZPropertyInfo IMatching.DisplayInvoiceAddressOverrideInfo
		{
			get { return GetZPropertyInfo(Schema.DisplayInvoiceAddressOverride); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal NotionalWHTTax => 0M;

		public ZPropertyInfo NotionalWHTTaxInfo => GetZPropertyInfo(nameof(IMatching.NotionalWHTTax));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal RealizedWHTTax => 0M;

		public ZPropertyInfo RealizedWTHTaxInfo => GetZPropertyInfo(nameof(IMatching.RealizedWHTTax));

		ZString IMatching.InvoiceTransactionReference => ZString.Empty;

		#endregion

		#region ITransaction Members

		ZString ITransaction.Ledger
		{
			get { return ZString.Empty; } // Contra has  no specific ledger - it is always related to both AR and AP
		}

		ZPropertyInfo ITransaction.LedgerInfo
		{
			get { return GetZPropertyInfo("Ledger"); }
		}
		
		ZString ITransaction.CurrencyCode
		{
			get
			{
				ZString code = ZString.Empty;
				if (ARRow != null && ARRow.TransactionCurrency != null)
				{
					code = ARRow.AH_RX_NKTransactionCurrency;
				}
				return code;
			}
		}

		ZPropertyInfo ITransaction.CurrencyCodeInfo
		{
			get { return GetWrappedZPropertyInfo("CurrencyCode", x => (ARRow != null && ARRow.TransactionCurrency != null) ? ARRow.AH_RX_NKTransactionCurrencyInfo : null); }
		}

		ZDecimal ITransaction.OverseasTotalAmount
		{
			get { return AH_OSTotal; }
		}

		ZPropertyInfo ITransaction.OverseasTotalAmountInfo
		{
			get { return AH_OSTotalInfo; }
		}

		ZDateTime ITransaction.PostDate
		{
			get { return AH_PostDate; }
			set { AH_PostDate = value; }
		}

		ZPropertyInfo ITransaction.PostDateInfo
		{
			get { return AH_PostDateInfo; }
		}

		ZDateTime ITransaction.TransactionDate
		{
			get { return AH_InvoiceDate; }
			set { AH_InvoiceDate = value; }
		}

		ZPropertyInfo ITransaction.TransactionDateInfo
		{
			get { return AH_InvoiceDateInfo; }
		}

		ZString ITransaction.TransactionNumber
		{
			get { return AH_TransactionNum; }
			set { }
		}

		ZPropertyInfo ITransaction.TransactionNumberInfo
		{
			get { return AH_TransactionNumInfo; }
		}

		ZString ITransaction.TransactionType
		{
			get { return TransactionTypes.Contra; }
		}

		ZPropertyInfo ITransaction.TransactionTypeInfo
		{
			get { return GetZPropertyInfo("TransactionType"); }
		}

		ZGuid ITransaction.Organization
		{
			get
			{
				if (ControllerLedger == LedgerTypes.AccountsReceivable)
				{
					return AH_ARAccount;
				}
				else
				{
					return AH_APAccount;
				}
			}
			set
			{
				if (ControllerLedger == LedgerTypes.AccountsReceivable)
				{
					AH_ARAccount = value;
				}
				else
				{
					AH_APAccount = value;
				}
			}
		}

		ZPropertyInfo ITransaction.OrganizationInfo
		{
			get
			{
				if (ControllerLedger == LedgerTypes.AccountsReceivable)
				{
					return AH_ARAccountInfo;
				}
				else
				{
					return AH_APAccountInfo;
				}
			}
		}

		[BusinessObjectTestExclude]
		public ZString SupportingDocumentNumber
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo SupportingDocumentNumberInfo
		{
			get { return GetZPropertyInfo(nameof(SupportingDocumentNumber)); }
		}

		public ZString OriginalTransactionNumber
		{
			get { return OriginalTransaction != null ? OriginalTransaction.AH_TransactionNum : ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionNumberInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionNumber)); }
		}

		public bool OriginalTransactionNumber_ReadOnly { get { return true; } }

		public ZString OriginalTransactionType
		{
			get { return OriginalTransaction != null ? (ZString)TransactionTypes.Contra : ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionType)); }
		}

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

		OrgHeaderCollection ITransaction.Headers
		{
			get
			{
				if (ControllerLedger == LedgerTypes.AccountsReceivable)
				{
					return ARRow.Lookups.Headers;
				}
				else
				{
					return APRow.Lookups.Headers;
				}
			}
		}

		public ZDateTime UnmatchDate
		{
			get { return unmatchDate; }
			set { SetNonPersistentPropertyValue(UnmatchDateInfo, ref unmatchDate, value); }
		}
		ZDateTime unmatchDate;

		public bool UnmatchDate_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo UnmatchDateInfo
		{
			get { return GetZPropertyInfo(nameof(UnmatchDate)); }
		}

		#region ReversalStatusCode

		[BusinessObjectTestExclude]
		public ZString ReversalStatusCode
		{
			get { return ZString.Empty; }
			set { }
		}

		public ZPropertyInfo ReversalStatusCodeInfo => GetZPropertyInfo(nameof(ReversalStatusCode));

		bool ITransaction.ReversalStatusCode_ReadOnly => true;

		ReadOnlyCodeDescriptionPairList ITransaction.ReversalStatusCodeList => null;

		#endregion

		#endregion

		#endregion

		#region IDataExportBatchSource Members

		ZBool IDataExportBatchSource.IsDataExportBatchSupported
		{
			get
			{
				var source = ARRow as IDataExportBatchSource;
				return source != null && source.IsDataExportBatchSupported;
			}
		}

		public DataExportBatchDependentCollection DataExportBatchCollection
		{
			get
			{
				if (!relatedBatchCollectionIsLoaded && relatedBatchCollection == null)
				{
					var source = ARRow as IDataExportBatchSource;
					if (source != null)
					{
						relatedBatchCollection = new DataExportBatchDependentCollection(source);
						relatedBatchCollection.Load();
					}
					relatedBatchCollectionIsLoaded = true;
				}
				return relatedBatchCollection;
			}
		}
		bool relatedBatchCollectionIsLoaded;
		DataExportBatchDependentCollection relatedBatchCollection;

		#endregion

		#region IIdentified Members

		public ZGuid Identifier
		{
			get { return ((IIdentified)ARRow).Identifier; }
		}

		#endregion

		#region IHandleDeleteError Members

		bool IHandleDeleteError.RollbackAfterDeleteError
		{
			get { return ARRow.IsInDatabase && (ARRow.IsDeleted || !(ARRow.IsCancelled && ARRow.IsCancelledHasChanged)); }
		}

		bool IHandleDeleteError.RebindAfterDeleteError
		{
			get { return false; }
		}

		bool IHandleDeleteError.DisableFormOnDeleteConcurrencyError
		{
			get { return !((IHandleDeleteError)this).RollbackAfterDeleteError; }
		}

		#endregion
	}
}
