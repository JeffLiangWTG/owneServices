using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class GuaranteesUserControl : EU.NCTS.GUI.Phase5GuaranteesUserControl
{
	public GuaranteesUserControl()
	{
		InitializeComponent();
	}

	protected override void AddAndRemoveColumns()
	{
		base.AddAndRemoveColumns();
		RemoveColumnStyle(AutoCusBondDetail.Schema.PW_SuretyCode);
	}
}
