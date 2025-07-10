using CargoWise.EntityFramework;
using Enterprise.Core.Forms;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.GB.GUI.Plugin
{
	public partial class GBSupportingDocumentsUserControl : SupportingDocumentsUserControl
	{
		public GBSupportingDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
		}

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new GBSupportingDocumentsFieldsControl();

		protected override void InitializeGridLayoutCore()
		{
			base.InitializeGridLayoutCore();
			RemoveColumnsExcept(reorderedColumnsSequence);
			SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
			ModifyColumns();
			SupportingDocumentsGrid.ReOrderColumns(reorderedColumnsSequence);
		}

		void ModifyColumns()
		{
			var csi_UnitOfQuantityColumnStyleInfo = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity);
			csi_UnitOfQuantityColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

			var csi_UnitOfQuantity2ColumnStyleInfo = SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity2);
			csi_UnitOfQuantity2ColumnStyleInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
		}

		readonly ZGridColumnInfo[] columnsToAdd =
		{
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_SubType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(54),
				IsMandatory = true
			},
			new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_Description,
				ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(372)
			},
			new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_Availability,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83)
			},
			new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				BindToList = "Lookups.ActionList",
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_Actions,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_ReferenceNumber2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(372)
			}
		};

		string[] reorderedColumnsSequence => CachedValueHelper.GetValue(ref reorderedColumnsSequenceCached, () => new string[]
		{
			SupportingDocument.Schema.CSI_Code,
			SupportingDocument.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_Actions,
			SupportingDocument.Schema.CSI_Availability,
			SupportingDocument.Schema.CSI_SubType,
			SupportingDocument.Schema.CSI_Quantity,
			SupportingDocument.Schema.CSI_UnitOfQuantity,
			SupportingDocument.Schema.CSI_Quantity2,
			SupportingDocument.Schema.CSI_UnitOfQuantity2,
			SupportingDocument.Schema.CSI_Value,
			SupportingDocument.Schema.CSI_RX_NKCurrency,
			SupportingDocument.Schema.CSI_DateOfIssue,
			SupportingDocument.Schema.CSI_DateOfExpiry,
			SupportingDocument.Schema.CSI_Description,
			SupportingDocument.Schema.CSI_ReferenceNumber2
		});
		CachedValue<string[]> reorderedColumnsSequenceCached;
	}
}
