using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer
{
	public class PaymentReceiptRemittanceDataImporter : FlatFileDataImporter
	{
		#region Overrides

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hardcoded Identifier for the DataImportHistory")]
		protected override string ImportTypeForDuplicatesPrevention => "Remittance";

		protected override int DaysToKeepHistoryFor => 7;

		protected override IValueObject CreateXsd()
		{
			return new Xsd.TxnHeaderCollection();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			return new PaymentReceiptRemittanceFileConverter(notificationSubscriber, FactoryProvider.Current);
		}

		protected override IFlatFileFormat FlatFileFormat
		{
			get { return new CsvFlatFileFormat(); }
		}

		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			var adapter = new RemittanceFileImportAdapter() { ProcessWithSaveExceptionHandling = ProcessWithSaveExceptionHandling };
			adapter.OnSavePaymentWithChequeNumberAllocator += (sender, e) => OnSavePaymentWithChequeNumberAllocator?.Invoke(sender, e);
			adapter.OnPrintMatchingDocument += (sender, e) => OnPrintMatchingDocument?.Invoke(sender, e);

			adapter.PerformRemittanceFileImport(xsd, notifications, factory: null);

#if DEBUG
			TestOnlyFirstFactoryUsed = adapter.TestOnlyFirstFactoryUsed;
#endif
			return true;
		}

		#endregion

		#region Events

		public event EventHandler OnSavePaymentWithChequeNumberAllocator;

		public event EventHandler<BoolResponseEventArgs> OnPrintMatchingDocument;

		#endregion

#if DEBUG
		public BusinessObjectFactory TestOnlyFirstFactoryUsed;
#endif
	}
}
