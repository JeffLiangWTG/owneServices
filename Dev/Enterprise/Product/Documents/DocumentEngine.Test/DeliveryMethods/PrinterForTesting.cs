using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class PrinterForTesting : Printer
	{
		internal PrinterForTesting(DocDeliveryPrintDetails printerDetails)
			: base(printerDetails)
		{
		}

		internal void SetAdditionalPropertiesForTesting(StmPrintJob printJob, DeliveryInfo deliveryInfo)
		{
			SetAdditionalProperties(printJob, deliveryInfo);
		}
	}
}
