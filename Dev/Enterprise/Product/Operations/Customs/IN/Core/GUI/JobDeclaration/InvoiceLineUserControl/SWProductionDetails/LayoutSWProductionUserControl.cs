using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class LayoutSWProductionUserControl : ZUserControl
{
	public LayoutSWProductionUserControl()
	{
		InitializeComponent();
		InitializeGridLayouts();
	}

	void InitializeGridLayouts()
	{
		SWProductionGrid.ApplyGridColumnLayout(new SWProductionGridColumnLayout());
		SWProductionDetailsPanel.UpdateLayout(new SWProductionDetailsLayout());
	}
}
