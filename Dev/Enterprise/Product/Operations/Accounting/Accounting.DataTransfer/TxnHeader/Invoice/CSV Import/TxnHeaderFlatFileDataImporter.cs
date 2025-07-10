using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile
{
	public partial class TxnHeaderFlatFileDataImporter : FlatFileDataImporter, IDataTransferResultReporter
	{
		public TxnHeaderFlatFileDataImporter()
			: base(new DataTransferBusinessObjectFactoryProvider())
		{
		}

		protected
#if DEBUG
		internal
#endif
		TxnHeaderFlatFileDataImporter(BusinessObjectFactory factory)
			: base(new DataTransferSingleBusinessObjectFactoryProvider(factory))
		{
		}

		public new DataTransferBusinessObjectFactoryProvider FactoryProvider
		{
			get { return (DataTransferBusinessObjectFactoryProvider)base.FactoryProvider; }
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new TxnHeaderFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IValueObject CreateXsd()
		{
			return new Xsd.TxnHeaderCollection();
		}

		protected override bool ShouldSuspendValidation
		{
			get { return false; }
		}

		protected override bool ExtractToDataAdapter(IValueObject xSD, INotifications notifications)
		{
			bool extractionSuccessful = false;
			WasTheLastDataTransferSuccessful = false;

			FinancialInvoiceDataAdapter adapter = new FinancialInvoiceDataAdapter(!ImportingSingleTransaction);
			adapter.RunExtraValidation = RunExtraValidation;

			Xsd.TxnHeaderCollection txnHeaderCollection = (Xsd.TxnHeaderCollection)xSD;

			FactoryProvider.Current.SetContext(BusinessContext.LegacyXMLImport);
			FactoryProvider.Current.SetContext(BusinessContext.AllowReopenJobWhenImporting);
			FactoryProvider.CurrentFactoryChanged += (sender, e) =>
				(sender as BusinessObjectFactoryProvider)?.Current.SetContext(BusinessContext.AllowReopenJobWhenImporting);

			try
			{
				if (txnHeaderCollection != null && txnHeaderCollection.Count > 0)
				{
					ValueObjectImportContext importContext = new ValueObjectImportContext(FactoryProvider, notifications);
					foreach (Xsd.TxnHeader header in txnHeaderCollection)
					{
						if (!adapter.IsValidLedgerTypeandTxnType(header, importContext))
						{
							return false;
						}
					}

					if (ImportingSingleTransaction)
					{
						if (txnHeaderCollection.Count == 1)
						{
							extractionSuccessful = true;
							fLastImportedInvoice = adapter.CreateOrUpdateFromValueObject(txnHeaderCollection[0], importContext);
						}
						else
						{
							notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("4f5ebb22-1290-4fb2-9123-01847f73a157", "This file contains more than one invoice. You can only import one invoice at a time using this feature")));
						}
					}
					else
					{
						List<List<Xsd.TxnHeader>> dividedCollection = new List<List<Xsd.TxnHeader>>();
						List<Xsd.TxnHeader> currentCollection = new List<Xsd.TxnHeader>(MaxTransactionCountCreatedInFactory);
						dividedCollection.Add(currentCollection);
						foreach (Xsd.TxnHeader txnHeader in txnHeaderCollection)
						{
							currentCollection.Add(txnHeader);
							if (currentCollection.Count == MaxTransactionCountCreatedInFactory)
							{
								currentCollection = new List<Xsd.TxnHeader>(MaxTransactionCountCreatedInFactory);
								dividedCollection.Add(currentCollection);
							}
						}

						using (var manager = Db.Connection.BeginTransactionWithManager()) // BusinessObjectFactory can't handle saving really large sets of data, so we're handling this ourselves
						{
							bool isValidToSave = true;
							foreach (List<Xsd.TxnHeader> txnHeaders in dividedCollection)
							{
								List<ITransactionParticipant> eDocFactories = new List<ITransactionParticipant>();
								foreach (Xsd.TxnHeader header in txnHeaders)
								{
									fLastImportedInvoice = adapter.CreateOrUpdateFromValueObject(header, importContext);
									eDocFactories.Add(fLastImportedInvoice.DocManagerInfo.MasterFactory);
								}
								if (!OnlySaveDataWhenNoRecordsHaveErrors || !importContext.NotificationsHasErrors)
								{
									FactoryProvider.SaveCurrentWithAdditionalParticipantsAndCreateNew(eDocFactories.ToArray());
									extractionSuccessful = true;
								}
								else
								{
									isValidToSave = false;
								}
							}
							if (isValidToSave)
							{
								manager.CommitTransaction(); // BusinessObjectFactory can't handle saving really large sets of data, so we're handling this ourselves
							}
							else
							{
								manager.RollbackTransaction(); // BusinessObjectFactory can't handle saving really large sets of data, so we're handling this ourselves
								extractionSuccessful = false;
							}
						}
					}
				}
				else
				{
					notifications.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("8d97820b-2d7c-485d-a2fa-c4ed616c69de", "This file does not contain any transaction headers.")));
				}
			}
			finally
			{
				FactoryProvider.Current.RemoveContext(BusinessContext.LegacyXMLImport);
			}
			WasTheLastDataTransferSuccessful = extractionSuccessful;
			return extractionSuccessful && !ImportingSingleTransaction;
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CsvFlatFileFormat(false); }
		}

		public InvoicingBase ImportedInvoice
		{
			get { return fLastImportedInvoice; }
		}

		InvoicingBase fLastImportedInvoice;

		public ZBool ImportingSingleTransaction
		{
			get { return fImportingSingleTransaction; }
			set { fImportingSingleTransaction = value; }
		}

		ZBool fImportingSingleTransaction;

		public ZBool RunExtraValidation
		{
			get { return fRunExtraValidation; }
			set { fRunExtraValidation = value; }
		}

		ZBool fRunExtraValidation;

		public int MaxTransactionCountCreatedInFactory
		{
			get { return MaxTransactionCountCreatedInFactory_innerValue; }
			set { MaxTransactionCountCreatedInFactory_innerValue = value; }
		}
		public int MaxTransactionCountCreatedInFactory_innerValue = 500;

		#region IDataTransferResultReporter Members

		public bool WasTheLastDataTransferSuccessful
		{
			get;
			private set;
		}

		#endregion
	}
}
