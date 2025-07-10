using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1067
	{
		public void Method()
		{
			//CW1067:Application ThreadException Rule
			Application.ThreadException += (sender, eventArgs) => { };
		}
	}
}
