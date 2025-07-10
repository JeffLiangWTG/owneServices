using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class Voucher : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T206";
		public ZString VoucherDate { get; set; }
		public ZInt FinancialYear { get; set; }
		public ZInt Period { get; set; }
		public ZString VoucherTypeNumber { get; set; }
		public ZString VoucherNumber { get; set; }
		public ZString VoucherLineNumber { get; set; }
		public ZString VoucherDescription { get; set; }
		public ZString GLAccountNumber { get; set; }
		public ZString GLAccountAssistedNumber1 { get; set; }
		public ZString GLAccountAssistedNumber2 { get; set; }
		public ZString GLAccountAssistedNumber3 { get; set; }
		public ZString GLAccountAssistedNumber4 { get; set; }
		public ZString GLAccountAssistedNumber5 { get; set; }
		public ZString GLAccountAssistedNumber6 { get; set; }
		public ZString GLAccountAssistedNumber7 { get; set; }
		public ZString GLAccountAssistedNumber8 { get; set; }
		public ZString GLAccountAssistedNumber9 { get; set; }
		public ZString GLAccountAssistedNumber10 { get; set; }
		public ZString GLAccountAssistedNumber11 { get; set; }
		public ZString GLAccountAssistedNumber12 { get; set; }
		public ZString GLAccountAssistedNumber13 { get; set; }
		public ZString GLAccountAssistedNumber14 { get; set; }
		public ZString GLAccountAssistedNumber15 { get; set; }
		public ZString GLAccountAssistedNumber16 { get; set; }
		public ZString GLAccountAssistedNumber17 { get; set; }
		public ZString GLAccountAssistedNumber18 { get; set; }
		public ZString GLAccountAssistedNumber19 { get; set; }
		public ZString GLAccountAssistedNumber20 { get; set; }
		public ZString GLAccountAssistedNumber21 { get; set; }
		public ZString GLAccountAssistedNumber22 { get; set; }
		public ZString GLAccountAssistedNumber23 { get; set; }
		public ZString GLAccountAssistedNumber24 { get; set; }
		public ZString GLAccountAssistedNumber25 { get; set; }
		public ZString GLAccountAssistedNumber26 { get; set; }
		public ZString GLAccountAssistedNumber27 { get; set; }
		public ZString GLAccountAssistedNumber28 { get; set; }
		public ZString GLAccountAssistedNumber29 { get; set; }
		public ZString GLAccountAssistedNumber30 { get; set; }
		public ZString CurrencyCode { get; set; }
		public ZString Unit { get; set; }
		public ZDecimal DebitQuantity { get; set; }
		public ZDecimal DebitCurrencyAmount { get; set; }
		public ZDecimal DebitAmountLocalCurrency { get; set; }
		public ZDecimal CreditQuantity { get; set; }
		public ZDecimal CreditCurrencyAmount { get; set; }
		public ZDecimal CreditAmountLocalCurrency { get; set; }
		public ZString ExRateTypeNumber { get; set; }
		public ZDecimal ExRate { get; set; }
		public ZDecimal UnitPrice { get; set; }
		public ZString VoucherHeaderExtendedFieldSchemasValue { get; set; }
		public ZString EntryLineExtendedFieldSchemas { get; set; }
		public ZString PaymentTypeCode { get; set; }
		public ZString VoucherType { get; set; }
		public ZString VoucherDocNumber { get; set; }
		public ZString VoucherDocDate { get; set; }
		public ZInt Attachments { get; set; }
		public ZString PreparedBy { get; set; }
		public ZString Reviwer { get; set; }
		public ZString EnteredBy { get; set; }
		public ZString Cashier { get; set; }
		public ZInt AccountingFlag { get; set; }
		public ZInt VoidFlag { get; set; }
		public ZString VoucherSourceSystem { get; set; }
		public ZString OrgCode { get; set; }
		public ZGuid AccountPK { get; set; }
		public ZString CashFlowCode { get; set; }
		public ZString CashFlowItemAttribute { get; set; }
	}

	public class VoucherCollection : NonPersistentBusinessObjectCollection<Voucher>	{
		public VoucherCollection(BusinessObjectFactory factory) : base(factory) { }

		public void FillCashFlowVoucherCollection(ZDateTime fromDate, ZDateTime endDate, ZString branchCode)
		{
			if (!AllowCashFlow) { return; }

			isCashFlow = true;
			AddElements(fromDate, endDate, branchCode);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Voucher();
		}

		#region Cash Flow

		ZBool isCashFlow = false;

		protected ZQuery TransactionTypeFilter
		{
			get
			{
				return new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, new string[] { TransactionTypes.Payment, TransactionTypes.Receipt, TransactionTypes.DirectReceipt, TransactionTypes.DirectPayment });
			}
		}

		protected virtual bool AllowCashFlow
		{
			get { return true; }
		}

		#endregion

		public virtual ZQuery ExtraFilter
		{
			get
			{
				return new ZQuery();
			}
		}

		public void AddElements(ZDateTime fromDate, ZDateTime endDate, ZString branchCode, bool exportWIPAccrualVoucher = true)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			var branch = GlbCompany.CurrentCompany.Branches.FirstOrDefault(branch1 => branch1.GB_Code == branchCode);
			if (branch != null)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_GB, branch.PK);
			}

			if (fromDate.IsValid && endDate.IsValid)
			{
				filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, fromDate);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_PostDate, SQLComparisonOperator.LessThan, endDate);
			}

			if (isCashFlow)
			{
				filter.AddToFilter(TransactionTypeFilter);
			}

			if (!ExtraFilter.IsEmpty)
			{
				filter.AddToFilter(ExtraFilter);
			}

			AccPeriodManagement accPeriodManagement = (new AccountingPeriodCalculator(Factory)).GetPeriodManagementFromDate(fromDate);
			if (accPeriodManagement != null)
			{
				ZInt year = accPeriodManagement.AM_Year;
				AccTransactionHeader[] transacitonList = Factory.Load(typeof(AccTransactionHeader), filter) as AccTransactionHeader[];

				if (transacitonList != null)
				{
					VoucherProviderFactory voucherFactory = new VoucherProviderFactory(Factory);
					foreach (AccTransactionHeader transaction in transacitonList)
					{
						if ((transaction.AH_TransactionType == TransactionTypes.Contra || (transaction.AH_TransactionType == TransactionTypes.Transfer && transaction.AH_Ledger == LedgerTypes.CashBook))
							&& transaction.AH_TransactionCount != AccTransactionHeader.TransactionCountConstants.BankTransferFromRow
							&& transaction.AH_TransactionCount != AccTransactionHeader.TransactionCountConstants.BankTransferToRowWhenReversing)
						{
							continue;
						}
						VoucherProvider provider = voucherFactory.GetProvider(transaction);
						if (provider == null)
						{
							continue;
						}

						SetVoucherValue(year, transaction, provider);
					}
				}

				if (exportWIPAccrualVoucher && branchCode.IsEmpty && endDate.Date == accPeriodManagement.AM_EndDate.Date.AddDays(1))
				{
					WIPVoucherProvider wIPVoucherProvider = WIPVoucherProvider.New(Factory, accPeriodManagement.AM_Period);
					AccrualVoucherProvider accrualVoucherProvider = AccrualVoucherProvider.New(Factory, accPeriodManagement.AM_Period);

					if (wIPVoucherProvider != null && wIPVoucherProvider.VoucherLines != null)
					{
						SetVoucherValue(year, null, wIPVoucherProvider);
					}

					if (accrualVoucherProvider != null && accrualVoucherProvider.VoucherLines != null)
					{
						SetVoucherValue(year, null, accrualVoucherProvider);
					}
				}
			}
		}

		protected virtual void SetVoucherValue(ZInt year, AccTransactionHeader transaction, VoucherProvider provider)
		{
			ZInt i = 0;
			foreach (VoucherLine voucherLine in provider.VoucherLines)
			{
				if (!voucherLine.ValidateLine())
				{
					Notifications.Add(voucherLine.ValidationError);
					continue;
				}
				if (isCashFlow && transaction?.BankAccount != null && transaction.BankAccount.GLHeader.PK == voucherLine.AccountPK)
				{
					continue;
				}

				i++;
				Voucher voucher = AddNew();
				SetVoucherLineValue(year, transaction, provider, voucher, voucherLine, i);
			}
		}

		protected virtual void SetVoucherLineValue(ZInt year, AccTransactionHeader transaction, VoucherProvider provider, Voucher voucher, VoucherLine voucherLine, ZInt i)
		{
			voucher.VoucherDate = voucherLine.VoucherDate.ToString("yyyyMMdd");
			voucher.FinancialYear = year;
			voucher.Period = provider.Period;
			voucher.VoucherTypeNumber = "1";
			voucher.VoucherNumber = voucherLine.VoucherType.Replace("-", "") + voucherLine.VoucherNumber;
			voucher.VoucherLineNumber = i.ToString();
			voucher.VoucherDescription = voucherLine.Description;
			voucher.GLAccountNumber = voucherLine.AccountNumber;
			voucher.CurrencyCode = voucherLine.CurrencyCode;

			voucher.DebitCurrencyAmount = voucherLine.OSDebitAmount;
			voucher.DebitAmountLocalCurrency = voucherLine.DebitAmount;

			voucher.CreditCurrencyAmount = voucherLine.OSCreditAmount;
			voucher.CreditAmountLocalCurrency = voucherLine.CreditAmount;
			voucher.ExRateTypeNumber = "1";
			voucher.ExRate = voucherLine.ExchangeRate <= 0 ? 1 : voucherLine.ExchangeRate;

			voucher.VoucherType = voucherLine.VoucherType.Replace("-", "");
			if (transaction != null)
			{
				voucher.PaymentTypeCode = transaction.AH_ReceiptType;
				voucher.VoucherDocNumber = GetInvoiceNumber(transaction);

				if (provider.TransactionType == TransactionTypes.Payment || provider.TransactionType == TransactionTypes.Receipt)
				{
					voucher.CashFlowCode = transaction.AH_TransactionCategory;
				}

				if (provider.TransactionType == TransactionTypes.DirectPayment || provider.TransactionType == TransactionTypes.DirectReceipt)
				{
					var gLHeader = Factory.Load<AccGLHeader>(voucherLine.AccountPK);
					if (gLHeader != null)
					{
						voucher.CashFlowCode = gLHeader.AG_CashFlowType;
					}
				}
			}

			if (provider.TransactionType == TransactionTypes.DirectReceipt || provider.TransactionType == TransactionTypes.Receipt)
			{
				voucher.CashFlowItemAttribute = "1";
				if (transaction != null && transaction.AH_IsCancelled)
				{
					voucher.CashFlowItemAttribute = "0";
				}
			}

			if (provider.TransactionType == TransactionTypes.DirectPayment || provider.TransactionType == TransactionTypes.Payment)
			{
				voucher.CashFlowItemAttribute = "0";
				if (transaction != null && transaction.AH_IsCancelled)
				{
					voucher.CashFlowItemAttribute = "1";
				}
			}

			voucher.VoucherDocDate = provider.PostDate.ToString("yyyyMMdd");
			voucher.Attachments = transaction == null ? voucherLine.AttachmentCount : transaction.AH_NumberOfSupportingDocuments;
			voucher.PreparedBy = provider.PostedBy;
			voucher.Reviwer = provider.Reviewer;
			voucher.EnteredBy = provider.EnterBy;
			voucher.Cashier = provider.Cashier;
			voucher.AccountingFlag = 1;
			voucher.VoidFlag = 0;
			voucher.OrgCode = provider.OrganisationCode;
			voucher.AccountPK = voucherLine.AccountPK;
			if (AccountingConfigurationRegistry.Instance.ARControlAccount.Value == voucher.AccountPK || AccountingConfigurationRegistry.Instance.APControlAccount.Value == voucher.AccountPK)
			{
				voucher.GLAccountAssistedNumber1 = provider.OrganisationCode;
			}
		}

		#region Notifications

		public IList<ValidationInfo> Notifications { get; set; } = new List<ValidationInfo>();

		#endregion

		protected ZString GetInvoiceNumber(AccTransactionHeader transaction)
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
	}
}