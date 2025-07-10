using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1090
	{
		public void Method()
		{
			//CW1090:Don't use System.Windows.Forms dialogs
			_ = new FolderBrowserDialog();
		}
	}
}
