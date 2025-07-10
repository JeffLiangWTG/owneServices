//CW1113:Do Not Show Message Box From Business Layer Analyzer
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	namespace MyApplication.Business
	{
		class CW1113
		{
			public void MyMethod(string value)
			{
				Globals.Message.Show(value, "Warning", MessageBoxButtons.OK, DialogResult.OK);
			}
		}
	}
}
