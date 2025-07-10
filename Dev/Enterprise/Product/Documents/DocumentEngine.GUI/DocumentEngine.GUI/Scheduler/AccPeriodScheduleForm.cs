using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class AccPeriodScheduleForm : ScheduleForm
	{
		public AccPeriodScheduleForm(AccPeriodSchedule businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}
	}
}
