using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class InvoiceLineExportSupportingDocumentsUserControl : EU.GUI.PlugIn.SupportingDocumentsUserControl
{
	public InvoiceLineExportSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}
	
	protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new InvoiceLineExportSupportingDocumentsFieldsUserControl();

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();
		var indexUOM = SupportingDocumentsGrid.ColumnStyles.IndexOf(SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity));
		SupportingDocumentsGrid.ColumnStyles.Remove(SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity));
		SupportingDocumentsGrid.ColumnStyles.Insert(indexUOM, new ZArchitecture.GUI.ZDropEditColumnStyleInfo
		{
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			ColumnName = SupportingDocument.Schema.CSI_UnitOfQuantity,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		});
		SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
	}

	readonly ZGridColumnInfo[] columnsToAdd =
	{
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
			ColumnName = SupportingDocument.Schema.CSI_AdditionalDescription,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
		},
		new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			ColumnName = SupportingDocument.Schema.CSI_ItemNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
		},
		new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			ColumnName = SupportingDocument.Schema.CSI_PackQty,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
		},
		new ZArchitecture.GUI.ZDropEditColumnStyleInfo
		{
			ColumnName = SupportingDocument.Schema.CSI_PackType,
			CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
		},
	};
}
