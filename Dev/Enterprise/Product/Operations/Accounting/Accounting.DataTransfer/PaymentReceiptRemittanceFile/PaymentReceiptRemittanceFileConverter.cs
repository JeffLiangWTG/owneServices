using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public class PaymentReceiptRemittanceFileConverter : FlatFileConverter
	{
		public PaymentReceiptRemittanceFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			var notifier = new NotificationManager(Notification);

			TxnHeaderCollection txnHeaderCollection = (TxnHeaderCollection)valueObject;
			TxnHeader txnHeader = null;
			string message;
			var matchDateStack = new Stack<ZDateTime>();

			foreach (FlatFileDataRow row in fileLines)
			{
				string rowType = row[(int)PAYRECLine.Type];

				switch (rowType)
				{
					case AccountingConstants.RemittanceFileRowTypes.Receipt:
					case AccountingConstants.RemittanceFileRowTypes.Payment:
						{
							txnHeader = txnHeaderCollection.AddNew();
							ProcessReceiptPaymentRow(txnHeader, row, notifier);
						}
						break;

					case AccountingConstants.RemittanceFileRowTypes.MatchHeader:
						txnHeader = null;
						var matchDate = TxnHeaderBuilder.GetZDateTimeFromField(row.GetFieldAsZDateTime((int)MHRLine.MatchDate, "yyyyMMdd"), notifier);
						matchDateStack.Push(matchDate);
						break;

					case AccountingConstants.RemittanceFileRowTypes.NettingClearingLine:
						{
							txnHeader = txnHeaderCollection.AddNew();
							ProcessJournalRow(txnHeader, row, notifier);
						}
						break;

					case AccountingConstants.RemittanceFileRowTypes.PaidTransaction:
						{
							var transactionType = row[(int)PAYRECLine.TransactionType];
							if (TxnHeaderBuilder.IsSupportedPaidTransactionType(transactionType, notifier))
							{
								if (matchDateStack.Any() && txnHeader == null)
								{
									txnHeader = txnHeaderCollection.AddNew();
									txnHeader.FullyPaidDate = matchDateStack.Pop();
									ProcessPaidTransactionRow(txnHeader, row, notifier);
								}
								else if (txnHeader != null)
								{
									var paidTxnHeader = txnHeader.PaidTransactions.AddNew();
									ProcessPaidTransactionRow(paidTxnHeader, row, notifier);
								}
								else
								{
									message = Res.GetString("1e224682-0146-4aac-a985-da4dbb4d2ad7", "'{0}' line type can't be the first line in a file.", AccountingConstants.RemittanceFileRowTypes.PaidTransaction);
									notifier.AddWarningToNotifications(message);
								}
							}
						}
						break;

					default:
						{
							message = Res.GetString("593d410e-e071-426b-b1c1-09a65fcf03e9", "Unrecognized row type was detected. Only '{0}', '{1}', '{2}' and '{3}' row types are used for import. The following row will be ignored: '{4}'."
								, AccountingConstants.RemittanceFileRowTypes.MatchHeader, AccountingConstants.RemittanceFileRowTypes.Receipt, AccountingConstants.RemittanceFileRowTypes.Payment, AccountingConstants.RemittanceFileRowTypes.PaidTransaction, rowType) + "\r\n";
							notifier.AddWarningToNotifications(message);
						}
						break;
				}
			}

			if (matchDateStack.Any())
			{
				notifier.AddWarningToNotifications(RemittanceFileImportHelper.AtLeastTwoPTRLinesForMHRError);
			}
		}

		public IValueObject ConvertCSVContentToXsd(string csvContent)
		{
			string[] lines = csvContent.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			foreach (var line in lines)
			{
				FlatFileDataRow flatFileLine = new CsvFlatFileFormat().ConvertToRow(line);
				if (flatFileLine != null)
				{
					fileLines.Add(flatFileLine);
				}
			}

			FinancialInvoices xsd = new FinancialInvoices();
			MapImport(xsd.TxnHeader, fileLines);

			return xsd;
		}

		#region Implementation

		void ProcessPaidTransactionRow(TxnHeader txnHeader, FlatFileDataRow row, NotificationManager notifier)
		{
			var matchLine = new PaymentReceiptRemittanceMatchLine(row);
			TxnHeaderBuilder.BuidPaidTransaction(txnHeader, matchLine, Factory, notifier);
		}

		void ProcessJournalRow(TxnHeader txnHeader, FlatFileDataRow row, NotificationManager notifier)
		{
			string rowType = row[(int)NCLLine.Type];

			TxnHeaderBuilder.SetLedger(txnHeader, row[(int)NCLLine.Ledger], notifier);
			TxnHeaderBuilder.SetTransactionType(txnHeader, row[(int)NCLLine.TransactionType], notifier);

			TxnHeaderBuilder.SetPostDate(txnHeader, TxnHeaderBuilder.GetZDateTimeFromField(row.GetFieldAsZDateTime((int)NCLLine.PostDate, "yyyyMMdd"), notifier));
			TxnHeaderBuilder.SetDueDate(txnHeader, TxnHeaderBuilder.GetZDateTimeFromField(row.GetFieldAsZDateTime((int)NCLLine.DueDate, "yyyyMMdd"), notifier));

			var orgCode = row[(int)NCLLine.AccountCode];
			if (!orgCode.IsEmpty)
			{
				TxnHeaderBuilder.SetDebtorOrCreditor(txnHeader, orgCode, null);
			}
			TxnHeaderBuilder.SetDescription(txnHeader, row[(int)NCLLine.Description]);

			ZString curencyCode = row[(int)NCLLine.Currency];
			TxnHeaderBuilder.SetOsInvoiceAmtInclTax(txnHeader, row.GetFieldAsZDecimal((int)NCLLine.OSAmount), curencyCode, Factory, notifier);
			TxnHeaderBuilder.SetOsInvoiceAmtExclTax(txnHeader, row.GetFieldAsZDecimal((int)NCLLine.OSAmount), curencyCode, Factory, notifier);
			if (!row[(int)NCLLine.LocalCurrency].IsEmpty)
			{
				ZString localCurrency = row[(int)NCLLine.LocalCurrency];
				if (!row[(int)NCLLine.LocalAmount].IsEmpty)
				{
					ZDecimal localAmount = row.GetFieldAsZDecimal((int)NCLLine.LocalAmount);
					TxnHeaderBuilder.SetLocalInvoiceAmtInclTax(txnHeader, localAmount, localCurrency, Factory, notifier);
					TxnHeaderBuilder.SetLocalInvoiceAmtExclTax(txnHeader, localAmount, localCurrency, Factory, notifier);
				}
			}

			TxnHeaderBuilder.SetBranchCode(txnHeader, row[(int)NCLLine.Branch]);
			TxnHeaderBuilder.SetDepartmentCode(txnHeader, row[(int)NCLLine.Department]);
		}

		void ProcessReceiptPaymentRow(TxnHeader txnHeader, FlatFileDataRow row, NotificationManager notifier)
		{
			TxnHeaderBuilder.BuildReceiptPaymentHeader(txnHeader, new PaymentReceiptRemittanceHeader(row), Factory, notifier);
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Enumeration used inside PaymentReceiptRemittanceFileConverter")]
		enum PAYRECLine
		{
			Type,
			Ledger,
			TransactionType,
			ReceiptPaymentDate,
			PostDate,
			AccountCode,
			Description,
			ReceiptPaymentType,
			BankAccountCode,
			ChequeBook_PaymentOnly,
			ChequeOrReference,
			Currency,
			OSAmount,
			LocalAmount,
			Branch,
			Department,
			ChequeDrawer_ReceiptOnly,
			ChequeDrawerBank_ReceiptOnly,
			ChequeDrawerBankBranch_ReceiptOnly,
			OverrideAddress_PaymentOnly,
			OverrideContact_PaymentOnly
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Enumeration used inside PaymentReceiptRemittanceFileConverter")]
		enum NCLLine
		{
			Type,
			Ledger,
			TransactionType,
			PostDate,
			DueDate,
			AccountCode,
			Description,
			Currency,
			OSAmount,
			LocalCurrency,
			LocalAmount,
			Branch,
			Department
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Enumeration used inside PaymentReceiptRemittanceFileConverter")]
		enum MHRLine
		{
			Type,
			MatchDate
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Enumeration used inside PaymentReceiptRemittanceFileConverter")]
		enum PTRLine
		{
			Type,
			Ledger,
			TransactionType,
			TransactionNumber,
			AmountPaid,
			Organisation,
			PaymentReference,
			TransactionDescription,
			TransactionDate,
			PostDate,
			DueDate,
			Currency,
			AmountPaidInLocalCurrency,
			LocalCurrency,
			MatchStatus,
			MatchStatusReasonCode
		}

		#endregion
	}

	public static class RemittanceFileImportHelper
	{
		public static string AtLeastTwoPTRLinesForMHRError => Res.GetString("c19f6560-3938-4d0c-a130-bf4c6cf11754", "The MHR line type must be accompanied by at least two PTR lines.");
	}
}
