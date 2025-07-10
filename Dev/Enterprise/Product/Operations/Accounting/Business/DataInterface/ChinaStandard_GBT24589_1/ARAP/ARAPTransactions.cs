using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class ARAPTransactions : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T303";
		public ZString ClientCode { get; set; }
		public ZString GLAccountNumber { get; set; }
		public ZString VoucherDate { get; set; }
		public ZString EnteredDate { get; set; }
		public ZInt FinancialYear { get; set; }
		public ZInt Period { get; set; }
		public ZString VoucherTypeNumber { get; set; }
		public ZString VoucherNumber { get; set; }
		public ZString BaseCurrency { get; set; }
		public ZDecimal ExRate { get; set; }
		public ZString CRDR { get; set; }
		public ZDecimal LocalBalance { get; set; }
		public ZDecimal OSBalance { get; set; }
		public ZDecimal LocalTransactionAmount { get; set; }
		public ZString TransactionCurrency { get; set; }
		public ZDecimal OSTransactionAmount { get; set; }
		public ZString Description { get; set; }
		public ZString DueDate { get; set; }
		public ZString VerifyMatchVoucherNumber { get; set; }
		public ZString VerifyMatchDate { get; set; }
		public ZString BillsTypeCode { get; set; }
		public ZString TransactionTypeCode { get; set; }
		public ZString BillsNumber { get; set; }
		public ZString InvoiceNumber { get; set; }
		public ZString ContractNumber { get; set; }
		public ZString ItemCode { get; set; }
		public ZString PaymentTypeCode { get; set; }
		public ZString PaymentDate { get; set; }
		public ZString VerifyMatchFlag { get; set; }
		public ZString RemittanceDraftNumber { get; set; }
	}

	public class ARAPTransactionsCollection : NonPersistentBusinessObjectCollection<ARAPTransactions>	{
		public ARAPTransactionsCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ARAPTransactions();
		}

		public void BuildTransactions(ZString ledgerType, ZDateTime fromDate, ZDateTime endDate)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, ledgerType);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			if (fromDate.IsValid)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, fromDate);
			}

			if (endDate.IsValid)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThan, endDate);
			}

			ZQuery ledgerAndTypeFilter = new ZQuery();
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Invoice));
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.AdjustmentNote), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.CreditNote), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Payment), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Receipt), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Discount), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Overpayment), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Transfer), JoinCondition.Or);
			ledgerAndTypeFilter.AddToFilter(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Contra), JoinCondition.Or);

			filter.AddToFilter(ledgerAndTypeFilter);

			AccTransactionHeader[] transacitonList = Factory.Load(typeof(AccTransactionHeader), filter) as AccTransactionHeader[];
			VoucherProviderFactory voucherFactory = new VoucherProviderFactory(Factory);

			ZGuid aPcontrolAccountPK = AccountingConfigurationRegistry.Instance.APControlAccount.Value;
			ZGuid aRcontrolAccountPK = AccountingConfigurationRegistry.Instance.ARControlAccount.Value;

			if (transacitonList != null)
			{
				foreach (AccTransactionHeader transaction in transacitonList)
				{
					VoucherProvider provider = voucherFactory.GetProvider(transaction);

					if (provider == null)
					{
						continue;
					}

					foreach (VoucherLine voucherLine in provider.VoucherLines)
					{
						if (ledgerType == LedgerTypes.AccountsPayable && voucherLine.AccountPK != aPcontrolAccountPK)
						{
							continue;
						}

						if (ledgerType == LedgerTypes.AccountsReceivable && voucherLine.AccountPK != aRcontrolAccountPK)
						{
							continue;
						}

						ZString clientCode;
						ZDecimal localBalance;
						if (transaction.AH_TransactionType == TransactionTypes.Transfer || transaction.AH_TransactionType == TransactionTypes.Contra)
						{
							clientCode = voucherLine.OrganisationCode;
							localBalance = voucherLine.OutstandingAmount;
						}
						else
						{
							clientCode = provider.OrganisationCode;
							localBalance = transaction.AH_OutstandingAmount;
						}

						ARAPTransactions arTransactions = AddNew();

						arTransactions.ClientCode = clientCode;
						arTransactions.GLAccountNumber = voucherLine.AccountNumber;
						arTransactions.VoucherDate = voucherLine.VoucherDate.ToString("yyyyMMdd");
						arTransactions.EnteredDate = transaction.AH_SystemCreateTimeUtc.ToString("yyyyMMdd");
						arTransactions.FinancialYear = voucherLine.VoucherDate.Year;
						arTransactions.Period = provider.Period;
						arTransactions.VoucherTypeNumber = "1";
						arTransactions.VoucherNumber = voucherLine.VoucherNumber;
						arTransactions.BaseCurrency = GlbCompany.CurrentCompany.LocalCurrency.RX_DescMultilingual.ToString(Constants.Languages.ChineseSimplified);

						arTransactions.TransactionCurrency = arTransactions.BaseCurrency;
						arTransactions.TransactionCurrency = ChineseUtils.GetCurrencyNameForChinese(Factory, voucherLine.CurrencyCode);

						arTransactions.ExRate = voucherLine.ExchangeRate <= 0 ? 1 : voucherLine.ExchangeRate;

						if (ledgerType == LedgerTypes.AccountsPayable)
						{
							arTransactions.CRDR = localBalance == 0 ? (NoResString)"平" : (NoResString)"贷";
							arTransactions.LocalBalance = localBalance == 0 ? 0 : -1 * localBalance;
							arTransactions.LocalTransactionAmount = voucherLine.CreditAmount - voucherLine.DebitAmount;
							arTransactions.OSTransactionAmount = voucherLine.OSCreditAmount - voucherLine.OSDebitAmount;
						}

						if (ledgerType == LedgerTypes.AccountsReceivable)
						{
							arTransactions.CRDR = localBalance == 0 ? (NoResString)"平" : (NoResString)"借";
							arTransactions.LocalBalance = localBalance;
							arTransactions.LocalTransactionAmount = voucherLine.DebitAmount - voucherLine.CreditAmount;
							arTransactions.OSTransactionAmount = voucherLine.OSDebitAmount - voucherLine.OSCreditAmount;
						}

						arTransactions.OSBalance = arTransactions.LocalBalance == 0 ? 0 : Env.CurrentCompany.ExchangeRate.LocalToForeign(arTransactions.LocalBalance, arTransactions.ExRate, voucherLine.CurrencyCode);

						if (arTransactions.TransactionCurrency == arTransactions.BaseCurrency)
						{
							arTransactions.OSTransactionAmount = arTransactions.LocalTransactionAmount;
						}

						arTransactions.Description = voucherLine.Description;
						arTransactions.DueDate = provider.DueDate.ToString("yyyyMMdd");
						arTransactions.VerifyMatchVoucherNumber = voucherLine.VoucherNumber;
						arTransactions.VerifyMatchDate = provider.PostDate.ToString("yyyyMMdd");
						arTransactions.BillsTypeCode = GetBillsTypeCode(ledgerType, transaction.AH_TransactionType);
						arTransactions.TransactionTypeCode = voucherLine.VoucherType.Replace("-", "");
						arTransactions.BillsNumber = provider.TransactionNumber;

						arTransactions.InvoiceNumber = GetInvoiceNumber(transaction);
						arTransactions.ContractNumber = ZString.Empty;
						arTransactions.ItemCode = ZString.Empty;
						arTransactions.PaymentTypeCode = transaction.AH_ReceiptType;
						arTransactions.PaymentDate = transaction.AH_FullyPaidDate.ToString("yyyyMMdd");
						arTransactions.VerifyMatchFlag = "1";
						arTransactions.RemittanceDraftNumber = transaction.AH_ChequeOrReference;
					}
				}
			}
		}

		ZString GetInvoiceNumber(AccTransactionHeader transaction)
		{
			switch (transaction.AH_TransactionType)
			{
				case TransactionTypes.AdjustmentNote:
				case TransactionTypes.CreditNote:
				case TransactionTypes.Invoice:
					if (transaction.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						return transaction.AH_TransactionNum;
					}
					if (transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						return transaction.AH_TransactionReference;
					}
					break;
			}
			return ZString.Empty;
		}

		ZString GetBillsTypeCode(ZString ledgerType, ZString transactionType)
		{
			switch (transactionType)
			{
				case TransactionTypes.Payment:
				case TransactionTypes.Receipt:
				case TransactionTypes.Transfer:
				case TransactionTypes.Contra:
					return ledgerType == LedgerTypes.AccountsPayable ? ARAPBillTypes.Codes.D04 : ARAPBillTypes.Codes.D02;

				default:
					return ledgerType == LedgerTypes.AccountsPayable ? ARAPBillTypes.Codes.D03 : ARAPBillTypes.Codes.D01;
			}
		}
	}
}

