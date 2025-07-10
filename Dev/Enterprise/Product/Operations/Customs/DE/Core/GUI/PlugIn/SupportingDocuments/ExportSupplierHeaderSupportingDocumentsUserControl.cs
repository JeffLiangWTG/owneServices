using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	public partial class ExportSupplierHeaderSupportingDocumentsUserControl : SupportingDocumentsUserControl
	{
		public ExportSupplierHeaderSupportingDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new ExportSupplierHeaderSupportingDocumentsFieldsControl();

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RemoveColumnsExcept(reorderedColumnsSequence);
			ModifyColumns();
			SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
			SupportingDocumentsGrid.ReOrderColumns(reorderedColumnsSequence);
		}

		void ModifyColumns()
		{
			var csi_referenceNumber = (ZMultiControlColumnStyleInfo)SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber);
			csi_referenceNumber.MaxLengthOverride = 35;
			csi_referenceNumber.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
		}

		readonly ZGridColumnInfo[] columnsToAdd =
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = SupportingDocument.Schema.CSI_AdditionalDescription,
				CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("0DE812F5-21B9-4679-BDDD-976C63F45D18", "Issuing Authority"),
				MaxLengthOverride = 70,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350)
			},
			new ZCodeFindBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = SupportingDocument.Schema.CSI_FullType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
			},
			new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = SupportingDocument.Schema.CSI_ItemNumber,
				CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("E5CD4910-5A39-4255-8C55-D90EC5C4D81D", "Document Line Item Number"),
				MaxLengthOverride = 5,
				Decimals = 0,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			}
		};

		string[] reorderedColumnsSequence => CachedValueHelper.GetValue(ref reorderedColumnsSequenceCached, () => new string[]
		{
			SupportingDocument.Schema.CSI_FullType,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_DateOfIssue,
			SupportingDocument.Schema.CSI_DateOfExpiry,
			SupportingDocument.Schema.CSI_AdditionalDescription,
			SupportingDocument.Schema.CSI_ItemNumber
		});
		CachedValue<string[]> reorderedColumnsSequenceCached;
	}
}
