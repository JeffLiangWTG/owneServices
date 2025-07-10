using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(OverwriteOrCreateNewForm))]
	sealed class OverwriteOrCreateNewFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new OverwriteOrCreateNewForm("test");
		}

		protected override bool AllowTabBackwardCore(Control control, Control previousControl)
		{
			return control.Name == "OverwriteButton" && previousControl.Name == "CreateNewButton";
		}
	}
}
