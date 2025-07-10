using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class Import5FNMessageDetailsUserControl : ZUserControl
	{
		public Import5FNMessageDetailsUserControl()
		{
			InitializeComponent();
			UpdateLayout();
		}

		void UpdateLayout()
		{
			DetailsPanel.UpdateLayout(new MessageSendingDetailsLayout());
			PostClearanceDetailsPanel.UpdateLayout(new MessageSendingPostClearanceDetailsLayout());
			DutyReductionDetailsPanel.UpdateLayout(new MessageSendingDutyReductionDetailsLayout());
			ReExportReductionDetailsPanel.UpdateLayout(new MessageSendingReExportReductionDetailsLayout());
		}
	}
}
