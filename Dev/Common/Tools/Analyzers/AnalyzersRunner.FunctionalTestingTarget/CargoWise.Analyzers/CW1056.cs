using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1056
	{
		public void Method()
		{
			//CW1056:Do Not Use GC.Collect()
			GC.Collect();
		}
	}
}
