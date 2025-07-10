// CW1109:Do Not Use System Windows Forms Form Or KForm Analyzer

using System.Windows.Forms;
using CargoWise.Windows.UI;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1109 : System.IDisposable
	{
		readonly Form formField = new KForm();

		public void Confirm()
		{
			var localForm = new Form();
			localForm = formField;
		}

		public void Dispose()
		{
			throw new System.NotImplementedException();
		}
	}
}
