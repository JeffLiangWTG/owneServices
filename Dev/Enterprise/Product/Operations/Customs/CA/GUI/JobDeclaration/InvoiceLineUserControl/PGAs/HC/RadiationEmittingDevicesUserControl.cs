namespace Enterprise.Customs.CA.GUI
{
	public partial class RadiationEmittingDevicesUserControl : HCProgramsBasedUserUserControl
	{
		public RadiationEmittingDevicesUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			IntendedUseCodeDropEdit.Visible = false;
			GTINNumberTextBox.Visible = false;
			ManufactureDateEdit.Visible = false;
			BrandNameTextBox.Visible = false;
			BatchLotNumberTextBox.Visible = false;
			ExpiryDateEdit.Visible = false;
			ManufacturerUserControl.Visible = false;
			TradeNameTextBox.Visible = false;
			ExceptProcessing1CheckBox.Visible = false;
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
