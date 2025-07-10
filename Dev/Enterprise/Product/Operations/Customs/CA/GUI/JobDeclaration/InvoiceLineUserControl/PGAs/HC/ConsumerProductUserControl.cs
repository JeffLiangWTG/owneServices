namespace Enterprise.Customs.CA.GUI
{
	public partial class ConsumerProductUserControl : HCProgramsBasedUserUserControl
	{
		public ConsumerProductUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
			InitializeLazyCreate(isOnInvoiceLine);
		}

		void InitializeLazyCreate(bool isOnInvoiceLine)
		{
			if (!isOnInvoiceLine)
			{
				DetailsGroupBox.Controls.Remove(UnitVolumeDropEdit);
				DetailsGroupBox.Controls.Remove(QuantityDropEdit);
				DetailsGroupBox.Controls.Remove(GrossWeightDropEdit);
				DetailsGroupBox.Controls.Remove(NetWeightCalcDropEdit);
			}
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			ExpiryDateEdit.Visible = false;
			ModelNameTextBox.Visible = false;
			ExceptProcessing1CheckBox.Visible = false;
			LPCOGridUserControl.Visible = true;
		}

		protected internal override bool ComponentGroupBoxSupported => false;
	}
}
