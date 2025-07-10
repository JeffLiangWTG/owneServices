using System;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public partial class DDRFileGenerator
	{
		public DDRFileGenerator(TextWriter writer, DirectDebitBatchHeader header)
		{
			this.Writer = writer;
			this.Header = header;
		}

		public void Create()
		{
			int recordCount = 0;
			if (HasDescriptiveHeaderRecord)
			{
				WriteDescriptiveHeaderRecord(Header);
				recordCount++;
			}
			decimal amountTotal = 0M;

			bool isBankCurrencyLocal = IsBankCurrencyLocal;

			foreach (TransactionHeader detail in Header.Lines)
			{
				amountTotal += isBankCurrencyLocal ? detail.AH_LocalTotalAmount : detail.AH_OSTotalAmount;
				recordCount++;
				WriteDetailRecord(detail, recordCount.ToString());
			}
			int detailedRecordCount = HasDescriptiveHeaderRecord ? recordCount - 1 : recordCount;
			WriteFileTotalRecord(amountTotal, detailedRecordCount.ToString(), GetFileHashingTotal(Header));
		}

		protected TextWriter Writer;
		protected readonly DirectDebitBatchHeader Header;
		protected static string EntryDescriptionString
		{
			get { return Res.GetString("5ccaefcf-5a85-4e21-be75-3292f4ba20e3", "Payment"); }
		}
		protected string ZeroFilledReservedField = "0000000000";
		protected string NineFilledReservedField = "999-999";

		protected virtual bool HasDescriptiveHeaderRecord
		{
			get { return true; }
		}

		protected virtual string GetFileHashingTotal(DirectDebitBatchHeader header)
		{
			return "";
		}

		protected virtual void WriteFileTotalRecord(ZDecimal amountTotal, string detailRecordCount, string fileHashTotal)
		{
			string recordType = "7";

			StringBuilder totalRecord = new StringBuilder(120);

			totalRecord.Append(FitToWidth(recordType, 1));
			totalRecord.Append(FitToWidth(NineFilledReservedField, 7));
			totalRecord.Append(FitToWidth(RESERVED, 12));
			totalRecord.Append(GetFormattedAmountString(amountTotal.ToString()));
			totalRecord.Append(GetFormattedAmountString(amountTotal.ToString()));
			totalRecord.Append(ZeroFilledReservedField);
			totalRecord.Append(FitToWidth(RESERVED, 24));
			totalRecord.Append(detailRecordCount.PadLeft(6, '0'));
			totalRecord.Append(FitToWidth(RESERVED, 40));

			Writer.WriteLine(totalRecord.ToString());
		}

		protected virtual string GetInstitutionName(DirectDebitBatchHeader row)
		{
			return row.BankAccount.AB_AutoDDRFormat;
		}

		protected virtual void WriteDescriptiveHeaderRecord(DirectDebitBatchHeader row)
		{
			string recordType = "0";
			string sequenceNumber = "01";
			string institutionName = GetInstitutionName(row);
			string userSupplyingFile = row.BankAccount.AB_BankAccountName;
			string eFTUserID = row.BankAccount.AB_AccountEFTUserID;
			string descriptionOfEntries = GetDescriptionOfEntries();
			string dateToBeProcessed = Env.Time.CurrentLocalDate.ToString(DateFormat);

			StringBuilder headerRecord = new StringBuilder(120);

			headerRecord.Append(FitToWidth(recordType, 1));
			headerRecord.Append(FitToWidth(RESERVED, 17));
			headerRecord.Append(FitToWidth(sequenceNumber, 2));
			headerRecord.Append(FitToWidth(institutionName, 3));
			headerRecord.Append(FitToWidth(RESERVED, 7));
			headerRecord.Append(FitToWidth(userSupplyingFile, 26));
			headerRecord.Append(eFTUserID.PadLeft(6, '0'));
			headerRecord.Append(FitToWidth(descriptionOfEntries, 12));
			headerRecord.Append(FitToWidth(dateToBeProcessed, 6));
			headerRecord.Append(FitToWidth(RESERVED, 40));

			Writer.WriteLine(headerRecord.ToString());
		}

		protected virtual void WriteDetailRecord(TransactionHeader row, string recordCount)
		{
			string recordType = "1";
			string payeeBSBNumber = GetPayeeBankBSB(row);
			string payeeBankAccountNum = GetFormattedBankAccountNumber(GetPayeeBankAccountNumber(row));
			string indicator = " ";
			string transactionCode = "50";
			string amountToCredit = GetFormattedAmountString(GetAmountString(row));
			string accountTitle = GetAccountTitle(row);
			string lodgementReference = row.AH_ChequeOrReference;
			string traceRecordBSB = row.BankAccount.AB_BSB;
			string traceRecordAccountNumber = GetFormattedBankAccountNumber(row.BankAccount.AB_AccountNum);
			string remitterName = row.BankAccount.AB_BankAccountName;

			StringBuilder detailRecord = new StringBuilder(120);

			detailRecord.Append(FitToWidth(recordType, 1));
			detailRecord.Append(FitToWidth(payeeBSBNumber, 7));
			detailRecord.Append(FitToWidth(payeeBankAccountNum, 9));
			detailRecord.Append(FitToWidth(indicator, 1));
			detailRecord.Append(FitToWidth(transactionCode, 2));
			detailRecord.Append(amountToCredit);
			detailRecord.Append(FitToWidth(accountTitle, 32));
			detailRecord.Append(FitToWidth(lodgementReference, 18));
			detailRecord.Append(FitToWidth(traceRecordBSB, 7));
			detailRecord.Append(traceRecordAccountNumber);
			detailRecord.Append(FitToWidth(remitterName, 16));
			detailRecord.Append(FitToWidth(GetDetailRecordWHTField(row), 8));

			Writer.WriteLine(detailRecord.ToString());
		}

		protected virtual string GetAmountString(TransactionHeader row)
		{
			return (IsBankCurrencyLocal ? row.AH_LocalTotalAmount : row.AH_OSTotalAmount).ToString();
		}

		protected virtual string GetDetailRecordWHTField(TransactionHeader row)
		{
			return ZeroFilledReservedField;
		}

		protected ZString GetAccountTitle(TransactionHeader row)
		{
			AccAPAccountDetails accountDetails = GetAccountDetails(row);
			return ((IsPayment(row) && accountDetails != null) ? accountDetails.A1_AccountName : row.AH_ChequeDrawer);
		}

		protected ZString GetPayeeBankAccountNumber(TransactionHeader row)
		{
			AccAPAccountDetails accountDetails = GetAccountDetails(row);
			return ((IsPayment(row) && accountDetails != null) ? accountDetails.A1_BankAccount : row.AH_DrawerBank);
		}

		protected ZString GetPayeeBankBSB(TransactionHeader row)
		{
			AccAPAccountDetails accountDetails = GetAccountDetails(row);
			return ((IsPayment(row) && accountDetails != null) ? accountDetails.A1_BankBsb : row.AH_DrawerBranch);
		}

		protected bool IsPayment(TransactionHeader row)
		{
			return row.AH_TransactionType == TransactionTypes.Payment;
		}

		protected const string RESERVED = ""; //used for blank reserved fields
		protected const string EntryDescription = "PAYMENTS";
		protected const string DateFormat = "ddMMyy";

		protected virtual string GetDescriptionOfEntries()
		{
			return EntryDescription;
		}

		protected string FitToWidth(string value, int width)
		{
			return FitToWidthCore(value, width, ' ');
		}

		protected string FitToWidthCore(string value, int width, char paddingChar)
		{
			if (value.Length < width)
			{
				value = value.PadRight(width, paddingChar);
			}
			else if (value.Length > width)
			{
				value = value.Substring(0, width);
			}
			return value;
		}

		protected string FitToWidthPadLeft(string value, int width)
		{
			return FitToWidthPadLeftCore(value, width, ' ');
		}

		protected string FitToWidthPadLeftCore(string value, int width, char paddingChar)
		{
			if (value.Length < width)
			{
				value = value.PadLeft(width, paddingChar);
			}
			else if (value.Length > width)
			{
				value = value.Substring(value.Length - width, width);
			}
			return value;
		}

		protected string GetFormattedBankAccountNumber(string bankAccountNumber)
		{
			if (bankAccountNumber.Length > 9)
			{
				bankAccountNumber = bankAccountNumber.Replace("-", "");
			}
			else if (bankAccountNumber.Length < 9)
			{
				bankAccountNumber = bankAccountNumber.PadLeft(9);
			}
			return bankAccountNumber;
		}

		protected virtual string GetFormattedAmountString(string amountString)
		{
			return GetFormattedAmountStringCore(amountString, 8, '0');
		}

		protected string GetFormattedAmountStringCore(string amountString, int dollarLength, char paddingChar)
		{
			string[] dollarsAndCents;
			dollarsAndCents = amountString.Trim().Split('.');
			string dollars = dollarsAndCents[0];
			if (dollars.Length < dollarLength)
			{
				dollars = dollars.PadLeft(dollarLength, paddingChar);
			}
			else if (dollars.Length > dollarLength)
			{
				throw new ArgumentException("The Bank DDR system does not allow amounts greater than $99999999.99");
			}
			string cents = "";
			if (dollarsAndCents.Length < 2)
			{
				cents = "00";
			}
			else if (dollarsAndCents.Length == 2)
			{
				if (dollarsAndCents[1].Length < 2)
				{
					cents = dollarsAndCents[1].PadRight(2, '0');
				}
				else if (dollarsAndCents[1].Length > 2)
				{
					cents = dollarsAndCents[1].Substring(0, 2);
				}
				else
				{
					cents = dollarsAndCents[1];
				}
			}
			else if (dollarsAndCents.Length > 2)
			{
				throw new ArgumentException("There were two decimals in the Amount to be selected");
			}

			return dollars + cents;
		}

		AccAPAccountDetails GetAccountDetails(TransactionHeader row)
		{
			AccAPAccountDetails fAccountDetails = null;
			if (row.Header != null)
			{
				ZString currencyCode = (!row.AH_RX_NKTransactionCurrency.IsEmpty) ? row.AH_RX_NKTransactionCurrency : row.AH_Calc_LocalRXCode;
				ZString paymentType = row.AH_ReceiptType == ZArchitecture.Core.ReceiptTypes.DirectDebitLine ? (ZString)ZArchitecture.Core.ReceiptTypes.DirectDebit : row.AH_ReceiptType;
				fAccountDetails = row.Header.CompanyData.AccountDetailsCollection.GetAccountDetails(paymentType, currencyCode, true);
			}
			return fAccountDetails;
		}

		protected bool IsBankCurrencyLocal
		{
			get
			{
				return Header.BankAccount != null && Header.BankAccount.AB_RX_NKAccountCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
		}
	}
}
