using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public partial class StmScheduleTaskFilterControl : ZFilterStripControl
	{
		public StmScheduleTaskFilterControl(IBusinessObjectCollection gridCollection, StmScheduleTaskFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
