namespace Enterprise.Customs.CA.GUI
{
	public partial class PesticideUserControl : HCProgramsBasedUserUserControl
	{
		public PesticideUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			if (!isOnInvoiceLine)
			{
				DetailsGroupBox.Controls.Remove(UNDGGuidFindBox);
			}
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			GTINNumberTextBox.Visible = false;
			ExpiryDateEdit.Visible = false;
			ModelNameTextBox.Visible = false;
			LPCOGridUserControl.Visible = true;
		}
	}
}
