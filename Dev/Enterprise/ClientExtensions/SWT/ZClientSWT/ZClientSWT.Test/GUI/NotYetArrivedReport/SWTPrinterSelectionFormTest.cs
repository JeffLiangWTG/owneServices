using System.Windows.Forms;
using Enterprise.DocumentEngine;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.SWT.GUI.Testing
{
	[TestedType(typeof(SWTPrinterSelectionForm))]
	class SWTPrinterSelectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			DeliveryInstructions instructions = new DeliveryInstructions();
			return new SWTPrinterSelectionForm(instructions);
		}
	}
}
