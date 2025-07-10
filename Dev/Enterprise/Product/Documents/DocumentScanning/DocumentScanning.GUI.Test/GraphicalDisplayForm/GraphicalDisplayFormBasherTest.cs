using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(GraphicalDisplayForm))]
	sealed class GraphicalDisplayFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new GraphicalDisplayForm(false);
		}
	}
}
