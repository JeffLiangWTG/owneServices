using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI;

public partial class ExportEntryLineAdditionalDataUserControl : EU.GUI.EntryLineAdditionalDataUserControl
{
	public ExportEntryLineAdditionalDataUserControl()
	{
		InitializeComponent();
		SetUpEntryLineSupportingDocumentsGridColumns();
	}

	void SetUpEntryLineSupportingDocumentsGridColumns()
	{
		EntryLineSupportingDocumentsGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = ReadOnlySupportingDocument.Schema.CSI_AdditionalDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
			},
			new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = ReadOnlySupportingDocument.Schema.CSI_ItemNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
			},
			new ZArchitecture.ZCheckBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = ReadOnlySupportingDocument.Schema.IsDocumentHeader,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60)
			}
		});
	}
}
