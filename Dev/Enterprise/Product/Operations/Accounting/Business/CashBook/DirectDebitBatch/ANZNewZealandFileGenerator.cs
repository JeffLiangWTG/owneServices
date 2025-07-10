using System;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class ANZNewZealandFileGenerator : DDRFileGenerator
	{
		public ANZNewZealandFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected const string Separator = ",";

		protected override void WriteDescriptiveHeaderRecord(DirectDebitBatchHeader row)
		{
			string recordType = "1";
			string batchDueDate = Env.Time.CurrentLocalDate.ToString("yyyyMMdd");
			string todaysDate = Env.Time.CurrentLocalDate.ToString("yyyyMMdd");

			StringBuilder headerRecord = new StringBuilder();
			headerRecord.Append(recordType + Separator);
			headerRecord.Append(Separator);
			headerRecord.Append(Separator);
			headerRecord.Append(Separator);
			headerRecord.Append(Separator);
			headerRecord.Append(Separator);
			headerRecord.Append(batchDueDate.Trim() + Separator);
			headerRecord.Append(todaysDate.Trim() + Separator);

			Writer.WriteLine(headerRecord.ToString());
		}

		protected override void WriteDetailRecord(TransactionHeader row, string recordCount)
		{
			ZString recordType = "2";
			ZString accountNumber = GetPayeeBankBSB(row) + GetPayeeBankAccountNumber(row);
			ZString transactionCode = "50";
			ZDecimal totalAmount = IsBankCurrencyLocal ? row.AH_LocalTotalAmount : row.AH_OSTotalAmount;
			ZString amount = GetFormattedAmountString(totalAmount.ToString(2));
			ZString otherPartyName = GetAccountTitle(row);
			if (otherPartyName.Length > 20)
			{
				otherPartyName = otherPartyName.SubstringSafe(0, 20);
			}

			ZString subscriberName = row.BankAccount.AB_BankAccountName;
			if (subscriberName.Length > 20)
			{
				subscriberName = subscriberName.SubstringSafe(0, 20);
			}

			StringBuilder detailRecord = new StringBuilder();
			detailRecord.Append(recordType.Trim() + Separator);
			detailRecord.Append(accountNumber.Trim() + Separator);
			detailRecord.Append(transactionCode.Trim() + Separator);
			detailRecord.Append(amount.Trim() + Separator);
			detailRecord.Append(otherPartyName.Trim() + Separator);
			detailRecord.Append(Separator);
			detailRecord.Append(Separator);
			detailRecord.Append(Separator);
			detailRecord.Append(Separator);
			detailRecord.Append(subscriberName.Trim() + Separator);
			detailRecord.Append(Separator);
			detailRecord.Append(Separator);

			Writer.WriteLine(detailRecord.ToString());
		}

		protected override void WriteFileTotalRecord(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
			string recordType = "3";

			StringBuilder controlRecord = new StringBuilder();
			controlRecord.Append(recordType + Separator);
			controlRecord.Append(GetFormattedAmountString(amountTotal.ToString(2)).Trim() + Separator);
			controlRecord.Append(detailRecordCount.Trim() + Separator);
			controlRecord.Append(fileHashTotal.Trim());

			Writer.WriteLine(controlRecord.ToString());
		}

		protected override string GetFileHashingTotal(DirectDebitBatchHeader header)
		{
			ulong hashTotal = 0;
			string result = "";

			foreach (TransactionHeader row in header.Lines)
			{
				string accountNumber = GetPayeeBankBSB(row) + GetPayeeBankAccountNumber(row);
				if (accountNumber.Length == 15 || accountNumber.Length == 16)
				{
					string currentAmountString = accountNumber.Substring(2, 11);
					ulong currentAmount = Convert.ToUInt64(currentAmountString);
					hashTotal += currentAmount;
				}
			}

			result = hashTotal.ToString();
			int resultLength = result.Length;
			if (resultLength > 11)
			{
				result = result.Substring(resultLength - 11);
			}
			else if (resultLength < 11)
			{
				result = result.PadLeft(11, '0');
			}

			return result;
		}

		protected override string GetFormattedAmountString(string amountString)
		{
			string result = "";
			if (amountString.IndexOf(".") == -1)
			{
				result = amountString + "00";
			}
			else
			{
				result = amountString.Replace(".", "");
			}
			return result;
		}
	}
}