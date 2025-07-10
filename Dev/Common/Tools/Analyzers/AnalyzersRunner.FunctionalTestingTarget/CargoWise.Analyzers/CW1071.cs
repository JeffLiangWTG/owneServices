using System;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1071
	{
		public void Method()
		{
			//CW1071:Do Not Use GC.WaitForPendingFinalizers or .GetTotalMemory(true)
			GC.WaitForPendingFinalizers();

			//CW1071:Do Not Use GC.WaitForPendingFinalizers or .GetTotalMemory(true)
			GC.GetTotalMemory(true);
		}
	}
}
