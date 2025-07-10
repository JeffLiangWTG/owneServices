using Enterprise.Accounting.Business.EInvoicing;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public abstract class ComplianceDocumentBatchToGEIConverter : AccEInvoiceBatchToGEIConverter
	{
		protected override void PerformBeforeConvert(AccEInvoicingBatch batch)
		{
			var exporter = new ComplianceDocumentBatchExporter();
			ComplianceBatch = exporter.CreateComplianceDocumentBatch(batch);
		}

		protected ComplianceDocumentBatch ComplianceBatch;
	}
}