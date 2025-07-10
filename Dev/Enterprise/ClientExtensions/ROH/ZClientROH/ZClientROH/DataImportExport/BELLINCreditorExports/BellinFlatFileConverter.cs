using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.Rohlig.Bellin
{
	public class BellinFlatFileConverter : AccountingFlatFileConverter
	{
		public BellinFlatFileConverter(INotifications notify, BusinessObjectFactory factory) : base(notify, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			FlatFileDataRowCollection rows = new FlatFileDataRowCollection();
			rows.Add(ExportTransactionHeader((Xsd.TxnHeader)valueObject));
			return rows;
		}

		BellinFlatFileDataRow ExportTransactionHeader(Xsd.TxnHeader header)
		{
			BellinFlatFileDataRow row = new BellinFlatFileDataRow();
			row.InvoiceNumber = header.TxnNumber;
			row.CounterpartNumber = header.DebtorOrCreditor.EDICode;
			row.InvoiceDate = header.InvoiceDate;
			row.MaturityDate = header.DueDate;
			row.Amount = header.OsInvoiceAmtInclTax.Value * -1m;
			row.Currency = header.OsInvoiceAmtInclTax.CurrencyCode;

			// Everything else is left blank

			IncrementTransactionsProcessed(header.TxnType);

			return row;
		}

		void IncrementTransactionsProcessed(Xsd.TxnType txnType)
		{
			switch (txnType)
			{
				case Xsd.TxnType.CRD:
					fNumberOfCreditNotesProcessed++;
					break;

				case Xsd.TxnType.INV:
					fNumberOfInvoicesProcessed++;
					break;

				case Xsd.TxnType.ADJ:
					fNumberOfAdjustmentNotesProcessed++;
					break;
			}
		}

		protected override ZBool fCheckThatAllTransactionsAreExported
		{
			get { return ZBool.True; }
		}
	}
}

#region Implementation
#endregion
