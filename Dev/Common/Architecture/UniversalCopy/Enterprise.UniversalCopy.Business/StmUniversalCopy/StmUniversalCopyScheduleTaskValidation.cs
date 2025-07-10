using CargoWise.EntityFramework;
using Enterprise.Scheduler.Business;

namespace Enterprise.UniversalCopy.Business
{
	public class StmUniversalCopyScheduleTaskValidation : StmScheduleTaskValidation
	{
		public StmUniversalCopyScheduleTaskValidation(StmUniversalCopyScheduleTask parent)
			: base(parent)
		{ }

		protected override void CheckS5_ScheduleDescription()
		{
			MandatoryValidation.CheckEntered(Parent.S5_ScheduleDescriptionInfo);
			base.CheckS5_ScheduleDescription();
		}

		protected override void CheckS5_ParentID()
		{
			MandatoryValidation.CheckEntered(Parent.S5_ParentIDInfo);
		}
	}
}
