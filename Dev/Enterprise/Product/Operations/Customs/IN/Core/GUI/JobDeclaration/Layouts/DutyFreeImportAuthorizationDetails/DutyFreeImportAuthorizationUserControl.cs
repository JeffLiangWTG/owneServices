using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class DutyFreeImportAuthorizationUserControl : ZUserControl
{
	public DutyFreeImportAuthorizationUserControl()
	{
		InitializeComponent();
		InitializeGridLayouts();
	}

	void InitializeGridLayouts()
	{
		ExportItemDetailsGrid.ApplyGridColumnLayout(new DfiaExportItemDetailsGridColumnsLayout());
		ImportItemDetailsGrid.ApplyGridColumnLayout(new DfiaImportItemDetailsGridColumnsLayout());
	}
}
