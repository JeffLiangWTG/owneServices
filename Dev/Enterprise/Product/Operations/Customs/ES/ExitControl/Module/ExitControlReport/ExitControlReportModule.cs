using Enterprise.Customs.EU.ExitControl.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.ExitControl.Module
{
	public class ExitControlReportModule : EU.ExitControl.Module.ExitControlReportModule
	{
		protected override IFilterControl GetNewFilterControl() => new ExitControlReportFilterStripControl(GridCollection, (ExitControlReportFilterBusinessObject)FilterBusinessObject);
	}
}
