using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.GUI
{
	public partial class InvoiceLineUserControl : Customs.GUI.DeclarationInvoiceLineUserControl
	{
		public InvoiceLineUserControl()
		{
			InitializeComponent();
			AddColumnsToGrid();
		}

		protected override ZBool DynamicLayoutApplied => true;

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new InvoiceLineDetailsLayout();

		protected void AddColumnsToGrid()
		{
			CustomsInvoiceLinesBoundGrid.ColumnStyles.AddRange(
				new[]
				{
					new ZDropEditColumnStyleInfo()
					{
						ColumnName = JobComInvoiceLine.Schema.JI_NewUsed,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					},
				});
		}
	}
}
