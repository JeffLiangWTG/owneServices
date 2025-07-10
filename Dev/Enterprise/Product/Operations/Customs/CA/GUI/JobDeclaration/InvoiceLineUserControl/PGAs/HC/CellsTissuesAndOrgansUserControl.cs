namespace Enterprise.Customs.CA.GUI
{
	public partial class CellsTissuesAndOrgansUserControl : HCProgramsBasedUserUserControl
	{
		public CellsTissuesAndOrgansUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
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
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
