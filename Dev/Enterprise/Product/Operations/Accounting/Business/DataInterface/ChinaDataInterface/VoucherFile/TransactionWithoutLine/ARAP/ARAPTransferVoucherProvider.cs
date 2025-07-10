using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class ARAPTransferVoucherProvider : TransactionWithoutLinesVoucherProvider
	{
		public ARAPTransferVoucherProvider(AccTransactionHeader transaction)
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

					fVoucherLines[0] = new VoucherLine(Transaction);
					SetOriginalVoucherLine(fVoucherLines[0]);
					fVoucherLines[0].OrganisationCode = Transaction.Header == null ? ZString.Empty : Transaction.Header.OH_Code;
					fVoucherLines[0].OutstandingAmount = Transaction.AH_OutstandingAmount;
					if (IncludeOrganisationCode)
					{
						fVoucherLines[0].AdditionalAccountDescription += new VoucherDebitCreditLookUp(Transaction).GetOrganisationCode();
					}

					if (SecondRow != null)
					{
						fVoucherLines[1] = new VoucherLine(SecondRow);
						SetSecondVoucherLine(fVoucherLines[1]);
					}
				}
				return fVoucherLines[1] == null ? fVoucherLines : (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
			}
		}

		protected override int MaxVoucherLineNo
		{
			get { return 2; }
		}

		protected void SetSecondVoucherLine(VoucherLine voucherLine)
		{
			voucherLine.AccountPK = GetGLAccountPKFromTransactionHeader();

			if (IncludeOrganisationCode)
			{
				voucherLine.AdditionalAccountDescription += new VoucherDebitCreditLookUp(SecondRow).GetOrganisationCode();
			}
			voucherLine.OutstandingAmount = SecondRow.AH_OutstandingAmount;
			voucherLine.OrganisationCode = SecondRow.Header == null ? ZString.Empty : SecondRow.Header.OH_Code;
			voucherLine.VoucherType = VoucherTypeLookUp.GetVoucherType();
			voucherLine.VoucherNumber = VoucherNumberLookUp.GetVoucherNumber();
			voucherLine.DebitAmount = new VoucherDebitCreditLookUp(SecondRow).GetDebit();
			voucherLine.CreditAmount = new VoucherDebitCreditLookUp(SecondRow).GetCredit();
			voucherLine.OSDebitAmount = new VoucherDebitCreditLookUp(SecondRow).GetOSDebit();
			voucherLine.OSCreditAmount = new VoucherDebitCreditLookUp(SecondRow).GetOSCredit();
			voucherLine.VoucherDate = GetVoucherDate();
			voucherLine.Description = GetVoucherDescription();
			voucherLine.CurrencyCode = GetCurrencyCode();
			voucherLine.ForeignCurrencyAmount = GetForeignCurrencyAmount();
			voucherLine.ExchangeRate = GetExchangeRate();
		}

		ZQuery fSecondRowFilter;
		ZQuery SecondRowFilter
		{
			get
			{
				if (fSecondRowFilter == null)
				{
					fSecondRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, Transaction.AH_TransactionNum);
					fSecondRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, Transaction.AH_Ledger);
					fSecondRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, Transaction.AH_TransactionType);
					fSecondRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCount, (byte)2);
					fSecondRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GB, Transaction.AH_GB);
					fSecondRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, Transaction.AH_GC);
				}
				return fSecondRowFilter;
			}
		}

		AccTransactionHeader fSecondRow;
		AccTransactionHeader SecondRow
		{
			get
			{
				if (fSecondRow == null)
				{
					fSecondRow = Factory.LoadTop1(typeof(AccTransactionHeader), SecondRowFilter) as AccTransactionHeader;
				}
				return fSecondRow;
			}
		}
	}
}
