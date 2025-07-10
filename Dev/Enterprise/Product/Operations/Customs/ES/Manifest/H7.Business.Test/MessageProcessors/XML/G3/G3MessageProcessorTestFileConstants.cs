namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public static class G3MessageProcessorTestFileConstants
	{
		public static string XMLTestFilePath => $"{baseTestFilePath}.XML";
		public static string G3TestFilePath = $"{XMLTestFilePath}.G3";
		public static string G3DeclarationTestFilePath = $"{G3TestFilePath}.G3Declaration";
		public static string G3RevokeTestFilePath => $"{G3TestFilePath}.G3Revoke";

		const string baseTestFilePath = "Enterprise.Customs.ES.Manifest.H7.Business.Testing.MessageProcessors.TestFiles";
	}
}
