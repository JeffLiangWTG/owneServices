using System;
using System.ComponentModel;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	public partial class DateScheduleForm : ScheduleForm
	{
		public DateScheduleForm(DateSchedule businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(DescriptionLabel, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		public new DateSchedule BusinessEntity
		{
			get { return (DateSchedule)base.BusinessEntity; }
		}

		internal Action<bool> ActionOnClosed;
		protected override void OnClosed(EventArgs e)
		{
			ActionOnClosed?.Invoke(BusinessEntity.ByHourAndMinute);
			base.OnClosed(e);
		}
	}
}
