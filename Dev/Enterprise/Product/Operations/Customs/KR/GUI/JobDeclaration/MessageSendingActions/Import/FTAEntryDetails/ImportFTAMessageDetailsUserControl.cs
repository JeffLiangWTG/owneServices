using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportFTAMessageDetailsUserControl : ZUserControl
	{
		public ImportFTAMessageDetailsUserControl()
		{
			InitializeComponent();
			UpdateLayout();
		}

		void UpdateLayout()
		{
			FTAEntryLineDetailsPanel.UpdateLayout(new FTAEntryDetailsLayout());
		}
	}
}
