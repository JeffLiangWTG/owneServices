using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Visualisation.Testing
{
	[TestedType(typeof(VisualiserForm))]
	sealed class VisualiserFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new VisualiserForm();
	}
}
