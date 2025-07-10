using System;
using System.Xml.Schema;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class ARAPJournalDataAdapter : BaseAccountingDataAdapter<Journal, Xsd.TxnHeader>, Accounting.Integration.IARAPJournalDataAdapter
	{
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
			get { return AccountingXmlSchemaDefinitions.Instance.SingleFinancialInvoiceSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.FinancialInvoicesSchema; }
		}

		protected override Journal NewBusinessObject(Xsd.TxnHeader xmlTxnHeader, IValueObjectImportContext context)
		{
			Journal bizObj = null;

			Type newBizObjType = TxnHeaderMapper.GetBizObjTypeFromIValueObject(xmlTxnHeader);
			if (newBizObjType != null)
			{
				bizObj = context.Factory.New(newBizObjType) as Journal;
			}

			return bizObj;
		}

		public override Journal CreateOrUpdateFromValueObject(Xsd.TxnHeader value, IValueObjectImportContext context)
		{
			Journal journal = null;
			try
			{
				journal = base.CreateOrUpdateFromValueObject(value, context);
			}
			catch (ArgumentNullException)
			{
				NotificationManager notificationManager = new NotificationManager(context);
				notificationManager.AddErrorToNotifications(Res.GetString("f2a22844-923a-4d0d-9e93-8dab5667650f", "This transaction cannot be imported. Transaction type is not compatible."));
			}
			return journal;
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(Journal bizObj, Xsd.TxnHeader constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting journal is currently not supported");
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(Journal bizObj, Xsd.TxnHeader value, IValueObjectImportContext context)
		{
			Xsd.XmlInterchange interchange = (Xsd.XmlInterchange)context.Interchange;
			NotificationManager notificationManager = new NotificationManager(context);
			string errorContext = Res.GetString("ae48e0a2-e1d9-4c55-a355-70345a3752a6", "Transaction {0} {1} {2}:", value.Ledger.ToString(), value.TxnType.ToString(), value.Description) + " ";

			if (bizObj != null)
			{
				notificationManager.AddInfoNotification(Res.GetString("4160cda9-a6d4-4825-99c7-52481d530cc3", "Processing Transaction: {0}", errorContext));
				ProcessTransaction(bizObj, value, context, notificationManager, errorContext);
				notificationManager.AddNewlineNotification();
			}
			else if (value.GetType() == typeof(Xsd.TxnHeader))
			{
				string errorMessage = Res.GetString("3500b2e1-b63e-41aa-87c6-34648bb8a47c", "This transaction cannot be imported.") + " ";
				errorMessage += Res.GetString("ac02551b-cac6-4fed-9267-041b7bcbd422", "Ledger: {0},", value.Ledger.ToString()) + " ";
				errorMessage += Res.GetString("159a8afa-e978-47bb-9b30-32153f9b7885", "Transaction Type: {0}.", value.TxnType.ToString());
				notificationManager.AddErrorToNotifications(errorMessage);
			}
			else
			{
				notificationManager.AddErrorToNotifications(Res.GetString("3500b2e1-b63e-41aa-87c6-34648bb8a47c", "This transaction cannot be imported."));
			}
		}

		void ProcessTransaction(Journal bizObj, Xsd.TxnHeader xmlTransactionHeader, IValueObjectImportContext context,
			NotificationManager notificationManager, string errorContext)
		{
			TransactionHeaderBuilder builder = new TransactionHeaderBuilder(notificationManager);
			builder.SetValuesOnJournalBusinessObject(bizObj, xmlTransactionHeader, context, errorContext);
			builder.RunValidationAndReportErrors(bizObj);

			if (notificationManager.ErrorsHaveBeenReported)
			{
				bizObj.Delete();
				notificationManager.AddErrorToNotifications("  " + Res.GetString("e834a069-ce77-4996-ba53-cd998b35a507", "This transaction has errors and was not imported"));
			}
			else
			{
				notificationManager.AddInfoNotification("  " + Res.GetString("4bcab094-8229-4fbb-beec-fe3e2e429aa0", "Completed Processing Transaction"));
			}
		}

		#endregion

		#region Test metheds/property wrapper
		public Journal NewBusinessObject_ForTestOnly(Xsd.TxnHeader xmlTxnHeader, IValueObjectImportContext context)
		{
			return NewBusinessObject(xmlTxnHeader, context);
		}
		#endregion
	}
}
