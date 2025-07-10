using System;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Germany
{
	internal class AccInvoiceBatchContextGermanyTestContent : IDisposable
	{
		public const string InvoiceDocumentContentDummy = "We simulate an attached document by manually filling this property";

		public TestObjectCreator ObjectCreator { get; set; }
		public InvoicingBase ArInvoice { get; set; }
		public AccEInvoicingBatch Batch { get; set; }
		public AccEInvoicingTransactionPivot Pivot { get; set; }

		public Event EventDataObject { get; set; }

		public ServiceTaskLogForTesting ServiceTaskLog { get; set; }
		public UniversalMessageProcessingManager Manager { get; set; }

		public void Dispose()
		{
			EventDataObject?.Dispose();
		}
	}
}
