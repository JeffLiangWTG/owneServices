using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public partial class ReportStatisticsFilterControl : ZFilterStripControl
	{
		public ReportStatisticsFilterControl(IBusinessObjectCollection gridCollection, ReportStatisticsFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
