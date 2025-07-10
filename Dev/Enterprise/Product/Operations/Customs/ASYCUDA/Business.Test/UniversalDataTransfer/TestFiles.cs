namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	internal static class TestFiles
	{
		public static string GetTestFilePath(string filename) => $"{RootPath}{filename}";

		const string RootPath = "Enterprise.Customs.ASYCUDA.Business.Testing.UniversalDataTransfer.TestFiles.";
	}
}
