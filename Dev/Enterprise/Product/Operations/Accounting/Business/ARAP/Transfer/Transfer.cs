using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Common;
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
using OrgBalance = Enterprise.Accounting.Business.OrganisationBalance.OrganisationBalance;

namespace Enterprise.Accounting.Business.ARAP
{
	[PropertyDescriptorCollection(typeof(MatchingPropertyDescriptorCollection))]
	[ProvideMetaDataProperty("PropertyDescriptions", MetaDataTypes.Description)]
	[ProvideMetaDataProperty("PropertyReadonlyness", MetaDataTypes.ReadOnly)]
	public abstract partial class Transfer : NonPersistentBusinessObjectWithLogsAndNotes, IPayablesAndReceivables, IObsoleteValidation, IDataExportBatchSource, IHandleDeleteError
	{
		public abstract class Schema
		{
			public const string AH_TransactionNum = "AH_TransactionNum";
			public const string AH_Ledger = "AH_Ledger";
			public const string AH_Desc = "AH_Desc";
			public const string AH_PostDate = "AH_PostDate";
			public const string AH_InvoiceDate = "AH_InvoiceDate";
			public const string CreatedDate = "CreatedDate";
			public const string CreatingUser = "CreatingUser";
			public const string AH_ExchangeRate = "AH_ExchangeRate";
			public const string AH_ExchangeRateAmount = "AH_ExchangeRateAmount";
			public const string AH_ExchangeRateCurrencyCode = "AH_ExchangeRateCurrencyCode";
			public const string AH_Calc_LocalRX = "AH_Calc_LocalRX";
			public const string AH_RX_NKTransactionCurrency = "AH_RX_NKTransactionCurrency";
			public const string AH_OSTotal = "AH_OSTotal";
			public const string AH_InvoiceAmount = "AH_InvoiceAmount";
			public const string AH_APAccount = "AH_APAccount";
			public const string AH_ARAccount = "AH_ARAccount";
			public const string AH_Calc_FromBeforeTransfer = "AH_Calc_FromBeforeTransfer";
			public const string AH_Calc_FromAfterTransfer = "AH_Calc_FromAfterTransfer";
			public const string AH_Calc_ToBeforeTransfer = "AH_Calc_ToBeforeTransfer";
			public const string AH_Calc_ToAfterTransfer = "AH_Calc_ToAfterTransfer";
			public const string AH_FromAccount = "AH_FromAccount";
			public const string AH_ToAccount = "AH_ToAccount";
			public const string AH_Calc_FromDueDate = "AH_Calc_FromDueDate";
			public const string AH_Calc_ToDueDate = "AH_Calc_ToDueDate";
			public const string AH_NumberOfSupportingDocuments = "AH_NumberOfSupportingDocuments";
			public const string AH_NumberOfSupportingDocumentsVisible_ReadOnly = "AH_NumberOfSupportingDocumentsVisible_ReadOnly";
			public const string DisplayInvoiceAddressOverride = "DisplayInvoiceAddressOverride";
		}

		public static class TransferDirectionTypes
		{
			public const string TransferFrom = "TransferFrom";
			public const string TransferTo = "TransferTo";
		}

		protected Transfer(BusinessObjectFactory factory)
			: base(factory)
		{
			factory.Saving += new BusinessObjectFactory.SavingEventHandler(SetTransactionNumberOnSaving);
		}

		public static Transfer New(Type transferType, BusinessObjectFactory factory)
		{
			Transfer result = GetInstanceFromType(transferType, factory);
			if (result != null)
			{
				result.InitialiseNew();
			}
			return result;
		}

		public static Transfer Load(Type transferType, BusinessObjectFactory factory, TransferRow fromRow, TransferRow toRow, ZString transferLedger)
		{
			Transfer result = GetInstanceFromType(transferType, factory);
			if (!transferLedger.IsEmpty)
			{
				result.TransferLedger = transferLedger;
			}
			if (result != null)
			{
				result.LoadInternal(fromRow, toRow);
			}
			return result;
		}

		protected static Transfer GetInstanceFromType(Type transferType, BusinessObjectFactory factory)
		{
			BindingFlags createFlags = BindingFlags.NonPublic | BindingFlags.Instance;

			return Activator.CreateInstance(transferType, createFlags, null, new object[] { factory }, null) as Transfer;
		}

		protected override BusinessObject LogsAndNotesTarget
		{
			get { return TransferLedger == TransferDirectionTypes.TransferFrom ? TransferFrom : TransferTo; }
		}

		#region Non Persistent Business Object Overrides

		public override string TablePrefix => AccTransactionHeaderSchema.Constants.Prefix;

		public override string TableName => AccTransactionHeaderSchema.Constants.TableName;

		public override SchemaGuidColumn PKSchemaColumn => AccTransactionHeaderSchema.PK;

		#endregion

		#region Business Object Overrides

		protected override ZGuid GetPK()
		{
			return TransferLedger == TransferDirectionTypes.TransferFrom ? TransferFrom.PK : TransferTo.PK;
		}

		protected override void AddToFactoryCache()
		{
			// Do not call base here. Called from factory methods, once From and To rows are set up.
		}

		public override void Delete()
		{
			//Call reverse in here 
		}

		public override bool IsInDatabase
		{
			get { return TransferFrom.IsInDatabase && TransferTo.IsInDatabase; }
		}

