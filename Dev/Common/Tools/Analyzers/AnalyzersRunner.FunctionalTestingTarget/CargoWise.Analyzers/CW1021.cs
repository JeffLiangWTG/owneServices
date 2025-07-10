namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	static class CW1021
	{
		//CW1021:Static Fields Are Thread Static Rule
		public static string notReadOnly = "Hello";
	}
}
