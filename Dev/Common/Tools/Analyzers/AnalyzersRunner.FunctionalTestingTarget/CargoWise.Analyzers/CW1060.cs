using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1060
	{
		public void Method()
		{
			//CW1060:Do not use System.DateTime.Now
			_ = DateTime.Now;
		}
	}
}
