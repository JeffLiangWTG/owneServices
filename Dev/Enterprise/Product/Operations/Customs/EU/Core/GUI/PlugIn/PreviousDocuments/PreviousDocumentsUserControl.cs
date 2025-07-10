using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class PreviousDocumentsUserControl : BaseCustomsEntryUserControl, ISupportingInfoUserControls
	{
		public PreviousDocumentsUserControl()
		{
			InitializeComponent();
		}

		string ISupportingInfoUserControls.GridBindingMember => "FilteredInvoiceLines";

		ZGrid ISupportingInfoUserControls.Grid => PreviousDocumentsGrid;
	}
}
