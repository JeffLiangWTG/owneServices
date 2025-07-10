namespace Enterprise.Customs.CA.GUI
{
	public partial class HumanDrugsUserControl : HCProgramsBasedUserUserControl
	{
		public HumanDrugsUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
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
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
