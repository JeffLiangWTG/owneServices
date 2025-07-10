using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1091
	{
		public void Method()
		{
			//CW1091:Do Not Set CausesValidation to false
			new Control().CausesValidation = false;
		}
	}
}
