#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class DDRFileGenerator
	{
		public void WriteDescriptiveHeaderRecord_ForTestOnly(DirectDebitBatchHeader row)
		{
			WriteDescriptiveHeaderRecord(row);
		}

		public void WriteDetailRecord_ForTestOnly(Base.Transaction.TransactionHeader row, string recordCount)
		{
			WriteDetailRecord(row, recordCount);
		}

		public void WriteFileTotalRecord_ForTestOnly(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
			WriteFileTotalRecord(amountTotal, detailRecordCount, fileHashTotal);
		}

		public string GetFormattedBankAccountNumber_ForTestOnly(string bankAccountNumber)
		{
			return GetFormattedBankAccountNumber(bankAccountNumber);
		}

		public string GetFormattedAmountString_ForTestOnly(string amountString)
		{
			return GetFormattedAmountString(amountString);
		}

		public string FitToWidthCore_ForTestOnly(string value, int width, char paddingChar)
		{
			return FitToWidthCore(value, width, paddingChar);
		}

		public string GetFormattedAmountStringCore_ForTestOnly(string amountString, int dollarLength, char paddingChar)
		{
			return GetFormattedAmountStringCore(amountString, dollarLength, paddingChar);
		}

		public string FitToWidthPadLeft_ForTestOnly(string value, int width)
		{
			return FitToWidthPadLeft(value, width);
		}

		public string NineFilledReservedField_ForTestOnly
		{
			get { return NineFilledReservedField; }
			set { NineFilledReservedField = value; }
		}

		public string ZeroFilledReservedField_ForTestOnly
		{
			get { return ZeroFilledReservedField; }
			set { ZeroFilledReservedField = value; }
		}

		public static string RESERVED_ForTestOnly => RESERVED;

		public static string EntryDescription_ForTestONly => EntryDescription;

		public static string DateFormat_ForTestOnly => DateFormat;
	}
}

#endif
