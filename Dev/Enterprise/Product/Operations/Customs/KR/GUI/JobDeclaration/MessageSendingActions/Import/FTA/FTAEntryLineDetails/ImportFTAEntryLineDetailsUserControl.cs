using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportFTAEntryLineDetailsUserControl : ZUserControl
	{
		public ImportFTAEntryLineDetailsUserControl()
		{
			InitializeComponent();
			FTAEntryLineDetailsPanel.UpdateLayout(new FTAEntryLineDetailsLayout());
		}
	}
}
