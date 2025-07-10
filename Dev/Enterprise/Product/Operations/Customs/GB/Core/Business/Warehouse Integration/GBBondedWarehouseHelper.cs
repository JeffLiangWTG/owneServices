using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.Business
{
	public static class GBBondedWarehouseHelper
	{
		public static void SetupForBondedWarehousing(Declaration.CusEntryHeader entry, EDIMessage outgoingMessage, ZString responseFunction, ILoggingInformation logger)
		{
			if (ShouldUpdateWarehouse(entry.CH_WarehouseTransactionStatus))
			{
				var bondedWarehouseProcessor = new GBBondedWarehouseProcessor(entry, outgoingMessage, responseFunction, logger);
				bondedWarehouseProcessor.SetupForBondedWarehousing();
			}
		}

		static bool ShouldUpdateWarehouse(ZString warehouseTransactionStatus) => !warehouseTransactionStatus.IsEmpty && (WarehouseTransactionStatusList.IsPendingInward(warehouseTransactionStatus) || WarehouseTransactionStatusList.IsPendingOutward(warehouseTransactionStatus));
	}
}
