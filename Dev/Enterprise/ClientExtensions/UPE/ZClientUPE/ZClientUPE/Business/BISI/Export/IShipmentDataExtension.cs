using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business.BISI
{
	public static class IShipmentDataExtension
	{
		public const string BISIReference = "BISI";

		public static void LogBISIEventAfterUploaded(this IShipmentData shipmentData)
		{
			if (shipmentData.Logs != null)
			{
				shipmentData.Logs.AddNew(Events.DataExport, BISIReference);
			}
		}
	}
}
