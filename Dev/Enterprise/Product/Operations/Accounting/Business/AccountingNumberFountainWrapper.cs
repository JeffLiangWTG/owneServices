using System;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingIServices;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Business
{
	public class AccountingNumberFountainWrapper
	{
		public AccountingNumberFountainWrapper(AccountingNumberFountainPooler numberFountainPooler)
			: this(numberFountainPooler, NumberFountainType.None)
		{
		}

		public AccountingNumberFountainWrapper(AccountingNumberFountainPooler numberFountainPooler, NumberFountainType numberFountainType, int? transactionNumDigits = null)
		{
			this.numberFountainPooler = numberFountainPooler;
			this.numberFountainType = numberFountainType;
			priorityTransactionNumDigits = transactionNumDigits;
			SetTransactionNumberDigitsWithFallback();
			periodCalculator = new AccountingPeriodCalculator(InnerFactory);
		}

		void SetTransactionNumberDigitsWithFallback()
		{
			int result = (priorityTransactionNumDigits == null) ? FormattedNumberFountainFactory.DefaultFormatDigits : priorityTransactionNumDigits.Value;

			if (numberFountainType != NumberFountainType.None &&
				(priorityTransactionNumDigits == null || AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Inner.HasValueForAnyLevel()))
			{
				var customisationCollection = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value;
				var element = customisationCollection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber];
				if (element != null && element.Include)
				{
					result = element.Length;
				}
			}

			transactionNumDigits = result;
		}

		readonly int? priorityTransactionNumDigits;
		int transactionNumDigits;
		protected internal AccountingNumberFountainPooler numberFountainPooler;
		readonly NumberFountainType numberFountainType;
		protected AccountingPeriodCalculator periodCalculator;

		protected virtual string GetNext(IDbConnected factory, ZDateTime postDate)
		{
			return GetNumberFountain(postDate).GetNextFormatted(factory);
		}

		public INumberFountainProxy GetNumberFountain(ZDateTime postDate)
		{
			return GetNumberFountainCore(postDate);
		}

		protected virtual INumberFountainProxy GetNumberFountainCore(ZDateTime postDate)
		{
			return numberFountainPooler.GetTodaysPeriodFountain(transactionNumDigits);
		}

		#region Generation

		public string Generate(IAccountingNumberFountainDataSource dataSource)
		{
			if (dataSource == null)
			{
				throw new ArgumentNullException(nameof(dataSource));
			}

			var transactionHeader = dataSource as TransactionHeader;

			if (transactionHeader != null)
			{
				NumberFountainTransactionDataProvider.Initialize(transactionHeader);
			}

			var factory = dataSource.Factory;
			var branch = dataSource.Branch;
			var department = dataSource.Department;

			if (factory == null)
			{
				throw new ArgumentNullException(nameof(dataSource), "Factory in dataSource.Factory cannot be null");
			}

			if (branch == null)
			{
				throw new ArgumentNullException(nameof(dataSource), "Branch in dataSource.Branch cannot be null");
			}

			if (department == null)
			{
				throw new ArgumentNullException(nameof(dataSource), "Department in dataSource.Department cannot be null");
			}

			return GenerateCore(dataSource);
		}

		protected virtual string GenerateCore(IAccountingNumberFountainDataSource dataSource)
		{
			var factory = dataSource.Factory;
			var postDate = dataSource.PostDate;

			string result;
			SetTransactionNumberDigitsWithFallback();

			if (numberFountainType == NumberFountainType.None)
			{
				result = GetNext(factory, postDate);
			}
			else
			{
				TransactionNumberSequenceCustomisationCollection customisationCollection = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.Value;
				ZString nonConfigurableKey = GetNonUserConfigurableFountainKeyFromFountainType(numberFountainType);

				if (nonConfigurableKey == GetKey(customisationCollection, nonConfigurableKey, false, dataSource))
				{
					result = GetNext(factory, postDate);
				}
				else
				{
					result = GenerateNumber(customisationCollection, nonConfigurableKey, dataSource, GlbCompany.CurrentCompany.PK.ToGuid());
				}
			}

			return result;
		}

		protected string GenerateNumber(TransactionNumberSequenceCustomisationCollection customisationCollection, ZString nonConfigurableKey, IAccountingNumberFountainDataSource dataSource, Guid companyPk)
		{
			var factory = dataSource.Factory;

			string fountainKey = GetKey(customisationCollection, nonConfigurableKey, true, dataSource);
			Func<long> lasySeedGetter = () => { return Env.NumberFountains.GetAccountingNumberGeneratorFountain(fountainKey, companyPk).GetNext(factory); };
			return GenerateNumberCore(lasySeedGetter, customisationCollection, dataSource);
		}

		string GenerateNumberCore(Func<long> lazySeedGetter, TransactionNumberSequenceCustomisationCollection customisationCollection, IAccountingNumberFountainDataSource dataSource)
		{
			var sortedCollection = customisationCollection.Cast<TransactionNumberSequenceCustomisation>().OrderBy(e => e.Order).ToArray();

			string defaultCNSequenceNum = null;
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
			{
				defaultCNSequenceNum = TryGenerateSequenceNumberForChinaDefaultSettings(customisationCollection, dataSource.PostDate);
			}

			string result = null;
			StringBuilder builder = new StringBuilder();

			foreach (TransactionNumberSequenceCustomisation element in sortedCollection)
			{
				if (element.Include)
				{
					if (element.ElementName == TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber)
					{
						result = defaultCNSequenceNum ?? GetValue(element, lazySeedGetter(), dataSource);
					}
					else
					{
						result = GetValue(element, 0, dataSource);
					}
					builder.Append(result);
				}
			}

			return builder.ToString();
		}

		public ZString GetNonUserConfigurableFountainKeyFromFountainType(NumberFountainType type)
		{
			ZString result;

			switch (type)
			{
				case NumberFountainType.ARInvoice:
					result = LedgerTypes.AccountsReceivable + TransactionTypes.Invoice;
					break;
				case NumberFountainType.ARCreditNote:
					if (AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.Value.Value)
					{
						result = LedgerTypes.AccountsReceivable + TransactionTypes.Invoice;
					}
					else
					{
						result = LedgerTypes.AccountsReceivable + TransactionTypes.CreditNote;
					}
					break;
				case NumberFountainType.ARAdjustmentNote:
					if (AccountingConfigurationRegistry.Instance.ShareSequentialInvoiceTransactionNumbers.Value.Value)
					{
						result = LedgerTypes.AccountsReceivable + TransactionTypes.Invoice;
					}
					else
					{
						result = LedgerTypes.AccountsReceivable + TransactionTypes.AdjustmentNote;
					}
					break;
				case NumberFountainType.ARJournal:
					result = LedgerTypes.AccountsReceivable + TransactionTypes.Journal;
					break;
				case NumberFountainType.ARTransfer:
					result = LedgerTypes.AccountsReceivable + TransactionTypes.Transfer;
					break;
				case NumberFountainType.ARExchangeDifference:
					result = LedgerTypes.AccountsReceivable + TransactionTypes.ExchangeDifference;
					break;
				case NumberFountainType.ARDiscount:
					result = LedgerTypes.AccountsReceivable + TransactionTypes.Discount;
					break;
				case NumberFountainType.SelfBillingInvoice:
					result = LedgerTypes.AccountsPayable + Constants.TransactionCategory.Codes.SelfBilling;
					break;
				case NumberFountainType.APInvoice:
					result = LedgerTypes.AccountsPayable + TransactionTypes.Invoice;
					break;
				case NumberFountainType.APInvoiceInternalReference:
					result = LedgerTypes.AccountsPayable + "SBI";
					break;
				case NumberFountainType.APCreditNote:
					result = LedgerTypes.AccountsPayable + "SBC";
					break;
				case NumberFountainType.APCreditNoteInternalReference:
					result = LedgerTypes.AccountsPayable + TransactionTypes.CreditNote;
					break;
				case NumberFountainType.APAdjustmentNote:
					result = LedgerTypes.AccountsPayable + "SBA";
					break;
				case NumberFountainType.APAdjustmentNoteInternalReference:
					result = LedgerTypes.AccountsPayable + TransactionTypes.AdjustmentNote;
					break;
				case NumberFountainType.APJournal:
					result = LedgerTypes.AccountsPayable + TransactionTypes.Journal;
					break;
				case NumberFountainType.APTransfer:
					result = LedgerTypes.AccountsPayable + TransactionTypes.Transfer;
					break;
				case NumberFountainType.APExchangeDifference:
					result = LedgerTypes.AccountsPayable + TransactionTypes.ExchangeDifference;
					break;
				case NumberFountainType.APOverpayment:
					result = LedgerTypes.AccountsPayable + TransactionTypes.Overpayment;
					break;
				case NumberFountainType.APDiscount:
					result = LedgerTypes.AccountsPayable + TransactionTypes.Discount;
					break;
				case NumberFountainType.Contra:
					result = TransactionTypes.Contra;
					break;
				case NumberFountainType.Payment:
					result = TransactionTypes.Payment;
					break;
				case NumberFountainType.Receipt:
					result = TransactionTypes.Receipt;
					break;
				case NumberFountainType.DirectPayment:
					result = LedgerTypes.CashBook + TransactionTypes.DirectPayment;
					break;
				case NumberFountainType.DirectReceipt:
					result = LedgerTypes.CashBook + TransactionTypes.DirectReceipt;
					break;
				case NumberFountainType.OpeningPayment:
					result = LedgerTypes.CashBook + TransactionTypes.OpeningPayment;
					break;
				case NumberFountainType.OpeningReceipt:
					result = LedgerTypes.CashBook + TransactionTypes.OpeningReceipt;
					break;
				case NumberFountainType.CashBookExchangeDifference:
					result = LedgerTypes.CashBook + TransactionTypes.ExchangeDifference;
					break;
				case NumberFountainType.CashBookTransfer:
					result = LedgerTypes.CashBook + TransactionTypes.Transfer;
					break;
				case NumberFountainType.GLJournal:
					result = LedgerTypes.General + TransactionTypes.Journal;
					break;
				case NumberFountainType.AROverpayment:
					result = LedgerTypes.AccountsReceivable + TransactionTypes.Overpayment;
					break;
				case NumberFountainType.CFXJournal:
					result = LedgerTypes.JobCosting + TransactionTypes.Journal;
					break;
				case NumberFountainType.JRJournal:
					result = LedgerTypes.JobCosting + TransactionTypes.JobRevenueJournal;
					break;
				case NumberFountainType.PaymentBatch:
					result = LedgerTypes.AccountsPayable + "PYB";
					break;
				case NumberFountainType.EPaymentQuoteInternalRef:
					result = LedgerTypes.AccountsPayable + "EPQ";
					break;
				case NumberFountainType.APInvoiceApproval:
					result = "APIAP";
					break;
				case NumberFountainType.CreditControlApproval:
					result = "CCAP";
					break;
				case NumberFountainType.APPaymentApprovalReference:
					result = LedgerTypes.AccountsPayable + "PYA";
					break;
				case NumberFountainType.ARPaymentApprovalReference:
					result = LedgerTypes.AccountsReceivable + "PYA";
					break;
				case NumberFountainType.EPaymentDealInternalReference:
					result = TransactionTypes.Payment + "EPD";
					break;
				case NumberFountainType.EPaymentBeneficiaryRequestInternalRef:
					result = TransactionTypes.Payment + "EPB";
					break;
				default:
					result = ZString.Empty;
					break;
			}

			return result;
		}

		string GetKey(TransactionNumberSequenceCustomisationCollection customisationCollection, ZString nonUserConfigurableKey, bool fountainOnly, IAccountingNumberFountainDataSource dataSource)
		{
			var sortedCollection = customisationCollection.Cast<TransactionNumberSequenceCustomisation>().OrderBy(e => e.Order).ToArray();

			StringBuilder builder = new StringBuilder(nonUserConfigurableKey);

			foreach (TransactionNumberSequenceCustomisation element in sortedCollection)
			{
				var isFountainIncluded = element.Include && (!fountainOnly || element.Fountain);
				if ((isFountainIncluded || IsFountainMandatory(element, dataSource)) && element.ElementName != TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber)
				{
					builder.Append(GetValue(element, 0, dataSource));
				}
			}

			return builder.ToString();
		}

		protected virtual bool IsFountainMandatory(TransactionNumberSequenceCustomisation element, IAccountingNumberFountainDataSource dataSource) => false;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity", Justification = "This method should not be split.")]
		protected virtual string GetValue(TransactionNumberSequenceCustomisation element, long seed, IAccountingNumberFountainDataSource dataSource)
		{
			AccPeriodManagement periodManagement = null;
			string result = string.Empty;
			if (element == null)
			{
				throw new ArgumentNullException(nameof(element));
			}

			var branch = dataSource.Branch;
			var department = dataSource.Department;
			var postDate = dataSource.PostDate;

			switch (element.ElementName)
			{
				case TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber:
					result = seed.ToString("D" + element.Length, CultureInfo.InvariantCulture);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode:
					result = branch.GB_Code.SubstringSafe(0, ZInt.ParseSafe(element.Code, 3));
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode:
					result = department.GE_Code.SubstringSafe(0, ZInt.ParseSafe(element.Code, 3));
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.CustomElement1:
				case TransactionNumberSequenceCustomisation.ElementNames.CustomElement2:
					result = element.Code;
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.YearAsDigits:
					result = GetDetailFromCode(postDate.Year, element.Code);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.YearAsLetter:
					result = GetLetterFromYear(postDate.Year);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.MonthAs2Digits:
					result = postDate.Month.ToString("00", CultureInfo.InvariantCulture);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.MonthAsLetter:
					result = "ABCDEFGHIJKL"[postDate.Month - 1].ToString(CultureInfo.InvariantCulture);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits:
					periodManagement = periodCalculator.GetPeriodManagementFromDate(postDate, GlbCompany.CurrentCompany.PK);
					if (periodManagement != null)
					{
						result = GetDetailFromCode(periodManagement.AM_Year, element.Code);
					}
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsLetter:
					periodManagement = periodCalculator.GetPeriodManagementFromDate(postDate, GlbCompany.CurrentCompany.PK);
					if (periodManagement != null)
					{
						result = GetLetterFromYear(periodManagement.AM_Year);
					}
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits:
					periodManagement = periodCalculator.GetPeriodManagementFromDate(postDate, GlbCompany.CurrentCompany.PK);
					if (periodManagement != null)
					{
						result = new ZString(periodManagement.AM_Period.ToString()).Right(2);
					}
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TaxAndNonTax:
					result = GetTaxOrNonTaxCode(dataSource, element);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.SelfBillingAndStandard:
					result = GetSelfBillingOrStandardCode(dataSource, element);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.CorrectedAndOriginal:
					result = GetCorrectedOrOriginalCode(dataSource, element);
					break;
				case TransactionNumberSequenceCustomisation.ElementNames.TransactionTypePrefix:
					result = GetTransactionTypePrefix(dataSource);
					break;
			}

			return result;
		}

		ZString GetTaxOrNonTaxCode(IAccountingNumberFountainDataSource dataSource, TransactionNumberSequenceCustomisation element)
		{
			var result = ZString.Empty;
			var transactionHeader = dataSource as TransactionHeader;

			if (transactionHeader != null)
			{
				result = NumberFountainTransactionDataProvider.GetIsTaxReported(transactionHeader) ? element.Code1 : element.Code2;
			}

			return result;
		}

		ZString GetSelfBillingOrStandardCode(IAccountingNumberFountainDataSource dataSource, TransactionNumberSequenceCustomisation element)
		{
			var result = ZString.Empty;
			var transactionHeader = dataSource as TransactionHeader;

			if (transactionHeader != null)
			{
				result = NumberFountainTransactionDataProvider.GetIsSelfBilling(transactionHeader) ? element.Code1 : element.Code2;
			}

			return result;
		}

		ZString GetCorrectedOrOriginalCode(IAccountingNumberFountainDataSource dataSource, TransactionNumberSequenceCustomisation element)
		{
			var result = ZString.Empty;
			var transactionHeader = dataSource as TransactionHeader;

			if (transactionHeader != null)
			{
				result = NumberFountainTransactionDataProvider.GetIsCorrected(transactionHeader) ? element.Code1 : element.Code2;
			}

			return result;
		}

		ZString GetTransactionTypePrefix(IAccountingNumberFountainDataSource dataSource)
		{
			var result = ZString.Empty;
			var transactionHeader = dataSource as TransactionHeader;

			if (transactionHeader != null)
			{
				var transactionTypePrefixCollection = AccountingConfigurationRegistry.Instance.TransactionTypePrefix.Value.Cast<TransactionTypePrefix>();
				var macthedPrefix = transactionTypePrefixCollection.FirstOrDefault(x => x.Ledger == transactionHeader.AH_Ledger && x.TransactionType == transactionHeader.AH_TransactionType);
				if (macthedPrefix != null)
				{
					result = macthedPrefix.Prefix;
				}
			}

			return result;
		}

		protected string GetLetterFromYear(int year)
		{
			return "BCDEFGHIJKLMNOPQRSTUVWXYZA"[year % 26].ToString(CultureInfo.InvariantCulture);
		}

		protected string GetDetailFromCode(int year, string code)
		{
			var detail = ZInt.ParseSafe(code, 4);
			return new ZString(year.ToString(CultureInfo.InvariantCulture)).Right(detail);
		}

		// To keep new default China number fountain continue the sequence of the old one.
		string TryGenerateSequenceNumberForChinaDefaultSettings(TransactionNumberSequenceCustomisationCollection customisationCollection, ZDateTime postDate)
		{
			string result = null;

			if (customisationCollection.Cast<TransactionNumberSequenceCustomisation>().Count(x => x.Fountain) == 3)
			{
				var sequenceNum = customisationCollection[TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber];
				var accountingYear = customisationCollection[TransactionNumberSequenceCustomisation.ElementNames.AccountingYearAsDigits];
				var accountingPeriod = customisationCollection[TransactionNumberSequenceCustomisation.ElementNames.AccountingPeriodAs2Digits];
				if (sequenceNum != null && sequenceNum.Include && sequenceNum.Fountain &&
					accountingYear != null && accountingYear.Code == "2" && accountingYear.Include && accountingYear.Fountain &&
					accountingPeriod != null && accountingPeriod.Include && accountingPeriod.Fountain &&
					accountingYear.Order < accountingPeriod.Order)
				{
					var period = periodCalculator.GetPeriodFromDate(postDate);
					var periodString = (period >= 100) ? period.ToString().Substring(2) : period.ToString();
					result = numberFountainPooler.GetPeriodFountain(periodString, sequenceNum.Length).GetNextFormatted(InnerFactory);
					result = new ZString(result).Right(sequenceNum.Length);
				}
			}

			return result;
		}

		#endregion

		BusinessObjectFactory InnerFactory
		{
			get
			{
				if (innerFactory == null)
				{
					innerFactory = new BusinessObjectFactory();
				}
				return innerFactory;
			}
		}
		BusinessObjectFactory innerFactory;

#if DEBUG

		// Just returns no-argument PeekPreliminary
		public string PeekPreliminary(IDbConnected factory)
		{
			return numberFountainPooler.GetTodaysPeriodFountain(transactionNumDigits).PeekPreliminaryFormatted(factory);
		}

		public string PeekPreliminary(IDbConnected factory, string voucherPeriod)
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China)
			{
				if (voucherPeriod.Length > 2) // i.e. has at least 3 digits in the period
				{
					ZString postPeriodString = voucherPeriod.Substring(2);
					return numberFountainPooler.GetPeriodFountain(postPeriodString, transactionNumDigits).PeekPreliminaryFormatted(factory);
				}
				else // this should never be the case
				{
					return numberFountainPooler.GetPeriodFountain(voucherPeriod, transactionNumDigits).PeekPreliminaryFormatted(factory);
				}
			}
			else
			{
				return PeekPreliminary(factory);
			}
		}
#endif
	}
}
