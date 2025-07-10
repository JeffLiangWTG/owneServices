using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging
{
	public partial class DeclarationMessageTypeList
	{
		public static ZBool IsArrivalWithOBS(ZString messageType) =>
				 messageType == Codes.NctsUnloadingRemarks
			   || messageType == Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs
			   || messageType == Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb;

		public static ZBool IsArrivalWithAVI(ZString messageType) =>
			messageType == DeclarationMessageTypeList.Codes.NctsArrivalNotification
				|| messageType == DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithUnloadingRemarksAviPlusObs
				|| messageType == DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb
				|| messageType == DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi;

		public static ZBool IsArrivalWithTNN(ZString messageType) =>
			messageType == DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb
				|| messageType == DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi;
	}
}
