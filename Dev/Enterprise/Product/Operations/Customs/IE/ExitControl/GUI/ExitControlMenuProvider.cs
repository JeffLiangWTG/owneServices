using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;

namespace Enterprise.Customs.IE.ExitControl.GUI
{
	public class ExitControlMenuProvider : IExitControlMenuProvider
	{
		IExitControlMainMenuProvider IExitControlMenuProvider.GetExitControlMainMenuProvider(EU.ExitControl.Business.CusExitHeader header) => new ExitControlMainMenuProvider((CusExitHeader)header);
		IConsignmentsGridUserControlMenuProvider IExitControlMenuProvider.GetConsignmentsGridUserControlMenuProvider(IConsignmentsGridUserControlProvider provider) => new ConsignmentsGridUserControlMenuProvider(provider);
		IReportsGridUserControlMenuProvider IExitControlMenuProvider.GetReportsGridUserControlMenuProvider(IReportsGridUserControlProvider provider) => new ReportsGridUserControlMenuProvider(provider);
	}
}
