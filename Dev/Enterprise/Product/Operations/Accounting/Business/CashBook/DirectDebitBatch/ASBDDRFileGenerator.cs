using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class ASBDDRFileGenerator : DDRFileGenerator
	{
		public ASBDDRFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
			: base(writer, header)
		{
		}

		public const string DirectCreditFileType = "12";
		public const string BeginNewRecordFlag = "13";
		public const string CreditTransactionCode = "051";
		public const string TrailerRecordFlag = "99";
		public const string DefaultHashTotal = "00000000000";	// 11 zeros
		public const string Padding = " ";

		public const string DetailInternalReference = "REMITTANCE";
		public const string DetailPayeeParticulars = "REFER";

		#region WriteDescriptiveHeaderRecord

		protected override void WriteDescriptiveHeaderRecord(DirectDebitBatchHeader row)
		{
			string payerBankNumber = row.BankAccount.AB_BSB.Substring(0, 2);
			string payerBranchNumber = row.BankAccount.AB_BSB.Substring(2, 4);
			string payerUniqueNumber = row.BankAccount.AB_AccountNum.Substring(0, 7);
			string payerSuffix = row.BankAccount.AB_AccountNum.Substring(7, 2);

			StringBuilder builder = new StringBuilder();
			builder.Append(DirectCreditFileType);
			builder.Append(payerBankNumber);
			builder.Append(payerBranchNumber);
			builder.Append(payerUniqueNumber);
			builder.Append(FitToWidth(payerSuffix, 3));
			builder.Append(FitToWidth(ZDateTime.Today.ToString("ddMMyyyy"), 13));
			builder.Append(FitToWidth(row.BankAccount.AB_BankAccountName, 20));
			builder.Append(RESERVED.PadRight(109));

			Writer.WriteLine(builder.ToString());
		}

		#endregion

		#region WriteDetailRecord

		protected override void WriteDetailRecord(TransactionHeader row, string recordCount)
		{
			string payeeBankNumber = GetPayeeBankBSB(row).SubstringSafe(0, 2);
			string payeeBranchNumber = GetPayeeBankBSB(row).SubstringSafe(2, 4);
			string payeeUniqueNumber = GetPayeeBankAccountNumber(row).SubstringSafe(0, 7);

			string payeeSuffix = string.Empty;
			int payeeAccountNumLength = GetPayeeBankAccountNumber(row).Length;
			if (payeeAccountNumLength == 9 || payeeAccountNumLength == 10)
			{
				payeeSuffix = GetPayeeBankAccountNumber(row).Substring(7, payeeAccountNumLength - 7);
			}
			else
			{
				ErrorReporter.ReportOnce(GetType().ToString(), "Accounting: ASB DDR File Creation: Account number was " + payeeAccountNumLength + " digits long; must be 9 or 10");
			}

			ZDecimal totalAmount = IsBankCurrencyLocal ? row.AH_LocalTotalAmount : row.AH_OSTotalAmount;
			string payeeName = GetAccountTitle(row);
			string payeeCode = GetPayeeCode(row);
			string payeeReference = GetPayeeReference(row);

			StringBuilder builder = new StringBuilder();
			builder.Append(BeginNewRecordFlag);
			builder.Append(payeeBankNumber);
			builder.Append(payeeBranchNumber);
			builder.Append(payeeUniqueNumber);
			builder.Append(FitToWidth(payeeSuffix, 3));
			builder.Append(CreditTransactionCode);
			builder.Append(GetFormattedAmountString(totalAmount.ToString()));
			builder.Append(FitToWidth(payeeName, 20));
			builder.Append(FitToWidth(DetailInternalReference, 12));
			builder.Append(FitToWidth(payeeCode, 12));
			builder.Append(FitToWidth(payeeReference, 12));
			builder.Append(FitToWidth(DetailPayeeParticulars, 12));
			builder.Append(FitToWidth(RESERVED, 1));
			builder.Append(FitToWidth(row.BankAccount.AB_BankAccountName, 20));
			builder.Append(FitToWidth(GlbCompany.CurrentCompany.GC_Code, 12));
			builder.Append(FitToWidth(RESERVED, 12));
			builder.Append(FitToWidth("DDR " + payeeReference, 12));
			builder.Append(FitToWidth(RESERVED, 4));

			Writer.WriteLine(builder.ToString());
		}

		ZString GetPayeeReference(TransactionHeader row)
		{
			return row.AH_ChequeOrReference;
		}

		ZString GetPayeeCode(TransactionHeader row)
		{
			return (IsPayment(row) ? row.Header.OH_FullNameTruncated : row.AH_ChequeDrawer);
		}

		#endregion

		#region WriteTrailingRecord

		protected override void WriteFileTotalRecord(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(FitToWidth(BeginNewRecordFlag, 2));
			builder.Append(FitToWidth(TrailerRecordFlag, 2));
			builder.Append(FitToWidthPadLeft(fileHashTotal, 11));
			builder.Append(FitToWidth(RESERVED, 6));
			builder.Append(GetFormattedAmountString(amountTotal.ToString()));
			builder.Append(FitToWidth(RESERVED, 129));

			Writer.WriteLine(builder.ToString());
		}

		#endregion

		#region GetFileHashingTotal

		protected override string GetFileHashingTotal(DirectDebitBatchHeader row)
		{
			return DefaultHashTotal;
		}

		#endregion
	}
}