using System.Windows.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class InvoiceLinePreviousDocumentsUserControlUCC6 : InvoiceLinePreviousDocumentsUserControl
{
	public InvoiceLinePreviousDocumentsUserControlUCC6()
	{
		InitializeComponent();
		SetVisibilities();
	}

	void SetVisibilities()
	{
		DescriptionTextBox.Visible = false;
	}

	protected override void InitializeGridLayoutCore()
	{
		PreviousDocumentsGrid.ColumnStyles.Clear();
		PreviousDocumentsGrid.ColumnStyles.AddRange(new IZColumnStyleInfo[]
		{
			new ZCodeFindBoxColumnStyleInfo
			{
				ColumnName = PreviousDocument.Schema.CSI_Code,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
				CharacterCasing = CharacterCasing.Normal
			},
			new ZTextBoxColumnStyleInfo
			{
				ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_ReferenceNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250),
				CharacterCasing = CharacterCasing.Normal
			},
			new ZTextBoxColumnStyleInfo
			{
				ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_ItemNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			},
			new ZDropEditColumnStyleInfo
			{
				ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_UnitOfQuantity,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			},
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_Quantity,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				Decimals = PreviousDocument.Schema.CSI_QuantityDecimalPrecision
			},
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_PackQty,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			},
			new ZDropEditColumnStyleInfo
			{
				ColumnName = Customs.Business.AutoCusSupportingInfo.Schema.CSI_PackType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
			},
		});
	}
}
