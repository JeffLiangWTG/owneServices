using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.MFI.Data
{
	public class EStatementConverter : FlatFileConverter
	{
		public EStatementConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			Xsd.TxnHeaderCollection collection = (Xsd.TxnHeaderCollection)valueObject;
			Xsd.TxnHeader txnHeader = collection.AddNew();

			if (IsDataValid(fileLines))
			{
				int i = 0;
				foreach (FlatFileDataRow row in fileLines)
				{
					if (i == 0)
					{
						DefaultCharge = row[InvoiceHeaderConstants.DefaultChargeCode].ToUpper();
						ProcessInvoiceHeader(row, txnHeader);
					}
					else if (i > 1)
					{
						if (!row[EStatementConstants.InvoiceDate].IsEmpty)
						{
							ProcessInvoiceLines(row, txnHeader);
						}
					}
					i++;
				}
			}
			else
			{
				Notification.Notify(new ErrorNotification(ErrorType.InvalidFileFormat, "Record line length too short"));
			}
		}

		internal protected void ProcessInvoiceHeader(FlatFileDataRow row, Xsd.TxnHeader txnHeader)
		{
			txnHeader.DebtorOrCreditor = new Xsd.Organisation();
			txnHeader.DebtorOrCreditor.EDICode = row.GetField(InvoiceHeaderConstants.DebtorCode);
			txnHeader.TxnReference = row.GetField(InvoiceHeaderConstants.InvoiceNumber);
			txnHeader.Ledger = TxnHeaderMapper.GetTxnHeaderLedger(LedgerTypes.AccountsPayable);
			txnHeader.TxnNumber = row.GetField(InvoiceHeaderConstants.InvoiceNumber);
			txnHeader.TxnType = TxnHeaderMapper.GetTxnHeaderTxnType(TransactionTypes.Invoice);
			txnHeader.OsInvoiceAmtExclTax.CurrencyCode = GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
		}

		internal protected void ProcessInvoiceLines(FlatFileDataRow invoiceLineRow, Xsd.TxnHeader txnHeader)
		{
			Xsd.TxnLine txnLine = txnHeader.TxnLines.AddNew();

			SetDescription(invoiceLineRow, txnLine);

			Type invoiceType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader);

			ZDecimal amount = ZDecimal.ParseSafe(invoiceLineRow.GetField(EStatementConstants.Amount), 0);
			txnLine.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(amount, GlbCompany.CurrentCompany.LocalCurrency, invoiceType);

			ZDecimal taxAmount = ZDecimal.ParseSafe(invoiceLineRow.GetField(EStatementConstants.TaxAmount), 0);
			txnLine.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(taxAmount, GlbCompany.CurrentCompany.LocalCurrency, invoiceType);

			ZDecimal totalAmount = ZDecimal.ParseSafe(invoiceLineRow.GetField(EStatementConstants.TotalAmount), 0);
			txnLine.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(totalAmount, GlbCompany.CurrentCompany.LocalCurrency, invoiceType);

			txnLine.ConsolOrJobNo = invoiceLineRow.GetField(EStatementConstants.JobReference);
			ZString chargeCode = invoiceLineRow.GetField(EStatementConstants.ChargeCode).ToUpper();
			txnLine.ChargeCode = (!chargeCode.IsEmpty) ? chargeCode : DefaultCharge;

			if (invoiceLineRow[EStatementConstants.IsFinal] == ZBool.True.ToString())
			{
				txnLine.IsFinalCharge = true;
				txnLine.IsFinalChargeSpecified = true;
			}
			else if (invoiceLineRow[EStatementConstants.IsFinal] == ZBool.False.ToString())
			{
				txnLine.IsFinalCharge = false;
			}
		}

		internal void SetDescription(FlatFileDataRow invoiceLineRow, Xsd.TxnLine txnLine)
		{
			ZStringBuilder result = new ZStringBuilder();
			result.Append(invoiceLineRow.GetField(EStatementConstants.InvoiceDate));
			if (!invoiceLineRow[EStatementConstants.Description].IsEmpty)
			{
				result.Append(", " + invoiceLineRow[EStatementConstants.Description]);
			}

			if (!invoiceLineRow[EStatementConstants.Items].IsEmpty)
			{
				result.Append(", " + invoiceLineRow[EStatementConstants.Items]);
			}

			if (!invoiceLineRow[EStatementConstants.UnitOfMeasure].IsEmpty)
			{
				result.Append(", " + invoiceLineRow[EStatementConstants.UnitOfMeasure]);
			}

			if (!invoiceLineRow[EStatementConstants.Weight].IsEmpty)
			{
				result.Append(", " + invoiceLineRow[EStatementConstants.Weight]);
			}

			txnLine.Description = result.ToString();
		}

		public static class InvoiceHeaderConstants
		{
			public const int DebtorCode = 0;
			public const int InvoiceNumber = 1;
			public const int DefaultChargeCode = 3;
		}

		public static class EStatementConstants
		{
			public const int InvoiceDate = 0;
			public const int IsFinal = 3;
			public const int ChargeCode = 9;
			public const int Description = 10;
			public const int JobReference = 11;
			public const int Items = 12;
			public const int UnitOfMeasure = 13;
			public const int Weight = 14;
			public const int Amount = 15;
			public const int TaxAmount = 16;
			public const int TotalAmount = 17;
		}

		internal protected bool IsDataValid(FlatFileDataRowCollection rows)
		{
			bool result = false;
			if (rows.Count > 1)
			{
				FlatFileDataRow headerRow = rows[1];
				result = (headerRow.FieldCount == FieldsCount);
			}
			return result;
		}

		#region Test internal variables
		internal void InternalMapImportTest(IValueObject valueObject, FlatFileDataRowCollection fileLines) => MapImport(valueObject, fileLines);
		internal FlatFileDataRow InternalExtractRowFromFormatTest(IFlatFileFormat fileFormat, ZString rawRow) => ExtractRowFromFormat(fileFormat, rawRow);
		internal INotifications InternalNotificationTest => Notification;
		#endregion
		internal ZString DefaultCharge;
		public const int FieldsCount = 18;
	}
}
