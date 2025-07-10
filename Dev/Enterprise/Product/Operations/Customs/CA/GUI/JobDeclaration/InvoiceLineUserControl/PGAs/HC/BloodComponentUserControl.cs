namespace Enterprise.Customs.CA.GUI
{
	public partial class BloodComponentUserControl : HCProgramsBasedUserUserControl
	{
		public BloodComponentUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			ManufactureDateEdit.Visible = false;
			BrandNameTextBox.Visible = false;
			BatchLotNumberTextBox.Visible = false;
			ManufacturerUserControl.Visible = false;
			TradeNameTextBox.Visible = false;
			ModelNameTextBox.Visible = false;
			ExceptProcessing1CheckBox.Visible = false;
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
