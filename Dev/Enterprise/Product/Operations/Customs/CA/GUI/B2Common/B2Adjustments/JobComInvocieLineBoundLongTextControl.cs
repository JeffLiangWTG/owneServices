using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.GUI
{
	public class JobComInvocieLineBoundLongTextControl : Customs.GUI.LongTextControl
	{
		protected override void OnCurrentDataItemChanged(System.EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var invoiceLine = (JobComInvoiceLine)CurrentItem;
			if (invoiceLine != null && invoiceLine.IsB2AsAccountForSeededLine)
			{
				this.ReadOnly = true;
			}
			else
			{
				this.ReadOnly = false;
			}
		}
	}
}
