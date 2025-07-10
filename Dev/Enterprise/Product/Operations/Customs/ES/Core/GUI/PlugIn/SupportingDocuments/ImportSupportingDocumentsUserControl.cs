using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class ImportSupportingDocumentsUserControl : EU.GUI.PlugIn.SupportingDocumentsUserControl
{
	public ImportSupportingDocumentsUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	protected override BaseCustomsEntryUserControl GetSupportingDocumentsFieldsControl() => new ImportSupportingDocumentsFieldsUserControl();

	protected override void InitializeGridLayoutCore()
	{
		base.InitializeGridLayoutCore();

		var indexUOM = SupportingDocumentsGrid.ColumnStyles.IndexOf(SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity));
		SupportingDocumentsGrid.ColumnStyles.Remove(SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity));
		SupportingDocumentsGrid.ColumnStyles.Insert(indexUOM, new ZDropEditColumnStyleInfo
		{
			CharacterCasing = CharacterCasing.Upper,
			ColumnName = SupportingDocument.Schema.CSI_UnitOfQuantity,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
		});
		SupportingDocumentsGrid.ColumnStyles.AddRange(columnsToAdd);
	}

	readonly ZGridColumnInfo[] columnsToAdd =
	{
		new ZDropEditColumnStyleInfo
		{
			CharacterCasing = CharacterCasing.Upper,
			ColumnName = SupportingDocument.Schema.CSI_Procedure,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90)
		},
		new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CharacterCasing = CharacterCasing.Normal,
			ColumnName = SupportingDocument.Schema.CSI_AdditionalDescription,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
		},
		new ZArchitecture.ZCalcEditColumnStyleInfo
		{
			ColumnName = SupportingDocument.Schema.CSI_ItemNumber,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125)
		},
		new ZCodeFindBoxColumnStyleInfo
		{
			ColumnName = SupportingDocument.Schema.CSI_RN_NKCountryCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(142)
		},
	};

	JobDeclaration ESDeclaration => (JobDeclaration)base.JobDeclaration;

	protected override void ChangeGridColumnsVisibility()
	{
		base.ChangeGridColumnsVisibility();
		bool isImportVersionH1 = ESDeclaration.IsUCC6AndIsImport;
		SupportingDocumentsGrid.SetAvailability(isImportVersionH1, AutoCusSupportingInfo.Schema.CSI_AdditionalDescription);
		SupportingDocumentsGrid.SetAvailability(isImportVersionH1, AutoCusSupportingInfo.Schema.CSI_ItemNumber);
		SupportingDocumentsGrid.SetAvailability(isImportVersionH1, AutoCusSupportingInfo.Schema.CSI_RN_NKCountryCode);
	}
}
