using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.GUI
{
	public partial class SendingImportPreviousDocumentsUserControl : ImportPreviousDocumentsUserControl, ISupportingInfoUserControls
	{
		public SendingImportPreviousDocumentsUserControl()
		{
			InitializeComponent();
		}

		string ISupportingInfoUserControls.GridBindingMember => "EntryInstruction";// binding member

		ZGrid ISupportingInfoUserControls.Grid => PreviousDocumentsGrid;
	}
}