		#endregion

		#region ReadOnly

		protected virtual bool GetPropertyReadonlyness(PropertyDescriptor property)
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

		protected void SetTransactionNumberOnSaving(BusinessObjectFactory factory)
		{
			if (!TransferFrom.IsInDatabase || !TransferTo.IsInDatabase)
			{
				ZString transactionNumber = TransferNumberFountain.Generate(TransferFrom);
				TransferFrom.AH_TransactionNum = transactionNumber;
				TransferTo.AH_TransactionNum = transactionNumber;
			}
		}

		internal abstract AccountingNumberFountainWrapper TransferNumberFountain { get; }

		#region Proxy Properties

		public int LocalDecimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		#region AH_TransactionNum

		[MaxLength(AccTransactionHeader.Schema.AH_TransactionNumMaxLength)]
		public ZString AH_TransactionNum
		{
			get { return TransferFrom.AH_TransactionNum; }
		}

		public ZPropertyInfo AH_TransactionNumInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_TransactionNum); }
		}

		#endregion

		#region AH_Ledger

		public ZString AH_Ledger
		{
			get { return TransferFrom.AH_Ledger; }
		}

		#endregion

		#region AH_Desc

		[MaxLength(AccTransactionHeader.Schema.AH_DescMaxLength)]
		public ZString AH_Desc
		{
			get { return TransferFrom.AH_Desc; }
			set
			{
				TransferFrom.AH_Desc = value;
				TransferTo.AH_Desc = value;
				AH_DescInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateAH_Desc();
				}
			}
		}

		public ZPropertyInfo AH_DescInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_Desc); }
		}

		#endregion

		#region AH_PostDate

		public ZDateTime AH_PostDate
		{
			get { return TransferFrom.AH_PostDate; }
			set
			{
				TransferFrom.AH_PostDate = value;
				TransferTo.AH_PostDate = value;
				if (!IsValidationSuspended)
				{
					ValidateAH_PostDate();
				}
				AH_PostDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_PostDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
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
			get { return TransferFrom.AH_InvoiceDate; }
			set
			{
				TransferFrom.AH_InvoiceDate = value;
				TransferTo.AH_InvoiceDate = value;
				AH_InvoiceDateInfo.RefreshBinding();

				if (!IsValidationSuspended)
				{
					ValidateAH_InvoiceDate();
				}
			}
		}

		public ZPropertyInfo AH_InvoiceDateInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.AH_InvoiceDate); }
		}

		#endregion

		#region CreatedDate

		public ZDateTime CreatedDate
		{
			get { return TransferFrom.CreatedDate; }
		}

		public ZPropertyInfo CreatedDateInfo
		{
			get { return GetZPropertyInfo(Schema.CreatedDate); }
		}

		#endregion

		#region CreatingUser

		public ZString CreatingUser
		{
			get { return TransferFrom.CreatingUser; }
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
					if (AH_Ledger == Enterprise.ZArchitecture.Core.LedgerTypes.AccountsReceivable)
					{
						fExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Sell, AH_ExchangeRateAmountInfo, (ZPropertyInfoString)AH_ExchangeRateCurrencyCodeInfo, null);
					}
					else
					{
						fExchangeRate = new ZAccExchangeRate(this, ExchangeRateType.Buy, AH_ExchangeRateAmountInfo, (ZPropertyInfoString)AH_ExchangeRateCurrencyCodeInfo, null);
					}
				}

				return fExchangeRate;
			}
		}

		#endregion

		#region AH_ExchangeRateAmount

		public ZDecimal AH_ExchangeRateAmount
		{
			get { return TransferTo.AH_ExchangeRate; }
			set
			{
				if (TransferTo.AH_ExchangeRate != value)
				{
					TransferTo.AH_ExchangeRate = value;
					TransferFrom.AH_ExchangeRate = value;

					ValidateAH_ExchangeRateAmount();
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
			get { return TransferTo.AH_RX_NKTransactionCurrency; }
			set
			{
				if (TransferTo.AH_RX_NKTransactionCurrency != value)
				{
					TransferTo.AH_RX_NKTransactionCurrency = value;
					TransferFrom.AH_RX_NKTransactionCurrency = value;

					ValidateAH_ExchangeRateCurrency();
					AH_ExchangeRateCurrencyCodeInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AH_ExchangeRateCurrencyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.AH_ExchangeRateCurrencyCode); }
		}

		#endregion

		#region AH_Calc_LocalRX

		[List("Currencies")]
		public ZGuid AH_Calc_LocalRX
		{
			get { return TransferTo.AH_Calc_LocalRX; }
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
			get { return TransferTo.AH_RX_NKTransactionCurrency; }
		}

		public ZPropertyInfo AH_RX_NKTransactionCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.AH_RX_NKTransactionCurrency); }
		}

		#endregion

		#region AH_OSTotal

		public ZDecimal AH_OSTotal
		{
			get { return fAH_OS_Total; }
			set
			{
				if (AH_OSTotal != value)
				{
					fAH_OS_Total = value;
					TransferFrom.AH_OSTotalAmount = value;
					TransferTo.AH_OSTotalAmount = value;

					RefreshAfterTransferBindings();
				}

				if (!IsValidationSuspended)
				{
					ValidateAH_OSTotal();
				}
			}
		}

		public ZPropertyInfo AH_OSTotalInfo
		{
			get { return GetZPropertyInfo(Schema.AH_OSTotal); }
		}

		#endregion

		#region AH_InvoiceAmount

		public ZDecimal AH_InvoiceAmount
		{
			get { return fAH_InvoiceAmount; }
			set
			{
				fAH_InvoiceAmount = value;
				TransferFrom.AH_LocalExTaxAmount = value;
				TransferTo.AH_LocalExTaxAmount = value;
				AH_InvoiceAmountInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_InvoiceAmountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_InvoiceAmount); }
		}

		protected bool AH_InvoiceAmount_ReadOnly
		{
			get { return ah_InvoiceAmount_ReadOnly || TransferTo.AH_LocalExTaxAmountInfo.ReadOnly; }
			set { ah_InvoiceAmount_ReadOnly = value; }
		}
		bool ah_InvoiceAmount_ReadOnly;

		#endregion

		#region AH_FromAccount

		[List("TransferFrom.Lookups.Headers")]
		public ZGuid AH_FromAccount
		{
			get { return TransferFrom.AH_OH; }
			set
			{
				TransferFrom.AH_OH = value;
				if (!IsValidationSuspended)
				{
					ValidateAH_FromAccount();
				}
				AH_FromAccountInfo.RefreshBinding();
				AH_Calc_FromBeforeTransferInfo.RefreshBinding();
				AH_Calc_FromAfterTransferInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_FromAccountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_FromAccount); }
		}

		#endregion

		#region AH_ToAccount

		[List("TransferTo.Lookups.Headers")]
		public ZGuid AH_ToAccount
		{
			get { return TransferTo.AH_OH; }
			set
			{
				TransferTo.AH_OH = value;
				if (!IsValidationSuspended)
				{
					ValidateAH_ToAccount();
				}
				AH_ToAccountInfo.RefreshBinding();
				AH_Calc_ToBeforeTransferInfo.RefreshBinding();
				AH_Calc_ToAfterTransferInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_ToAccountInfo
		{
			get { return GetZPropertyInfo(Schema.AH_ToAccount); }
		}

		#endregion

		#region AH_Calc_FromBeforeTransfer

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_FromBeforeTransfer
		{
			get { return OutstandingBalanceFrom.TotalOutstanding; }
		}

		public ZPropertyInfo AH_Calc_FromBeforeTransferInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_FromBeforeTransfer); }
		}

		#endregion

		#region AH_Calc_FromAfterTransfer

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_FromAfterTransfer
		{
			get { return OutstandingBalanceFrom.TotalOutstanding - AH_InvoiceAmount; }
		}

		public ZPropertyInfo AH_Calc_FromAfterTransferInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_FromAfterTransfer); }
		}

		#endregion

		#region AH_Calc_ToBeforeTransfer

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_ToBeforeTransfer
		{
			get { return OutstandingBalanceTo.TotalOutstanding; }
		}

		public ZPropertyInfo AH_Calc_ToBeforeTransferInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_ToBeforeTransfer); }
		}

		#endregion

		#region AH_Calc_ToAfterTransfer

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal AH_Calc_ToAfterTransfer
		{
			get { return OutstandingBalanceTo.TotalOutstanding + AH_InvoiceAmount; }
		}

		public ZPropertyInfo AH_Calc_ToAfterTransferInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_ToAfterTransfer); }
		}

		#endregion

		#region AH_Calc_FromDueDate

		public ZDateTime AH_Calc_FromDueDate
		{
			get { return TransferFrom.AH_DueDate; }
			set
			{
				TransferFrom.AH_DueDate = value;
				AH_Calc_FromDueDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_Calc_FromDueDateInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_FromDueDate); }
		}

		#endregion

		#region AH_Calc_ToDueDate

		public ZDateTime AH_Calc_ToDueDate
		{
			get { return TransferTo.AH_DueDate; }
			set
			{
				TransferTo.AH_DueDate = value;
				AH_Calc_ToDueDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo AH_Calc_ToDueDateInfo
		{
			get { return GetZPropertyInfo(Schema.AH_Calc_ToDueDate); }
		}

		#endregion

		#region LocalCurrencySubUnitRatio

		public ZInt LocalCurrencySubUnitRatio
		{
			get { return TransferFrom.AH_Calc_LocalRXDecimals; }
		}

		public ZPropertyInfo LocalCurrencySubUnitRatioInfo
		{
			get { return GetZPropertyInfo(nameof(LocalCurrencySubUnitRatio)); }
		}

		#endregion

		#region OSCurrencySubUnitRatio

		public ZInt OSCurrencySubUnitRatio
		{
			get { return TransferFrom.AH_Calc_RXDecimals; }
		}

		public ZPropertyInfo OSCurrencySubUnitRatioInfo
		{
			get { return GetZPropertyInfo(nameof(OSCurrencySubUnitRatio)); }
		}

		#endregion

		#region AH_NumberOfSupportingDocuments
		#region AH_NumberOfSupportingDocuments.Visible

		public ZBool AH_NumberOfSupportingDocumentsVisible_ReadOnly
		{
			get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China; }
		}

		public ZPropertyInfo AH_NumberOfSupportingDocumentsVisibleInfo
		{
			get { return GetZPropertyInfo(Schema.AH_NumberOfSupportingDocumentsVisible_ReadOnly); }
		}

		#endregion
		public ZByte AH_NumberOfSupportingDocuments
		{
			get { return TransferTo.AH_NumberOfSupportingDocuments; }
			set
			{
				if (TransferTo.AH_NumberOfSupportingDocuments != value)
				{
					TransferTo.AH_NumberOfSupportingDocuments = value;
					TransferFrom.AH_NumberOfSupportingDocuments = value;

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
		public void ValidateAH_NumberOfSupportingDocuments()
		{
			AH_NumberOfSupportingDocumentsInfo.ClearAllNotifications();
			if (AH_NumberOfSupportingDocuments < 0)
			{
				AH_NumberOfSupportingDocumentsInfo.AddError(Res.GetString("cfbc714a-a76c-42b2-a526-85821332b4f9", "Number of attachments cannot be less than 0."));
			}
			AH_NumberOfSupportingDocumentsInfo.RunAdditionalValidation();
		}
		#endregion
		#endregion

		#region List Properties

		public RefCurrencyCollection Currencies
		{
			get { return FindboxLookupCollections.GetCurrencyCollection(Factory); }
		}
		#endregion

		#region IIdentified Members

		ZGuid IIdentified.Identifier
		{
			get { return TransferLedger == TransferDirectionTypes.TransferFrom ? ((IIdentified)TransferFrom).Identifier : ((IIdentified)TransferTo).Identifier; }
		}

		#endregion

		#region IHandleDeleteError Members

		bool IHandleDeleteError.RollbackAfterDeleteError
		{
			get { return TransferFrom.IsInDatabase && (TransferFrom.IsDeleted || !(TransferFrom.IsCancelled && TransferFrom.IsCancelledHasChanged)); }
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

		public void RefreshAfterTransferBindings()
		{
			AH_OSTotalInfo.RefreshBinding();
			AH_Calc_FromAfterTransferInfo.RefreshBinding();
			AH_Calc_ToAfterTransferInfo.RefreshBinding();
		}

		public abstract OrgBalance OutstandingBalanceFrom { get; }

		public abstract OrgBalance OutstandingBalanceTo { get; }

		void LoadInternal(TransferRow fromRow, TransferRow toRow)
		{
			fAreTransferRowsInDB = true;

			TransferFrom = fromRow;
			TransferTo = toRow;

			TransferFrom.ParentTransfer = this;
			TransferTo.ParentTransfer = this;

			RegisterEditableChildObject(TransferFrom);
			RegisterEditableChildObject(TransferTo);

			fAH_InvoiceAmount = TransferFrom.AH_LocalExTaxAmount;
			fAH_OS_Total = TransferFrom.AH_OSExTaxAmount;

			SetReadOnlyIncludingChildren(true);
			SetupLocalForeignDataEntry();

			base.AddToFactoryCache();
		}

		public TransferRow TransferFrom
		{
			get
			{
				if (fTransferFrom == null)
				{
					fTransferFrom = (TransferRow)Factory.New(TransferFromType);
					HasChanges = false;
				}

				return fTransferFrom;
			}
			set { fTransferFrom = value; }
		}

		public TransferRow TransferTo
		{
			get
			{
				if (fTransferTo == null)
				{
					fTransferTo = (TransferRow)Factory.New(TransferToType);
					HasChanges = false;
				}

				return fTransferTo;
			}
			set { fTransferTo = value; }
		}

		public abstract Type TransferFromType
		{
			get;
		}

		public abstract Type TransferToType
		{
			get;
		}

		public ZBool AreTransferRowsInDB
		{
			get { return fAreTransferRowsInDB; }
		}

		public ZString TransferLedger
		{
			get
			{
				if (fTransferLedger.IsEmpty)
				{
					fTransferLedger = TransferDirectionTypes.TransferFrom;
				}

				return fTransferLedger;
			}
			set { fTransferLedger = value; }
		}
		ZString fTransferLedger;

		#region Validation

		public void ValidateAH_Desc()
		{
			AH_DescInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_DescInfo);
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
						AH_PostDateInfo.AddError(Res.GetString("ae7f16cc-fb62-4337-8965-16de7ab8d738", "Reversing post date cannot be before the original post date of '{0}'.", OriginalTransaction.AH_PostDate.ToShortDateString()));
					}
				}

				if (!AH_PostDateInfo.HasErrors() && TransferTo.AH_PostDateInfo.HasNotifications())
				{
					AH_PostDateInfo.AddAllNotificationsFrom(TransferTo.AH_PostDateInfo);
				}
			}
		}

		public void ValidateAH_InvoiceDate()
		{
			AH_InvoiceDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_InvoiceDateInfo);
		}

		public void ValidateAH_ExchangeRateAmount()
		{
			if (!IsValidationSuspended)
			{
				AH_ExchangeRateAmountInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AH_ExchangeRateAmountInfo);
				AH_ExchangeRateAmountInfo.RunAdditionalValidation();
			}
		}

		public void ValidateAH_ExchangeRateCurrency()
		{
			if (!IsValidationSuspended)
			{
				AH_ExchangeRateCurrencyCodeInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AH_ExchangeRateCurrencyCodeInfo);
				AH_ExchangeRateCurrencyCodeInfo.RunAdditionalValidation();
			}
		}

		public void ValidateAH_InvoiceAmount()
		{
			AH_InvoiceAmountInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_InvoiceAmountInfo);
		}

		public void ValidateAH_OSTotal()
		{
			AH_OSTotalInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_OSTotalInfo);
		}

		public void ValidateAH_FromAccount()
		{
			AH_FromAccountInfo.ClearAllNotifications();
			if (AH_FromAccount != ZGuid.Empty && AH_FromAccount == AH_ToAccount)
			{
				AH_FromAccountInfo.AddError(Res.GetString("31b3f678-0cc6-4da2-841b-c4c1844bf7a7", "From account and To account must be different"));
			}
			MandatoryValidation.CheckEntered(AH_FromAccountInfo);
			ListValidation.ErrorIfInvalidPK(AH_FromAccountInfo, TransferFrom.Lookups.Headers);
			ListValidation.ErrorIfCancelledAndEditable(AH_FromAccountInfo);
		}

		public void ValidateAH_ToAccount()
		{
			AH_ToAccountInfo.ClearAllNotifications();
			if (AH_ToAccount != ZGuid.Empty && AH_FromAccount == AH_ToAccount)
			{
				AH_ToAccountInfo.AddError(Res.GetString("dd736a5e-917b-40ae-9345-f0a2443f3e1f", "To account and From account must be different"));
			}
			MandatoryValidation.CheckEntered(AH_ToAccountInfo);
			ListValidation.ErrorIfInvalidPK(AH_ToAccountInfo, TransferTo.Lookups.Headers);
			ListValidation.ErrorIfCancelledAndEditable(AH_ToAccountInfo);
		}

		public void ValidateAH_Calc_FromDueDate()
		{
			AH_Calc_FromDueDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_Calc_FromDueDateInfo);
		}

		public void ValidateAH_Calc_ToDueDate()
		{
			AH_Calc_ToDueDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AH_Calc_ToDueDateInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAH_Desc();
			ValidateAH_ExchangeRateAmount();
			ValidateAH_ExchangeRateCurrency();
			ValidateAH_FromAccount();
			ValidateAH_InvoiceAmount();
			ValidateAH_InvoiceDate();
			ValidateAH_OSTotal();
			ValidateAH_PostDate();
			ValidateAH_ToAccount();
			ValidateAH_Calc_FromDueDate();
			ValidateAH_Calc_ToDueDate();
		}

		#endregion

		#region UserAllowedToBackPost

		public virtual bool UserAllowedToBackPost
		{
			get { return false; }
		}

		#endregion

		#region IReversing Members

		public bool IsReversing
		{
			get { return fIsReversing; }
		}
		protected bool fIsReversing;

		public bool IsReversed
		{
			get { return ((IReversing)TransferFrom).IsReversed || ((IReversing)TransferTo).IsReversed; }
		}

		public bool AllowReversingIfMatched
		{
			get { return false; }
		}

		public bool IsMatched
		{
			get { return ((IMatching)TransferFrom).IsMatched || ((IMatching)TransferTo).IsMatched; }
		}

		public void GenerateReverseTransaction(bool mustTransform)
		{
			fIsReversing = true;

			fReverseTransaction = Transfer.New(this.GetType(), Factory);
			fReverseTransaction.IsReverseTransaction = true;
			fReverseTransaction.OriginalTransaction = this;

			fReverseTransaction.AH_ExchangeRateCurrencyCode = this.AH_ExchangeRateCurrencyCode;
			fReverseTransaction.AH_ExchangeRateAmount = this.AH_ExchangeRateAmount;

			fReverseTransaction.ExchangeRate.Currency = ExchangeRate.Currency;
			fReverseTransaction.ExchangeRate.Rate = ExchangeRate.Rate;

			fReverseTransaction.AH_OSTotal = AH_OSTotal * -1;

			fReverseTransaction.TransferFrom.AH_OSTotal = TransferFrom.AH_OSTotal * -1;
			fReverseTransaction.TransferFrom.AH_LocalExTaxAmount = TransferFrom.AH_LocalExTaxAmount * -1;
			fReverseTransaction.TransferFrom.AH_LocalTaxAmount = TransferFrom.AH_LocalTaxAmount * -1;
			fReverseTransaction.TransferFrom.AH_LocalOutstandingAmount = TransferFrom.AH_LocalTotalAmount * -1;

			fReverseTransaction.TransferFrom.AH_AB = TransferFrom.AH_AB;
			fReverseTransaction.TransferFrom.AH_AG = TransferFrom.AH_AG;
			fReverseTransaction.TransferFrom.AH_OH = TransferFrom.AH_OH;
			using (fReverseTransaction.TransferFrom.GetValidationSuspender())
			{
				fReverseTransaction.TransferFrom.AH_GB = TransferFrom.AH_GB;
				fReverseTransaction.TransferFrom.AH_GE = TransferFrom.AH_GE;
			}
			fReverseTransaction.TransferFrom.AH_TransactionCategory = TransferFrom.AH_TransactionCategory;
			fReverseTransaction.TransferFrom.AH_JH = TransferFrom.AH_JH;
			fReverseTransaction.TransferFrom.AH_CashBasisGSTIndicator = TransferFrom.AH_CashBasisGSTIndicator;
			fReverseTransaction.TransferFrom.AH_ChequeDrawer = TransferFrom.AH_ChequeDrawer;
			fReverseTransaction.TransferFrom.AH_ChequeOrReference = TransferFrom.AH_ChequeOrReference;
			fReverseTransaction.TransferFrom.AH_DrawerBank = TransferFrom.AH_DrawerBank;
			fReverseTransaction.TransferFrom.AH_DrawerBranch = TransferFrom.AH_DrawerBranch;
			fReverseTransaction.TransferFrom.AH_InvoiceTerm = TransferFrom.AH_InvoiceTerm;
			fReverseTransaction.TransferFrom.AH_InvoiceTermDays = TransferFrom.AH_InvoiceTermDays;
			fReverseTransaction.TransferFrom.AH_ReceiptType = TransferFrom.AH_ReceiptType;
			fReverseTransaction.TransferFrom.AH_DueDate = ZDateTime.Now;

			fReverseTransaction.TransferTo.AH_OSTotal = TransferTo.AH_OSTotal * -1;
			fReverseTransaction.TransferTo.AH_LocalExTaxAmount = TransferTo.AH_LocalExTaxAmount * -1;
			fReverseTransaction.TransferTo.AH_LocalTaxAmount = TransferTo.AH_LocalTaxAmount * -1;
			fReverseTransaction.TransferTo.AH_LocalOutstandingAmount = TransferTo.AH_LocalTotalAmount * -1;

			fReverseTransaction.TransferTo.AH_AB = TransferTo.AH_AB;
			fReverseTransaction.TransferTo.AH_AG = TransferTo.AH_AG;
			fReverseTransaction.TransferTo.AH_OH = TransferTo.AH_OH;
			using (fReverseTransaction.TransferTo.GetValidationSuspender())
			{
				fReverseTransaction.TransferTo.AH_GB = TransferTo.AH_GB;
				fReverseTransaction.TransferTo.AH_GE = TransferTo.AH_GE;
			}
			fReverseTransaction.TransferTo.AH_TransactionCategory = TransferTo.AH_TransactionCategory;
			fReverseTransaction.TransferTo.AH_JH = TransferTo.AH_JH;
			fReverseTransaction.TransferTo.AH_CashBasisGSTIndicator = TransferTo.AH_CashBasisGSTIndicator;
			fReverseTransaction.TransferTo.AH_ChequeDrawer = TransferTo.AH_ChequeDrawer;
			fReverseTransaction.TransferTo.AH_ChequeOrReference = TransferTo.AH_ChequeOrReference;
			fReverseTransaction.TransferTo.AH_DrawerBank = TransferTo.AH_DrawerBank;
			fReverseTransaction.TransferTo.AH_DrawerBranch = TransferTo.AH_DrawerBranch;
			fReverseTransaction.TransferTo.AH_InvoiceTerm = TransferTo.AH_InvoiceTerm;
			fReverseTransaction.TransferTo.AH_InvoiceTermDays = TransferTo.AH_InvoiceTermDays;
			fReverseTransaction.TransferTo.AH_ReceiptType = TransferTo.AH_ReceiptType;
			fReverseTransaction.TransferTo.AH_DueDate = ZDateTime.Now;
		}

		public IReversing ReverseTransaction
		{
			get { return fReverseTransaction; }
		}
		protected Transfer fReverseTransaction;

		public bool IsReverseTransaction
		{
			get { return fIsReverseTransaction; }
			set
			{
				fIsReverseTransaction = value;
				TransferFrom.IsReverseTransaction = value;
				TransferTo.IsReverseTransaction = value;
			}
		}
		protected bool fIsReverseTransaction;

		public Transfer OriginalTransaction
		{
			get { return fOriginalTransaction; }
			set { fOriginalTransaction = value; }
		}
		protected Transfer fOriginalTransaction;

		public void SetCancellationFlag(bool isCancelled)
		{
			TransferFrom.AH_IsCancelled = new ZBool(isCancelled);
			TransferTo.AH_IsCancelled = new ZBool(isCancelled);
		}

		public void SetTransactionBelongsToGroupField(ZGuid groupingGuidValue)
		{
			ZGuid groupingGuid = TransferFrom.AH_TransactionBelongsToGroup;
			if (fReverseTransaction != null)
			{
				fReverseTransaction.TransferFrom.AH_TransactionBelongsToGroup = groupingGuid;
				fReverseTransaction.TransferTo.AH_TransactionBelongsToGroup = groupingGuid;
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

		#region ITransaction Members

		ZString ITransaction.Ledger
		{
			get { return TransferFrom.AH_Ledger; }
		}

		ZPropertyInfo ITransaction.LedgerInfo
		{
			get { return TransferFrom == null ? GetZPropertyInfo("Ledger") : TransferFrom.AH_LedgerInfo; }
		}

		ZString ITransaction.CurrencyCode
		{
			get
			{
				ZString code = ZString.Empty;
				if (TransferFrom != null)
				{
					code = TransferFrom.AH_RX_NKTransactionCurrency;
				}

				return code;
			}
		}

		ZPropertyInfo ITransaction.CurrencyCodeInfo
		{
			get { return GetWrappedZPropertyInfo("CurrencyCode", x => (TransferFrom != null ? TransferFrom.AH_RX_NKTransactionCurrencyInfo : null)); }
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
			get { return ZArchitecture.Core.TransactionTypes.Transfer; }
		}

		ZPropertyInfo ITransaction.TransactionTypeInfo
		{
			get { return GetZPropertyInfo("TransactionType"); }
		}

		ZGuid ITransaction.Organization
		{
			get { return AH_FromAccount; }
			set { AH_FromAccount = value; }
		}

		ZPropertyInfo ITransaction.OrganizationInfo
		{
			get { return AH_FromAccountInfo; }
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
			get { return OriginalTransaction != null ? (ZString)TransactionTypes.Transfer : ZString.Empty; }
		}

		public ZPropertyInfo OriginalTransactionTypeInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalTransactionType)); }
		}

		public bool OriginalTransactionType_ReadOnly { get { return true; } }

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

		OrgHeaderCollection ITransaction.Headers
		{
			get { return TransferFrom.Lookups.Headers; }
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

		#region IMatching Members

		ZString IMatching.RelatedDisbursementTransactions { get { return ZString.Empty; } }
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

		void IMatching.FullyPay(ZDateTime fullyPaidDate)
		{
			((IMatching)TransferFrom).FullyPay(fullyPaidDate);
			((IMatching)TransferTo).FullyPay(fullyPaidDate);
		}

		void IMatching.PartiallyPay()
		{
		}

		bool IMatching.IsAllPaidInTheSameCurrency(ZString currencyNK)
		{
			return currencyNK == AH_RX_NKTransactionCurrency;
		}

		ZDecimal IMatching.CalculatePaidOutstandingAmountInSpecificCurrencyOnly(ZString currencyNK)
		{
			return currencyNK == AH_RX_NKTransactionCurrency ? ((IMatching)this).OSPartialPaymentAmount : 0;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		void IMatching.GenerateMatchLinksCore()
		{
			TransferFrom.GenerateMatchLinks();
			TransferTo.GenerateMatchLinks();

			if (((IMatching)TransferFrom).CurrentMatchGroup != null && ((IMatching)TransferTo).CurrentMatchGroup != null)
			{
				((IMatching)this).CurrentMatchGroup.AddRange(((IMatching)TransferFrom).CurrentMatchGroup);
				((IMatching)TransferFrom).CurrentMatchGroup.RemoveAll();
				((IMatching)this).CurrentMatchGroup.AddRange(((IMatching)TransferTo).CurrentMatchGroup);
				((IMatching)TransferTo).CurrentMatchGroup.RemoveAll();
			}
			else
			{
				ErrorReporter.ReportOnce("Matchlinks from child Header rows in Transfer were null. From PK: " + TransferFrom.PK + "   ToPK: " + TransferTo.PK);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1121:DoNotIncludeColumnValuesOrNamesInErrorReporterKey", Justification = "Baseline")]
		void IMatching.GeneratePaymentApprovalItems(PaymentApprovalBase approval)
		{
			((IMatching)TransferFrom).GeneratePaymentApprovalItems(approval);
			((IMatching)TransferTo).GeneratePaymentApprovalItems(approval);

			if (((IMatching)TransferFrom).PaymentApprovalItems != null && ((IMatching)TransferTo).PaymentApprovalItems != null)
			{
				((IMatching)this).PaymentApprovalItems.AddRange(((IMatching)TransferFrom).PaymentApprovalItems);
				((IMatching)this).PaymentApprovalItems.AddRange(((IMatching)TransferTo).PaymentApprovalItems);
			}
			else
			{
				ErrorReporter.ReportOnce("PaymentApprovalItems from child Header rows in Transfer were null. From PK: " + TransferFrom.PK + "   ToPK: " + TransferTo.PK);
			}
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
			// NOTE: This should not be used!
			// CanUnmatch(ZDecimal) does not apply to Transfer because it consists of 2 rows rather than 1.  
			// Instead call CanUnmatch() on each individual TransferRow
			throw new NotSupportedException("Transfer is made up of 2 rows so you should check CanUnmatch on each of the 2 rows");
		}

		bool ShouldReverseOnUnmatching
		{
			get { return TransferFrom.AH_TransactionCreatedByMatching && TransferTo.AH_TransactionCreatedByMatching; }
		}

		void IMatching.Unmatch(ZDecimal matchLinkAmount, ZDecimal matchLinkOSAmount)
		{
			if (ShouldReverseOnUnmatching)
			{
				// Note: using LatestMatchLink should be valid since system-generated Transfers
				// must be matched in a single matching session
				((IMatching)TransferFrom).Unmatch(TransferFrom.LatestMatchLink.AP_Amount, TransferFrom.LatestMatchLink.AP_OSAmount);
				((IMatching)TransferTo).Unmatch(TransferTo.LatestMatchLink.AP_Amount, TransferTo.LatestMatchLink.AP_OSAmount);
				ReversingFactory revFactory = new ReversingFactory();
				ReversingBase revBase = revFactory.NewReversing(this);
				revBase.Reverse();
			}
		}

		void IMatching.ChangeUnmatchDate(ZDateTime unmatchDate)
		{
			if (ShouldReverseOnUnmatching)
			{
				((IMatching)TransferFrom).ChangeUnmatchDate(unmatchDate);
				((IMatching)TransferTo).ChangeUnmatchDate(unmatchDate);

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

		[List("Organisations")]
		ZGuid IMatching.Organisation
		{
			get { return ZGuid.Empty; }
		}

		ZPropertyInfo IMatching.OrganisationInfo
		{
			get { return GetZPropertyInfo("Organisation"); }
		}

		ZString IMatching.Ledger
		{
			get { return AH_Ledger; }
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
			get { return ZArchitecture.Core.TransactionTypes.Transfer; }
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
			get { return ZGuid.Empty; }
		}

		ZPropertyInfo IMatching.BranchGuidInfo
		{
			get { return GetZPropertyInfo("BranchGuid"); }
		}

		ZGuid IMatching.DepartmentGuid
		{
			get { return ZGuid.Empty; }
		}

		ZPropertyInfo IMatching.DepartmentGuidInfo
		{
			get { return GetZPropertyInfo("DepartmentGuid"); }
		}

		[MaxLength(3)]
		ZString IMatching.CurrencyCode
		{
			get { return AH_RX_NKTransactionCurrency; }
		}

		ZInt IMatching.CurrencyDecimals
		{
			get { return TransferTo.AH_Calc_RXDecimals; }
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

		ZPropertyInfo IMatching.PaymentCurrencyCodeInfo
		{
			get { return GetZPropertyInfo("CurrencyCode"); }
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
			get { return TransferTo.Lookups.Headers; }
		}

		GlbBranchCollection IMatching.BranchCollection
		{
			get { return FindboxLookupCollections.GetAllBranchesCollection(Factory); }
		}

		GlbDepartmentCollection IMatching.DepartmentCollection
		{
			get { return FindboxLookupCollections.GetDepartmentCollection(Factory); }
		}

		ZGuid IMatching.DisplayInvoiceAddressOverride { get { return ZGuid.Empty; } }
		ZPropertyInfo IMatching.DisplayInvoiceAddressOverrideInfo { get { return GetZPropertyInfo(Schema.DisplayInvoiceAddressOverride); } }

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal NotionalWHTTax => 0M;
		public ZPropertyInfo NotionalWHTTaxInfo => GetZPropertyInfo(nameof(IMatching.NotionalWHTTax));

		[DecimalPlaces(nameof(LocalDecimals))]
		public ZDecimal RealizedWHTTax => 0M;
		public ZPropertyInfo RealizedWTHTaxInfo => GetZPropertyInfo(nameof(IMatching.RealizedWHTTax));

		ZString IMatching.InvoiceTransactionReference => ZString.Empty;

		#endregion

		#region IDataExportBatchSource Members

		ZBool IDataExportBatchSource.IsDataExportBatchSupported
		{
			get
			{
				var source = TransferFrom as IDataExportBatchSource;
				return source != null && source.IsDataExportBatchSupported;
			}
		}

		public DataExportBatchDependentCollection DataExportBatchCollection
		{
			get
			{
				if (!relatedBatchCollectionIsLoaded && relatedBatchCollection == null)
				{
					var source = TransferFrom as IDataExportBatchSource;
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

		#region Implementation

		protected ZBool fAreTransferRowsInDB;
		protected TransferRow fTransferFrom;
		protected TransferRow fTransferTo;
		protected ZAccExchangeRate fExchangeRate;
		protected ZDecimal fAH_OS_Total;
		protected ZDecimal fAH_InvoiceAmount;
		protected RefCurrencyCollection fCurrencies;
		protected OrgHeaderCollection fFromAccountOrgs;
		protected OrgHeaderCollection fToAccountOrgs;

		protected virtual void InitialiseNew()
		{
			base.SetDefaultValues();

			TransferFrom = (TransferRow)Factory.New(TransferFromType);
			TransferTo = (TransferRow)Factory.New(TransferToType);

			TransferFrom.ParentTransfer = this;
			TransferTo.ParentTransfer = this;

			RegisterEditableChildObject(TransferFrom);
			RegisterEditableChildObject(TransferTo);

			using (TransferFrom.SuspendSettingHasChanges())
			using (TransferTo.SuspendSettingHasChanges())
			{
				AH_PostDate = (ZDateTime)Env.Time.CurrentLocalDateTime;
				AH_InvoiceDate = (ZDateTime)Env.Time.CurrentLocalDateTime;
				AH_Calc_FromDueDate = (ZDateTime)Env.Time.CurrentLocalDateTime;
				AH_Calc_ToDueDate = (ZDateTime)Env.Time.CurrentLocalDateTime;

				TransferFrom.AH_TransactionCount = 1;
				TransferTo.AH_TransactionCount = 2;

				TransferFrom.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
				TransferTo.AH_TransactionBelongsToGroup = TransferFrom.AH_TransactionBelongsToGroup;
				SetupLocalForeignDataEntry();
			}

			fAreTransferRowsInDB = false;

			base.AddToFactoryCache();
		}

		void SetupLocalForeignDataEntry()
		{
			new LocalForeignDataEntry(AH_RX_NKTransactionCurrencyInfo, AH_ExchangeRateAmountInfo, AH_InvoiceAmountInfo, AH_OSTotalInfo, ExchangeRate);
		}

		protected IDescription GetPropertyDescriptions(PropertyDescriptor property)
		{
			switch (property.Name)
			{
				case Schema.AH_FromAccount:
				case Schema.AH_ToAccount:
					return new TranslatableDescription(ResString.GetMultilingualString("08251A2D-9914-4e05-AE48-03CB90D6CCC8", "Account"));
				default:
					return null;
			}
		}

		#endregion
	}
}
