using System;
using System.Xml.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	internal class UnallocatedTransactionDataAdapter : BaseAccountingDataAdapter<TransactionPendingAllocation, TxnHeader>
	{
		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialTransactionsSchema; }
		}

		protected override void ExportToValueObjectCore(TransactionPendingAllocation invoice, TxnHeader xmlInvoiceHeader, IValueObjectExportContext context)
		{
			xmlInvoiceHeader.Ledger = TxnHeaderMapper.GetTxnHeaderLedger(invoice.AH_Ledger);
			xmlInvoiceHeader.DebtorOrCreditor = OrganisationAdapter.ExportToValueObject(invoice.Header, context);

			if (invoice.Header != null && invoice.Header.MiscServ.OM_OJ_ARDebtorGroup != null && invoice.Header.MiscServ.OM_OJ_ARDebtorGroup.IsValid)
			{
				AccountsReceivable receivable = xmlInvoiceHeader.DebtorOrCreditor.OrganisationDetails.AccountsReceivables.AddNew();
				receivable.AccountGroup = invoice.Factory.Load<OrgDebtorGroup>(invoice.Header.MiscServ.OM_OJ_ARDebtorGroup).OJ_Code;
			}

			if (invoice.Header != null && invoice.Header.MiscServ.OM_OG_APCreditorGroup != null && invoice.Header.MiscServ.OM_OG_APCreditorGroup.IsValid)
			{
				AccountsPayable payable = xmlInvoiceHeader.DebtorOrCreditor.OrganisationDetails.AccountsPayables.AddNew();
				payable.AccountGroup = invoice.Factory.Load<OrgCreditorGroup>(invoice.Header.MiscServ.OM_OG_APCreditorGroup).OG_Code;
			}

			xmlInvoiceHeader.TxnType = TxnHeaderMapper.GetTxnHeaderTxnType(invoice.AH_TransactionType);
			xmlInvoiceHeader.TxnCount = invoice.AH_TransactionCount.ToString();
			xmlInvoiceHeader.TxnCategory = invoice.AH_TransactionCategory;
			xmlInvoiceHeader.TxnNumber = invoice.TransactionNumberPrefixed;
			xmlInvoiceHeader.JobInvoiceNo = invoice.AH_ConsolidatedInvoiceRef;
			xmlInvoiceHeader.TxnReference = invoice.AH_TransactionReference;

			xmlInvoiceHeader.Description = invoice.AH_Desc;
			xmlInvoiceHeader.InvoiceDate = invoice.AH_InvoiceDate;
			xmlInvoiceHeader.InvTerm = invoice.AH_InvoiceTerm;
			xmlInvoiceHeader.InvTermDays = invoice.AH_InvoiceTermDays.ToString();
			xmlInvoiceHeader.DueDate = invoice.AH_DueDate;
			xmlInvoiceHeader.PostDate = invoice.AH_PostDate;

#if DEBUG
			xmlInvoiceHeader.TxnHeaderGUID = "headerGUID";
#else
			xmlInvoiceHeader.TxnHeaderGUID = invoice.PK.ToString();
#endif
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(invoice.Factory);
			xmlInvoiceHeader.GLPeriod = BusinessObjectRetriever.GetGLPeriodFromDate(invoice.Factory, invoice.AH_PostDate).ToString();

			xmlInvoiceHeader.Branch = invoice.Branch.GB_Code;
			xmlInvoiceHeader.Department = invoice.Department.GE_Code;

			xmlInvoiceHeader.LocalInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalExTaxAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalExTaxAmount + invoice.AH_LocalTaxAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType());  // Uses Multiplier
			xmlInvoiceHeader.LocalTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalTaxAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.LocalWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_LocalWHTAmount, invoice.Branch.Company.LocalCurrency, invoice.GetType()); // Uses Multiplier

			xmlInvoiceHeader.OsInvoiceAmtExclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSExTaxAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSExTaxAmount + invoice.AH_OSTaxAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.OsTaxAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSTaxAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier
			xmlInvoiceHeader.OsWHTAmount = TxnHeaderMapper.GetXmlFinancialValue(invoice.AH_OSWHTAmount, invoice.TransactionCurrency, invoice.GetType()); // Uses Multiplier

			xmlInvoiceHeader.GlAccount = ZString.Empty;
			xmlInvoiceHeader.CreatedUserId = invoice.CreatingUserID;

			xmlInvoiceHeader.ENettStoragePaymentDetails = new ENettStoragePaymentDetails();
			xmlInvoiceHeader.ENettStoragePaymentDetails.IsSpecified = false;
		}

		protected override void ImportFromValueObjectCore(TransactionPendingAllocation bizObj, TxnHeader value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		public override string RootElementName
		{
			get { return "UnallocatedTransaction"; }
		}

		public override string RootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}
	}
}
