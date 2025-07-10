namespace AnalyzersRunner.FunctionalTestingTarget.WTG.Analyzers
{
	class WTG1001
	{
		//WTG1001:Do not use the 'private' keyword
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1144:Private test methods")]
		private void Test()
		{ }
	}
}
