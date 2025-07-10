namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing
{
	public static class MessageProcessorTestFileConstants
	{
		public static string XMLTestFilePath => $"{baseTestFilePath}.XML";
		public static string G5TestFilePath => $"{XMLTestFilePath}.G5";
		public static string ExpeditionG5TestFilePath => $"{G5TestFilePath}.ExpeditionG5";
		public static string ExpAmendmentG5TestFilePath => $"{G5TestFilePath}.ExpAmendmentG5";
		public static string ReceptionG5TestFilePath => $"{G5TestFilePath}.ReceptionG5";
		public static string ExpCancelG5TestFilePath => $"{G5TestFilePath}.ExpCancelG5";

		const string baseTestFilePath = "Enterprise.Customs.ES.TemporaryStorage.Business.Testing.MessageProcessor.TestFiles";
	}
}
