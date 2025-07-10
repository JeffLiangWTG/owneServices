using System.Diagnostics;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1058
	{
		public void Method()
		{
			//CW1058:Do Not Use Debugger.IsAttached
			_ = Debugger.IsAttached;
		}
	}
}
