namespace Enterprise.Customs.CA.GUI
{
	public class GroupInvoiceUserControl : Customs.GUI.BaseInvoiceGroupingUserControl
	{
		protected override string ColumnTitleForGSTApplies
		{
			get { return CACustomsSupplierHeaderUserControl.IsCIFComponent; }
		}
	}
}
