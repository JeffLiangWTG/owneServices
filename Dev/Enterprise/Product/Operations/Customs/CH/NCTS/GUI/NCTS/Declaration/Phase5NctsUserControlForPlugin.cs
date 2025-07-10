using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

public partial class Phase5NctsUserControlForPlugin : EU.NCTS.GUI.Phase5NctsUserControlForPlugin
{
	public Phase5NctsUserControlForPlugin(EU.NCTS.Business.NctsHeader nctsMovement) : base(nctsMovement)
	{
	}

	protected override ZUserControl GetNctsArrivalUserControl() => new Phase5ArrivalNotificationTabUserControl();
}
