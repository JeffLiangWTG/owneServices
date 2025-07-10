using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IExitControlMainMenuSupporter
	{
		void AddConsignmentsGridMenuItems(ZMenuItem[] menuItems);

		void AddReportsGridMenuItems(ZMenuItem[] menuItems);
	}
}
