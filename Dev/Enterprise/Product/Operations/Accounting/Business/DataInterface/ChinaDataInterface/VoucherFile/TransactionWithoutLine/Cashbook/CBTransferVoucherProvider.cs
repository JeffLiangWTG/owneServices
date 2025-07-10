using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class CBTransferVoucherProvider : TransactionWithoutLinesVoucherProvider
	{
		public CBTransferVoucherProvider(AccTransactionHeader transaction)
			: base(transaction, new ControlAccountProvider())
		{
			ZQuery transferRowFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, transaction.AH_Ledger);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Transfer);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.AH_TransactionNum);
			transferRowFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, transaction.PK);
			TransactionHeaderCollection fRelatedTransactions = new TransactionHeaderCollection(Factory, transferRowFilter);
			fRelatedTransactions.Load();
			if (fRelatedTransactions.Count > 0)
			{
				fRelatedTransaction = fRelatedTransactions[0];
			}
		}

		readonly AccTransactionHeader fRelatedTransaction;

		public override VoucherLine[] VoucherLines
		{
			get
			{
				if (fVoucherLines == null)
				{
					fVoucherLines = new VoucherLine[1];
					fVoucherLines[0] = new VoucherLine(Transaction);
					SetOriginalVoucherLine(fVoucherLines[0]);
					if (fRelatedTransaction != null)
					{
						Array.Resize(ref fVoucherLines, 2);
						fVoucherLines[1] = new VoucherLine(fRelatedTransaction);
						SetRelatedVoucherLine(fVoucherLines[1]);
					}
				}
				return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
			}
		}

		void SetRelatedVoucherLine(VoucherLine relatedVoucherLine)
		{
			ResetAllLookUp();
			AccTransactionHeader savedTransaction = Transaction;
			Transaction = fRelatedTransaction;
			SetOriginalVoucherLine(relatedVoucherLine);
			Transaction = savedTransaction;
		}

		protected override ZGuid GetGLAccountPKFromTransactionHeader()
		{
			ZGuid accountPK = ZGuid.Empty;
			if (Transaction.BankAccount != null)
			{
				accountPK = Transaction.BankAccount.GLHeader.PK;
			}
			return accountPK;
		}
	}
}
