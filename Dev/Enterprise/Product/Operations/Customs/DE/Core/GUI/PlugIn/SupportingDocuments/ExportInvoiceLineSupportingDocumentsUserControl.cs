using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class ExportInvoiceLineSupportingDocumentsUserControl : SupportingDocumentsUserControl
	{
		public ExportInvoiceLineSupportingDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new ExportInvoiceLineSupportingDocumentsFieldsControl();

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();

			var columnsToKeep = reorderedColumnsSequence.ToList();
			columnsToKeep.Remove(SupportingDocument.Schema.CSI_UnitOfQuantity);
			columnsToKeep.Remove(SupportingDocument.Schema.CSI_UnitOfQuantity2);
			RemoveColumnsExcept(columnsToKeep);
			ModifyColumns();
			SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
			SupportingDocumentsGrid.ReOrderColumns(reorderedColumnsSequence);
		}

		void ModifyColumns()
		{
			var csi_valueColumn = (ZCalcEditColumnStyleInfo)SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Value);
			csi_valueColumn.Decimals = 2;
		}

		readonly ZGridColumnInfo[] columnsToAdd =
		{
			new ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = SupportingDocument.Schema.CSI_Description,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250)
			},
			new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = SupportingDocument.Schema.CSI_FullType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			},
			new ZTextBoxColumnStyleInfo
			{
				ColumnName = SupportingDocument.Schema.CSI_ReferenceNumber2,
				CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("AC904A77-1358-43CD-A5F8-5F4FF29BBA6A", "License/Detail"),
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			},
			new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = SupportingDocument.Schema.CSI_UnitOfQuantity,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107)
			},
			new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = SupportingDocument.Schema.CSI_UnitOfQuantity2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107)
			},
			new ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = SupportingDocument.Schema.CSI_AdditionalDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
			},
			new ZCalcEditColumnStyleInfo
			{
				ColumnName = SupportingDocument.Schema.CSI_ItemNumber,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
			}
		};

		string[] reorderedColumnsSequence => CachedValueHelper.GetValue(ref reorderedColumnsSequenceCached, () => new string[]
		{
			SupportingDocument.Schema.CSI_FullType,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_Description,
			SupportingDocument.Schema.CSI_ReferenceNumber2,
			SupportingDocument.Schema.CSI_Quantity,
			SupportingDocument.Schema.CSI_UnitOfQuantity,
			SupportingDocument.Schema.CSI_UnitOfQuantity2,
			SupportingDocument.Schema.CSI_Value,
			SupportingDocument.Schema.CSI_RX_NKCurrency,
			SupportingDocument.Schema.CSI_DateOfIssue,
			SupportingDocument.Schema.CSI_DateOfExpiry,
			SupportingDocument.Schema.CSI_AdditionalDescription,
			SupportingDocument.Schema.CSI_ItemNumber
		});
		CachedValue<string[]> reorderedColumnsSequenceCached;
	}
}
