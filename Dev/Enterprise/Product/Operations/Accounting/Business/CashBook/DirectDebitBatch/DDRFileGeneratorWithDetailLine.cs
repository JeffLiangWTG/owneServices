using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public class DDRFileGeneratorWithDetailLine : DDRFileGenerator
	{
		public DDRFileGeneratorWithDetailLine(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		protected override void WriteFileTotalRecord(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
			if (Header != null)
			{
				string recordType = "1";
				string payeeBSBNumber = Header.BankAccount.AB_BSB;
				string payeeBankAccountNum = GetFormattedBankAccountNumber(Header.BankAccount.AB_AccountNum);
				string indicator = " ";
				string transactionCode = "13";
				string amountToDebit = amountTotal.ToString();
				string accountTitle = Header.BankAccount.AB_BankAccountName;
				string lodgementReference = Header.AH_ChequeOrReference;
				string traceRecordBSB = Header.BankAccount.AB_BSB;
				string traceRecordAccountNumber = GetFormattedBankAccountNumber(Header.BankAccount.AB_AccountNum);
				string remitterName = Header.BankAccount.AB_BankAccountName;

				StringBuilder detailRecord = new StringBuilder(120);

				detailRecord.Append(FitToWidth(recordType, 1));
				detailRecord.Append(FitToWidth(payeeBSBNumber, 7));
				detailRecord.Append(FitToWidth(payeeBankAccountNum, 9));
				detailRecord.Append(FitToWidth(indicator, 1));
				detailRecord.Append(FitToWidth(transactionCode, 2));
				detailRecord.Append(GetFormattedAmountString(amountToDebit));
				detailRecord.Append(FitToWidth(accountTitle, 32));
				detailRecord.Append(FitToWidth(lodgementReference, 18));
				detailRecord.Append(FitToWidth(traceRecordBSB, 7));
				detailRecord.Append(traceRecordAccountNumber);
				detailRecord.Append(FitToWidth(remitterName, 16));
				detailRecord.Append(FitToWidth(ZeroFilledReservedField, 8));

				Writer.WriteLine(detailRecord.ToString());
			}

			string nineFilledReservedField = "999-999";

			StringBuilder totalRecord = new StringBuilder(120);

			int recordCountInteger = Utilities.ConvertToInt32(detailRecordCount) + 1;
			detailRecordCount = recordCountInteger.ToString();

			totalRecord.Append(FitToWidth("7", 1));
			totalRecord.Append(FitToWidth(nineFilledReservedField, 7));
			totalRecord.Append(FitToWidth(RESERVED, 12));
			totalRecord.Append(ZeroFilledReservedField);
			totalRecord.Append(GetFormattedAmountString(amountTotal.ToString()));
			totalRecord.Append(GetFormattedAmountString(amountTotal.ToString()));
			totalRecord.Append(FitToWidth(RESERVED, 24));
			totalRecord.Append(detailRecordCount.PadLeft(6, '0'));
			totalRecord.Append(FitToWidth(RESERVED, 40));

			Writer.WriteLine(totalRecord.ToString());
		}
	}
}
