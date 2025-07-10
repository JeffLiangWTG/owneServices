using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ExportMessagesUserControl : Customs.GUI.MessageUserControl
	{
		public ExportMessagesUserControl()
		{
			InitializeComponent();
			MessagesTabControl.SelectedTab = messageDetailsTabPage;

			RequiresMergeLabel.AllowOverlap(MainPanel);
		}
	}
}
