namespace Enterprise.Customs.CA.GUI
{
	public class InvoiceLineChargesUserControl : Customs.GUI.InvoiceLineChargesUserControl
	{
		protected override string ColumnTitleForGSTApplies
		{
			get { return CACustomsSupplierHeaderUserControl.IsCIFComponent; }
		}
	}
}
