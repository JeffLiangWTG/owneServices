namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1098:Do Not Initialize String Fields With Res.GetString", Justification = "Testing")]
	static class EDI001
	{
		//EDI001:Resource String Static Reference Rule
		public static string field = Res.GetString("resource key", "english text");
	}
}
