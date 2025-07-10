using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class ImportInvoiceLinePreviousDocumentsUserControlUCC6 : InvoiceLinePreviousDocumentsUserControl
{
	public ImportInvoiceLinePreviousDocumentsUserControlUCC6()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		PreviousDocumentsGrid.ColumnStyles.AddRange(new IZColumnStyleInfo[]
		{
			new ZDropEditColumnStyleInfo
			{
				ColumnName = AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60),
			},
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = AutoCusSupportingInfo.Schema.CSI_Quantity2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				Decimals = PreviousDocument.Schema.CSI_Quantity2DecimalPrecision
			},
		});

		PreviousDocumentsGrid.RemoveUnneededColumns(columnNames);
		PreviousDocumentsGrid.ReOrderColumns(columnNames);
	}

	readonly string[] columnNames = {
		AutoCusSupportingInfo.Schema.CSI_Code,
		AutoCusSupportingInfo.Schema.CSI_ReferenceNumber,
		AutoCusSupportingInfo.Schema.CSI_ItemNumber,
		AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity2,
		AutoCusSupportingInfo.Schema.CSI_Quantity2,
		AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity,
		AutoCusSupportingInfo.Schema.CSI_Quantity,
	};
}
