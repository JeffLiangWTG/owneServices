namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public static class MessageProcessorTestFileConstants
	{
		public static string EdifactTestFilePath => $"{baseTestFilePath}.Edifact";
		public static string XMLTestFilePath => $"{baseTestFilePath}.XML";
		public static string NCTSTestFilePath => $"{XMLTestFilePath}.NCTS5";
		public static string CancelNCTSTestFilePath => $"{NCTSTestFilePath}.CancelNCTS5";
		public static string DepartureNCTSTestFilePath => $"{NCTSTestFilePath}.DepartureNCTS5";
		public static string ArrivalNCTSTestFilePath => $"{NCTSTestFilePath}.ArrivalNCTS5";
		public static string AmendmentNCTSTestFilePath => $"{NCTSTestFilePath}.AmendmentNCTS5";
		public static string DepartureClearanceNCTSTestFilePath => $"{NCTSTestFilePath}.DepartureClearanceNCTS5";
		public static string NotifGoodsNCTSTestFilePath => $"{NCTSTestFilePath}.NotifGoodsNCTS5";
		public static string DepartureControlNCTSTestFilePath => $"{NCTSTestFilePath}.InboxNotificationControlComunicationNCTS5";
		public static string QueryNCTSTestFilePath => $"{NCTSTestFilePath}.QueryNCTS5";
		public static string DepartureInvalidateTransitNCTSTestFilePath => $"{NCTSTestFilePath}.InboxNotificationInvalidateTransitNCTS5";
		public static string DepartureDissatisfiedItemNCTSTestFilePath => $"{NCTSTestFilePath}.InboxNotificationDissatisfiedItemNCTS5";
		public static string TNNNCTSTestFilePath => $"{NCTSTestFilePath}.TNNNCTS5";
		public static string AnnexNCTSTestFilePath => $"{NCTSTestFilePath}.AnnexNCTS5";
		public static string NotificationUnloadingNCTSTestFilePath => $"{NCTSTestFilePath}.NotificationUnloadingNCTS5";

		const string baseTestFilePath = "Enterprise.Customs.ES.NCTS.Business.Testing.MessageProcessor.TestFiles";
	}
}
