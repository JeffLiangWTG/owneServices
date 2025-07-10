using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI
{
	public class ExportInvoiceLineChargesUserControl : EU.GUI.InvoiceLineChargesUserControl
	{
		public ExportInvoiceLineChargesUserControl()
		{
			InitializeChargesGridLayout();
			InitializeApportionedChargesGridLayout();
		}

		void InitializeChargesGridLayout()
		{
			using (ChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ChargesGrid.RemoveFromAvailableColumns(InvoiceLineCharge.Schema.J7_IsDutiable);
				ChargesGrid.RemoveFromAvailableColumns(InvoiceLineCharge.Schema.J7_IsGSTApplicable);
			}
		}

		void InitializeApportionedChargesGridLayout()
		{
			using (ApportionedChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ApportionedChargesGrid.RemoveFromAvailableColumns(InvoiceLineCharge.Schema.J7_IsDutiable);
				ApportionedChargesGrid.RemoveFromAvailableColumns(InvoiceLineCharge.Schema.J7_IsGSTApplicable);
			}
		}
	}
}
