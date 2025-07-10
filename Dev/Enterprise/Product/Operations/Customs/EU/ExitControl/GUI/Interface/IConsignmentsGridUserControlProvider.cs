using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IConsignmentsGridUserControlProvider
	{
		CusExitHeader ExitHeader { get; }
		IConsignmentsGridUserControl UserControl { get; }
	}
}
