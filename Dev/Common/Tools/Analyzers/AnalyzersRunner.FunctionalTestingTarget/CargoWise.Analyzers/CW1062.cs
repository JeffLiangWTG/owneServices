using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1062
	{
		public void Method()
		{
			//CW1062:Do not use System.DateTime.Today Rule
			_ = DateTime.Today;
		}
	}
}
