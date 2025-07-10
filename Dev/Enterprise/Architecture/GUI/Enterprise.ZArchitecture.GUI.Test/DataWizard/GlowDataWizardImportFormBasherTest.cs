using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(GlowDataWizardImportForm))]
	sealed class GlowDataWizardImportFormBasherTest : ZFormBasherTest
	{
		readonly GlowLog log = new GlowLog();

		protected override Form GetFormToBashCore()
		{
			return new GlowDataWizardImportForm(log);
		}
	}
}
