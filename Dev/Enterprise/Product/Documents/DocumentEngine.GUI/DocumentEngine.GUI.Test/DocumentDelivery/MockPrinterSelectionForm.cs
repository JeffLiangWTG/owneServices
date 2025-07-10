using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class MockPrinterSelectionForm : PrinterSelectionForm
	{
		public MockPrinterSelectionForm(DeliveryInstructions instructions)
			: base(instructions)
		{
		}

		public new ZButton OKButton
		{
			get { return base.OKButton; }
		}

		public new ZGrid PrintersGrid
		{
			get { return base.PrintersGrid; }
		}
	}
}
