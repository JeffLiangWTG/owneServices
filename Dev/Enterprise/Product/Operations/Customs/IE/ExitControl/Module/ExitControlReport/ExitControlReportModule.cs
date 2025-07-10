using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.ExitControl.Module
{
	public class ExitControlReportModule : EU.ExitControl.Module.ExitControlReportModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ExitControlReportFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new ExitControlReportFilterStripControl(GridCollection, (ExitControlReportFilterBusinessObject)FilterBusinessObject);
	}
}
