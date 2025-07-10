namespace Enterprise.Customs.CA.GUI
{
	public partial class DonorSemenUserControl : HCProgramsBasedUserUserControl
	{
		public DonorSemenUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			GTINNumberTextBox.Visible = false;
			ManufactureDateEdit.Visible = false;
			BrandNameTextBox.Visible = false;
			BatchLotNumberTextBox.Visible = false;
			ExpiryDateEdit.Visible = false;
			ManufacturerUserControl.Visible = false;
			TradeNameTextBox.Visible = false;
			ModelNameTextBox.Visible = false;
			ExceptProcessing1CheckBox.Visible = false;
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
