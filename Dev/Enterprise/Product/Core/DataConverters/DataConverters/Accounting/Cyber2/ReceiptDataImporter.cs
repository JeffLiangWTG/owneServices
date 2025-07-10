using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters.Accounting.Cyber2
{
	public class ReceiptDataImporter : LedgerDataImporter
	{
		public ReceiptDataImporter(ProgressLogger logger, ZString dataSourcePath, ZBool importToCSVFile, ZString accountType)
			: base(logger, dataSourcePath, importToCSVFile, accountType)
		{
		}

		protected override internal ZString SqlText
		{
			get { return new ZString("select * from acc_receipts where d_c_flag = '" + AccountType + "' and (balance >= 0.005 or balance <= -0.005) order by accountid"); }
		}

		protected override internal DataWriter GetNextDataWriter(BusinessObjectFactory factory)
		{
			DataRow newRecord = GetNextDataRow();

			LedgerWriter writer = new LedgerWriter(factory);

			writer.Account = new ZString(newRecord[Cyber2Schema.Receipt.AccountId]);
			writer.Branch = new ZString(newRecord[Cyber2Schema.Receipt.Branch]);
			writer.Currency = GetLocalCurrencyCodeIfCurrencyIsEmpty(new ZString(newRecord[Cyber2Schema.Receipt.Currency]));
			writer.LocalBalance = (newRecord[Cyber2Schema.Receipt.Balance] == DBNull.Value) ? ZDecimal.Zero : ConvertToNegative(Convert.ToDecimal(newRecord[Cyber2Schema.Receipt.Balance]));
			writer.Date = (newRecord[Cyber2Schema.Receipt.Date] == DBNull.Value) ? ZDateTime.Empty : new ZDateTime(newRecord[Cyber2Schema.Receipt.Date]);
			writer.DueDate = writer.Date;
			int recNo = (newRecord[Cyber2Schema.Receipt.Number] == DBNull.Value) ? ZInt.Zero : new ZInt(newRecord[Cyber2Schema.Receipt.Number]);
			writer.Reference = recNo.ToString();
			writer.Ledger = GetEnterpriseLedgerType(new ZString(newRecord[Cyber2Schema.Receipt.D_C_Flag]));
			writer.LocalBalance = writer.LocalBalance.Round(2);

			return writer;
		}

		internal ZDecimal ConvertToNegative(ZDecimal number)
		{
			return -number;
		}

		protected override string DataTypeDescription
		{
			get { return "Receipts"; }
		}

		protected override internal ZString CSVOutputHeader
		{
			get { return new ZString("Account,Reference,InvoiceDate,DueDate,Currency,ForeignAmount,LocalAmount,Branch,Department"); }
		}
	}
}
