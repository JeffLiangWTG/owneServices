using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class BCSDDRFileGenerator : DDRFileGenerator
	{
		public BCSDDRFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected const string Separator = ",";

		protected override void WriteDescriptiveHeaderRecord(DirectDebitBatchHeader row)
		{
		}

		protected override void WriteFileTotalRecord(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
		}

		protected override void WriteDetailRecord(TransactionHeader row, string recordCount)
		{
			string transactionCode = "01";
			string originatingSortCode = Header.BankAccount.AB_BSB;
			string originatingAccountNumber = Header.BankAccount.AB_AccountNum;
			string destinationSortCode = GetPayeeBankBSB(row);
			string destinationAccountNumber = GetPayeeBankAccountNumber(row);
			string destinationAccountName = GetAccountTitle(row);
			if (destinationAccountName.Length > 18)
			{
				destinationAccountName = destinationAccountName.Substring(0, 18);
			}

			decimal totalAmount = IsBankCurrencyLocal ? row.AH_LocalTotalAmount : row.AH_OSTotalAmount;
			string amount = totalAmount.ToString("0.00", CultureInfo.InvariantCulture);
			string userName = Header.CreatingUser;
			if (userName.Length > 18)
			{
				userName = userName.Substring(0, 18);
			}

			string userReference = row.AH_ChequeOrReference;
			if (userReference.Length > 18)
			{
				userReference = userReference.Substring(0, 18);
			}

			StringBuilder detailRecord = new StringBuilder();
			detailRecord.Append(transactionCode.Trim() + Separator);
			detailRecord.Append(originatingSortCode.Trim() + Separator);
			detailRecord.Append(originatingAccountNumber.Trim() + Separator);
			detailRecord.Append(destinationSortCode.Trim() + Separator);
			detailRecord.Append(destinationAccountNumber.Trim() + Separator);
			detailRecord.Append(destinationAccountName.Trim() + Separator);
			detailRecord.Append(amount.Trim() + Separator);
			detailRecord.Append(userName.Trim() + Separator);
			detailRecord.Append(userReference.Trim());

			Writer.WriteLine(detailRecord.ToString());
		}
	}
}
