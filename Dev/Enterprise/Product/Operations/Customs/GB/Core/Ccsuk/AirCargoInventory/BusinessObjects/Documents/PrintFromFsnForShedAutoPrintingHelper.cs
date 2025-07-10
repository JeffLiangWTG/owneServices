using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Integration;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	internal class PrintFromFsnForShedAutoPrintingHelper
	{
		public PrintFromFsnForShedAutoPrintingHelper(GbEDIMessage fsnEdiMessage, ICcsukCusAwb awb, ILogger logger)
		{
			this.awb = awb;
			this.logger = logger;
			this.fsnEdiMessage = fsnEdiMessage;
		}

		internal void CalculatePrintJobToCreateAndUpdateAllOutturns()
		{
			if (!GBCustomsDataRegistry.Instance.CcsukAllowAutoPrintOfRRA.Value)
			{
				// Feature is disabled in registry
				return;
			}
			if (awb.Status1Date.IsEmpty)
			{
				logger.Log(LogType.Information, "Cannot auto-print RRA (no status 1) for " + awb.ReferenceNumberWithShed);
				return;
			}
			if (!awb.Status2Granted)
			{
				logger.Log(LogType.Information, "Cannot auto-print RRA (no status 2) for " + awb.ReferenceNumberWithShed);
				return;
			}
			if (!awb.ReadOnlyAndPermissionHelper.CAC_ReleasedForRemovalForShedRra.Contains(awb.CustomsActionCode))
			{
				logger.Log(LogType.Information, "Cannot auto-print RRA (no status 3) for " + awb.ReferenceNumberWithShed);
				return;
			}

			if (awb.ReadOnlyAndPermissionHelper.CanReleaseAwbForShedRRA(fsnEdiMessage))
			{
				logger.Log(LogType.Information, "Requesting to auto print RRA for " + awb.ReferenceNumberWithShed);
				new NonPersistentErtsReleaseOrchestrator(awb, logger).ReleaseAndPrintRRAOnFsnOrAwb(fsnEdiMessage);
			}
		}

		readonly ICcsukCusAwb awb;
		readonly GbEDIMessage fsnEdiMessage;
		readonly ILogger logger;
	}
}
