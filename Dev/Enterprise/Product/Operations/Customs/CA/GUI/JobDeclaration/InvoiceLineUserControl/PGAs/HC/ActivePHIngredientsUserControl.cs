using System.Collections.Generic;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	public partial class ActivePHIngredientsUserControl : HCProgramsBasedUserUserControl
	{
		public ActivePHIngredientsUserControl(bool isOnInvoiceLine) : base(isOnInvoiceLine)
		{
			InitializeComponent();
		}

		protected override void SetControlsVisibility()
		{
			base.SetControlsVisibility();
			ExpiryDateEdit.Visible = false;
			ManufacturerUserControl.Visible = false;
			TradeNameTextBox.Visible = false;
			ModelNameTextBox.Visible = false;
			ExceptProcessing1CheckBox.Visible = false;
		}

		protected override void SetLPCOGrid()
		{
			var list = new List<string>(HCPGAHeader.AvailableLPCOFields);
			list.Remove(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation);
			LPCOGridUserControl.RemoveExceptAvailableColumns(list);
		}
	}
}
