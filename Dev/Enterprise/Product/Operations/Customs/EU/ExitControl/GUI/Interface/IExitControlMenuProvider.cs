using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IExitControlMenuProvider
	{
		IExitControlMainMenuProvider GetExitControlMainMenuProvider(CusExitHeader header);
		IConsignmentsGridUserControlMenuProvider GetConsignmentsGridUserControlMenuProvider(IConsignmentsGridUserControlProvider provider);
		IReportsGridUserControlMenuProvider GetReportsGridUserControlMenuProvider(IReportsGridUserControlProvider provider);
	}
}
