using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class ARAPContraVoucherProvider : TransactionWithoutLinesVoucherProvider
	{
		public ARAPContraVoucherProvider(AccTransactionHeader transaction)
			: base(transaction, new ControlAccountProvider())
		{
		}

		protected override ZGuid GetGLAccountPKFromTransactionHeader()
		{
			return GetGLAccountPKFromControlAccount();
		}

		public override VoucherLine[] VoucherLines
		{
			get
			{
				if (fVoucherLines == null)
				{
					fVoucherLines = new VoucherLine[MaxVoucherLineNo];

					fVoucherLines[0] = new VoucherLine(Factory);
					SetVoucherLine(fVoucherLines[0], FirstRow, AccountingConfigurationRegistry.Instance.APControlAccount.Value);

					fVoucherLines[1] = new VoucherLine(Factory);
					SetVoucherLine(fVoucherLines[1], SecondRow, AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
				}
				return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
			}
		}

		protected ZString GetDescription(AccTransactionHeader transactionHeader)
		{
			VoucherDescriptionLookUp voucherDescriptionLookUp = new VoucherDescriptionLookUp(transactionHeader);

			ZString headerDescription = voucherDescriptionLookUp.GetDescription();
			ZString transactionLineDescription = VoucherDebitCreditLookUp.GetTransactionLineDescription();
			ZString jobNumber = VoucherDebitCreditLookUp.GetJobNumber();

			return DataInterfaceUtils.GetVoucherDescription(headerDescription, transactionLineDescription, jobNumber, IncludeJobNumber);
		}

		void SetVoucherLine(VoucherLine voucherLine, AccTransactionHeader rowToProcess, Guid controlAccount)
		{
			if (rowToProcess != null)
			{
				voucherLine.AccountPK = controlAccount;

				if (IncludeOrganisationCode)
				{
					voucherLine.AdditionalAccountDescription += new VoucherDebitCreditLookUp(rowToProcess).GetOrganisationCode();
				}
				voucherLine.OutstandingAmount = rowToProcess.AH_OutstandingAmount;
				voucherLine.OrganisationCode = rowToProcess.Header == null ? ZString.Empty : rowToProcess.Header.OH_Code;
				voucherLine.VoucherType = new VoucherTypeLookUp(rowToProcess).GetVoucherType();
				voucherLine.VoucherNumber = new VoucherNumberLookUp(rowToProcess).GetVoucherNumber();
				voucherLine.DebitAmount = new VoucherDebitCreditLookUp(rowToProcess).GetDebit();
				voucherLine.CreditAmount = new VoucherDebitCreditLookUp(rowToProcess).GetCredit();
				voucherLine.OSDebitAmount = new VoucherDebitCreditLookUp(rowToProcess).GetOSDebit();
				voucherLine.OSCreditAmount = new VoucherDebitCreditLookUp(rowToProcess).GetOSCredit();
				voucherLine.VoucherDate = GetVoucherDate();
				voucherLine.Description = GetDescription(rowToProcess);
				voucherLine.CurrencyCode = GetCurrencyCode();
				voucherLine.ForeignCurrencyAmount = GetForeignCurrencyAmount();
				voucherLine.ExchangeRate = GetExchangeRate();
			}
		}

		protected override int MaxVoucherLineNo
		{
			get { return 2; }
		}

		ZQuery fFirstRowFilter;
		ZQuery FirstRowFilter
		{
			get { return fFirstRowFilter ?? (fFirstRowFilter = SetRowFilter(LedgerTypes.AccountsPayable)); }
		}

		ZQuery SetRowFilter(string ledger)
		{
			ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, Transaction.AH_TransactionNum);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, Transaction.AH_TransactionType);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ledger);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GB, Transaction.AH_GB);
			filter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Transaction.AH_GC);

			return filter;
		}

		AccTransactionHeader fFirstRow;
		AccTransactionHeader FirstRow
		{
			get
			{
				return fFirstRow ??
					   (fFirstRow = Factory.LoadTop1(typeof(AccTransactionHeader), FirstRowFilter) as AccTransactionHeader);
			}
		}

		ZQuery fSecondRowFilter;
		ZQuery SecondRowFilter
		{
			get { return fSecondRowFilter ?? (fSecondRowFilter = SetRowFilter(LedgerTypes.AccountsReceivable)); }
		}

		AccTransactionHeader fSecondRow;
		AccTransactionHeader SecondRow
		{
			get
			{
				return fSecondRow ??
					   (fSecondRow = Factory.LoadTop1(typeof(AccTransactionHeader), SecondRowFilter) as AccTransactionHeader);
			}
		}
	}
}
