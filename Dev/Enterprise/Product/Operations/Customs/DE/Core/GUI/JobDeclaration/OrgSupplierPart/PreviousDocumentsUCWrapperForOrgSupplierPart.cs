using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	public partial class PreviousDocumentsUCWrapperForOrgSupplierPart : PreviousDocumentsUserControl
	{
		public PreviousDocumentsUCWrapperForOrgSupplierPart()
		{
			InitializeComponent();

			Controls.Remove(TopPanel);
			Controls.Remove(BottomPanel);
		}
	}
}
