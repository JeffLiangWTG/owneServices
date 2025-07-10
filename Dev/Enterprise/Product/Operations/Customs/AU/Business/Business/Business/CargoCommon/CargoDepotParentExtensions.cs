namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class CargoDepotParentExtensions
	{
		public static void LogReadyForLocalDeliveryIfIsCargoStatusClear(this ICargoDepotEventParent parent)
		{
			if (parent.IsCargoStatusClear)
			{
				parent.IsHeldAtOutturn = false;
				if (parent.ReadyForLocalDeliveryLogs.Count == 0)
				{
					parent.ReadyForLocalDeliveryLogs.AddNew();
				}
			}
		}

		public static void LogCargoReceivedAtDepot(this ICargoDepotEventParent parent)
		{
			parent.IsHeldAtOutturn = true;
			if (parent.CargoReceivedAtDepotLogs.Count == 0)
			{
				parent.CargoReceivedAtDepotLogs.AddNew();
			}
		}

		public static void ResetCargoReceivedAtDepotLogs(this ICargoDepotEventParent parent, string reference)
		{
			parent.IsHeldAtOutturn = true;
			parent.CargoReceivedAtDepotLogs.CancelAll();
			parent.CargoReceivedAtDepotLogs.AddNew(reference);
		}

		public static void OutturnCargo(this ICargoDepotEventParent parent, CusOutturn outturn)
		{
			if (outturn.C5_OutturnResultType != CMROutturnResultType.Codes.ShortLanded || outturn.C5_PackagesOutturned > 0)
			{
				parent.LogCargoReceivedAtDepot();
				parent.LogReadyForLocalDeliveryIfIsCargoStatusClear();
			}
			else
			{
				parent.CargoReceivedAtDepotLogs.CancelAll();
			}
		}
	}
}
