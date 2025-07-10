using Enterprise.Messaging.Integration;
namespace Enterprise.Customs.GB.Chief.NES
{
	public static class NesConstants
	{
		public const string ApplicationName = "NES email processor";
		public const string SubjectForAcks = "X.400 Inter-Personal Notification";
		public const string ChiefRecipient = "EDRCHIEF";
		public const string ChiefTestFunction = "CHIEFTEST";
		public const string ChiefLiveFunction = "CHIEFLIVE";
		public const string ApplicationCode = ApplicationCodeList.Codes.GbNesAllMessageTypes;
		public const string WtgAlert = "WTGEDCSALERT";// if you change this, change the predicate in the service task definition
		public const string Code = "GNE"; // GB Nes Email
	}
}
