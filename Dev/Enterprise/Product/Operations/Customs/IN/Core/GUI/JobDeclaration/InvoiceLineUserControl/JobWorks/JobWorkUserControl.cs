using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public partial class JobWorkUserControl : ZUserControl
{
	public JobWorkUserControl()
	{
		InitializeComponent();
		InitializeGridLayout();
	}

	void InitializeGridLayout()
	{
		var gridLayout = new JobWorkGridColumnsLayout();
		JobWorkGrid.ApplyGridColumnLayout(gridLayout);
	}
}

