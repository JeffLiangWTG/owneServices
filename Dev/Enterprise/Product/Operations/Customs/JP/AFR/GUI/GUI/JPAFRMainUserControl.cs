using Enterprise.Customs.JP.AFR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRMainUserControl : ZUserControl
	{
		public JPAFRMainUserControl()
		{
			InitializeComponent();
			JPH_VesselNameFindBox.PopupSelected += JPH_VesselNameFindBox_PopupSelected;
		}

		void JPH_VesselNameFindBox_PopupSelected(object sender, EmbeddedModulePopup.SelectedEventArgs e)
		{
			if (e.SelectedBusinessObjects.Length == 1)
			{
				var vessel = (RefVessel)e.SelectedBusinessObjects[0];
				Header.VesselCombination.OnVesselSelected(vessel);
			}
		}

		protected JPAFRHeader Header => (JPAFRHeader)CurrentDataItem;

		public void UpdateControlLayout(bool isShippingLineEntry)
		{
			this.JPH_MasterBillNumberTextBox.Visible = !isShippingLineEntry;
			this.JPH_OperationalCarrierVoyageNoTextBox.Visible = isShippingLineEntry;
			this.JPH_VesselDetailsChangedCheckBox.Visible = !isShippingLineEntry;
			this.JPH_DischargePortSuffixTextBox.Visible = isShippingLineEntry;
			if (isShippingLineEntry)
			{
				this.JPH_BillRegistrationStatusTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("62125735-5EF0-4FFF-99A2-4E379E1E77A5", "ATD Registration", "ATD Registration Status", "Departure Time Registration Status");
				this.JPH_BillRegistrationStatusTextBox.UpdateCaption();
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			JPH_VesselNameFindBox.PopupSelected -= JPH_VesselNameFindBox_PopupSelected;
		}
	}
}
