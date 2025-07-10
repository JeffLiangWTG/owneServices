using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.PlugIn;

public partial class InvoiceLineSupportingDocumentsUserControl : EU.GUI.PlugIn.SupportingDocumentsUserControl
{
	public InvoiceLineSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		SupportingDocumentsGrid.RemoveUnneededColumns(columnNames);

		SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);

		SupportingDocumentsGrid.SetResourceStringForColumn(SupportingDocument.Schema.CSI_Value, Res.GetData("B5086AB5 -0375-44CE-99CD-ABA7ACDE9CF5", "Amount"));
		SupportingDocumentsGrid.SetResourceStringForColumn(SupportingDocument.Schema.CSI_DateOfExpiry, Res.GetData("8F334554-7C7E-40D3-921B-E28A3AA579E4", "Validity Date"));
		SupportingDocumentsGrid.SetResourceStringForColumn(SupportingDocument.Schema.CSI_AdditionalDescription, Res.GetData("714FC0BE-4032-426C-AC4A-DF1B7A70F8D0", "Issuing Authority"));

		var newUOMColumn = new ZDropEditColumnStyleInfo(SupportingDocument.Schema.CSI_UnitOfQuantity, ControlDpiScalingHelper.ScaleToCurrentDpiX(50)) { CaptionResourceString = Res.GetData("30B4A487-3E6A-4999-86CE-DFD2F20F36DB", "UOM", "Unit of Measure", "Unit of Measurement") };
		SupportingDocumentsGrid.ColumnStyles.Add(newUOMColumn);

		SupportingDocumentsGrid.ReOrderColumns(columnNames);
	}

	protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new InvoiceLineSupportingDocumentsFieldsControl();

	readonly string[] columnNames =
	{
		SupportingDocument.Schema.CSI_Code,
		SupportingDocument.Schema.CSI_ReferenceNumber,
		SupportingDocument.Schema.CSI_Quantity,
		SupportingDocument.Schema.CSI_Value,
		SupportingDocument.Schema.CSI_RX_NKCurrency,
		SupportingDocument.Schema.CSI_DateOfExpiry
	};

	readonly ZGridColumnInfo[] columnsToAdd =
	{
		new ZTextBoxColumnStyleInfo(SupportingDocument.Schema.CSI_AdditionalDescription, ControlDpiScalingHelper.ScaleToCurrentDpiX(180)),
		new ZTextBoxColumnStyleInfo(SupportingDocument.Schema.CSI_ItemNumber, ControlDpiScalingHelper.ScaleToCurrentDpiX(80)),
	};
}
