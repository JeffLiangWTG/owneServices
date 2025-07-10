#if DEBUG

using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public partial class AutoJobClosureDiagnosisForm
	{
		public ZButton DiagnoseButton_ForTestOnly => this.diagnoseButton;

		public ZButton ClipboardButton_ForTestOnly => this.clipboardButton;

		public ZTextBox TextBoxStackTrace_ForTestOnly => this.TextBoxStackTrace;
	}
}

#endif
