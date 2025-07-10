using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1076
	{
		public void Method()
		{
			//CW1076:Do Not Use MessageBox.Show
			MessageBox.Show("text");
		}
	}
}
