namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class ScanHouseBillBillProviderExtensions
	{
		public static bool IsHVLVShipment(this IScanHouseBillProvider house)
		{
			return house.ShipmentType == Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
		}
	}
}
