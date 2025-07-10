using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTStmScheduleTaskValidation : StmScheduleTaskValidation
	{
		public MENTStmScheduleTaskValidation(MENTStmScheduleTask parent)
			: base(parent)
		{
		}

		new MENTStmScheduleTask Parent
		{
			get { return (MENTStmScheduleTask)base.Parent; }
		}

		protected override void CheckS5_IsActive()
		{
			base.CheckS5_IsActive();

			if (Parent.S5_IsActiveInfo.HasChanges && !Env.Security.MENTAgedScoreQueryChangeCollectionActive.IsAllowed)
			{
				Parent.S5_IsActiveInfo.AddError(Env.Security.MENTAgedScoreQueryChangeCollectionActive.ErrorMessageForNotAllowed);
			}
		}

		protected override void CheckS5_ParentID()
		{
			MandatoryValidation.CheckEntered(Parent.S5_ParentIDInfo);
		}
	}
}
