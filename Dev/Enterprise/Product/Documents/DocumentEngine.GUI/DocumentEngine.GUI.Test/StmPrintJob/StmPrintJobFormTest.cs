using System.Windows.Forms;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	[TestedType(typeof(StmPrintJobForm))]
	sealed class StmPrintJobFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new StmPrintJobForm(Factory.New<StmPrintJob>());
		}
	}
}
