namespace Enterprise.Customs.CA.GUI
{
	public partial class MedicalDevicesUserControl : HCProgramsBasedUserUserControl
	{
		public MedicalDevicesUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			ExpiryDateEdit.Visible = false;
			ManufacturerUserControl.Visible = false;
			TradeNameTextBox.Visible = false;
			LPCOGridUserControl.Visible = true;
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
