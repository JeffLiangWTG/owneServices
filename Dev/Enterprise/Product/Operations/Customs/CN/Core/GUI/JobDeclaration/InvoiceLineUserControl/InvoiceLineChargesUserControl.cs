using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.GUI
{
	public partial class InvoiceLineChargesUserControl : Customs.GUI.InvoiceLineChargesUserControl
	{
		public InvoiceLineChargesUserControl()
		{
			InitializeComponent();

			ChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ColumnTitleForDutiable);
			ChargesGrid.SetColumnWidth(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, 80);
			ChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGst);
			ChargesGrid.RefreshTableStyles();

			ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, ColumnTitleForDutiable);
			ApportionedChargesGrid.SetColumnWidth(JobComInvHeaderChargeSchema.J7_IsDutiable.Name, 80);
			ApportionedChargesGrid.SetColumnCaption(JobComInvHeaderChargeSchema.J7_IsGSTApplicable.Name, ColumnTitleForGst);
			ApportionedChargesGrid.RefreshTableStyles();
		}

		protected string ColumnTitleForDutiable => Res.GetString("a380fd72-87ce-4e16-ac6c-76516675b61d", "Add to FOB?");

		protected string ColumnTitleForGst => Res.GetString("90c682b7-bc3a-4b18-afbb-3a637bfadbed", "Add to CIF?");
	}
}
