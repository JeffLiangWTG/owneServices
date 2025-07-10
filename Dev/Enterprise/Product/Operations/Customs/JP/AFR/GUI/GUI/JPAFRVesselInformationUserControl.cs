using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRVesselInformationUserControl : ZUserControl
	{
		public JPAFRVesselInformationUserControl()
		{
			InitializeComponent();
			JPH_VesselNameFindBox.PopupSelected += JPH_VesselNameFindBox_PopupSelected;
		}

		void JPH_VesselNameFindBox_PopupSelected(object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length == 1)
			{
				var vessel = (RefVessel)e.SelectedBusinessObjects[0];
				Header.VesselCombination.OnVesselSelected(vessel);
			}
		}

		public void UpdateControlLayout(bool isShippingLineEntry)
		{
			this.JPH_VesselDetailsChangedCheckBox.Visible = !isShippingLineEntry;
		}

		protected JPAFRHeader Header => (JPAFRHeader)CurrentDataItem;
	}
}
