namespace AnalyzersRunner.FunctionalTestingTarget.WTG.Analyzers
{
	class WTG1007
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0100:Remove redundant equality", Justification = "This test triggers 2 analyzers, but only one is tested here")]
		public void Method(bool flag)
		{
			//WTG1007:Do not compare bool to a constant value
			_ = flag == true;
		}
	}
}
