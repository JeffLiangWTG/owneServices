using System.Windows.Forms;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	[TestedType(typeof(DocumentVisualizerProgressForm))]
	sealed class DocumentVisualizerProgressFormFormBasherTest : ProgressFormBasherTest
	{
		protected override Form GetFormToBashCore() => new DocumentVisualizerProgressForm();
	}
}
