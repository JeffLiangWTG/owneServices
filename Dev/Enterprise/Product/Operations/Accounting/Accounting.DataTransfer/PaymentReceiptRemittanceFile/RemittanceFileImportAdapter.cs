using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Schema;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public class RemittanceFileImportAdapter : BaseAccountingDataAdapter<TransactionHeader, Xsd.FinancialInvoices>
	{
		public Action<Action, INotifications> ProcessWithSaveExceptionHandling { get; set; }

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.NettingClearingJournalsSchema; }
		}

		public override string RootCollectionElementName
		{
			get { return "NettingClearingJournals"; }
		}

		public override string RootElementName
		{
			get { return "FinancialInvoices"; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		protected override TransactionHeader CreateOrUpdateFromValueObjectCore(Xsd.FinancialInvoices value, IValueObjectImportContext context)
		{
			if (value != null && value is Xsd.FinancialInvoices)
			{
				PerformRemittanceFileImport(value.TxnHeader, context, context.Factory);
			}

			return null;
		}

		protected override void ExportToValueObjectCore(TransactionHeader bizObj, Xsd.FinancialInvoices constructedValueObject, IValueObjectExportContext context)
		{
			//Export implemented via CSV export
			return;
		}

		protected override void ImportFromValueObjectCore(TransactionHeader bizObj, Xsd.FinancialInvoices value, IValueObjectImportContext context)
		{
		}

		[SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		public void PerformRemittanceFileImport(IValueObject xSD, INotifications notifications, BusinessObjectFactory factory)
		{
			Argument.NotNull(ProcessWithSaveExceptionHandling, nameof(ProcessWithSaveExceptionHandling));
			var txnHeaderCollection = (Xsd.TxnHeaderCollection)xSD;

			var txnHeaderProcessor = new PaymentReceiptRemittanceFileProcessor(factory, notifications);
			txnHeaderProcessor.OnSavePaymentWithChequeNumberAllocator += OnSavePaymentWithChequeNumberAllocator;
			txnHeaderProcessor.OnPrintMatchingDocument += OnPrintMatchingDocument;
			txnHeaderProcessor.ProcessWithSaveExceptionHandling = ProcessWithSaveExceptionHandling;
			txnHeaderProcessor.ProcessTxnHeaderCollection(txnHeaderCollection);

#if DEBUG
			TestOnlyFirstFactoryUsed = txnHeaderProcessor.TestOnlyFirstFactoryUsed;
#endif
		}

		#region Events

#if DEBUG
		public BusinessObjectFactory TestOnlyFirstFactoryUsed;
#endif

		public event EventHandler OnSavePaymentWithChequeNumberAllocator;

		public event EventHandler<BoolResponseEventArgs> OnPrintMatchingDocument;

		#endregion
	}
}
