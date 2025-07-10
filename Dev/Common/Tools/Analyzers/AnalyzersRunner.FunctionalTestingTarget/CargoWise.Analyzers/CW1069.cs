using System.Diagnostics;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1069
	{
		public void Method()
		{
			//CW1069:Do Not Use EventLog.WriteEntry
			EventLog.WriteEntry("", "");
		}
	}
}
