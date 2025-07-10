using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IConsignmentsGridUserControl
	{
		ZGrid ConsignmentsGrid { get; }
		CusExitConsignment CurrentDataItem { get; }
	}
}
