using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1122
	{
		public void Method()
		{
			//CW1122:Do Not Use DateTime Parse Method
			DateTime.Parse("test");
		}
	}
}
