using System;
using System.Data;

using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters.Accounting.Cyber2
{
	public class InvoiceDataImporter : LedgerDataImporter
	{
		public InvoiceDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool importToCSVFile, ZString accountType)
			: base(logger, dataSourcePath, importToCSVFile, accountType)
		{
		}

		protected override string DataTypeDescription
		{
			get { return "Invoices"; }
		}

		protected override internal DataWriter GetNextDataWriter(BusinessObjectFactory factory)
		{
			DataRow invoiceRow = GetNextDataRow();

			LedgerWriter writer = new LedgerWriter(factory);

			writer.Account = new ZString(invoiceRow[Cyber2Schema.Invoice.Account]).Trim();
			writer.Branch = new ZString(invoiceRow[Cyber2Schema.Invoice.Branch]).Trim();
			writer.Reference = new ZString(invoiceRow[Cyber2Schema.Invoice.Number]).Trim();
			writer.Currency = GetLocalCurrencyCodeIfCurrencyIsEmpty(new ZString(invoiceRow[Cyber2Schema.Invoice.Currency]).Trim());
			writer.LocalBalance = (invoiceRow[Cyber2Schema.Invoice.LocalBalance] == DBNull.Value) ? 0.0m : Convert.ToDecimal(invoiceRow[Cyber2Schema.Invoice.LocalBalance]);
			writer.ForeignBalance = (invoiceRow[Cyber2Schema.Invoice.ForeignBalance] == DBNull.Value) ? 0.0m : Convert.ToDecimal(invoiceRow[Cyber2Schema.Invoice.ForeignBalance]);
			writer.Date = (invoiceRow[Cyber2Schema.Invoice.InvoiceDate] == DBNull.Value) ? ZDateTime.Empty : new ZDateTime(invoiceRow[Cyber2Schema.Invoice.InvoiceDate]);
			writer.DueDate = (invoiceRow[Cyber2Schema.Invoice.DueDate] == DBNull.Value) ? ZDateTime.Empty : new ZDateTime(invoiceRow[Cyber2Schema.Invoice.DueDate]);
			writer.Ledger = GetEnterpriseLedgerType(new ZString(invoiceRow[Cyber2Schema.Invoice.D_C_Flag]));

			writer.LocalBalance = writer.LocalBalance.Round(2);
			writer.ForeignBalance = writer.ForeignBalance.Round(2);

			return writer;
		}

		protected override internal ZString CSVOutputHeader
		{
			get { return new ZString("Account,Reference,InvoiceDate,DueDate,Currency,ForeignAmount,LocalAmount,Branch,Department"); }
		}

		protected override internal ZString SqlText
		{
			get { return new ZString("select " + Fields + " from acc_invoices where " + WhereClause + " order by accountid"); }
		}

		protected string Fields
		{
			get { return "accountID, branch, number, balance, balancef, Curr_name1, doc_date, datedue, amt_local, amt_foreig, d_c_flag"; }
		}

		protected string WhereClause
		{
			get { return "(balance>= 0.005 or balance <= -0.005) and d_c_flag = '" + AccountType + "'"; }
		}
	}
}
