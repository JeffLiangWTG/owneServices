// CW1112:Do Not Bind To Enabled Analyzer

using System.Windows.Forms;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1112
	{
		public ControlBindingsCollection DataBindings;

		public void MyMethod(object dataSource, string dataMember)
		{
			DataBindings.Add("Enabled", dataSource, dataMember);
		}
	}
}
