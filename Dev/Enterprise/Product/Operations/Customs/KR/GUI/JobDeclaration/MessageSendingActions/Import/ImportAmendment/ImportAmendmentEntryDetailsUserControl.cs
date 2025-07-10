using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportAmendmentEntryDetailsUserControl : ZUserControl
	{
		public ImportAmendmentEntryDetailsUserControl()
		{
			InitializeComponent();
			DynamicImportAmendmentEntryDetailsLayoutPanel.UpdateLayout(new ImportAmendmentEntryDetailsLayout());
			DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel.UpdateLayout(new  ImportAmendmentPenaltyRefundDetailsLayout());
		}
	}
}
