using System;
using System.Xml.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class FinancialInvoiceAsJournalDataAdapter : BaseAccountingDataAdapter<TransactionHeader, Xsd.TxnHeader>, Accounting.Integration.IFinancialInvoiceAsJournalDataAdapter
	{
		public FinancialInvoiceAsJournalDataAdapter()
			: base()
		{ }

		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "FinancialTransactions"; }
		}

		public override string RootElementName
		{
			get { return "FinancialInvoice"; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialTransactionsSchema; }
		}

		#endregion

		protected override void ImportFromValueObjectCore(TransactionHeader bizObj, Xsd.TxnHeader value, IValueObjectImportContext context)
		{
			throw new NotSupportedException("Exporting is currently not supported");
		}

		protected override void ExportToValueObjectCore(TransactionHeader bizObj, Xsd.TxnHeader constructedValueObject, IValueObjectExportContext context)
		{
			constructedValueObject.Ledger = TxnHeaderMapper.GetTxnHeaderLedger(bizObj.AH_Ledger);
			constructedValueObject.DebtorOrCreditor = OrganisationAdapter.ExportToValueObject(bizObj.Header, context);
			constructedValueObject.TxnType = TxnHeaderMapper.GetTxnHeaderTxnType(ZArchitecture.Core.TransactionTypes.Journal);

			constructedValueObject.OsInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(bizObj.AH_Calc_OSOutstandingAmount, bizObj.TransactionCurrency, bizObj.GetType());
			constructedValueObject.LocalInvoiceAmtInclTax = TxnHeaderMapper.GetXmlFinancialValue(bizObj.AH_LocalOutstandingAmount, bizObj.Branch.Company.LocalCurrency, bizObj.GetType());
			Journal bizObjAsJournal = bizObj as Journal;
			if (bizObjAsJournal != null && bizObjAsJournal.DebitCreditSign == DebitCreditDataEntry.DR)
			{
				constructedValueObject.OsInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(constructedValueObject.OsInvoiceAmtInclTax);
				constructedValueObject.LocalInvoiceAmtInclTax = TxnHeaderMapper.SwapXmlFinancialValueSign(constructedValueObject.LocalInvoiceAmtInclTax);
			}
			constructedValueObject.OsInvoiceAmtExclTax = constructedValueObject.OsInvoiceAmtInclTax;
			constructedValueObject.LocalInvoiceAmtExclTax = constructedValueObject.LocalInvoiceAmtInclTax;

			constructedValueObject.Description = ZString.Format("{0}:{1}:{2}", bizObj.AH_TransactionType, bizObj.AH_TransactionNum,
				bizObj.AH_Desc);
			constructedValueObject.InvoiceDate = bizObj.AH_InvoiceDate;
			constructedValueObject.DueDate = bizObj.AH_DueDate;
			constructedValueObject.PostDate = bizObj.AH_PostDate;

			constructedValueObject.Branch = bizObj.Branch.GB_Code;
			constructedValueObject.Department = bizObj.Department.GE_Code;

			constructedValueObject.GlAccount = bizObj.Factory.Load<AccGLHeader>(bizObj.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsPayable ?
																		(Guid)AccountingConfigurationRegistry.Instance.APJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) : (Guid)AccountingConfigurationRegistry.Instance.ARJournalAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)).AG_AccountNum;

			constructedValueObject.CreatedUserId = bizObj.CreatingUserID;
			constructedValueObject.DebtorOrCreditorGUID = bizObj.AH_OH.ToString();

			constructedValueObject.ENettStoragePaymentDetails = new Xsd.ENettStoragePaymentDetails();
			constructedValueObject.ENettStoragePaymentDetails.IsSpecified = false;
		}
	}
}
