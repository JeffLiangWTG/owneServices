using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public class ExportInvoiceLineChargesUserControl : BaseInvoiceLineChargesUserControl
	{
		public ExportInvoiceLineChargesUserControl() : base()
		{
			RemoveColumn(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name);
			var columnTitleForDutiable = Res.GetString("71414e0d-e97f-447e-865e-e4e50be1a84a", "Add to Customs Value");
			ChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, columnTitleForDutiable);
			ChargesGrid.SetColumnWidth(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, 120);

			ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, columnTitleForDutiable);
			ApportionedChargesGrid.SetColumnWidth(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, 120);
		}
	}
}
