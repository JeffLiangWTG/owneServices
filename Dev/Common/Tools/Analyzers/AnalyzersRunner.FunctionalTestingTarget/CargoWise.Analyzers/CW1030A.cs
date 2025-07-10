using System.IO;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1030A
	{
		public void Method()
		{
			//CW1030A:File.OpenWrite Rule
			File.Open("", mode: FileMode.OpenOrCreate);
		}
	}
}
