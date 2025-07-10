using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ErrorReporting.Module
{
	partial class ErrorReportFilterControl : ZFilterStripControl
	{
		public ErrorReportFilterControl(IBusinessObjectCollection gridCollection, ErrorReportFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
