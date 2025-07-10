using System.Reflection;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class MockDocDeliveryForm : DocDeliveryForm
	{
		public MockDocDeliveryForm(DeliveryInstructions instructions)
			: base(instructions, Env.Security.None)
		{
		}

		public MockDocDeliveryForm(PrintTask printTask, DeliveryInstructions instructions)
			: base(printTask, instructions, Env.Security.None)
		{
		}

		public bool IsDocument => (bool)typeof(DocDeliveryForm).GetProperty("IsDocument", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(this);

		public bool IsForm => (bool)typeof(DocDeliveryForm).GetProperty("IsForm", BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance).GetValue(this);

		public new ZCheckBox ShowOnlyPrintersUserCanPrintToCheckBox => base.ShowOnlyPrintersUserCanPrintToCheckBox;

		public new ZButton DeliverButton => base.DeliverButton;

		public new ZButton PreviewButton => base.PreviewButton;

		public new ZButton CloseButton => base.CloseButton;

		public new ZGroupBox MultiDocPackGroupbox => base.MultiDocPackGroupbox;

		public new ZTemplateTabControl MainTabControl => base.MainTabControl;

		public new ZTabPage DocumentsTabPage => base.DocumentsTabPage;

		public new ZDropEdit LanguageZDropEdit => base.LanguageZDropEdit;

		public new ZTabPage MainPage => base.MainPage;

		public new ZCheckBox PrintAsDraftCheckbox => base.PrintAsDraftCheckbox;

		public new ZRadioButton PrintMultipleDocPackRadioButton => base.PrintMultipleDocPackRadioButton;

		public new ZGuidDropEdit PrinterSelectionGuidDropEdit => base.PrinterSelectionGuidDropEdit;

		public new ZRadioButton AutoDeliverMultipleDocPackRadioButton => base.AutoDeliverMultipleDocPackRadioButton;

		public new bool ProcessCmdKey(ref Message msg, Keys keyData) => base.ProcessCmdKey(ref msg, keyData);

		public void ShowVisualiserFormCoreForTest(DocPackVisualiserManager visualizerManager) => base.ShowVisualiserFormCore(visualizerManager);

		public new ZTabPage IncludedEDocsTabPage => base.IncludedEDocsTabPage;

		public new ZGrid DocumentsGrid => base.DocumentsGrid;

		public new ZGrid IncludedEDocsGrid => base.IncludedEDocsGrid;
	}
}
