using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1049
	{
		public void Method()
		{
			//CW1049:Don't Use Application Do Events Rule
			Application.DoEvents();
		}
	}
}
