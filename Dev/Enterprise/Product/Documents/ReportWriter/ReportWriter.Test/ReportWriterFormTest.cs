using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(ReportWriterForm))]
	sealed class ReportWriterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ReportWriterForm(new MainBizObj(Factory));
		}
	}
}
