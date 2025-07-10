using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	public partial class AdditionalInfosUCWrapperForOrgSupplierPart : AdditionalInfosUserControl
	{
		public AdditionalInfosUCWrapperForOrgSupplierPart()
		{
			InitializeComponent();

			Controls.Remove(AdditionalInfosGrid);
			Controls.Remove(AdditionalInfosPanel);
		}
	}
}
