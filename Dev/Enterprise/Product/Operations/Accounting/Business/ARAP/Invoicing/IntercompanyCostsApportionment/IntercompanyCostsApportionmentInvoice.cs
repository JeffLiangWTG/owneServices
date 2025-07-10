using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class IntercompanyCostsApportionmentInvoice : NonPersistentBusinessObject, IObsoleteValidation, IInvoiceTerms
	{
		public IntercompanyCostsApportionmentInvoice(BusinessObjectFactory factory)
				: base(factory)
		{
			InvoiceTaxDateCacheProvider = new InvoiceTaxDateCacheProvider();
		}

		#region Properties

		#region Lines

		public IntercompanyCostsApportionmentInvoiceLineCollection Lines
		{
			get
			{
				if (fLines == null)
				{
					fLines = new IntercompanyCostsApportionmentInvoiceLineCollection(Factory, this);
					RegisterEditableChildObject(fLines);
					fLines.CountChanged += (_, _) =>
					{
						InvoiceTaxDateCacheProvider.StaleCache();
					};
				}
				return fLines;
			}
		}
		IntercompanyCostsApportionmentInvoiceLineCollection fLines;

		#endregion

		#region HeaderDefaultCurrency

		protected ZString HeaderDefaultCurrency
		{
			get
			{
				ZString currency = ZString.Empty;
				if (Header != null)
				{
					if (Ledger == LedgerTypes.AccountsPayable)
					{
						currency = Header.CompanyData.OB_RX_NKAPDefltCurrency;
					}
				}
				return (currency == ZString.Empty) ? GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency : currency;
			}
		}

		#endregion

		#region Creditor

		[List("Creditors")]
		public ZGuid Creditor
		{
			get
			{
				return fCreditor;
			}
			set
			{
				if (fCreditor != value)
				{
					SetNonPersistentPropertyValue(CreditorInfo, ref fCreditor, value);

					if (Header != null)
					{
						RefCurrency exchangeCurrency = RefCurrency.LoadFromCurrencyCode(Factory, HeaderDefaultCurrency);
						if (exchangeCurrency != null)
						{
							ExchangeRate.Currency = exchangeCurrency.RX_Code;
						}
					}

					TaxBranch = AccountingMasterFilesUtils.GetTaxBranchResetValue(IsMiscServTaxApplicable);

					TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();
					//Lines.SetGSTReadOnlyStateForAllLines();	//TODO: check
					Lines.CalculateGSTForAllLines();
					if (Header != null && Ledger == LedgerTypes.AccountsPayable)
					{
						IsSelfBillingInvoice = Header.CompanyData.OB_APCostsSelfBilled;
					}
					if (!IsValidationSuspended)
					{
						ValidateCreditor();
					}
				}
			}
		}
		ZGuid fCreditor;

		public ZPropertyInfo CreditorInfo
		{
			get { return GetZPropertyInfo(nameof(Creditor)); }
		}

		public void ValidateCreditor()
		{
			CreditorInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(CreditorInfo, Creditors);
			if (CreditorInfo.Value.IsEmpty)
			{
				CreditorInfo.AddError(Res.GetString("db0425fd-2706-406f-8eca-001da5a40056", "Please enter a Creditor."));
			}
		}

		#endregion

		#region Creditors

		public CreditorCollection Creditors
		{
			get { return fCreditors ?? (fCreditors = new CreditorCollection(Factory)); }
		}
		CreditorCollection fCreditors;

		#endregion

		#region Currency

		[MaxLength(3)]
		[RelatedBusinessObject("CurrencyObject")]
		[List("Currencies")]
		public ZString Currency
		{
			get { return fCurrency; }
			set
			{
				if (fCurrency != value)
				{
					CheckMaximumLength(CurrencyInfo, value);

					SetNonPersistentPropertyValue(CurrencyInfo, ref fCurrency, value);
					SetExchangeRate();
					CurrencyInfo.RefreshBinding();
				}
				if (!IsValidationSuspended)
				{
					ValidateCurrency();
				}
			}
		}
		ZString fCurrency;

		public RefCurrency CurrencyObject
		{
			get { return fCurrencyObject ?? (fCurrencyObject = RefCurrency.LoadFromCurrencyCode(Factory, Currency)); }
		}
		RefCurrency fCurrencyObject;

		public ZPropertyInfo CurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(Currency)); }
		}

		public void ValidateCurrency()
		{
			CurrencyInfo.ClearAllNotifications();
			if (CurrencyInfo.Value.IsEmpty)
			{
				CurrencyInfo.AddError(Res.GetString("6083052c-649a-45be-9765-430a732e6a16", "Please enter an Exchange Rate currency."));
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(CurrencyInfo);
			}
		}

		public RefCurrencyCollection Currencies => fCurrencies ?? (fCurrencies = new RefCurrencyCollection(Factory));
		RefCurrencyCollection fCurrencies;

		public bool IsLocalCurrencyTransaction => Company.GC_RX_NKLocalCurrency == Currency;

		GlbCompany Company => Header?.CompanyData?.Company ?? GlbCompany.CurrentCompany;

		#endregion

		#region Exchange Rate

		public ZExchangeRate ExchangeRate
		{
			get
			{
				if (fExchangeRate == null)
				{
					fExchangeRate = new ZAccExchangeRate(this, RateType, ExchangeRateInfo, (ZPropertyInfoString)CurrencyInfo, null);
					fExchangeRate.IsCurrencyRequired = true;
					fExchangeRate.IsRateRequired = true;
				}
				return fExchangeRate;
			}
		}
		ZExchangeRate fExchangeRate;

		ZDecimal ExchangeRateInternal
		{
			get
			{
				return fExchangeRateInternal;
			}
			set
			{
				if (fExchangeRateInternal != value)
				{
					SetNonPersistentPropertyValue(ExchangeRateInfo, ref fExchangeRateInternal, value);
					Lines.updateLineTaxes();
					Lines.updateLineTotals();
					Lines.updateApportionmentsExchangeRate();
				}
				if (!IsValidationSuspended)
				{
					ValidateExchangeRateInternal();
				}
			}
		}
		ZDecimal fExchangeRateInternal;

		ZPropertyInfo ExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRateInternal)); }
		}

		public void ValidateExchangeRateInternal()
		{
			ExchangeRateInfo.ClearAllNotifications();
			if (ExchangeRateInternal == 0.0M)
			{
				ExchangeRateInfo.AddError(Res.GetString("03ab8cd5-8bec-4be1-b6da-54b9953a1df8", "Please enter a valid Exchange Rate."));
			}
			else
			{
				var notification = ExchangeRateCalculator.CheckExchangeRate(Currency, IsLocalCurrencyTransaction,
					Company, ExchangeRateType.Buy, ExchangeRateValidLedgerEnum.AP, InvoiceDate, PostedDate, InvoiceTaxDate, ExchangeRateInternal);
				if (notification != null)
				{
					ExchangeRateInfo.AddError(notification.Message);
				}
			}
		}

		public ExchangeRateType RateType
		{
			get
			{
				return ExchangeRateType.Buy;
			}
		}

		internal void SetExchangeRate()
		{
			ZDecimal? newExRate = null;

			if (IsLocalCurrencyTransaction)
			{
				newExRate = 1;
			}
			else if (ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, IsLocalCurrencyTransaction, Company.PK))
			{
				newExRate = ExchangeRateCalculator.GetOverrideExchangeRate(Currency, IsLocalCurrencyTransaction, Company.PK, RateType, ExchangeRateValidLedgerEnum.AP, AH_InvoiceDate, AH_PostDate, InvoiceTaxDate);
			}

			if (newExRate.HasValue && ExchangeRateInternal != newExRate)
			{
				ExchangeRateInternal = newExRate.Value;
				ExchangeRate.DontSetTodaysRateOnCurrencyChange = true; //do not recalculate rate by ExchangeRate class internally
			}
		}

		#endregion

		#region Description

		[MaxLength(128)]
		public ZString Description
		{
			get
			{
				return fDescription;
			}
			set
			{
				if (fDescription != value)
				{
					SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);
				}
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region Invoice Number

		[MaxLength(38)]
		public ZString InvoiceNumber
		{
			get
			{
				return fInvoiceNumber;
			}
			set
			{
				if (fInvoiceNumber != value)
				{
					SetNonPersistentPropertyValue(InvoiceNumberInfo, ref fInvoiceNumber, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateInvoiceNumber();
				}
			}
		}
		ZString fInvoiceNumber;

		public ZPropertyInfo InvoiceNumberInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceNumber)); }
		}

		protected bool InvoiceNumber_ReadOnly
		{
			get { return IsSelfBillingInvoice; }
		}

		public void ValidateInvoiceNumber()
		{
			InvoiceNumberInfo.ClearAllNotifications();
			if (TransactionType == TransactionTypes.CreditNote && AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, GlbCompany.CurrentCompany.PK))
			{
				InvoiceNumberInfo.AddError(AccountingMasterFilesUtils.APCreditNoteDisallowedMessage);
			}
			if (!IsSelfBillingInvoice)
			{
				if (InvoiceNumberInfo.Value.IsEmpty)
				{
					InvoiceNumberInfo.AddError(Res.GetString("88825b40-a83e-4c54-9a78-8877372941c5", "Please enter an Invoice Number."));
				}
				if (((invoice == null) || (invoice != null && !invoice.IsInDatabase)) && Creditor.IsValid)
				{
					if ((AccountingUtils.APTransactionNumberExists(TransactionType, InvoiceNumber, Creditor, InvoiceDate)).HasNotification)
					{
						InvoiceNumberInfo.AddError(Res.GetString("49c4ee1f-5c53-426c-bae0-2899bf4283f5", "The transaction number is already in use. Please select another one."));
					}
				}
			}
		}

		#endregion

		#region Document Received Date

		public ZDateTime DocumentReceivedDate
		{
			get
			{
				return fDocumentReceivedDate;
			}
			set
			{
				if (fDocumentReceivedDate != value)
				{
					SetNonPersistentPropertyValue(DocumentReceivedDateInfo, ref fDocumentReceivedDate, value);
					TermsAndDueDateCalculationProvider.CalculateDueDate();
				}
				if (!IsValidationSuspended)
				{
					ValidateDocumentReceivedDate();
				}
			}
		}
		ZDateTime fDocumentReceivedDate;

		public ZPropertyInfo DocumentReceivedDateInfo
		{
			get { return GetZPropertyInfo(nameof(DocumentReceivedDate)); }
		}

		public void ValidateDocumentReceivedDate()
		{
			DocumentReceivedDateInfo.ClearAllNotifications();

			if (AccountingMasterFilesRegistry.Instance.DocumentReceivedDateMustBeEntered.Value)
			{
				MandatoryValidation.CheckEntered(DocumentReceivedDateInfo);
			}
		}

		internal void SetDefaultDocumentReceivedDate()
		{
			var defaultLogic = AccountingMasterFilesRegistry.Instance.DocumentReceivedDateDefaultingLogic.Value;
			if (AH_DocumentReceivedDate.IsEmpty && defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.CreateDate)
			{
				AH_DocumentReceivedDate = ZDateTime.Now;
			}
			else if (defaultLogic == AccountingMasterFilesConstants.DocReceivedDateDefaultLogics.Code.InvoiceDate)
			{
				AH_DocumentReceivedDate = AH_InvoiceDate;
			}
		}

		#endregion

		#region Posted Date

		public ZDateTime PostedDate
		{
			get
			{
				return fPostedDate;
			}
			set
			{
				if (fPostedDate != value)
				{
					SetNonPersistentPropertyValue(PostedDateInfo, ref fPostedDate, value);
					if (!IsLocalCurrencyTransaction && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, IsLocalCurrencyTransaction, GlbCompany.CurrentCompany.PK,
										AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code))
					{
						SetExchangeRate();
					}
					Lines.updateApportionments();
				}
				if (!IsValidationSuspended)
				{
					ValidatePostedDate();
				}
			}
		}
		ZDateTime fPostedDate;

		public ZPropertyInfo PostedDateInfo
		{
			get { return GetZPropertyInfo(nameof(PostedDate)); }
		}

		public void ValidatePostedDate()
		{
			PostedDateInfo.ClearAllNotifications();
			TypeValidation.CheckValidZDateTimeWithoutRange(PostedDateInfo);
			TypeValidation.CheckValidZDateTimeRange(PostedDateInfo);
			/*if (PostedDateInfo.Value.IsEmpty || !PostedDateInfo.Value.IsValid)
			{
					PostedDateInfo.AddError(Res.GetString("08d84885-7b89-48d2-bad3-1786821b2376", "Please enter a valid Posted Date."));
			}
			else if (PostedDate.CompareTo(ZDateTime.Now) > 0)
			{
					PostedDateInfo.AddError(Res.GetString("18caec76-54ac-4c11-8e83-581184dae194", "Posted Date can not be in the future."));
			}*/
			MandatoryValidation.CheckEntered(PostedDateInfo);
			PeriodValidation.CheckDateFallsIntoValidPeriod(PostedDateInfo);

			if (!PostedDateInfo.HasErrors())
			{
				if (PostedDate.Date > ZDateTime.Today)
				{
					PostedDateInfo.AddError(Res.GetString("a371aab1-4471-4ee6-8cf6-eae3b2d24e8a", "The post date must be equal to or prior to today's date"));
				}
			}
			if (!PostedDateInfo.HasErrors())
			{
				if (PostedDate.Date < ZDateTime.Today)
				{
					PostedDateInfo.AddWarning(PreviousPostDateWarning);
				}
			}
		}

		public ZString PreviousPostDateWarning
		{
			get
			{
				return Res.GetString("6eeccee9-f40b-4bc0-9be4-bff77f0553fc", "You are posting to a previous date. If this transaction is posted, there may be implications in the following subsystems \r\n - Financial Reports\r\n - Sub-Ledger Reports\r\n - Bank Reconciliation\r\n - Reversing");
			}
		}

		#endregion

		#region Due Date

		public ZDateTime DueDate
		{
			get
			{
				return fDueDate;
			}
			set
			{
				if (fDueDate != value)
				{
					SetNonPersistentPropertyValue(DueDateInfo, ref fDueDate, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateDueDate();
				}
			}
		}
		ZDateTime fDueDate;

		public ZPropertyInfo DueDateInfo
		{
			get { return GetZPropertyInfo(nameof(DueDate)); }
		}

		public void ValidateDueDate()
		{
			DueDateInfo.ClearAllNotifications();
			if (DueDateInfo.Value.IsEmpty || !DueDateInfo.Value.IsValid)
			{
				DueDateInfo.AddError(Res.GetString("20c3046e-932f-4dce-8136-76d01501ce91", "Please enter a valid Due Date."));
			}
		}

		#endregion

		#region Invoice Date

		public ZDateTime InvoiceDate
		{
			get
			{
				return fInvoiceDate;
			}
			set
			{
				if (fInvoiceDate != value)
				{
					SetNonPersistentPropertyValue(InvoiceDateInfo, ref fInvoiceDate, value);
					if (!IsLocalCurrencyTransaction && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, IsLocalCurrencyTransaction, Company.PK,
									AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code,
									AccountingConstants.InvoicePostingExchangeRateOption.EarliestOfInvoiceOrTaxDate.Code))
					{ 
						SetExchangeRate();
					}
					TermsAndDueDateCalculationProvider.CalculateDueDate();
					SetDefaultDocumentReceivedDate();
				}
				if (!IsValidationSuspended)
				{
					ValidateInvoiceDate();
				}
			}
		}
		ZDateTime fInvoiceDate;

		public ZPropertyInfo InvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(InvoiceDate)); }
		}

		public void ValidateInvoiceDate()
		{
			InvoiceDateInfo.ClearAllNotifications();
			if (InvoiceDateInfo.Value.IsEmpty || !InvoiceDateInfo.Value.IsValid)
			{
				InvoiceDateInfo.AddError(Res.GetString("436b1e1f-b71e-4c00-899f-79ca43a8f72d", "Please enter a valid Invoice Date."));
			}
		}

		#endregion

		#region ExpectedInvoiceTotal

		public ZBool ShouldValidateExpectedInvoiceTotal
		{
			get
			{
				return fShouldValidateExpectedInvoiceTotal;
			}
			set
			{
				if (fShouldValidateExpectedInvoiceTotal != value)
				{
					SetNonPersistentPropertyValue(ShouldValidateExpectedInvoiceTotalInfo, ref fShouldValidateExpectedInvoiceTotal, value);
					if (!value)
					{
						ExpectedInvoiceTotal = ZDecimal.Zero;
					}
				}
			}
		}
		ZBool fShouldValidateExpectedInvoiceTotal;

		public ZPropertyInfo ShouldValidateExpectedInvoiceTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ShouldValidateExpectedInvoiceTotal)); }
		}

		public ZDecimal ExpectedInvoiceTotal
		{
			get
			{
				return fExpectedInvoiceTotal;
			}
			set
			{
				if (fExpectedInvoiceTotal != value)
				{
					SetNonPersistentPropertyValue(ExpectedInvoiceTotalInfo, ref fExpectedInvoiceTotal, value);
					if (!IsValidationSuspended)
					{
						ValidateExpectedInvoiceTotal();
					}
				}
			}
		}
		ZDecimal fExpectedInvoiceTotal;

		public ZPropertyInfo ExpectedInvoiceTotalInfo
		{
			get { return GetZPropertyInfo(nameof(ExpectedInvoiceTotal)); }
		}

		protected bool ExpectedInvoiceTotal_ReadOnly
		{
			get { return !fShouldValidateExpectedInvoiceTotal; }
		}

		public void ValidateExpectedInvoiceTotal()
		{
			ExpectedInvoiceTotalInfo.ClearAllNotifications();
			ZDecimal total = 0.0M;
			foreach (IntercompanyCostsApportionmentInvoiceLine line in Lines)
			{
				total += line.Amount;
			}
			if (ShouldValidateExpectedInvoiceTotal && total != ExpectedInvoiceTotal)
			{
				ExpectedInvoiceTotalInfo.AddError(Res.GetString("165d9dca-15f0-43a7-ba84-ced4100ec4a9", "Expecting invoice lines to sum to a total of {0} but they sum to {1}.", ExpectedInvoiceTotal, total));
			}
		}

		#endregion

		#region TransactionCategory

		[MaxLength(3)]
		public ZString TransactionCategory
		{
			get
			{
				return fTransactionCategory;
			}
			set
			{
				if (fTransactionCategory != value)
				{
					SetNonPersistentPropertyValue(TransactionCategoryInfo, ref fTransactionCategory, value);
					TermsAndDueDateCalculationProvider.SetInvoiceTermsAndDays();
				}
			}
		}
		ZString fTransactionCategory;

		public ZPropertyInfo TransactionCategoryInfo
		{
			get { return GetZPropertyInfo(nameof(TransactionCategory)); }
		}

		#endregion

		#region IsSelfBillingInvoice

		public ZBool IsSelfBillingInvoice
		{
			get
			{
				return TransactionCategory == Constants.TransactionCategory.Codes.SelfBilling;
			}
			set
			{
				if ((IsSelfBillingInvoice != value || TransactionCategory.IsEmpty) &&
						(Ledger == LedgerTypes.AccountsPayable || Ledger == LedgerTypes.UnapprovedPayableTransactions || Ledger == LedgerTypes.IncompleteTransactions))
				{
					TransactionCategory = value ? Constants.TransactionCategory.Codes.SelfBilling : Constants.TransactionCategory.Codes.Standard;
				}
				if (value)
				{
					InvoiceNumber = ZString.Empty;
				}
			}
		}

		#endregion

		#region Ledger

		public ZString Ledger { get; set; }

		#endregion

		#region Branch

		public ZGuid Branch { get; set; }

		#endregion

		#region TaxBranch

		[List("Branches")]
		public ZGuid TaxBranch
		{
			get
			{
				return fTaxBranch;
			}
			set
			{
				if (fTaxBranch != value)
				{
					SetNonPersistentPropertyValue(TaxBranchInfo, ref fTaxBranch, value);
					Lines.Cast<IntercompanyCostsApportionmentInvoiceLine>().ForEach(x => x.TaxBranch = value);
					ValidateTaxBranch();
				}
			}
		}
		ZGuid fTaxBranch;

		public ZPropertyInfo TaxBranchInfo
		{
			get { return GetZPropertyInfo(nameof(TaxBranch)); }
		}

		public GlbBranchCollection Branches => AccountingMasterFilesUtils.GetBranchesOfCurrentCompany(Factory);

		public bool TaxBranch_ReadOnly => !IsMiscServTaxApplicable || !Env.Security.NewPayablesOverrideTaxBranchAllows.IsAllowedWithConstraint();

		#endregion

		#region Department

		public ZGuid Department { get; set; }

		#endregion

		#region TransactionType

		public ZString TransactionType
		{
			get { return InvoiceAmount >= 0 ? TransactionTypes.Invoice : TransactionTypes.CreditNote; }
		}

		#endregion

		#region InvoiceAmount

		public ZDecimal InvoiceAmount
		{
			get
			{
				ZDecimal amount = 0.0M;
				foreach (IntercompanyCostsApportionmentInvoiceLine line in Lines)
				{
					amount += line.Amount;
				}
				return amount;
			}
		}

		#endregion

		#region GSTAmount

		public ZDecimal GSTAmount
		{
			get
			{
				ZDecimal amount = 0.0M;
				foreach (IntercompanyCostsApportionmentInvoiceLine line in Lines)
				{
					amount += line.GSTAmount;
				}
				return amount;
			}
		}

		#endregion

		#region OSTotal

		public ZDecimal OSTotal
		{
			get
			{
				return (InvoiceAmount + GSTAmount) * ExchangeRate.Rate;
			}
		}

		#endregion

		#region OutstandingAmount

		public ZDecimal OutstandingAmount
		{
			get
			{
				return InvoiceAmount + GSTAmount;
			}
		}

		#endregion

		#region IsMiscServTaxApplicable

		public bool IsMiscServTaxApplicable
		{
			get
			{
				if (Header != null && Header.MiscServ != null)
				{
					return (Ledger == LedgerTypes.AccountsReceivable ?
							Header.CompanyData.IsARTaxApplicable :
							Header.CompanyData.IsAPTaxApplicable);
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region IsMiscServWHTApplicable

		public bool IsMiscServWHTApplicable
		{
			get
			{
				if (Header != null && Header.MiscServ != null)
				{
					return (Ledger == LedgerTypes.AccountsReceivable ?
							Header.MiscServ.OM_ARWHTApplicable :
							Header.MiscServ.OM_APWHTApplicable);
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region GSTInclusive

		public ZBool GSTInclusive
		{
			get
			{
				return fGSTInclusive;
			}
			set
			{
				if (fGSTInclusive != value)
				{
					SetNonPersistentPropertyValue(GSTInclusiveInfo, ref fGSTInclusive, value);
					foreach (IntercompanyCostsApportionmentInvoiceLine line in Lines)
					{
						line.GSTInclusiveAmountInfo.RefreshBinding();
					}
				}
			}
		}
		ZBool fGSTInclusive;

		public ZPropertyInfo GSTInclusiveInfo
		{
			get { return GetZPropertyInfo(nameof(GSTInclusive)); }
		}

		#endregion

		#endregion

		#region Calculation Providers

		protected TermsAndDueDateCalculationProvider TermsAndDueDateCalculationProvider
		{
			get
			{
				if (fTermsAndDueDateCalculationProvider == null)
				{
					if (Ledger == LedgerTypes.AccountsPayable)
					{
						fTermsAndDueDateCalculationProvider = new APTermsAndDueDateCalculationProvider(this);
					}
				}
				return fTermsAndDueDateCalculationProvider;
			}
		}
		TermsAndDueDateCalculationProvider fTermsAndDueDateCalculationProvider;

		#endregion

		#region IInvoiceTerms

		public ZDateTime AH_PostDate
		{
			get { return PostedDate; }
			set { PostedDate = value; }
		}

		public ZDateTime AH_InvoiceDate
		{
			get { return InvoiceDate; }
			set { InvoiceDate = value; }
		}

		public ZDateTime AH_DueDate
		{
			get { return DueDate; }
			set { DueDate = value; }
		}

		public ZDateTime AH_DocumentReceivedDate
		{
			get { return DocumentReceivedDate; }
			set { DocumentReceivedDate = value; }
		}

		public ZPropertyInfo AH_DocumentReceivedDateInfo
		{
			get { return DocumentReceivedDateInfo; }
		}

		public ZPropertyInfo AH_PostDateInfo
		{
			get { return PostedDateInfo; }
		}

		public ZPropertyInfo AH_InvoiceDateInfo
		{
			get { return InvoiceDateInfo; }
		}

		public OrgHeader Header
		{
			get
			{
				return Factory.Load<OrgHeader>(Creditor);
			}
		}

		public JobInvoicingConsumerType JobType
		{
			get { return null; }
		}

		public ZString Direction
		{
			get { return ZString.Empty; }
		}

		public ZString TransportMode
		{
			get { return ZString.Empty; }
		}

		public ZGuid AH_GB
		{
			get { return ZGuid.Empty; }
		}

		public ZGuid AH_GE
		{
			get { return ZGuid.Empty; }
		}

		public ZString AH_TransactionCategory
		{
			get { return TransactionCategory; }
		}

		public ZBool AH_IsDisbursementCalc
		{
			get
			{
				return InvoiceTypeCalculationProvider.IsDisbursementInvoiceType(TransactionCategory);
			}
		}

		public ZByte AH_InvoiceTermDays
		{
			get
			{
				return fInvoiceTermDays;
			}
			set
			{
				if (fInvoiceTermDays != value)
				{
					SetNonPersistentPropertyValue(AH_InvoiceTermDaysInfo, ref fInvoiceTermDays, value);
					TermsAndDueDateCalculationProvider.CalculateDueDate();
				}
			}
		}
		ZByte fInvoiceTermDays;

		public ZPropertyInfo AH_InvoiceTermDaysInfo
		{
			get { return GetZPropertyInfo(nameof(AH_InvoiceTermDays)); }
		}

		[MaxLength(3)]
		public ZString AH_InvoiceTerm
		{
			get
			{
				return fInvoiceTerm;
			}
			set
			{
				if (fInvoiceTerm != value)
				{
					SetNonPersistentPropertyValue(AH_InvoiceTermInfo, ref fInvoiceTerm, value);
					TermsAndDueDateCalculationProvider.CalculateDueDate();
					if (value == Constants.InvoiceTerms.CashOnDelivery || value == Constants.InvoiceTerms.PaymentInAdvance)
					{
						AH_InvoiceTermDays = ZByte.Zero;
					}
				}
			}
		}
		ZString fInvoiceTerm;

		public ZPropertyInfo AH_InvoiceTermInfo
		{
			get { return GetZPropertyInfo(nameof(AH_InvoiceTerm)); }
		}

		JobHeader IInvoiceTerms.Job => null;

		#endregion

#if DEBUG
		internal
#endif
		ZDateTime InvoiceTaxDate => InvoiceTaxDateCacheProvider.GetEarliestInvoiceTaxDate(Lines.Cast<IntercompanyCostsApportionmentInvoiceLine>(), AH_InvoiceDate);

		#region PeriodValidationProvider

		PeriodValidationProvider fPeriodValidation;
		public PeriodValidationProvider PeriodValidation
		{
			get
			{
				if (fPeriodValidation == null)
				{
					fPeriodValidation = GetPeriodValidationProvider();
				}

				return fPeriodValidation;
			}
		}

		protected virtual PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new PeriodValidationProvider(Factory);
		}

		#endregion

		#region Defaults

		public ZString DefaultDescription
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(Ledger + TransactionType,
								 Ledger + " " + new CodeDescriptionPairList(OLookUpEditType.TransactionTypes).GetDescriptionFromCode(TransactionType));
			}
		}

		protected override void SetDefaultValues()
		{
			Description = DefaultDescription;
			IsSelfBillingInvoice = false;
			Ledger = LedgerTypes.AccountsPayable;
			Branch = GlbBranch.CurrentBranch.PK;
			Department = GlbDepartment.CurrentDepartment.PK;
			ExchangeRate.Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			PostedDate = ZDateTime.Now;
			InvoiceDate = ZDateTime.Now;
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCreditor();
			ValidateInvoiceNumber();
			ValidateInvoiceDate();
			ValidateDueDate();
			ValidatePostedDate();
			ValidateDocumentReceivedDate();
			ValidateCurrency();
			ValidateExchangeRateInternal();
			ValidateExpectedInvoiceTotal();
			ValidateThatRowsExist();
			ValidateTaxBranch();
		}

		void ValidateTaxBranch()
		{
			TaxBranchInfo.ClearAllNotifications();
			if (AccountingMasterFilesUtils.IsTaxBranchApplicable && IsMiscServTaxApplicable)
			{
				MandatoryValidation.CheckEntered(TaxBranchInfo);
				ListValidation.ErrorIfInvalidPK(TaxBranchInfo, Branches);
			}
		}

		void ValidateThatRowsExist()
		{
			ClearRowNotifications();
			if (Lines.Count == 0)
			{
				AddRowError(Res.GetString("4c9bb94a-eca4-4298-bd9b-0c37d36dd2ae", "You must have at least 1 line in the Charges grid."));
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("bb583c99-5c0d-4cfe-a8da-95b7e9d7390b", "Overhead Cost Apportionment");
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (invoice != null)
				{
					InvoiceNumber = invoice.AH_TransactionNum;
				}
			}
		}

		#endregion

		#region Implementation

		bool areBusinessObjectsAlreadyCreatedForPosting;
#if DEBUG
		internal
#endif
		InvoicingBase invoice;

		internal readonly InvoiceTaxDateCacheProvider InvoiceTaxDateCacheProvider;

		public InvoicingBase CreateBusinessObjectsForPosting()
		{
			if (!areBusinessObjectsAlreadyCreatedForPosting)
			{
				areBusinessObjectsAlreadyCreatedForPosting = true;
				var helper = new PostingHelper(this, Factory);
				try
				{
					invoice = helper.CreateTransactionLinesForPosting();
				}
				catch (EmptyComplianceSubTypeException)
				{
					areBusinessObjectsAlreadyCreatedForPosting = false;
					throw;
				}
			}
			return invoice;
		}

		Dictionary<ZGuid, GLJournal> fJournals;
		public Dictionary<ZGuid, GLJournal> Journals
		{
			get
			{
				if (fJournals == null)
				{
					fJournals = new Dictionary<ZGuid, GLJournal>();
				}
				return fJournals;
			}
		}

		public class PostingHelper
		{
			public PostingHelper(IntercompanyCostsApportionmentInvoice parent, BusinessObjectFactory factory)
			{
				Argument.NotNull(parent, "IntercompanyCostsApportionmentInvoice");
				this.parent = parent;
				this.factory = factory;
			}
			readonly IntercompanyCostsApportionmentInvoice parent;
			readonly BusinessObjectFactory factory;

			public InvoicingBase CreateTransactionLinesForPosting()
			{
				InvoicingBase invoice = null;

				if (parent.InvoiceAmount >= 0)
				{
					invoice = factory.New<APInvoice>();
				}
				else
				{
					invoice = factory.New<APCreditNote>();
				}

				createAPInvoice(invoice);
				addInvoiceLines(invoice);
				if (!invoice.Factory.IsEqualToCurrentSaveCount(invoice.FactoryCountUsedToGenerateTransactionReference))
				{
					if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
					{
						invoice.UpdateComplianceSubTypeAndSequenceNumberTogether(); //set AH_TransactionReference from compliance number sequence which is kind of number fountain.
						if (AccountingConfigurationRegistry.Instance.DisallowPostingTransactionWithEmptyComplianceSubtype.Value && invoice.AH_ComplianceSubType.IsEmpty)
						{
							invoice.Delete();
							throw new EmptyComplianceSubTypeException();
						}
					}
				}
				addGLJournals(invoice);

				return invoice;
			}

			void addGLJournals(InvoicingBase apInvoiceOrCreditNote)
			{
				foreach (IntercompanyCostsApportionmentInvoiceLine invoiceLine in parent.Lines)
				{
					foreach (IntercompanyCostsApportionment apportionment in invoiceLine.Apportionments)
					{
						if (apportionment.Company != GlbCompany.CurrentCompany.PK)
						{
							GLJournal apportionmentCompanyJournal = null;
							if (!parent.Journals.TryGetValue(apportionment.Company, out apportionmentCompanyJournal))
							{
								apportionmentCompanyJournal = CreateApportionmentCompanyJournal(apInvoiceOrCreditNote, apportionment);
							}

							GLJournalLine debitLine = (GLJournalLine)apportionmentCompanyJournal.Lines.AddNew();
							SetValuesToDebitLine(debitLine, apportionment);

							GLJournalLine creditLine = (GLJournalLine)apportionmentCompanyJournal.Lines.AddNew();
							SetValuesToCreditLine(creditLine, apportionment);
						}
					}
				}
			}

			void addInvoiceLines(InvoicingBase apInvoice)
			{
				foreach (IntercompanyCostsApportionmentInvoiceLine invoiceLine in parent.Lines)
				{
					InvoicingLineBase line;
					Dictionary<ZGuid, ZDecimal> localTotals = new Dictionary<ZGuid, ZDecimal>();
					Dictionary<ZGuid, ZDecimal> localTaxTotals = new Dictionary<ZGuid, ZDecimal>();
					Dictionary<ZGuid, ZDecimal> foreignTotals = new Dictionary<ZGuid, ZDecimal>();
					Dictionary<ZGuid, ZDecimal> foreignTaxTotals = new Dictionary<ZGuid, ZDecimal>();
					Dictionary<ZGuid, ZGuid> clearing = new Dictionary<ZGuid, ZGuid>();

					foreach (IntercompanyCostsApportionment apportionment in invoiceLine.Apportionments)
					{
						ZInt multiplier = apInvoice is APInvoice ? 1 : -1;
						if (apportionment.Company == GlbCompany.CurrentCompany.PK)
						{
							line = (InvoicingLineBase)apInvoice.Lines.AddNew();
							line.GenericCharge = invoiceLine.GenericCharge;
							line.AL_GB = apportionment.Branch;
							line.AL_GE = apportionment.Department;
							line.AL_OSExTaxAmount = apportionment.ForeignApportionedAmount * multiplier; //os ex tax amount;
							line.AL_SupplyType = invoiceLine.AL_SupplyType;
							line.AL_AT = invoiceLine.AL_AT;
							line.SetTaxDateSafe(invoiceLine.AL_TaxDate);
							line.AL_A9_VATClass = invoiceLine.AL_A9_VATClass;
							line.AL_OSTaxAmount = apportionment.ForeignGST * multiplier;
							line.AL_Desc = apportionment.Description;
							line.AL_GovtChargeCode = invoiceLine.AL_GovtChargeCode;
							line.AL_GB_TaxBranch = invoiceLine.TaxBranch;
						}
						if (localTotals.ContainsKey(apportionment.Company))
						{
							localTotals[apportionment.Company] = localTotals[apportionment.Company] + (apportionment.LocalAmount * multiplier);
						}
						else
						{
							localTotals.Add(apportionment.Company, apportionment.LocalAmount * multiplier);
						}
						if (!clearing.ContainsKey(apportionment.Company))
						{
							clearing.Add(apportionment.Company, apportionment.IntercompanyGLAccount);
						}

						if (foreignTotals.ContainsKey(apportionment.Company))
						{
							foreignTotals[apportionment.Company] = foreignTotals[apportionment.Company] + (apportionment.ForeignApportionedAmount * multiplier);
						}
						else
						{
							foreignTotals.Add(apportionment.Company, apportionment.ForeignApportionedAmount * multiplier);
						}

						if (foreignTaxTotals.ContainsKey(apportionment.Company))
						{
							foreignTaxTotals[apportionment.Company] = foreignTaxTotals[apportionment.Company] + (apportionment.ForeignGST * multiplier);
						}
						else
						{
							foreignTaxTotals.Add(apportionment.Company, apportionment.ForeignGST * multiplier);
						}

						if (localTaxTotals.ContainsKey(apportionment.Company))
						{
							localTaxTotals[apportionment.Company] = localTaxTotals[apportionment.Company] + (apportionment.LocalGST * multiplier);
						}
						else
						{
							localTaxTotals.Add(apportionment.Company, (apportionment.LocalGST * multiplier));
						}
					}

					foreach (ZGuid company in clearing.Keys)
					{
						if (company != GlbCompany.CurrentCompany.PK)
						{
							line = (InvoicingLineBase)apInvoice.Lines.AddNew();
							line.AL_GB = invoiceLine.Branch;
							line.AL_GE = invoiceLine.AL_GE;
							line.AL_AG = clearing[company];
							line.AL_OSExTaxAmount = foreignTotals[company];
							line.AL_SupplyType = invoiceLine.AL_SupplyType;
							line.AL_AT = invoiceLine.AL_AT;
							line.SetTaxDateSafe(invoiceLine.AL_TaxDate);
							line.AL_A9_VATClass = invoiceLine.AL_A9_VATClass;
							line.AL_OSTaxAmount = foreignTaxTotals[company];
							line.AL_LocalExTaxAmount = localTotals[company];
							line.AL_LocalTaxAmount = localTaxTotals[company];
							line.AL_Desc = invoiceLine.Description;
							line.AL_GovtChargeCode = invoiceLine.AL_GovtChargeCode;
							line.AL_GB_TaxBranch = invoiceLine.TaxBranch;
						}
					}
				}
			}

			void createAPInvoice(InvoicingBase apInvoice)
			{
				apInvoice.AH_OH = parent.Creditor;
				apInvoice.IsSelfBillingInvoice = parent.IsSelfBillingInvoice;
				if (!apInvoice.IsSelfBillingInvoice)
				{
					apInvoice.AH_TransactionNum = parent.InvoiceNumber;
				}
				apInvoice.AH_Desc = parent.Description;
				apInvoice.AH_PostDate = parent.PostedDate;
				apInvoice.AH_InvoiceDate = parent.InvoiceDate;
				apInvoice.AH_TransactionCategory = parent.TransactionCategory;
				apInvoice.AH_RX_NKTransactionCurrency = parent.ExchangeRate.Currency;
				apInvoice.AH_ExchangeRate = parent.ExchangeRate.Rate;
				apInvoice.AH_InvoiceTerm = parent.AH_InvoiceTerm;
				apInvoice.AH_InvoiceTermDays = parent.AH_InvoiceTermDays;
				apInvoice.AH_GB = parent.Branch;
				apInvoice.AH_GE = parent.Department;
				apInvoice.AH_DocumentReceivedDate = parent.DocumentReceivedDate;
				apInvoice.AH_DueDate = parent.DueDate;
				apInvoice.AH_GB_TaxBranch = parent.TaxBranch;
				using (apInvoice.SetExchangeRateSuspender.GetSuspender())
				{
					apInvoice.UseJobExchangeRate = false;
				}
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
			GLJournal CreateApportionmentCompanyJournal(InvoicingBase apInvoiceOrCreditNote, IntercompanyCostsApportionment apportionment)
			{
				var apportionmentCompanyJournal = factory.New<GLJournal>();
				var journalDescription = ResString.GetMultilingualString("9bed8eb4-c38a-4f8d-831b-ebde033fa2f0", "INTERCOMPANY COST ALLOCATION FROM {0} AP {1} {2} {3}",
													 GlbCompany.CurrentCompany.GC_Code,
													 apInvoiceOrCreditNote.AH_TransactionType,
													 apInvoiceOrCreditNote.AH_TransactionNum,
													 apInvoiceOrCreditNote.Header.OH_Code);

				StmALog addedLog = apportionmentCompanyJournal.Logs.AddedLog;
				if (addedLog != null)
				{
					using (((IUpdateFieldsLock)addedLog).LockForUpdatingKeyFields())
					{
						addedLog.SL_Reference = journalDescription.GetUnresolvedString();
					}
				}
				apportionmentCompanyJournal.AH_Desc = journalDescription;
				parent.Journals.Add(apportionment.Company, apportionmentCompanyJournal);

				apportionmentCompanyJournal.AH_GB = apportionment.Branch;
				apportionmentCompanyJournal.AH_GE = apportionment.Department;
				apportionmentCompanyJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
				apportionmentCompanyJournal.PostPeriod = apportionment.AccountingPeriod;
				return apportionmentCompanyJournal;
			}

			void SetValuesToDebitLine(GLJournalLine debitLine, IntercompanyCostsApportionment apportionment)
			{
				debitLine.AL_AG = apportionment.GetCostGLAccount();
				debitLine.AL_GB = apportionment.Branch;
				debitLine.AL_GE = apportionment.Department;
				debitLine.DebitCreditSign = apportionment.ForeignApportionedAmount >= 0 ? nameof(DebitCredit.DR) : nameof(DebitCredit.CR);
				debitLine.UnsignedOSLineAmount = apportionment.CompanyLocalAmount;
			}

			void SetValuesToCreditLine(GLJournalLine creditLine, IntercompanyCostsApportionment apportionment)
			{
				creditLine.AL_AG = apportionment.CompanyPostToGLAccount;
				creditLine.AL_GB = apportionment.Branch;
				creditLine.AL_GE = apportionment.Department;
				creditLine.DebitCreditSign = apportionment.ForeignApportionedAmount >= 0 ? nameof(DebitCredit.CR) : nameof(DebitCredit.DR);
				creditLine.UnsignedOSLineAmount = apportionment.CompanyLocalAmount;
			}
		}

		#endregion

	}
}
