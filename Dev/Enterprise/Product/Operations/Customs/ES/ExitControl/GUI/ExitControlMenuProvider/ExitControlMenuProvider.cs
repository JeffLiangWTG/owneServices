using Enterprise.Customs.ES.ExitControl.Business;

namespace Enterprise.Customs.ES.ExitControl.GUI
{
	public class ExitControlMenuProvider : EU.ExitControl.GUI.IExitControlMenuProvider
	{
		EU.ExitControl.GUI.IExitControlMainMenuProvider EU.ExitControl.GUI.IExitControlMenuProvider.GetExitControlMainMenuProvider(EU.ExitControl.Business.CusExitHeader header) => new ExitControlMainMenuProvider((CusExitHeader)header);
		EU.ExitControl.GUI.IConsignmentsGridUserControlMenuProvider EU.ExitControl.GUI.IExitControlMenuProvider.GetConsignmentsGridUserControlMenuProvider(EU.ExitControl.GUI.IConsignmentsGridUserControlProvider provider) => new ConsignmentsGridUserControlMenuProvider(provider);
		EU.ExitControl.GUI.IReportsGridUserControlMenuProvider EU.ExitControl.GUI.IExitControlMenuProvider.GetReportsGridUserControlMenuProvider(EU.ExitControl.GUI.IReportsGridUserControlProvider provider) => new ReportsGridUserControlMenuProvider(provider);
	}
}
