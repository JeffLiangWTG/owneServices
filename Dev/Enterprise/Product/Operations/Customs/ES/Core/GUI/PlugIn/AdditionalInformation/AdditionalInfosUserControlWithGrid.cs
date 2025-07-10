using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI;

public partial class AdditionalInfosUserControlWithGrid : EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid
{
	public AdditionalInfosUserControlWithGrid()
	{
		InitializeComponent();

		AdditionalInfosGrid.ColumnStyles.AddRange(columnsToAdd);
		AdditionalInfosGrid.SetAvailability(IsUCC6AndIsImport, AdditionalInfo.Schema.CSI_RN_NKCountryCode);
	}

	protected override IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayout() => new InvoiceLineAdditionalInformationDetailsLayout();

	protected override IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayout() => new InvoiceHeaderAdditionalInformationDetailsLayout();

	readonly ZGridColumnInfo[] columnsToAdd =
	{
		new ZCodeFindBoxColumnStyleInfo
		{
			ModuleID = ZArchitecture.Modules.ModuleIDs.RefCountry,
			ColumnName = AdditionalInfo.Schema.CSI_RN_NKCountryCode,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(142)
		},
	};

	bool IsUCC6AndIsImport => base.CurrentDataItem is JobDeclaration jobDeclaration && jobDeclaration.IsUCC6AndIsImport;
}
