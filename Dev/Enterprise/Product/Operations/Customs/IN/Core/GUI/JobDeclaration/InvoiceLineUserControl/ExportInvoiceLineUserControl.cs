using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class ExportInvoiceLineUserControl : BaseInvoiceLineUserControl
{
	public ExportInvoiceLineUserControl()
	{
		InitializeComponent();

		CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		DutyFreeImportAuthorizationTabPage.RunWhenBindingOrFirstShown((sender, e) =>
		{
			DutyFreeImportAuthorizationPanel.UserControlType = typeof(DutyFreeImportAuthorizationUserControl);
		});
		JobWorkTabPage.RunWhenBindingOrFirstShown((sender, e) =>
		{
			JobWorkPanel.UserControlType = typeof(JobWorkUserControl);
		});
		InvoiceLineSWControlsTabPage.RunWhenBindingOrFirstShown((sender, e) =>
		{
			InvoiceLineSWControlsUserControl.UserControlType = typeof(SWControlsUserControl);
		});
		this.LineDetailTabControl.Controls.Remove(this.LineChargesTabPage);
	}
	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		AddColumnsToGrid();
	}

	void AddColumnsToGrid()
	{
		CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
		{
			ColumnName = JobComInvoiceLine.Schema.JI_UnitPrice,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			Decimals = 5
		});
		CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZCalcEditColumnStyleInfo
		{
			ColumnName = JobComInvoiceLine.Schema.JI_UnitQuantity,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			MaxValue = 99999999,
			Decimals = 0
		});
		CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
		{
			ColumnName = JobComInvoiceLine.Schema.JI_UnitUQ,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(30)
		});
		CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
		{
			ColumnName = JobComInvoiceLine.Schema.JI_AccessoryStatus,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			IsVisible = false,
		});
		CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
		{
			ColumnName = JobComInvoiceLine.Schema.JI_RN_NKCountryOfTransit,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
			IsVisible = false
		});
	}

	protected override ZBool DynamicLayoutApplied => true;

	protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ExportInvoiceLineDetailsLayout();
}
