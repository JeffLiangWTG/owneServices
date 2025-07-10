using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.FR.GUI.PlugIn
{
	public partial class SupportingDocumentsUserControl : EU.GUI.PlugIn.SupportingDocumentsUserControl
	{
		public SupportingDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new SupportingDocumentsFieldsControl();

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RemoveColumnsExcept(requiredColumns);
			ModifyColumns();
			SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
		}

		void SetColumnVisible()
		{
			if (IsUCC6AndIsImport)
			{
				SupportingDocumentsGrid.SetAvailability(true, SupportingDocument.Schema.CSI_ReferenceNumber2);
				SupportingDocumentsGrid.SetColumnVisible(true, SupportingDocument.Schema.CSI_ReferenceNumber2);
				SupportingDocumentsGrid.SetColumnVisible(false, SupportingDocument.Schema.CSI_Quantity3);
				SupportingDocumentsGrid.SetAvailability(false, SupportingDocument.Schema.CSI_Quantity3);
				SupportingDocumentsGrid.SetColumnVisible(true, SupportingDocument.Schema.CSI_ItemNumber);
				SupportingDocumentsGrid.SetAvailability(true, SupportingDocument.Schema.CSI_ItemNumber);
			}
			else
			{
				SupportingDocumentsGrid.SetAvailability(true, SupportingDocument.Schema.CSI_Quantity3);
				SupportingDocumentsGrid.SetColumnVisible(true, SupportingDocument.Schema.CSI_Quantity3);
				SupportingDocumentsGrid.SetColumnVisible(false, SupportingDocument.Schema.CSI_ReferenceNumber2);
				SupportingDocumentsGrid.SetAvailability(false, SupportingDocument.Schema.CSI_ReferenceNumber2);
				SupportingDocumentsGrid.SetColumnVisible(false, SupportingDocument.Schema.CSI_ItemNumber);
				SupportingDocumentsGrid.SetAvailability(false, SupportingDocument.Schema.CSI_ItemNumber);
			}
		}

		bool IsUCC6AndIsImport => CurrentDataItem is JobDeclaration declaration && declaration.IsUCC6AndIsImport;

		protected override void ChangeGridColumnsVisibility()
		{
			base.ChangeGridColumnsVisibility();
			SetColumnVisible();
		}

		void ModifyColumns()
		{
			var csi_valueColumn = (ZCalcEditColumnStyleInfo)SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Value);
			csi_valueColumn.Decimals = 4;
			var csi_quantity2Column = (ZCalcEditColumnStyleInfo)SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity2);
			csi_quantity2Column.BindToDecimalPlaces = "Quantity2DecimalPlaces";
		}

		readonly ZGridColumnInfo[] columnsToAdd =
		{
			new ZCheckBoxColumnStyleInfo(SupportingDocument.Schema.CSI_IsDTP, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30)),
			new ZCalcEditColumnStyleInfo(SupportingDocument.Schema.CSI_Quantity3, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140), 2),
			new ZCalcEditColumnStyleInfo(SupportingDocument.Schema.CSI_LineNo, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40), 2),
			new ZCalcEditColumnStyleInfo(SupportingDocument.Schema.CSI_ItemNumber, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40), 2),
			new ZTextBoxColumnStyleInfo
			{
				ColumnName = SupportingDocument.Schema.CSI_AdditionalDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CharacterCasing = CharacterCasing.Normal
			},
			new ZTextBoxColumnStyleInfo
			{
				ColumnName = SupportingDocument.Schema.CSI_Description,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CharacterCasing = CharacterCasing.Normal
			},
			new ZTextBoxColumnStyleInfo
			{
				ColumnName = SupportingDocument.Schema.CSI_ReferenceNumber2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				CharacterCasing = CharacterCasing.Normal
			}
		};

		string[] requiredColumns => new[]
				{
					SupportingDocument.Schema.CSI_Code,
					SupportingDocument.Schema.CSI_ReferenceNumber,
					SupportingDocument.Schema.CSI_Description,
					SupportingDocument.Schema.CSI_IsDTP,
					SupportingDocument.Schema.CSI_Quantity,
					SupportingDocument.Schema.CSI_UnitOfQuantity,
					SupportingDocument.Schema.CSI_Quantity2,
					SupportingDocument.Schema.CSI_UnitOfQuantity2,
					SupportingDocument.Schema.CSI_Value,
					SupportingDocument.Schema.CSI_RX_NKCurrency,
					SupportingDocument.Schema.CSI_DateOfIssue,
					SupportingDocument.Schema.CSI_DateOfExpiry,
					SupportingDocument.Schema.CSI_Quantity3,
					SupportingDocument.Schema.CSI_LineNo,
					SupportingDocument.Schema.CSI_AdditionalDescription,
					SupportingDocument.Schema.CSI_ReferenceNumber2,
					SupportingDocument.Schema.CSI_ItemNumber,
				};
	}
}
