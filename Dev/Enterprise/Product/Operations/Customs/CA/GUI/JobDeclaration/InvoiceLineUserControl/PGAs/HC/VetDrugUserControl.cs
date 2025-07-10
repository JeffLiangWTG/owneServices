namespace Enterprise.Customs.CA.GUI
{
	public partial class VetDrugUserControl : HCProgramsBasedUserUserControl
	{
		public VetDrugUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			ManufacturerUserControl.Visible = false;
			ExpiryDateEdit.Visible = false;
			TradeNameTextBox.Visible = false;
			ModelNameTextBox.Visible = false;
			ExceptProcessing1CheckBox.Visible = false;
			LPCOGridUserControl.Visible = true;
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
