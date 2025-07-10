using Enterprise.ZArchitecture;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1020
	{
		public void Method()
		{
			//CW1020:Don't Use Currency Manager Current Rule
			_ = new ZGrid().ListManager.Current;
		}
	}
}
