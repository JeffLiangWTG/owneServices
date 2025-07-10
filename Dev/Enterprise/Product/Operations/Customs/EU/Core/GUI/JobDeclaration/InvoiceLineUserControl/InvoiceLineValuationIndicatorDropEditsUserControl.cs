using System;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoiceLineValuationIndicatorDropEditsUserControl : ZUserControl
	{
		public InvoiceLineValuationIndicatorDropEditsUserControl()
		{
			InitializeComponent();
			PartyRelationShipDropEdit.SupportsEmptyCode = true;
			RestrictionsDropEdit.SupportsEmptyCode = true;
			SaleConditionsDropEdit.SupportsEmptyCode = true;
			DisposalAccrualDropEdit.SupportsEmptyCode = true;
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			// this forces the drop edits to display a description when the control opens first
			PartyRelationShipDropEdit.SelectItem(ValuationIndicatorCodeList.Codes.No);
			RestrictionsDropEdit.SelectItem(ValuationIndicatorCodeList.Codes.No);
			SaleConditionsDropEdit.SelectItem(ValuationIndicatorCodeList.Codes.No);
			DisposalAccrualDropEdit.SelectItem(ValuationIndicatorCodeList.Codes.No);
		}
	}
}
