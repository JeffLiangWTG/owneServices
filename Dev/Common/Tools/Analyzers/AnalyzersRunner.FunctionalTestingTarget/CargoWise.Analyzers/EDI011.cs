using System.IO;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class EDI011
	{
		public void Method()
		{
			//EDI011:Temp Path Rule
			_ = Path.GetTempPath();

			//EDI011:Temp Path Rule
			_ = Path.GetTempFileName();
		}
	}
}
