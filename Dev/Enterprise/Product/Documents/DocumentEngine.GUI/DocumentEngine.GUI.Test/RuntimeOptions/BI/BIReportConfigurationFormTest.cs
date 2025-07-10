using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(BiReportConfigurationForm))]
	sealed class BIReportConfigurationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new BiReportConfigurationForm();
		}
	}
}
