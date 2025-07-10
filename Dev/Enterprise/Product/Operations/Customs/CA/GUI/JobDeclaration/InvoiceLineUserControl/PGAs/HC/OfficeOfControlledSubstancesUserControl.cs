using Enterprise.Customs.CA.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class OfficeOfControlledSubstancesUserControl : HCProgramsBasedUserUserControl
	{
		public OfficeOfControlledSubstancesUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			GTINNumberTextBox.Visible = false;
			ManufactureDateEdit.Visible = false;
			ExpiryDateEdit.Visible = false;
			TradeNameTextBox.Visible = false;
			ModelNameTextBox.Visible = false;
			ExceptProcessing1CheckBox.Visible = false;
			LPCOGridUserControl.Visible = true;
		}

		protected override void SetComponentGrid()
		{
			base.SetComponentGrid();
			ComponentGridUserControl.SetColumnCaption(Component.Schema.CA_Name, Res.GetString("8c3b1b06-0025-4156-9ea7-c058c9aad637", "Description"));
			ComponentGridUserControl.SetColumnWidth(Component.Schema.CA_Name, 220);
		}
	}
}
