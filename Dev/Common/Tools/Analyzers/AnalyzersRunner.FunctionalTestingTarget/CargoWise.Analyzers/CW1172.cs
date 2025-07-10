namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1172
	{
		static string CaptionKey => "abcd-1234-efgh-5678";

		public void BadCode()
		{
			// CW1172 If the key of a Res.GetString() invocation is a property, it should be public static and have an initializer
			_ = Res.GetString(CaptionKey, "Addressee");
		}
	}

	static class Res
	{
		public static string GetString(string key, string value) => string.Empty;

		public static string _GetString(int asmid, string key, string value) => string.Empty;
	}
}
