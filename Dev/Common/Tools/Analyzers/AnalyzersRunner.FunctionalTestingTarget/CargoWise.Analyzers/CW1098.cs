namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1098
	{
		//CW1098:Do Not Initialize String Fields With Res.GetString
		internal readonly string field = Res.GetString("resource key", "english text");
	}
}
