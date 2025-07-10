using Enterprise.Customs.AsycudaCustoms.Business;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class ExportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public ExportSupplierHeaderUserControl()
		{
			InitializeComponent();

			this.InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		}

		protected override void RemoveColumns()
		{
			base.RemoveColumns();
			InvoiceChargesGrid.RemoveFromAvailableColumns(InvoiceCharge.Schema.J7_IsGSTApplicable);
			ApportionedChargesGrid.RemoveFromAvailableColumns(InvoiceCharge.Schema.J7_IsGSTApplicable);
			BaseGroupChargesGrid.RemoveFromAvailableColumns(InvoiceCharge.Schema.J7_IsGSTApplicable);
		}

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			InvoiceChargesGrid.SetColumnWidth(InvoiceCharge.Schema.J7_IsDutiable, 120);
			ApportionedChargesGrid.SetColumnWidth(InvoiceCharge.Schema.J7_IsDutiable, 120);
			BaseGroupChargesGrid.SetColumnWidth(InvoiceCharge.Schema.J7_IsDutiable, 120);
		}

		protected override string ColumnTitleWhenExportForDutiable => Res.GetString("8a95aa01-e182-422f-a2f4-55f863cdd027", "Add to Customs Value");
	}
}
