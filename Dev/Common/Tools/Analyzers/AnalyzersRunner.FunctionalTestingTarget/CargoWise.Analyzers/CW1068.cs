using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1068
	{
		public void Method()
		{
			//CW1068:Do Not Use Math.Round
			Math.Round(5.2);
		}
	}
}
