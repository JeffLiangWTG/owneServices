using System.Diagnostics;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1055
	{
		public void Method()
		{
			//CW1055:Do Not Use Processes.GetProcess or Process.GetProcessByName
			_ = Process.GetProcesses();

			//CW1055:Do Not Use Processes.GetProcess or Process.GetProcessByName
			_ = Process.GetProcessesByName("");
		}
	}
}
