using CargoWise.Windows.UI;
using Enterprise.Customs.IN.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class ExportSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
{
	public ExportSupplierHeaderUserControl()
	{
		InitializeComponent();
		InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
		AddGridColumns();
		InvoiceHeaderSWControlsTabPage.RunWhenBindingOrFirstShown((sender, e) =>
		{
			InvoiceHeaderSWControlsUserControl.UserControlType = typeof(SWControlsUserControl);
		});
	}

	void AddGridColumns()
	{
		var authorizedEconomicOperatorGroupName = Res.GetData("AEOColumnGroupName", "AEO Details");
		InvoiceHeadersBoundGrid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo
		{
			ColumnName = JobComInvoiceHeader.Schema.AuthorizedEconomicOperatorOrgPK,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			GroupName = authorizedEconomicOperatorGroupName,
			IsVisible = false
		});

		InvoiceHeadersBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = JobComInvoiceHeader.Schema.AuthorizedEconomicOperatorCountry,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			GroupName = authorizedEconomicOperatorGroupName,
			IsVisible = false,
		});

		InvoiceHeadersBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = JobComInvoiceHeader.Schema.AuthorizedEconomicOperatorCode,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
			GroupName = authorizedEconomicOperatorGroupName,
			IsVisible = false,
		});

		InvoiceHeadersBoundGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo
		{
			ColumnName = JobComInvoiceHeader.Schema.JZ_AuthorizedEconomicOperatorRole,
			Width = ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			GroupName = authorizedEconomicOperatorGroupName,
			IsVisible = false,
		});
	}
}
