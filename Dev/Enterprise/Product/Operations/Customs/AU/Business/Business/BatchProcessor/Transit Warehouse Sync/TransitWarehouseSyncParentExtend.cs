using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class TransitWarehouseSyncParentExtend
	{
		public static void RegisterSyncData(this ITransitWarehouseSyncDataParent parent)
		{
			if (parent != null)
			{
				var service = TransitWarehouseDataSyncServiceProvider.GetService(parent.Factory);
				service.Register(parent);
			}
		}

		public static void RegisterSyncEvent(this ITransitWarehouseSyncEventParent parent, ZString masterBillNumber, ZString houseBillNumber)
		{
			if (parent != null)
			{
				var service = TransitWarehouseEventSyncServiceProvider.GetService(parent.Factory);
				service.Register(parent, masterBillNumber, houseBillNumber);
			}
		}
	}
}
