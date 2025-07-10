using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportInvoiceLineDetailsGroupBoxUserControl : ZUserControl
	{
		public ImportInvoiceLineDetailsGroupBoxUserControl()
		{
			InitializeComponent();

			DetailsPanel.UpdateLayout(new ImportInvoiceLineDetailsLayout());
			QuantityAndWeightPanel.UpdateLayout(new ImportQuantityAndWeightLayout());
			DutyAndTaxInfoPanel.UpdateLayout(new ImportDutyAndTaxInfoLayout());
		}
	}
}
