using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.DirectReceiptPayment
{
	public class DirectReceiptPaymentDataAdapter : BaseAccountingDataAdapter<DirectTransactionHeaderBase, Xsd.TxnHeaderDirect>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "TxnHeaders"; }
		}

		public override string RootElementName
		{
			get { return "TxnHeader"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.SingleFinancialInvoiceSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialInvoicesSchema; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(DirectTransactionHeaderBase bizObj, Xsd.TxnHeaderDirect constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting Direct Payments and Receipts is not currently supported.");
		}

		#endregion

		#region Import

		public new void ImportFromValueObject(DirectTransactionHeaderBase transactionHeader, Xsd.TxnHeaderDirect value, IValueObjectImportContext context)
		{
			if (transactionHeader != null)
			{
				base.ImportFromValueObject(transactionHeader, value, context);
			}
			else
			{
				ZString errorMessage = Res.GetString("b95186c4-94e0-4dbd-a54a-c35830cb7690", "This transaction cannot be imported. Ledger: {0}, Transaction Type: {1}.", value.Ledger.ToString(), value.TxnType.ToString());
				context.Notify(new ErrorNotification(ErrorType.Error, errorMessage));
			}
		}

		protected override void ImportFromValueObjectCore(DirectTransactionHeaderBase transactionHeader, Xsd.TxnHeaderDirect xmlTransactionHeader, IValueObjectImportContext context)
		{
			PopulateMissingHeaderAmounts(xmlTransactionHeader);
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			NotificationManager = new NotificationManager(context);
			string errorContext = transactionHeader.HumanReadableName + " ";
			errorContext += xmlTransactionHeader.ReceiptPaymentType.ToString() + " ";
			errorContext += xmlTransactionHeader.ChequeOrReference + ": ";

			if (transactionHeader != null)
			{
				NotificationManager.AddInfoNotification(Res.GetString("8e6f00ce-f245-44f9-99c1-85953867773a", "Processing Transaction: {0}", errorContext));
				ProcessTransaction(transactionHeader, xmlTransactionHeader, context, errorContext);
				NotificationManager.AddNewlineNotification();
			}
			else if (xmlTransactionHeader.GetType() == typeof(Xsd.TxnHeaderDirect))
			{
				string errorMessage = Res.GetString("f2edd6df-04f8-4463-9f84-e52edb3b54ad", "This transaction cannot be imported.") + " ";
				errorMessage += Res.GetString("207bf259-0118-475d-ba46-7eaf7b3c349b", "Ledger: {0},", xmlTransactionHeader.Ledger.ToString()) + " ";
				errorMessage += Res.GetString("9ab50e3d-4f2a-4517-8558-b6769ece1112", "Transaction Type: {0}.", xmlTransactionHeader.TxnType.ToString());
				NotificationManager.AddErrorToNotifications(errorMessage);
			}
			else
			{
				NotificationManager.AddErrorToNotifications(Res.GetString("f2edd6df-04f8-4463-9f84-e52edb3b54ad", "This transaction cannot be imported."));
			}
		}

		void ProcessTransaction(DirectTransactionHeaderBase transactionHeader, Xsd.TxnHeader xmlTransactionHeader, IValueObjectImportContext context, string errorContext)
		{
			TransactionHeaderBuilder builder = new TransactionHeaderBuilder(NotificationManager);
			builder.SetValuesOnDirectReceiptPaymentBusinessObject(transactionHeader, xmlTransactionHeader, context, errorContext);
			builder.CheckTotalsOnTransactionHeaderWithLines(transactionHeader, xmlTransactionHeader, errorContext);
			builder.RunValidationAndReportErrors(transactionHeader);

			if (NotificationManager.ErrorsHaveBeenReported)
			{
				transactionHeader.Delete();
				NotificationManager.AddInfoNotification("  " + Res.GetString("6279baff-c54c-400d-8217-22878483ce2e", "This transaction has errors and was not imported"));
			}
			else
			{
				NotificationManager.AddInfoNotification("  " + Res.GetString("b66fbad5-c82e-47a4-962c-fde89cfd972c", "Completed Processing Transaction"));
			}
		}

		void TransformIntoEquivalentNonReversal(Xsd.TxnHeader xmlTransactionHeader)
		{
			if (IsReversingTransaction(xmlTransactionHeader))
			{
				if (xmlTransactionHeader.TxnType == Xsd.TxnType.DPY)
				{
					xmlTransactionHeader.TxnType = Xsd.TxnType.DRC;
				}
				else if (xmlTransactionHeader.TxnType == Xsd.TxnType.DRC)
				{
					xmlTransactionHeader.TxnType = Xsd.TxnType.DPY;
				}

				InvertSigns(xmlTransactionHeader);
			}
		}

		void PopulateMissingHeaderAmounts(Xsd.TxnHeader txnHeader)
		{
			bool exclTaxAmtIsSpecified = !txnHeader.OsInvoiceAmtExclTax.Value.IsEmpty;
			bool taxAmtIsSpecified = !txnHeader.OsTaxAmount.Value.IsEmpty;
			bool inclTaxAmtIsSpecified = !txnHeader.OsInvoiceAmtInclTax.Value.IsEmpty;

			if (exclTaxAmtIsSpecified && taxAmtIsSpecified && !inclTaxAmtIsSpecified)
			{
				txnHeader.OsInvoiceAmtInclTax.Value = txnHeader.OsInvoiceAmtExclTax.Value + txnHeader.OsTaxAmount.Value;
			}
			else if (exclTaxAmtIsSpecified && !taxAmtIsSpecified && inclTaxAmtIsSpecified)
			{
				txnHeader.OsTaxAmount.Value = txnHeader.OsInvoiceAmtInclTax.Value - txnHeader.OsInvoiceAmtExclTax.Value;
			}
			else if (!exclTaxAmtIsSpecified && taxAmtIsSpecified && inclTaxAmtIsSpecified)
			{
				txnHeader.OsInvoiceAmtExclTax.Value = txnHeader.OsInvoiceAmtInclTax.Value - txnHeader.OsTaxAmount.Value;
			}

			foreach (Xsd.TxnLine txnLine in txnHeader.TxnLines)
			{
				PopulateMissingAmountsForLine(txnLine);
			}
		}

		void PopulateMissingAmountsForLine(Xsd.TxnLine txnLine)
		{
			bool exclTaxAmtIsSpecified = !txnLine.OsInvoiceAmtExclTax.Value.IsEmpty;
			bool taxAmtIsSpecified = !txnLine.OsTaxAmount.Value.IsEmpty;
			bool inclTaxAmtIsSpecified = !txnLine.OsInvoiceAmtInclTax.Value.IsEmpty;

			if (exclTaxAmtIsSpecified && taxAmtIsSpecified && !inclTaxAmtIsSpecified)
			{
				txnLine.OsInvoiceAmtInclTax.Value = txnLine.OsInvoiceAmtExclTax.Value + txnLine.OsTaxAmount.Value;
			}
			else if (exclTaxAmtIsSpecified && !taxAmtIsSpecified && inclTaxAmtIsSpecified)
			{
				txnLine.OsTaxAmount.Value = txnLine.OsInvoiceAmtInclTax.Value - txnLine.OsInvoiceAmtExclTax.Value;
			}
			else if (!exclTaxAmtIsSpecified && taxAmtIsSpecified && inclTaxAmtIsSpecified)
			{
				txnLine.OsInvoiceAmtExclTax.Value = txnLine.OsInvoiceAmtInclTax.Value - txnLine.OsTaxAmount.Value;
			}
		}

		protected override DirectTransactionHeaderBase NewBusinessObject(Xsd.TxnHeaderDirect xmlTxnHeader, IValueObjectImportContext context)
		{
			DirectTransactionHeaderBase bizObj = null;

			TransformIntoEquivalentNonReversal(xmlTxnHeader);

			Type newBizObjType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(xmlTxnHeader);

			if (newBizObjType != null && typeof(DirectTransactionHeaderBase).IsAssignableFrom(newBizObjType))
			{
				bizObj = (DirectTransactionHeaderBase)context.Factory.New(newBizObjType);
			}

			return bizObj;
		}

		bool IsReversingTransaction(Xsd.TxnHeader txnHeader)
		{
			Type bizObjType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(txnHeader);

			if (bizObjType != null && txnHeader != null && txnHeader.OsInvoiceAmtExclTax.Value < 0)
			{
				return bizObjType == typeof(DirectPayment) || bizObjType == typeof(DirectReceipt);
			}

			return false;
		}

		NotificationManager NotificationManager;

		#endregion

		#region Test metheds/property wrapper
		public DirectTransactionHeaderBase NewBusinessObject_ForTestOnly(Xsd.TxnHeaderDirect xmlTxnHeader, IValueObjectImportContext context)
		{
			return NewBusinessObject(xmlTxnHeader, context);
		}

		public void PopulateMissingHeaderAmounts_ForTestOnly(Xsd.TxnHeader txnHeader)
		{
			PopulateMissingHeaderAmounts(txnHeader);
		}

		public void InvertSigns_ForTestOnly(Xsd.TxnHeader txnHeader)
		{
			InvertSigns(txnHeader);
		}
		#endregion
	}
}
