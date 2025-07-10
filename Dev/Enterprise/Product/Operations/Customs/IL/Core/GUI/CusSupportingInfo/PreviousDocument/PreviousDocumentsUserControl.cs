using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.GUI
{
	public partial class PreviousDocumentsUserControl : BaseCustomsEntryUserControl, ISupportingInfoUserControls
	{
		public PreviousDocumentsUserControl()
		{
			InitializeComponent();
		}

		string ISupportingInfoUserControls.GridBindingMember => "CustomsEntryInstructions";

		ZGrid ISupportingInfoUserControls.Grid => PreviousDocumentsGrid;
	}
}
