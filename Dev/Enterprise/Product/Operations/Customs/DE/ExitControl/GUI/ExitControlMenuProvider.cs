using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.GUI;

namespace Enterprise.Customs.DE.ExitControl.GUI
{
	public class ExitControlMenuProvider : IExitControlMenuProvider
	{
		public IExitControlMainMenuProvider GetExitControlMainMenuProvider(CusExitHeader header) => new ExitControlMainMenuProvider((Business.CusExitHeader)header);

		public IConsignmentsGridUserControlMenuProvider GetConsignmentsGridUserControlMenuProvider(IConsignmentsGridUserControlProvider provider) => new ConsignmentsGridUserControlMenuProvider(provider);

		public IReportsGridUserControlMenuProvider GetReportsGridUserControlMenuProvider(IReportsGridUserControlProvider provider) => new ReportsGridUserControlMenuProvider(provider);
	}
}
