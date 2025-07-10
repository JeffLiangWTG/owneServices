using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1061
	{
		public void Method()
		{
			//CW1061:Do not use System.DateTime.UtcNow Rule
			_ = DateTime.UtcNow;
		}
	}
}
