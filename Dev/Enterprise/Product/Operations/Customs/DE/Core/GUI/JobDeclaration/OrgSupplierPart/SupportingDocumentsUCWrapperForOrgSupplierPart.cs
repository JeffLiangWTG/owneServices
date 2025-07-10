using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	public partial class SupportingDocumentsUCWrapperForOrgSupplierPart : SupportingDocumentsUserControl
	{
		public SupportingDocumentsUCWrapperForOrgSupplierPart()
		{
			InitializeComponent();

			Controls.Remove(SupportingDocumentsGrid);
			Controls.Remove(gridSplitter);
			Controls.Remove(BottomPanel);
		}
	}
}
