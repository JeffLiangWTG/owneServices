using System.Reflection;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1089
	{
		public void Method()
		{
			//CW1089:Do Not Use Assembly.GetEntryAssembly()
			Assembly.GetEntryAssembly();
		}
	}
}
