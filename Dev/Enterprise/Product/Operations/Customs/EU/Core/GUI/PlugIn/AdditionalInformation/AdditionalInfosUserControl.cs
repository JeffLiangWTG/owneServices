using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class AdditionalInfosUserControl : BaseCustomsEntryUserControl, ISupportingInfoUserControls
	{
		public AdditionalInfosUserControl()
		{
			InitializeComponent();
		}

		string ISupportingInfoUserControls.GridBindingMember => "FilteredInvoiceLines";

		ZGrid ISupportingInfoUserControls.Grid => AdditionalInfosGrid;
	}
}
