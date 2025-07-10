namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public static class H7MessageProcessorTestFileConstants
	{
		public static string XMLTestFilePath => $"{baseTestFilePath}.XML";
		public static string H7TestFilePath => $"{XMLTestFilePath}.H7";
		public static string AnnexH7TestFilePath => $"{H7TestFilePath}.AnnexH7";
		public static string CancellationH7TestFilePath => $"{H7TestFilePath}.CancellationH7";
		public static string DeclarationH7TestFilePath => $"{H7TestFilePath}.DeclarationH7";
		public static string ReexportH7TestFilePath => $"{H7TestFilePath}.ReexportH7";
		public static string QueryH7TestFilePath => $"{H7TestFilePath}.QueryH7";
		const string baseTestFilePath = "Enterprise.Customs.ES.Manifest.H7.Business.Testing.MessageProcessors.TestFiles";
	}
}
