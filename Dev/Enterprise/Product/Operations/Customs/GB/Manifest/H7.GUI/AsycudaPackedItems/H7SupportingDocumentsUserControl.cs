using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI
{
	public partial class H7SupportingDocumentsUserControl : EU.GUI.PlugIn.SupportingDocumentsUserControl, IAdditionalTabPage
	{
		public H7SupportingDocumentsUserControl()
		{
			InitializeComponent();
			InitializeGridLayout();
			new ControlRebinder().Rebind(this, "FilteredInvoiceLines.SupportingDocuments", "SupportingDocuments");
			new ControlRebinder().Rebind(SupportingDocumentsGrid, "FilteredInvoiceLines.SupportingDocuments", "SupportingDocuments");
		}

		protected override void InitializeGridLayoutCore()
		{
			RemoveColumnsExcept(OrderedColumns);
			SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
			SupportingDocumentsGrid.ReOrderColumns(OrderedColumns);
		}

		readonly ZGridColumnInfo[] columnsToAdd =
		[
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = AutoCusSupportingInfo.Schema.CSI_SubType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(54),
				IsMandatory = true
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = AutoCusSupportingInfo.Schema.CSI_Description,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(372)
			},
			new ZDropEditColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_Availability,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(83)
			},
			new ZDropEditColumnStyleInfo
			{
				BindToList = nameof(AsycudaPackedItem.Lookups) + "." + nameof(SupportingDocumentLookups.ActionList),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = SupportingDocument.Schema.CSI_Actions,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(58)
			},
			new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(372)
			}
		];

		protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new H7SupportingDocumentsFieldsControl();

		public string[] OrderedColumns => new[]
		{
			AutoCusSupportingInfo.Schema.CSI_Code,
			AutoCusSupportingInfo.Schema.CSI_ReferenceNumber,
			SupportingDocument.Schema.CSI_Actions,
			SupportingDocument.Schema.CSI_Availability,
			AutoCusSupportingInfo.Schema.CSI_SubType,
			AutoCusSupportingInfo.Schema.CSI_DateOfIssue,
			AutoCusSupportingInfo.Schema.CSI_DateOfExpiry,
			AutoCusSupportingInfo.Schema.CSI_Description,
			AutoCusSupportingInfo.Schema.CSI_ReferenceNumber2
		};

		#region IAdditionalTabPage Members

		ZUserControl IAdditionalTabPage.AdditionalTabPageUserControl => this;

		ResourceStringData IAdditionalTabPage.AdditionalTabPageCaption => Res.GetData("c8fefd90-2732-490b-aa41-ac1f77e01b5e", "Supporting Documents");

		AdditionalTabPageVisibility IAdditionalTabPage.AdditionalControlVisibility => new AdditionalTabPageVisibility(h => true, null);

		int IAdditionalTabPage.TabPageSequence => 40;

		#endregion
	}
}
