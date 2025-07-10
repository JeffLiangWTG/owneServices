using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1013
	{
		public void Method()
		{
			//CW1013:CargoWise ProgressBar Rule
			_ = new ProgressBar();
		}
	}
}
