using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class InvoiceHeaderSupportingDocumentsUserControl : SupportingDocumentsUserControl
{
	public InvoiceHeaderSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new InvoiceHeaderSupportingDocumentsFieldsControl();

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		var columnsToKeep = reorderedColumnsSequence.ToList();
		RemoveColumnsExcept(columnsToKeep);
		SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
		SupportingDocumentsGrid.ReOrderColumns(reorderedColumnsSequence);
		SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfExpiry).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
	}

	readonly ZGridColumnInfo[] columnsToAdd =
	{
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
			ColumnName = SupportingDocument.Schema.CSI_AdditionalDescription,
			CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("556EEBD5-49B2-4222-8C13-39E3E665ECE4", "Issuing Authority"),
			MaxLengthOverride = 70,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180)
		},
		new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			ColumnName = SupportingDocument.Schema.CSI_ItemNumber,
			MaxLengthOverride = 5,
			Decimals = 0,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200)
		}
	};

	readonly string[] reorderedColumnsSequence =
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_DateOfExpiry,
		SupportingDocument.Schema.CSI_AdditionalDescription,
		SupportingDocument.Schema.CSI_ItemNumber
	};
}
