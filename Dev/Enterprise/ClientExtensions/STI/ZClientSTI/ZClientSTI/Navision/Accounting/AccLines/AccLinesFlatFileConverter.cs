using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GenericJob;
using Enterprise.Accounting.DataTransfer.Invoices.FlatFile;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.STI.Navision
{
	public class AccLinesFlatFileConverter : AccountingFlatFileConverter
	{
		public AccLinesFlatFileConverter(INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
		}

		protected override FlatFileDataRowCollection MapExport(IValueObject valueObject)
		{
			Xsd.TxnHeader header = (Xsd.TxnHeader)valueObject;
			FlatFileDataRowCollection dataRows = new FlatFileDataRowCollection();
			InvoicingBase invoice = Factory.LoadTop1<InvoicingBase>(GetInvoicingBaseQuery(header.Ledger, header.TxnType, header.TxnNumber));
			foreach (InvoicingLineBase line in invoice.Lines)
			{
				dataRows.Add(ExportLine(line, invoice, header));
			}

			return dataRows;
		}

		protected override ZBool fCheckThatAllTransactionsAreExported
		{
			get { return ZBool.False; }
		}

		AccLinesFlatFileDataRow ExportLine(InvoicingLineBase line, InvoicingBase invoice, Xsd.TxnHeader header)
		{
			AccLinesFlatFileDataRow row = new AccLinesFlatFileDataRow();
			ZString jobNumber = GetJobNumber(line);
			JobCharge chargeLine = Factory.LoadTop1<JobCharge>(new ZQuery(JobChargeSchema.JR_AL_ARLine, line.PK));

			row.DocumentType = (invoice.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Invoice) ? Constants.Invoice : Constants.CreditNote;
			row.SellToCustomerNumber = header.DebtorOrCreditor.EDICode;
			row.DocumentNumber = invoice.AH_ConsolidatedInvoiceRef;
			row.LineNumber = LineNumber + "0000";
			row.Type = Constants.GLAccount;
			row.GLAccountNumber = GetGLAccount(line);
			row.LocationCode = line.Branch.GB_Code;
			row.Description = line.AL_Desc;
			row.Quantity = Constants.Quantity;
			row.UnitPrice = line.AL_OSExTaxAmount;
			row.UnitCost = chargeLine != null ? chargeLine.JR_LocalCostAmt : ZDecimal.Zero;
			row.GSTPercentage = line.AL_TaxRateCalc;
			row.AmountExcludingGST = line.AL_OSExTaxAmount;
			row.AmountIncludingGST = line.AL_OverseasTotal;
			row.JobNumber = jobNumber;
			row.Currency = line.TransactionCurrency.RX_Code;
			row.ChargeCode = line.ChargeCode != null ? line.ChargeCode.AC_Code : ZString.Empty;
			row.ShortCutDimension1Code = line.Branch.GB_Code;

			MultiplyAmountsByNegativeOneIfCreditNote(row);

			return row;
		}

		#region Implementation

		int LineNumber
		{
			get { return ++fLineNumber; }
		}
		int fLineNumber;

		protected virtual ZString GetJobNumber(InvoicingLineBase line)
		{
			ZString result = ZString.Empty;
			if (line.Job != null)
			{
				result = line.Factory.Load<GenericJob>(line.Job.JH_ParentID).JobNumber;
			}
			return result;
		}

		protected virtual ZString GetGLAccount(InvoicingLineBase line)
		{
			ZString result = ZString.Empty;
			if (line.ChargeCode != null)
			{
				if (line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue && line.ChargeCode.RevenueAccount != null)
				{
					result = line.ChargeCode.RevenueAccount.AG_AccountNum;
				}
				else if (line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost && line.ChargeCode.CostAccount != null)
				{
					result = line.ChargeCode.CostAccount.AG_AccountNum;
				}
			}
			else if (line.GLHeader != null)
			{
				result = line.GLHeader.AG_AccountNum;
			}
			return result;
		}

		protected virtual ZQuery GetInvoicingBaseQuery(Xsd.TxnLedgerType ledger, Xsd.TxnType type, ZString transactionNum)
		{
			ZQuery invoicingBaseQuery = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, ledger.ToString());
			invoicingBaseQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, type.ToString());
			invoicingBaseQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transactionNum);
			invoicingBaseQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, SQLComparisonOperator.NotEqual, ZGuid.Empty);
			return invoicingBaseQuery;
		}

		void MultiplyAmountsByNegativeOneIfCreditNote(AccLinesFlatFileDataRow row)
		{
			if (row.DocumentType == Constants.CreditNote)
			{
				row.UnitPrice *= -1;
				row.AmountExcludingGST *= -1;
				row.AmountIncludingGST *= -1;
			}
		}

		#endregion
	}
}
